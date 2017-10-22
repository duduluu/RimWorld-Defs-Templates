namespace RimWorld
{
	public class CompProperties_Targetable : CompProperties_UseEffect
	{
		public bool psychicSensitiveTargetsOnly;

		public CompProperties_Targetable()
		{
			base.compClass = typeof(CompTargetable);
		}
	}
}