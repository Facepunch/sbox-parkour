using Sandbox;
using System.Linq;

namespace Facepunch.Parkour
{
    partial class Citizen : AnimEntity
	{

		public override void Spawn()
		{
			base.Spawn();

			var citizens = All.OfType<Citizen>();
			if ( citizens.Count() > 5 )
			{
				citizens.First()?.Delete();
			}

			SetModel( "models/citizen/citizen.vmdl" );
			Health = 100;

			// not sure this is really helping yet
			Components.Add( new Ragdoller() );
			Components.Add( new Dresser() ).DressFromRandom();
		}

		public override void OnKilled()
		{
			base.OnKilled();

			BecomeRagdollOnClient();
		}

		[ClientRpc]
		public void BecomeRagdollOnClient()
		{
			Components.Get<Ragdoller>()?.Ragdoll( Vector3.Zero, DamageFlags.Bullet, Vector3.Zero, Vector3.Zero, 0 );
		}

	}
}
