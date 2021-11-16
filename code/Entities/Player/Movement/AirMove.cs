﻿using Sandbox;
using Sandbox.ScreenShake;
using System;

namespace Facepunch.Parkour
{
	class AirMove : BaseMoveMechanic
	{

		public float Gravity => 800.0f;
		public float AirControl => 30.0f;
		public float AirAcceleration => 35.0f;

		public override bool AlwaysSimulate => true;

		private Vector3 velocityAtStart;
		private bool groundedAtStart;

		public AirMove( ParkourController controller )
			: base( controller )
		{

		}

		public override void Simulate()
		{
			if ( ctrl.GroundEntity != null ) return;

			var wishVel = ctrl.GetWishVelocity( true );
			var wishdir = wishVel.Normal;
			var wishspeed = wishVel.Length;

			ctrl.Accelerate( wishdir, wishspeed, AirControl, AirAcceleration );
			ctrl.Velocity += ctrl.BaseVelocity;
			ctrl.Move();
			ctrl.Velocity -= ctrl.BaseVelocity;
		}

		public override void PreSimulate()
		{
			ctrl.Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
			ctrl.Velocity += new Vector3( 0, 0, ctrl.BaseVelocity.z ) * Time.Delta;

			ctrl.BaseVelocity = ctrl.BaseVelocity.WithZ( 0 );

			velocityAtStart = ctrl.Velocity;
			groundedAtStart = ctrl.GroundEntity != null;
		}

		public override void PostSimulate()
		{
			ctrl.Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

			if ( ctrl.GroundEntity != null && !groundedAtStart )
				DoFallDamage();
		}

		private void DoFallDamage()
		{
			var fallSpeed = Math.Abs( velocityAtStart.z );

			if( ctrl.Pawn.IsClient )
			{
				new FallCameraModifier( fallSpeed );
			}

			if ( fallSpeed < 500 )
				return;

			//var walk = ctrl.GetMechanic<Walk>();

			//var willSlide = Input.Down( InputButton.Duck ) && ctrl.Velocity.WithZ( 0 ).Length > duck.SlideThreshold;
			var fallSpeedMaxLoss = 3000;
			var a = 1f - MathF.Min( fallSpeed / fallSpeedMaxLoss, 1 );

			ctrl.Velocity = ctrl.Velocity.ClampLength( ctrl.Velocity.Length * a );
			//walk.Momentum *= a;
		}

	}
}
