using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_FixBrokenDownBuilding : JobDriver
	{
		private const TargetIndex BuildingInd = TargetIndex.A;

		private const TargetIndex ComponentInd = TargetIndex.B;

		private const int TicksDuration = 1000;

		private Building Building
		{
			get
			{
				return (Building)base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		private Thing Components
		{
			get
			{
				return base.CurJob.GetTarget(TargetIndex.B).Thing;
			}
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			yield return Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, false);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
			Toil repair = Toils_General.Wait(1000);
			repair.FailOnDespawnedOrNull(TargetIndex.A);
			repair.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			repair.WithEffect(this.Building.def.repairEffect, TargetIndex.A);
			repair.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			yield return repair;
			yield return new Toil
			{
				initAction = (Action)delegate
				{
					((_003CMakeNewToils_003Ec__Iterator10)/*Error near IL_0163: stateMachine*/)._003C_003Ef__this.Components.Destroy(DestroyMode.Vanish);
					if (Rand.Value > ((_003CMakeNewToils_003Ec__Iterator10)/*Error near IL_0163: stateMachine*/)._003C_003Ef__this.pawn.GetStatValue(StatDefOf.FixBrokenDownBuildingSuccessChance, true))
					{
						Vector3 loc = (((_003CMakeNewToils_003Ec__Iterator10)/*Error near IL_0163: stateMachine*/)._003C_003Ef__this.pawn.DrawPos + ((_003CMakeNewToils_003Ec__Iterator10)/*Error near IL_0163: stateMachine*/)._003C_003Ef__this.Building.DrawPos) / 2f;
						MoteMaker.ThrowText(loc, ((_003CMakeNewToils_003Ec__Iterator10)/*Error near IL_0163: stateMachine*/)._003C_003Ef__this.Map, "TextMote_FixBrokenDownBuildingFail".Translate(), 3.65f);
					}
					else
					{
						((_003CMakeNewToils_003Ec__Iterator10)/*Error near IL_0163: stateMachine*/)._003C_003Ef__this.Building.GetComp<CompBreakdownable>().Notify_Repaired();
					}
				}
			};
		}
	}
}