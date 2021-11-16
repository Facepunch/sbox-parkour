using Sandbox;

namespace Facepunch.Parkour
{
    class ParkourCamera : Camera
	{

		public override void Activated()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			Position = pawn.EyePos;
			Rotation = pawn.EyeRot;

			ZNear = 3;
			FieldOfView = 100;
		}

		public override void Update()
		{
			if ( Local.Pawn is not ParkourPlayer pawn )
				return;

			Position = pawn.EyePos;
			Rotation = pawn.EyeRot;

			Viewer = pawn;
		}

	}
}
