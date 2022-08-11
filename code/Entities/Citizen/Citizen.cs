using Sandbox;
using System.Linq;

namespace Facepunch.Parkour
{
    partial class Citizen : AnimatedEntity
	{

		private DamageInfo lastDamageInfo { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			var citizens = All.OfType<Citizen>();
			if ( citizens.Count() > 5 )
			{
				citizens.First()?.Delete();
			}

			SetModel( "models/citizen/citizen.vmdl" );
			SetupPhysicsFromCapsule( PhysicsMotionType.Dynamic, new Capsule( Vector3.Zero, Vector3.Up * 64, 15 ) );

			Health = 100;
			EnableAllCollisions = true;

			// not sure this is really helping yet
			Components.Add( new Ragdoller() );

		}

		public override void OnKilled()
		{
			base.OnKilled();

			BecomeRagdollOnClient( Velocity, lastDamageInfo.Flags, lastDamageInfo.Position, lastDamageInfo.Force );
		}

		public override void TakeDamage( DamageInfo info )
		{
			lastDamageInfo = info;

			base.TakeDamage( info );
		}

		[ClientRpc]
		public void BecomeRagdollOnClient( Vector3 velocity, DamageFlags damageFlags, Vector3 forcePos, Vector3 force )
		{
			Components.Get<Ragdoller>()?.Ragdoll( velocity, damageFlags, forcePos, force, 0 );
		}

	}
}
