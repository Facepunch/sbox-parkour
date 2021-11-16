﻿using Facepunch.Movement;
using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Parkour
{
	partial class ParkourController : BasePlayerController
	{

		public float EyeHeight { get; private set; } = 64.0f;
		public float BodyGirth { get; set; } = 32.0f;
		public float BodyHeight { get; set; } = 72.0f;
		public Vector3 Mins { get; private set; }
		public Vector3 Maxs { get; private set; }

		private List<BaseMoveMechanic> mechanics = new();
		private BaseMoveMechanic activeMechanic => mechanics.FirstOrDefault( x => x.IsActive );
		private float _eyePosMult;

		public ParkourController()
		{
			mechanics.Add( new StepJump( this ) );
			mechanics.Add( new Walk( this ) );
			mechanics.Add( new AirMove( this ) );
			mechanics.Add( new WaterMove( this ) );
			mechanics.Add( new WallRun( this ) );
			mechanics.Add( new LadderMove( this ) );
			mechanics.Add( new VaultMove( this ) );
			mechanics.Add( new SideBoost( this ) );
			mechanics.Add( new Unstucker( this ) );
			mechanics.Add( new Ducker( this ) );
			mechanics.Add( new Slide( this ) );
			mechanics.Add( new Ram( this ) );
			mechanics.Add( new LedgeJump( this ) );


			mechanics.Add( new MoveDebug( this ) );
		}

		// should be able to add and remove mechanics without worrying 
		// about something breaking, so this can be bad
		public T GetMechanic<T>() where T : BaseMoveMechanic
		{
			return mechanics.FirstOrDefault( x => x is T ) as T;
		}

		public override void FrameSimulate()
		{
			base.FrameSimulate();

			EyeRot = Input.Rotation;
		}

		public override void Simulate()
		{
			var targetEyePosMult = activeMechanic != null ? activeMechanic.EyePosMultiplier : 1f;
			// todo: make this a real lerp
			_eyePosMult = _eyePosMult.LerpTo( targetEyePosMult, 15f * Time.Delta );

			EyePosLocal = Vector3.Up * (EyeHeight * Pawn.Scale) + TraceOffset;
			EyePosLocal *= _eyePosMult;
			EyeRot = Input.Rotation;
			UpdateBBox();

			// This is confusing and needs review:
			//		PreSimulate and PostSimulate are always called if 
			//		the mechanic is active or AlwaysSimulate=true

			//		Simulate is only called if the mechanic is active, 
			//		AlwaysSimulates, AND there's no other mechanic in control

			//		The control is for things like vaulting, it stops 
			//		all other mechanics until its finished with the vault

			// Pros: modular, easy to edit/add movement mechanics

			foreach ( var m in mechanics )
			{
				if ( !m.IsActive && !m.AlwaysSimulate ) continue;
				m.PreSimulate();
			}

			var control = activeMechanic;

			if ( control == null )
			{
				foreach ( var m in mechanics )
				{
					// try to activate, i.e. vault looks for a ledge in front of the player
					if ( !m.Try() ) continue; 
					control = m;
					break;
				}
			}

			if( control != null && control.TakesOverControl )
			{
				control.Simulate();
			}
			else
			{
				foreach ( var m in mechanics )
				{
					if ( !m.IsActive && !m.AlwaysSimulate ) continue;
					m.Simulate();
				}
			}

			foreach ( var m in mechanics )
			{
				if ( !m.IsActive && !m.AlwaysSimulate ) continue;
				m.PostSimulate();
			}
		}

		public virtual void SetBBox( Vector3 mins, Vector3 maxs )
		{
			Mins = mins;
			Maxs = maxs;
		}

		public virtual void UpdateBBox()
		{
			var girth = BodyGirth * 0.5f;

			var mins = new Vector3( -girth, -girth, 0 ) * Pawn.Scale;
			var maxs = new Vector3( +girth, +girth, BodyHeight ) * Pawn.Scale;

			activeMechanic?.UpdateBBox( ref mins, ref maxs, Pawn.Scale );

			SetBBox( mins, maxs );
		}

		public Vector3 GetWishVelocity( bool zeroPitch = false )
		{
			var result = new Vector3( Input.Forward, Input.Left, 0 );
			var inSpeed = result.Length.Clamp( 0, 1 );
			result *= Input.Rotation;

			if ( zeroPitch )
				result.z = 0;

			result = result.Normal * inSpeed;
			result *= GetWishSpeed();

			return result;
		}

		public virtual float GetWishSpeed()
		{
			var ws = -1f;
			if ( activeMechanic != null ) ws = activeMechanic.GetWishSpeed();
			if ( ws >= 0 ) return ws;

			return GetMechanic<Walk>().GetWishSpeed();
		}

		public void StepMove( float groundAngle = 46f, float stepSize = 18f )
		{
			MoveHelper mover = new MoveHelper( Position, Velocity );
			mover.Trace = mover.Trace.Size( Mins, Maxs ).Ignore( Pawn );
			mover.MaxStandableAngle = groundAngle;

			mover.TryMoveWithStep( Time.Delta, stepSize );

			Position = mover.Position;
			Velocity = mover.Velocity;
		}

		public void Move( float groundAngle = 46f )
		{
			MoveHelper mover = new MoveHelper( Position, Velocity );
			mover.Trace = mover.Trace.Size( Mins, Maxs ).Ignore( Pawn );
			mover.MaxStandableAngle = groundAngle;

			mover.TryMove( Time.Delta );

			Position = mover.Position;
			Velocity = mover.Velocity;
		}

		public void Accelerate( Vector3 wishdir, float wishspeed, float speedLimit, float acceleration )
		{
			if ( speedLimit > 0 && wishspeed > speedLimit )
				wishspeed = speedLimit;

			var currentspeed = Velocity.Dot( wishdir );
			var addspeed = wishspeed - currentspeed;

			if ( addspeed <= 0 )
				return;

			var accelspeed = acceleration * Time.Delta * wishspeed;

			if ( accelspeed > addspeed )
				accelspeed = addspeed;

			Velocity += wishdir * accelspeed;
		}

		public void ApplyFriction( float stopSpeed, float frictionAmount = 1.0f )
		{
			var speed = Velocity.Length;
			if ( speed < 0.1f ) return;

			var control = (speed < stopSpeed) ? stopSpeed : speed;
			var drop = control * Time.Delta * frictionAmount;

			// scale the velocity
			float newspeed = speed - drop;
			if ( newspeed < 0 ) newspeed = 0;

			if ( newspeed != speed )
			{
				newspeed /= speed;
				Velocity *= newspeed;
			}
		}

		public void ClearGroundEntity()
		{
			if ( GroundEntity == null ) return;

			GroundEntity = null;
			GroundNormal = Vector3.Up;
		}

		public override TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
		{
			return TraceBBox( start, end, Mins, Maxs, liftFeet );
		}

	}
}
