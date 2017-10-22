using System;
using System.Collections.Generic;

namespace RimWorld
{
	public class StorytellerCompProperties
	{
		public Type compClass;

		public float minDaysPassed;

		public IncidentTargetType allowedTargetTypes = IncidentTargetType.All;

		public float minIncChancePopulationIntentFactor = 0.05f;

		public StorytellerCompProperties()
		{
		}

		public StorytellerCompProperties(Type compClass)
		{
			this.compClass = compClass;
		}

		public virtual IEnumerable<string> ConfigErrors(StorytellerDef parentDef)
		{
			if (this.compClass == null)
			{
				yield return parentDef.defName + " has StorytellerCompProperties with null compClass.";
			}
		}

		public virtual void ResolveReferences(StorytellerDef parentDef)
		{
		}
	}
}