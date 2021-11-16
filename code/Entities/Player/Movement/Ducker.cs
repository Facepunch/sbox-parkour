using Sandbox;
using System.Diagnostics;

namespace Facepunch.Parkour
{
	class Ducker : BaseMoveMechanic
	{

		private Vector3 originalMins;
		private Vector3 originalMaxs;
		private float tuckDistance = 36f;

		public override float EyePosMultiplier => .5f;
		public float DuckSpeed => 110f;
		public float MaxDuckSpeed => 140f;

		public Ducker( ParkourController ctrl )
			: base( ctrl )
		{

		}

		protected override bool TryActivate()
		{
			if ( !Input.Down( InputButton.Duck ) ) return false;
			//let slide activate if we too fast
			if ( ctrl.GroundEntity != null && ctrl.Velocity.WithZ( 0 ).Length > MaxDuckSpeed ) return false;

			if ( ctrl.GroundEntity == null )
			{
				//ctrl.Position += new Vector3( 0, 0, tuckDistance );
			}

			return true;
		}

		public override void UpdateBBox( ref Vector3 mins, ref Vector3 maxs, float scale = 1f )
		{
			originalMins = mins;
			originalMaxs = maxs;

			maxs = maxs.WithZ( 36 * scale );
		}

		public override void Simulate()
		{
			ctrl.SetTag( "ducked" );

			if ( ctrl.GroundEntity != null && ctrl.Velocity.WithZ( 0 ).Length > MaxDuckSpeed )
			{
				IsActive = false;
				return;
			}

			if ( Input.Down( InputButton.Duck ) ) return;

			if ( ctrl.GroundEntity == null )
			{
				var untuckDist = tuckDistance;
				var groundTrace = ctrl.TraceBBox( ctrl.Position, ctrl.Position + Vector3.Down * tuckDistance, originalMins, originalMaxs );

				if ( groundTrace.Hit )
				{
					untuckDist = groundTrace.Distance;
				}

				if ( ctrl.GroundEntity == null )
				{
					//ctrl.Position -= new Vector3( 0, 0, untuckDist );
				}
			}

			var pm = ctrl.TraceBBox( ctrl.Position, ctrl.Position, originalMins, originalMaxs );
			if ( pm.StartedSolid ) return;

			IsActive = false;
		}

		public override float GetWishSpeed()
		{
			return DuckSpeed;
		}

	}
}
