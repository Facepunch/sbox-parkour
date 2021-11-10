using Sandbox;

namespace Facepunch.Parkour
{
	partial class ParkourPlayer : Player
	{

		public override void Spawn()
		{
			base.Spawn();

			Components.Add( new PlayerController() );
			Components.Add( new PlayerClothes() );
			Components.Add( new PlayerCameraEffects() );
		}

		public override void Respawn()
		{
			RaisePlayerSpawned();
			RaisePlayerSpawnedOnClient();

			base.Respawn();
		}

		public override void Simulate( Client cl )
		{
			GetActiveController()?.Simulate( cl, this, GetActiveAnimator() );

			foreach ( var component in Components.GetAll<PlayerComponent>() )
			{
				component.OnSimulate( cl );
			}
		}

		public override void OnKilled()
		{
			base.OnKilled();

			RaisePlayerKilled();
			RaisePlayerKilledOnClient();
		}

		public override void PostCameraSetup( ref CameraSetup setup )
		{
			base.PostCameraSetup( ref setup );

			foreach ( var component in Components.GetAll<PlayerComponent>() )
			{
				component.OnPostCameraSetup( ref setup );
			}
		}

		private void RaisePlayerKilled()
		{
			foreach ( var component in Components.GetAll<PlayerComponent>() )
			{
				component.OnKilled();
			}
		}

		private void RaisePlayerSpawned()
		{
			foreach ( var component in Components.GetAll<PlayerComponent>() )
			{
				component.OnSpawned();
			}
		}

		[ClientRpc]
		private void RaisePlayerKilledOnClient()
		{
			RaisePlayerKilled();
		}

		[ClientRpc]
		private void RaisePlayerSpawnedOnClient()
		{
			RaisePlayerSpawned();
		}

	}
}
