using System;
using System.Collections.Generic;
using System.Text;

namespace KSynthesizer
{
	// From https://github.com/xamarin/Xamarin.Forms/blob/master/Xamarin.Forms.Core/Easing.cs
	public class Easing
	{
		public static readonly Easing Linear = new Easing(x => x);

		public static readonly Easing SinOut = new Easing(x => (float)Math.Sin(x * Math.PI * 0.5f));
		public static readonly Easing SinIn = new Easing(x => 1.0f - (float)Math.Cos(x * Math.PI * 0.5f));
		public static readonly Easing SinInOut = new Easing(x => -(float)Math.Cos(Math.PI * x) / 2.0f + 0.5f);

		public static readonly Easing CubicIn = new Easing(x => x * x * x);
		public static readonly Easing CubicOut = new Easing(x => (float)Math.Pow(x - 1.0f, 3.0f) + 1.0f);

		public static readonly Easing CubicInOut = new Easing(x => x < 0.5f ? (float)Math.Pow(x * 2.0f, 3.0f) / 2.0f : (float)(Math.Pow((x - 1) * 2.0f, 3.0f) + 2.0f) / 2.0f);

		public static readonly Easing BounceOut;
		public static readonly Easing BounceIn;

		public static readonly Easing SpringIn = new Easing(x => x * x * ((1.70158f + 1) * x - 1.70158f));
		public static readonly Easing SpringOut = new Easing(x => (x - 1) * (x - 1) * ((1.70158f + 1) * (x - 1) + 1.70158f) + 1);

		readonly Func<float, float> _easingFunc;

		static Easing()
		{
			BounceOut = new Easing(p =>
			{
				if (p < 1 / 2.75f)
				{
					return 7.5625f * p * p;
				}
				if (p < 2 / 2.75f)
				{
					p -= 1.5f / 2.75f;

					return 7.5625f * p * p + .75f;
				}
				if (p < 2.5f / 2.75f)
				{
					p -= 2.25f / 2.75f;

					return 7.5625f * p * p + .9375f;
				}
				p -= 2.625f / 2.75f;

				return 7.5625f * p * p + .984375f;
			});

			BounceIn = new Easing(p => 1.0f - BounceOut.Ease(1 - p));
		}

		public Easing(Func<float, float> easingFunc)
		{
			if (easingFunc == null)
				throw new ArgumentNullException("easingFunc");

			_easingFunc = easingFunc;
		}

		public float Ease(float v)
		{
			return _easingFunc(v);
		}

		public static implicit operator Easing(Func<float, float> func)
		{
			return new Easing(func);
		}
	}
}
