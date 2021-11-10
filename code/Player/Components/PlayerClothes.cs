using Sandbox;

namespace Facepunch.Parkour
{
	class PlayerClothes : PlayerComponent
	{

		private Clothing.Container clothing;

		public override void OnSpawned()
		{
			if ( !Entity.IsServer )
				return;

			if ( !Entity.IsValid()
				|| !Entity.Client.IsValid() 
				|| Entity.Client.IsBot )
			{
				return;
			}

			if ( clothing == null )
			{
				clothing = new Clothing.Container();
				clothing.LoadFromClient( Entity.Client );
			}

			clothing.DressEntity( Entity );
		}

		public override void OnKilled()
		{
			if ( !Host.IsServer )
				return;

			clothing.ClearEntities();
		}

	}
}
