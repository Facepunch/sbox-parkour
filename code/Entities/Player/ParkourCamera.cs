using Sandbox;

namespace Facepunch.Parkour
{
    class ParkourCamera : CameraMode
	{

		private Vector3 _eyePosLocal;

		public override void Activated()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			_eyePosLocal = pawn.EyeLocalPosition;
			Position = pawn.Position + _eyePosLocal;
			Rotation = pawn.EyeRotation;

			ZNear = 3;
			FieldOfView = 100;
		}

		public override void Update()
		{
			if ( Local.Pawn is not ParkourPlayer pawn )
				return;

			_eyePosLocal = _eyePosLocal.LerpTo( pawn.EyeLocalPosition, 25f * Time.Delta );
			Position = pawn.Position + _eyePosLocal;
			Rotation = pawn.EyeRotation;

			Viewer = pawn;
		}

	}
}
