using Sandbox;

namespace Facepunch.Parkour
{
    class ParkourCamera : Camera
	{

		private Vector3 _lastPos;

		public override void Activated()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			Position = pawn.EyePos;
			Rotation = pawn.EyeRot;

			_lastPos = Position;

			ZNear = 3;
			FieldOfView = 100;
		}

		public override void Update()
		{
			if ( Local.Pawn is not ParkourPlayer pawn )
				return;

			var eyePos = pawn.EyePos;

			Position = eyePos.WithZ( _lastPos.z.LerpTo( eyePos.z, 50f * Time.Delta ) );
			Rotation = pawn.EyeRot;

			Viewer = pawn;

			_lastPos = Position;
		}

	}
}
