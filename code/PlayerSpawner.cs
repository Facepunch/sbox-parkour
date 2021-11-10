using Sandbox;
using System.Threading.Tasks;

namespace Facepunch.Parkour
{
    class PlayerSpawner : GameComponent
	{

		public override void ClientJoined( Client cl )
		{
			if ( !Host.IsServer )
				return;

			var player = new ParkourPlayer();
			cl.Pawn = player;

			player.Respawn();
		}

		public override async void ClientPawnKilled( Client cl, Entity pawn )
		{
			if ( !Host.IsServer )
				return;

			await Task.Delay( 1000 );

			if ( pawn is not Player pl || !pl.IsValid() )
				return;

			pl.Respawn();
		}

	}
}
