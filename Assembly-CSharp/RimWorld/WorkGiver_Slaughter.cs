using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Slaughter : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.OnCell;
			}
		}

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			foreach (Designation item in pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.Slaughter))
			{
				yield return item.target.Thing;
			}
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = t as Pawn;
			if (pawn2 != null && pawn2.RaceProps.Animal)
			{
				if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Slaughter) == null)
				{
					return false;
				}
				if (pawn.Faction != t.Faction)
				{
					return false;
				}
				if (pawn2.InAggroMentalState)
				{
					return false;
				}
				if (!pawn.CanReserve(t, 1, -1, null, forced))
				{
					return false;
				}
				if (pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
				{
					JobFailReason.Is("IsIncapableOfViolenceShort".Translate());
					return false;
				}
				return true;
			}
			return false;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return new Job(JobDefOf.Slaughter, t);
		}
	}
}