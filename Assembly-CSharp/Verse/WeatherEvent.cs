using System;
using UnityEngine;

namespace Verse
{
	public abstract class WeatherEvent
	{
		protected Map map;

		public abstract bool Expired
		{
			get;
		}

		public bool CurrentlyOverridesSky
		{
			get
			{
				return this.SkyTargetLerpFactor >= 0.0;
			}
		}

		public virtual SkyTarget SkyTarget
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public virtual float SkyTargetLerpFactor
		{
			get
			{
				return -1f;
			}
		}

		public virtual Vector2? OverrideShadowVector
		{
			get
			{
				return default(Vector2?);
			}
		}

		public WeatherEvent(Map map)
		{
			this.map = map;
		}

		public abstract void FireEvent();

		public abstract void WeatherEventTick();

		public virtual void WeatherEventDraw()
		{
		}
	}
}