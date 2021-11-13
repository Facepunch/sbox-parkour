﻿using Sandbox;
using System;

namespace Facepunch.Parkour
{
	class PlayerController : PlayerComponent
	{

		private Particles speedLines;

		public override void OnSpawned()
		{
			base.OnSpawned();

			if ( Host.IsClient )
			{
				speedLines = Particles.Create( "particles/player/speed_lines.vpcf", Entity, "hat" );
			}

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
			Entity.LagCompensation = false;
		}

		public override void OnSimulate( Client cl )
		{
			base.OnSimulate( cl );

			if ( Entity.IsClient )
			{
				var controller = Entity.Controller as ParkourController;
				var scale = Entity.Velocity.WithZ( 0 ).Length / controller.GetMechanic<Walk>().DefaultSpeed;
				speedLines?.SetPosition( 1, new Vector3( scale * 22f, 0, 0 ) );
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
			}

			if ( Entity.IsClient )
			{
				BecomeRagdoll( Vector3.Zero, DamageFlags.Bullet, Vector3.Zero, Vector3.Zero, 0 );
			}
		}

		public void BecomeRagdoll( Vector3 velocity, DamageFlags damageFlags, Vector3 forcePos, Vector3 force, int bone )
		{
			var ent = new ModelEntity();
			ent.Position = Entity.Position;
			ent.Rotation = Entity.Rotation;
			ent.Scale = Entity.Scale;
			ent.MoveType = MoveType.Physics;
			ent.UsePhysicsCollision = true;
			ent.EnableAllCollisions = true;
			ent.CollisionGroup = CollisionGroup.Debris;
			ent.SetModel( Entity.GetModelName() );
			ent.CopyBonesFrom( Entity );
			ent.CopyBodyGroups( Entity );
			ent.CopyMaterialGroup( Entity );
			ent.TakeDecalsFrom( Entity );
			ent.EnableHitboxes = true;
			ent.EnableAllCollisions = true;
			ent.SurroundingBoundsMode = SurroundingBoundsType.Physics;
			ent.RenderColor = Entity.RenderColor;
			ent.PhysicsGroup.Velocity = velocity;

			if ( Local.Pawn == Entity )
			{
				//ent.EnableDrawing = false; wtf
			}

			ent.SetInteractsAs( CollisionLayer.Debris );
			ent.SetInteractsWith( CollisionLayer.WORLD_GEOMETRY );
			ent.SetInteractsExclude( CollisionLayer.Player | CollisionLayer.Debris );

			foreach ( var child in Entity.Children )
			{
				if ( !child.Tags.Has( "clothes" ) ) continue;
				if ( child is not ModelEntity e ) continue;

				var model = e.GetModelName();

				var clothing = new ModelEntity();
				clothing.SetModel( model );
				clothing.SetParent( ent, true );
				clothing.RenderColor = e.RenderColor;
				clothing.CopyBodyGroups( e );
				clothing.CopyMaterialGroup( e );
			}

			if ( damageFlags.HasFlag( DamageFlags.Bullet ) ||
				 damageFlags.HasFlag( DamageFlags.PhysicsImpact ) )
			{
				PhysicsBody body = bone > 0 ? ent.GetBonePhysicsBody( bone ) : null;

				if ( body != null )
				{
					body.ApplyImpulseAt( forcePos, force * body.Mass );
				}
				else
				{
					ent.PhysicsGroup.ApplyImpulse( force );
				}
			}

			if ( damageFlags.HasFlag( DamageFlags.Blast ) )
			{
				if ( ent.PhysicsGroup != null )
				{
					ent.PhysicsGroup.AddVelocity( (Entity.Position - (forcePos + Vector3.Down * 100.0f)).Normal * (force.Length * 0.2f) );
					var angularDir = (Rotation.FromYaw( 90 ) * force.WithZ( 0 ).Normal).Normal;
					ent.PhysicsGroup.AddAngularVelocity( angularDir * (force.Length * 0.02f) );
				}
			}

			Entity.Corpse = ent;

			ent.DeleteAsync( 10.0f );
		}

	}
}
