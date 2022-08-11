using Sandbox;

namespace Facepunch.Parkour
{
	partial class ParkourPlayer : Player
	{

		public override void Spawn()
		{
			base.Spawn();

			// not sure this is really helping yet
			Components.Add( new PlayerController() );
			Components.Add( new PlayerCameraEffects() );
			Components.Add( new Ragdoller() );
		}

		public override void Respawn()
		{
			RaisePlayerSpawned();
			RaisePlayerSpawnedOnClient();

			Tags.Add("player");

			base.Respawn();
		}

		public override void Simulate( Client cl )
		{
			GetActiveController()?.Simulate( cl, this, GetActiveAnimator() );

			foreach ( var component in Components.GetAll<PlayerComponent>() )
			{
				if ( component is PlayerComponent c )
					c.OnSimulate( cl );
			}
		}

		public override void OnKilled()
		{
			base.OnKilled();

			RaisePlayerKilled();
			RaisePlayerKilledOnClient();
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

		public override void PostCameraSetup( ref CameraSetup setup )
		{
			foreach ( var component in Components.GetAll<PlayerComponent>() )
			{
				component.OnPostCameraSetup( ref setup );
			}
		}

	}
}
