using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_DropUnusedInventory : ThinkNode_JobGiver
	{
		private const int RawFoodDropDelay = 150000;

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.inventory == null)
			{
				return null;
			}
			if (!((Area)pawn.Map.areaManager.Home)[pawn.Position])
			{
				return null;
			}
			if (pawn.Faction != Faction.OfPlayer)
			{
				return null;
			}
			if (Find.TickManager.TicksGame > pawn.mindState.lastInventoryRawFoodUseTick + 150000)
			{
				for (int num = pawn.inventory.innerContainer.Count - 1; num >= 0; num--)
				{
					Thing thing = pawn.inventory.innerContainer[num];
					if (thing.def.IsIngestible && !thing.def.IsDrug && (int)thing.def.ingestible.preferability <= 4)
					{
						this.Drop(pawn, thing);
					}
				}
			}
			for (int num2 = pawn.inventory.innerContainer.Count - 1; num2 >= 0; num2--)
			{
				Thing thing2 = pawn.inventory.innerContainer[num2];
				if (thing2.def.IsDrug && pawn.drugs != null && !pawn.drugs.AllowedToTakeScheduledEver(thing2.def) && pawn.drugs.HasEverTaken(thing2.def) && !AddictionUtility.IsAddicted(pawn, thing2))
				{
					this.Drop(pawn, thing2);
				}
			}
			return null;
		}

		private void Drop(Pawn pawn, Thing thing)
		{
			Thing thing2 = default(Thing);
			pawn.inventory.innerContainer.TryDrop(thing, pawn.Position, pawn.Map, ThingPlaceMode.Near, out thing2, (Action<Thing, int>)null);
		}
	}
}