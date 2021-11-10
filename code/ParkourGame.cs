using Sandbox;

namespace Facepunch.Parkour
{
	public partial class ParkourGame : Sandbox.Game
	{
		public ParkourGame()
		{
			if ( IsServer )
			{
				Components.Add( new PlayerSpawner() );
			}

			if ( IsClient )
			{
				new ParkourHud();
			}
		}

		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			RaiseClientJoined( client );
			RaiseClientJoinedOnClient( client );
		}

		public override void OnKilled( Client client, Entity pawn )
		{
			base.OnKilled( client, pawn );

			foreach ( var component in Components.GetAll<GameComponent>() )
			{
				component.ClientPawnKilled( client, pawn );
			}
		}

		private void RaiseClientJoined( Client client )
		{
			foreach ( var component in Components.GetAll<GameComponent>() )
			{
				component.ClientJoined( client );
			}
		}

		[ClientRpc]
		private void RaiseClientJoinedOnClient( Client client )
		{
			RaiseClientJoined( client );
		}

		public override void DoPlayerSuicide( Client cl )
		{
			base.DoPlayerSuicide( cl );

			if ( cl.Pawn is not Player pl )
			{
				cl.Pawn?.Delete();
				pl = new ParkourPlayer();
				cl.Pawn = pl;
			}

			pl.Respawn();
		}

	}

}
