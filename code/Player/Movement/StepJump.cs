using Sandbox;

namespace Facepunch.Parkour
{
	class StepJump : BaseMoveMechanic
	{

		public override bool TakesOverControl => true;

		public StepJump( ParkourController controller )
			: base( controller )
		{

		}

		protected override bool TryActivate()
		{
			if ( !Input.Down( InputButton.Run ) ) return false;

			var wall = GetWallInfo( ctrl.Velocity.WithZ( 0 ).Normal );
			if ( wall == null ) return false;

			var wallr = wall.Value;
			if ( wallr.Height < 20 || wallr.Height > 50 ) return false;
			if ( wallr.Distance > 50 ) return false;

			return true;
		}

		public override void Simulate()
		{
			if ( TimeSinceActivate > .3f )
			{
				IsActive = false;
				return;
			}


		}

	}
}
