using Sandbox;
using System;

namespace Facepunch.Parkour
{
	class FallCameraModifier : CameraModifier
	{

		private float fallSpeed;
		private TimeSince lifetime = 0;
		private float pos = 0;

		private const float effectMaxSpeed = 1500f;
		private const float effectStrength = 500f;

		public FallCameraModifier( float fallSpeed )
		{
			this.fallSpeed = fallSpeed;
		}

		public override bool Update( ref CameraSetup setup )
		{
			var delta = ((float)lifetime).LerpInverse( 0, .5f, true );
			delta = Easing.EaseOut( delta );
			var invdelta = 1 - delta;

			pos += Time.Delta * invdelta;

			var a = Math.Min( Math.Abs(fallSpeed) / effectMaxSpeed, 1f );
			if ( fallSpeed < 0f ) a *= -1f;

			setup.Rotation *= Rotation.FromAxis( Vector3.Left, effectStrength * invdelta * pos * a );

			return lifetime < .5f;
		}

	}
}
