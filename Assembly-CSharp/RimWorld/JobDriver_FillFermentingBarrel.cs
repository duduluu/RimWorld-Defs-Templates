using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_FillFermentingBarrel : JobDriver
	{
		private const TargetIndex BarrelInd = TargetIndex.A;

		private const TargetIndex WortInd = TargetIndex.B;

		private const int Duration = 200;

		protected Building_FermentingBarrel Barrel
		{
			get
			{
				return (Building_FermentingBarrel)base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		protected Thing Wort
		{
			get
			{
				return base.CurJob.GetTarget(TargetIndex.B).Thing;
			}
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			Toil reserveWort = Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
			yield return reserveWort;
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true).FailOnDestroyedNullOrForbidden(TargetIndex.B);
			yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveWort, TargetIndex.B, TargetIndex.None, true, null);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.Wait(200).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			yield return new Toil
			{
				initAction = (Action)delegate
				{
					((_003CMakeNewToils_003Ec__Iterator2A)/*Error near IL_0164: stateMachine*/)._003C_003Ef__this.Barrel.AddWort(((_003CMakeNewToils_003Ec__Iterator2A)/*Error near IL_0164: stateMachine*/)._003C_003Ef__this.Wort);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}