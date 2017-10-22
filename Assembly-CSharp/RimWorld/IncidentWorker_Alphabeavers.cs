using System;
using Verse;

namespace RimWorld
{
	internal class IncidentWorker_Alphabeavers : IncidentWorker
	{
		public override bool TryExecute(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			PawnKindDef alphabeaver = PawnKindDefOf.Alphabeaver;
			IntVec3 intVec = default(IntVec3);
			if (!RCellFinder.TryFindRandomPawnEntryCell(out intVec, map, CellFinder.EdgeRoadChance_Animal, (Predicate<IntVec3>)null))
			{
				return false;
			}
			int num = Rand.RangeInclusive(6, 12);
			for (int num2 = 0; num2 < num; num2++)
			{
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(intVec, map, 10, null);
				Pawn newThing = PawnGenerator.GeneratePawn(alphabeaver, null);
				GenSpawn.Spawn(newThing, loc, map);
			}
			Find.LetterStack.ReceiveLetter("LetterLabelBeaversArrived".Translate(), "BeaversArrived".Translate(), LetterDefOf.BadNonUrgent, new TargetInfo(intVec, map, false), (string)null);
			return true;
		}
	}
}