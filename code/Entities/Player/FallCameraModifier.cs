using Sandbox;
using System;

namespace Facepunch.Parkour
{
	class FallCameraModifier : CameraModifier
	{

		private float fallSpeed;
		private float pos = 0;
		private float length;
		private float t;

		private const float effectMaxSpeed = 1500f;
		private const float effectStrength = 500f;

		public FallCameraModifier( float fallSpeed, float length = .5f )
		{
			this.length = length;
			this.fallSpeed = fallSpeed;
		}

		public override bool Update( ref CameraSetup setup )
		{
			var delta = ((float)t).LerpInverse( 0, length, true );
			delta = Easing.EaseOut( delta );
			var invdelta = 1 - delta;

			pos += Time.Delta * invdelta;

			var a = Math.Min( Math.Abs(fallSpeed) / effectMaxSpeed, 1f );
			if ( fallSpeed < 0f ) a *= -1f;

			setup.Rotation *= Rotation.FromAxis( Vector3.Left, effectStrength * invdelta * pos * a );

			t += Time.Delta;

			return t < length;
		}

	}
}
