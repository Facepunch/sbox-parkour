using Sandbox;

namespace Facepunch.Parkour
{
    class PlayerComponent : EntityComponent<ParkourPlayer>
	{
		public virtual void OnKilled() { }
		public virtual void OnSpawned() { }
		public virtual void OnSimulate( Client cl ) { }
		public virtual void OnPostCameraSetup( ref CameraSetup setup ) { }
	}
}
