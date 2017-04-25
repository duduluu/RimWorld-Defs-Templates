using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class JobDriver_PrepareCaravan_GatherItems : JobDriver
	{
		private const TargetIndex ToHaulInd = TargetIndex.A;

		private const TargetIndex CarrierInd = TargetIndex.B;

		private const int PlaceInInventoryDuration = 25;

		public Thing ToHaul
		{
			get
			{
				return base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		private Pawn Carrier
		{
			get
			{
				return (Pawn)base.CurJob.GetTarget(TargetIndex.B).Thing;
			}
		}

		private List<TransferableOneWay> Transferables
		{
			get
			{
				return ((LordJob_FormAndSendCaravan)base.CurJob.lord.LordJob).transferables;
			}
		}

		private TransferableOneWay Transferable
		{
			get
			{
				Thing toHaul = this.ToHaul;
				TransferableOneWay transferableOneWay = this.Transferables.Find((TransferableOneWay x) => x.things.Contains(toHaul));
				if (transferableOneWay != null)
				{
					return transferableOneWay;
				}
				transferableOneWay = TransferableUtility.TransferableMatching<TransferableOneWay>(toHaul, this.Transferables);
				if (transferableOneWay != null)
				{
					return transferableOneWay;
				}
				transferableOneWay = this.Transferables.Find((TransferableOneWay x) => x.ThingDef == toHaul.def);
				if (transferableOneWay != null)
				{
					return transferableOneWay;
				}
				throw new InvalidOperationException("Could not find any matching transferable.");
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			JobDriver_PrepareCaravan_GatherItems.<MakeNewToils>c__Iterator9 <MakeNewToils>c__Iterator = new JobDriver_PrepareCaravan_GatherItems.<MakeNewToils>c__Iterator9();
			<MakeNewToils>c__Iterator.<>f__this = this;
			JobDriver_PrepareCaravan_GatherItems.<MakeNewToils>c__Iterator9 expr_0E = <MakeNewToils>c__Iterator;
			expr_0E.$PC = -2;
			return expr_0E;
		}

		private Toil DetermineNumToHaul()
		{
			return new Toil
			{
				initAction = delegate
				{
					int num = GatherItemsForCaravanUtility.CountLeftToTransfer(this.pawn, this.Transferable, base.CurJob.lord);
					if (num <= 0)
					{
						this.pawn.jobs.EndCurrentJob(JobCondition.Succeeded, true);
					}
					else
					{
						base.CurJob.count = num;
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant,
				atomicWithPrevious = true
			};
		}

		private Toil AddCarriedThingToTransferables()
		{
			return new Toil
			{
				initAction = delegate
				{
					TransferableOneWay transferable = this.Transferable;
					if (!transferable.things.Contains(this.pawn.carryTracker.CarriedThing))
					{
						transferable.things.Add(this.pawn.carryTracker.CarriedThing);
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant,
				atomicWithPrevious = true
			};
		}

		private Toil FindCarrier()
		{
			return new Toil
			{
				initAction = delegate
				{
					Pawn pawn = this.FindBestCarrier(true);
					if (pawn == null)
					{
						bool flag = this.pawn.GetLord() == base.CurJob.lord;
						if (flag && !MassUtility.IsOverEncumbered(this.pawn))
						{
							pawn = this.pawn;
						}
						else
						{
							pawn = this.FindBestCarrier(false);
							if (pawn == null)
							{
								if (flag)
								{
									pawn = this.pawn;
								}
								else
								{
									IEnumerable<Pawn> source = from x in base.CurJob.lord.ownedPawns
									where JobDriver_PrepareCaravan_GatherItems.IsUsableCarrier(x, this.pawn, true)
									select x;
									if (!source.Any<Pawn>())
									{
										base.EndJobWith(JobCondition.Incompletable);
										return;
									}
									pawn = source.RandomElement<Pawn>();
								}
							}
						}
					}
					base.CurJob.SetTarget(TargetIndex.B, pawn);
				}
			};
		}

		private Toil PlaceTargetInCarrierInventory()
		{
			return new Toil
			{
				initAction = delegate
				{
					Pawn_CarryTracker carryTracker = this.pawn.carryTracker;
					Thing carriedThing = carryTracker.CarriedThing;
					this.Transferable.AdjustTo(Mathf.Max(this.Transferable.CountToTransfer - carriedThing.stackCount, 0));
					carryTracker.innerContainer.TryTransferToContainer(carriedThing, this.Carrier.inventory.innerContainer, carriedThing.stackCount);
				}
			};
		}

		public static bool IsUsableCarrier(Pawn p, Pawn forPawn, bool allowColonists)
		{
			return p == forPawn || (!p.DestroyedOrNull() && p.Spawned && !p.inventory.UnloadEverything && forPawn.CanReach(p, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn) && ((allowColonists && p.IsColonist) || ((p.RaceProps.packAnimal || p.HostFaction == Faction.OfPlayer) && !p.IsBurning() && !p.Downed && !MassUtility.IsOverEncumbered(p))));
		}

		private float GetCarrierScore(Pawn p)
		{
			float lengthHorizontal = (p.Position - this.pawn.Position).LengthHorizontal;
			float num = MassUtility.EncumbrancePercent(p);
			float num2 = 1f - num;
			return num2 - lengthHorizontal / 10f * 0.2f;
		}

		private Pawn FindBestCarrier(bool onlyAnimals)
		{
			Lord lord = base.CurJob.lord;
			Pawn pawn = null;
			float num = 0f;
			if (lord != null)
			{
				for (int i = 0; i < lord.ownedPawns.Count; i++)
				{
					Pawn pawn2 = lord.ownedPawns[i];
					if (pawn2 != this.pawn)
					{
						if (!onlyAnimals || pawn2.RaceProps.Animal)
						{
							if (JobDriver_PrepareCaravan_GatherItems.IsUsableCarrier(pawn2, this.pawn, false))
							{
								float carrierScore = this.GetCarrierScore(pawn2);
								if (pawn == null || carrierScore > num)
								{
									pawn = pawn2;
									num = carrierScore;
								}
							}
						}
					}
				}
			}
			return pawn;
		}
	}
}
