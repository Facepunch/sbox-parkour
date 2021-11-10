using Sandbox;

namespace Facepunch.Parkour
{
	public partial class ParkourGame : Sandbox.Game
	{
		public ParkourGame()
		{
			if ( IsClient )
			{
				new ParkourHud();
			}
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

		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var player = new ParkourPlayer();
			client.Pawn = player;

			player.Respawn();
		}
	}

}
