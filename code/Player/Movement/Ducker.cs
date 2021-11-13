using Sandbox;

namespace Facepunch.Parkour
{
	class Ducker : BaseMoveMechanic
	{

		private Vector3 originalMins;
		private Vector3 originalMaxs;
		private float tuckDistance = 36f;

		public float SlideThreshold => 130f;
		public float DuckSpeed => 110f;
		public float SlideBoost => 75f;
		public TimeSince TimeSinceSlide { get; set; }
		public bool Sliding { get; private set; }

		public Ducker( ParkourController ctrl )
			: base( ctrl )
		{

		}

		protected override bool TryActivate()
		{
			if ( Input.Down( InputButton.Duck ) ) 
				return true;
			return false;
		}

		public override void UpdateBBox( ref Vector3 mins, ref Vector3 maxs, float scale = 1f )
		{
			originalMins = mins;
			originalMaxs = maxs;

			if ( IsActive )
			{
				maxs = maxs.WithZ( (Sliding ? 20 : 36) * scale );
			}
		}

		public override void PreSimulate()
		{
			bool wants = Input.Down( InputButton.Duck );

			if ( wants != IsActive )
			{
				if ( wants ) TryDuck();
				else TryUnDuck();
			}

			if ( IsActive )
			{
				var wasSliding = Sliding;
				Sliding = ctrl.GroundEntity != null && ctrl.Velocity.Length > SlideThreshold;
				ctrl.SetTag( Sliding ? "sitting" : "ducked" );
				ctrl.EyePosLocal *= Sliding ? .35f : .5f;

				if ( Sliding && !wasSliding )
				{
					TimeSinceSlide = 0;

					var len = ctrl.Velocity.WithZ( 0 ).Length;
					var newLen = len + SlideBoost;
					ctrl.Velocity *= newLen / len;
				}
			}
		}

		private void TryDuck()
		{
			IsActive = true;

			if ( ctrl.GroundEntity == null )
			{
				ctrl.Position += new Vector3( 0, 0, tuckDistance );
			}
		}

		private void TryUnDuck()
		{
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
					ctrl.Position -= new Vector3( 0, 0, untuckDist );
				}
			}

			var pm = ctrl.TraceBBox( ctrl.Position, ctrl.Position, originalMins, originalMaxs );
			if ( pm.StartedSolid ) return;

			Sliding = false;
			IsActive = false;
		}

		public override float GetWishSpeed()
		{
			return DuckSpeed;
		}

	}
}
