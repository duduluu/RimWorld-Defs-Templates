using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_SeekAllowedArea : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!pawn.Position.IsForbidden(pawn))
			{
				return null;
			}
			Region region = pawn.GetRegion(RegionType.Set_Passable);
			if (region == null)
			{
				return null;
			}
			TraverseParms traverseParms = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
			RegionEntryPredicate entryCondition = (RegionEntryPredicate)((Region from, Region r) => r.Allows(traverseParms, false));
			Region reg = null;
			RegionProcessor regionProcessor = (RegionProcessor)delegate(Region r)
			{
				if (r.portal != null)
				{
					return false;
				}
				if (!r.IsForbiddenEntirely(pawn))
				{
					reg = r;
					return true;
				}
				return false;
			};
			RegionTraverser.BreadthFirstTraverse(region, entryCondition, regionProcessor, 9999, RegionType.Set_Passable);
			if (reg != null)
			{
				IntVec3 c = default(IntVec3);
				if (!reg.TryFindRandomCellInRegionUnforbidden(pawn, (Predicate<IntVec3>)null, out c))
				{
					return null;
				}
				return new Job(JobDefOf.Goto, c);
			}
			return null;
		}
	}
}