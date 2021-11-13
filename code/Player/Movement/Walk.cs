using Sandbox;

namespace Facepunch.Parkour
{
	class Walk : BaseMoveMechanic
	{

		public float StopSpeed => 100.0f;
		public float StepSize => 18.0f;
		public float GroundAngle => 46.0f;
		public bool AutoJump => false;
		public float JumpPower => 322f;
		public float SprintSpeed => 250f;
		public float WalkSpeed => 150.0f;
		public float DefaultSpeed => 350f;
		public float GroundFriction => 4.0f;
		public float MaxNonJumpVelocity => 140.0f;
		public float SurfaceFriction { get; set; } = 1f;
		public float Acceleration => 2f;
		public float DuckAcceleration => 5f;
		public float Momentum { get; set; }

		public override bool AlwaysSimulate => true;

		public Walk( ParkourController controller )
			: base( controller )
		{

		}

		public override void Simulate()
		{
			if ( ctrl.GroundEntity == null ) return;

			WalkMove();
			DoMomentum();
			CheckJumpButton();
		}

		public override void PostSimulate()
		{
			base.PostSimulate();

			CategorizePosition( ctrl.GroundEntity != null );
		}

		public override float GetWishSpeed()
		{
			if ( Input.Down( InputButton.Run ) ) return SprintSpeed;
			if ( Input.Down( InputButton.Walk ) ) return WalkSpeed;

			return DefaultSpeed;
		}

		private void WalkMove()
		{
			var wishVel = ctrl.GetWishVelocity( true );
			var wishdir = wishVel.Normal;
			var wishspeed = wishVel.Length;

			// todo: might wanna keep things contained...
			var ducker = ctrl.GetMechanic<Ducker>();
			var friction = GroundFriction * SurfaceFriction;
			bool sliding = false, ducking = false;

			if( ducker != null )
			{
				sliding = ducker.Sliding;
				ducking = ducker.IsActive;
				friction = sliding ? 1f : friction;
			}

			ctrl.Velocity = ctrl.Velocity.WithZ( 0 );
			ApplyFriction( StopSpeed, friction );

			if ( sliding )
			{
				if ( ctrl.GroundNormal.z < 1 )
				{
					var slopeDir = Vector3.Cross( Vector3.Up, Vector3.Cross( Vector3.Up, ctrl.GroundNormal ) );
					var dot = Vector3.Dot( ctrl.Velocity.Normal, slopeDir );
					var slopeForward = Vector3.Cross( ctrl.GroundNormal, ctrl.Pawn.Rotation.Right );
					var spdGain = 200f.LerpTo( 500f, 1f - ctrl.GroundNormal.z );

					if ( dot > 0 )
						spdGain *= -1;

					ctrl.Velocity += spdGain * slopeForward * Time.Delta;
					// todo: redo momentum, seems like a good source of prediction error
					Momentum += Time.Delta; 
				}
			}

			var accel = ducking && !sliding ? DuckAcceleration : Acceleration;
			accel += Momentum;

			if ( sliding )
				accel = .5f;

			ctrl.Velocity = ctrl.Velocity.WithZ( 0 );
			ctrl.Accelerate( wishdir, wishspeed, 0, accel );
			ctrl.Velocity = ctrl.Velocity.WithZ( 0 );

			// Add in any base velocity to the current velocity.
			ctrl.Velocity += ctrl.BaseVelocity;

			try
			{
				if ( ctrl.Velocity.Length < 1.0f )
				{
					ctrl.Velocity = Vector3.Zero;
					return;
				}

				// first try just moving to the destination
				var dest = (ctrl.Position + ctrl.Velocity * Time.Delta).WithZ( ctrl.Position.z );

				var pm = ctrl.TraceBBox( ctrl.Position, dest );

				if ( pm.Fraction == 1 )
				{
					ctrl.Position = pm.EndPos;
					StayOnGround();
					return;
				}
				
				ctrl.StepMove();
			}
			finally
			{

				// Now pull the base velocity back out.   Base velocity is set if you are on a moving object, like a conveyor (or maybe another monster?)
				ctrl.Velocity -= ctrl.BaseVelocity;
			}
			
			StayOnGround();
		}

		private void CheckJumpButton()
		{
			if ( !AutoJump && !Input.Pressed( InputButton.Jump ) )
				return;

			var flGroundFactor = 1.0f;
			var flMul = JumpPower;
			var startz = ctrl.Velocity.z;

			// todo: might wanna keep things contained...
			var ducker = ctrl.GetMechanic<Ducker>();
			var ducking = ducker != null && ducker.IsActive;

			if ( ducking )
				flMul *= 0.8f;

			var jumpPower = startz + flMul * flGroundFactor;

			ctrl.ClearGroundEntity();
			ctrl.Velocity = ctrl.Velocity.WithZ( jumpPower );
			ctrl.AddEvent( "jump" );
		}

