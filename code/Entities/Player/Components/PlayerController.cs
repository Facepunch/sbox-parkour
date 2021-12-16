using Sandbox;

namespace Facepunch.Parkour
{
	class PlayerController : PlayerComponent
	{

		public override void OnSpawned()
		{
			base.OnSpawned();

			if ( !Host.IsServer )
				return;

			Entity.SetModel( "models/citizen/citizen.vmdl" );

			Entity.Controller = new ParkourController();
			Entity.Animator = new StandardPlayerAnimator();
			Entity.Camera = new ParkourCamera();

			Entity.EnableAllCollisions = true;
			Entity.EnableDrawing = true;
			Entity.EnableHideInFirstPerson = true;
			Entity.EnableShadowInFirstPerson = true;
			Entity.Health = 100;

			Entity.Components.Get<Dresser>()?.DressFromAvatar();
		}

		public override void OnSimulate( Client cl )
		{
			base.OnSimulate( cl );


			// dev shit
			if ( !Entity.IsClient && Input.Pressed( InputButton.Attack1 ) )
			{
				var tr = Trace.Ray( Entity.EyePos, Entity.EyePos + Entity.EyeRot.Forward * 5000 )
					.WorldOnly()
					.Run();
				if ( tr.Hit )
				{
					var citizen = new Citizen();
					citizen.Position = tr.EndPos;
					citizen.Rotation = Rotation.LookAt( (Entity.Position - tr.EndPos).WithZ( 0 ) );
				}
			}
		}

		public override void OnKilled()
		{
			base.OnKilled();

			if ( Entity.IsServer )
			{
				Entity.Camera = new SpectateRagdollCamera();
				Entity.EnableDrawing = false;
				Entity.EnableAllCollisions = false;

				Entity.Components.Get<Dresser>()?.Clear(); 
			}

			if ( Entity.IsClient )
			{
				Entity.Components.Get<Ragdoller>()?.Ragdoll( Vector3.Zero, DamageFlags.Bullet, Vector3.Zero, Vector3.Zero, 0 );
			}
		}

	}
}
