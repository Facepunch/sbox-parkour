using Sandbox;

namespace Facepunch.Parkour
{
    class ParkourCamera : Camera
	{

		private Vector3 _eyePosLocal;

		public override void Activated()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			_eyePosLocal = pawn.EyePosLocal;
			Position = pawn.Position + _eyePosLocal;
			Rotation = pawn.EyeRot;

			ZNear = 3;
			FieldOfView = 100;
		}

		public override void Update()
		{
			if ( Local.Pawn is not ParkourPlayer pawn )
				return;

			_eyePosLocal = _eyePosLocal.LerpTo( pawn.EyePosLocal, 25f * Time.Delta );
			Position = pawn.Position + _eyePosLocal;
			Rotation = pawn.EyeRot;

			Viewer = pawn;
		}

	}
}
