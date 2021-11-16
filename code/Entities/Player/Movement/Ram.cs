using Sandbox;
using Sandbox.ScreenShake;

namespace Facepunch.Parkour
{
	class Ram : BaseMoveMechanic
	{

		public override bool AlwaysSimulate => true;

		public Ram( ParkourController ctrl )
			: base( ctrl )
		{

		}

		public override void PostSimulate()
		{
			if ( ctrl.Velocity.WithZ( 0 ).Length < 250 ) return;

			var trStart = ctrl.Position + Vector3.Up;
			var trEnd = ctrl.Position + ctrl.Velocity.Normal * ctrl.BodyGirth;
			var tr = Trace.Ray( trStart, trEnd )
				.EntitiesOnly()
				.Ignore( ctrl.Pawn )
				.Run();

			if ( !tr.Hit ) return;
			if ( tr.Entity is not Citizen citizen ) return;

			var lossVector = (citizen.Position - ctrl.Position).Normal;
			lossVector *= 100;
			ctrl.Velocity -= lossVector;

			if ( !ctrl.Pawn.IsServer )
			{
				new Perlin( 2, 1, 5 );
				return;
			}

			citizen.Velocity = ctrl.Velocity.Normal * 500;
			citizen.TakeDamage( new DamageInfo()
			{
				Damage = 100,
				Attacker = ctrl.Pawn,
				Flags = DamageFlags.Crush
			} );
		}

	}
}