		private void DoMomentum()
		{
			var a = ctrl.Velocity.WithZ( 0 ).Length / DefaultSpeed;

			Momentum = 0f.LerpTo( 2f, a );
		}

		/// <summary>
		/// Try to keep a walking player on the ground when running down slopes etc
		/// </summary>
		private void StayOnGround()
		{
			var start = ctrl.Position + Vector3.Up * 2;
			var end = ctrl.Position + Vector3.Down * StepSize;

			// See how far up we can go without getting stuck
			var trace = ctrl.TraceBBox( ctrl.Position, start );
			start = trace.EndPos;

			// Now trace down from a known safe position
			trace = ctrl.TraceBBox( start, end );

			if ( trace.Fraction <= 0 ) return;
			if ( trace.Fraction >= 1 ) return;
			if ( trace.StartedSolid ) return;
			if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > GroundAngle ) return;

			// This is incredibly hacky. The real problem is that trace returning that strange value we can't network over.
			// float flDelta = fabs( mv->GetAbsOrigin().z - trace.m_vEndPos.z );
			// if ( flDelta > 0.5f * DIST_EPSILON )

			ctrl.Position = trace.EndPos;
		}

		private void ApplyFriction( float stopSpeed, float frictionAmount = 1.0f )
		{
			var speed = ctrl.Velocity.Length;
			if ( speed < 0.1f ) return;

			var control = (speed < stopSpeed) ? stopSpeed : speed;
			var drop = control * Time.Delta * frictionAmount;

			// scale the velocity
			float newspeed = speed - drop;
			if ( newspeed < 0 ) newspeed = 0;

			if ( newspeed != speed )
			{
				newspeed /= speed;
				ctrl.Velocity *= newspeed;
			}
		}

		private void CategorizePosition( bool bStayOnGround )
		{
			// Doing this before we move may introduce a potential latency in water detection, but
			// doing it after can get us stuck on the bottom in water if the amount we move up
			// is less than the 1 pixel 'threshold' we're about to snap to.	Also, we'll call
			// this several times per frame, so we really need to avoid sticking to the bottom of
			// water on each call, and the converse case will correct itself if called twice.
			//CheckWater();

			var point = ctrl.Position - Vector3.Up * 2;
			var vBumpOrigin = ctrl.Position;

			//
			//  Shooting up really fast.  Definitely not on ground trimed until ladder shit
			//
			bool bMovingUpRapidly = ctrl.Velocity.z > MaxNonJumpVelocity;
			bool bMoveToEndPos = false;

			if ( ctrl.GroundEntity != null ) // and not underwater
			{
				bMoveToEndPos = true;
				point.z -= StepSize;
			}
			else if ( bStayOnGround )
			{
				bMoveToEndPos = true;
				point.z -= StepSize;
			}

			if ( bMovingUpRapidly )
			{
				ctrl.ClearGroundEntity();
				return;
			}

			var pm = ctrl.TraceBBox( vBumpOrigin, point, 4.0f );

			if ( pm.Entity == null || Vector3.GetAngle( Vector3.Up, pm.Normal ) > GroundAngle )
			{
				ctrl.ClearGroundEntity();
				bMoveToEndPos = false;
			}
			else
			{
				UpdateGroundEntity( pm );
			}

			if ( bMoveToEndPos && !pm.StartedSolid && pm.Fraction > 0.0f && pm.Fraction < 1.0f )
			{
				ctrl.Position = pm.EndPos;
			}
		}

		private void UpdateGroundEntity( TraceResult tr )
		{
			ctrl.GroundNormal = tr.Normal;

			// VALVE HACKHACK: Scale this to fudge the relationship between vphysics friction values and player friction values.
			// A value of 0.8f feels pretty normal for vphysics, whereas 1.0f is normal for players.
			// This scaling trivially makes them equivalent.  REVISIT if this affects low friction surfaces too much.
			SurfaceFriction = tr.Surface.Friction * 1.25f;
			if ( SurfaceFriction > 1 ) SurfaceFriction = 1;

			ctrl.GroundEntity = tr.Entity;

			if ( ctrl.GroundEntity != null )
			{
				ctrl.Velocity = ctrl.Velocity.WithZ( 0 );
				ctrl.BaseVelocity = ctrl.GroundEntity.Velocity;
			}
		}

	}
}
