using Sandbox;
using System;

namespace Facepunch.Parkour
{
    class PlayerCameraEffects : PlayerComponent
	{

		float walkBob = 0;
		float lean = 0;
		float fov = 0;

		public override void OnPostCameraSetup( ref CameraSetup setup ) 
		{
			var controller = Entity.Controller as ParkourController;

			var ducker = controller.GetMechanic<Ducker>();
			var slider = controller.GetMechanic<Slide>();
			var wishSpd = controller.GetWishSpeed();
			var bobSpeed = ducker != null && ducker.IsActive ? 10f : 25f;
			if ( slider != null && slider.IsActive ) bobSpeed = 2;

			var bobSpeedAlpha = Entity.Velocity.Length.LerpInverse( 0, wishSpd );
			var forwardspeed = Entity.Velocity.Normal.Dot( setup.Rotation.Forward );

			var left = setup.Rotation.Left;
			var up = setup.Rotation.Up;

			var wallrun = controller.GetMechanic<WallRun>();

			if ( Entity.GroundEntity != null || wallrun.IsActive )
			{
				walkBob += Time.Delta * bobSpeed * bobSpeedAlpha;
			}

			setup.Position += up * MathF.Sin( walkBob ) * bobSpeedAlpha * 3;
			setup.Position += left * MathF.Sin( walkBob * 0.6f ) * bobSpeedAlpha * 2;

			var targetLean = wallrun.IsActive
				? wallrun.Wall.Normal.Dot( setup.Rotation.Right ) * 12f
				: Entity.Velocity.Dot( setup.Rotation.Right ) * .03f;

			// Camera lean
			lean = lean.LerpTo( targetLean, Time.Delta * 15.0f );

			var appliedLean = lean;
			appliedLean += MathF.Sin( walkBob ) * bobSpeedAlpha * 0.2f;
			setup.Rotation *= Rotation.From( 0, 0, appliedLean );

			bobSpeedAlpha = (bobSpeedAlpha - 0.7f).Clamp( 0, 1 ) * 3.0f;

			fov = fov.LerpTo( bobSpeedAlpha * 10 * MathF.Abs( forwardspeed ), Time.Delta * 2.0f );

			setup.FieldOfView += fov;
		}

	}
}
