using Sandbox;

namespace Facepunch.Parkour
{
    class GameComponent : EntityComponent<ParkourGame>
	{

		public virtual void ClientJoined( Client cl ) { }
		public virtual void ClientPawnKilled( Client cl, Entity pawn ) { }

	}
}
