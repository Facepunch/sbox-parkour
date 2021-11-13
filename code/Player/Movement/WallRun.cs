using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Parkour
{
	class WallRun : BaseMoveMechanic
	{

		public float WallRunTime => 3f;
		public float WallRunMinHeight => 90f;
		public float WallJumpPower => 268f;
		public float MinWallHeight => 90;

		public override bool TakesOverControl => true;
		public WallInfo Wall { get; private set; }

		private TimeSince timeSinceWallRun;

		public WallRun( ParkourController ctrl )
			: base( ctrl )
		{

		}

		protected override bool TryActivate()
		{
			if ( !Input.Down( InputButton.Jump ) ) return false;
			if ( ctrl.GroundEntity != null ) return false;
			if ( ctrl.Velocity.z > 100 ) return false;
			if ( ctrl.Velocity.z < -150 ) return false;

			var wall = FindRunnableWall();
			if ( wall == null ) return false;

			Wall = wall.Value;
			ctrl.Velocity = ctrl.Velocity.WithZ( 0 );
			timeSinceWallRun = 0;

			return true;
		}

		public override void Simulate()
		{
			base.Simulate();

			if ( !StillWallRunning() )
			{
				IsActive = false;

				if ( ctrl.GroundEntity != null )
					ctrl.Velocity = ctrl.Velocity.WithZ( 0 );
				return;
			}

			if( Input.Pressed(InputButton.Jump) )
			{
				JumpOffWall();
				IsActive = false;
				return;
			}

			var wishVel = ctrl.GetWishVelocity( true );
			var gravity = timeSinceWallRun / WallRunTime * 150f;
			var lookingAtWall = Vector3.Dot( Wall.Normal, wishVel.Normal ) < -.5f;

			if ( lookingAtWall && Input.Forward > 0 && timeSinceWallRun < 1f )
			{
				ctrl.Velocity = new Vector3( 0, 0, 200 );
			}
			else
			{
				ctrl.Velocity = ctrl.Velocity.WithZ( -gravity );
			}

			var dest = ctrl.Position + ctrl.Velocity * Time.Delta;
			var pm = ctrl.TraceBBox( ctrl.Position, dest );

			if ( pm.Fraction == 1 )
			{
				ctrl.Position = pm.EndPos;
				return;
			}

			ctrl.Move();
		}

		private void JumpOffWall()
		{
			var jumpDir = new Vector3( Input.Forward, Input.Left, 0 ).Normal;
			jumpDir *= Rotation.FromYaw( Input.Rotation.Yaw() );

			if ( Vector3.Dot( Wall.Normal, jumpDir ) <= -.3f )
				jumpDir = Wall.Normal;

			var jumpVelocity = (jumpDir + Vector3.Up) * WallJumpPower;
			jumpVelocity += jumpDir * ctrl.Velocity.WithZ( 0 ).Length / 2f;

			//_jumpReleased = false;

			ctrl.Velocity = jumpVelocity;
			Wall = default;
		}

		private bool StillWallRunning()
		{
			if ( ctrl.GroundEntity != null )
				return false;

			if ( ctrl.WishVelocity.WithZ( 0 ).Length.AlmostEqual( 0f ) )
				return false;

			if ( ctrl.Velocity.Length < 1.0f && timeSinceWallRun > .5f )
				return false;

			var trStart = ctrl.Position + Wall.Normal;
			var trEnd = ctrl.Position - Wall.Normal * ctrl.BodyGirth * 2;
			var tr = ctrl.TraceBBox( trStart, trEnd );

			if ( !tr.Hit || tr.Normal != Wall.Normal )
				return false;

			return true;
		}

		private WallInfo? FindRunnableWall()
		{
			var testDirections = new List<Vector3>()
			{
				ctrl.Rotation.Forward,
				ctrl.Rotation.Right,
				ctrl.Rotation.Left,
				ctrl.Rotation.Backward
			};

			WallInfo? targetWall = null;

			foreach ( var dir in testDirections )
			{
				targetWall = GetWallInfo( dir );

				if ( targetWall == null ) continue;
				if ( targetWall.Value.Height < MinWallHeight ) continue;

				break;
			}

			if ( targetWall == null ) return null;
			if ( !targetWall.Value.Normal.z.AlmostEqual( 0, .1f ) ) return null;

			return targetWall;
		}

	}
}
