using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.AI;
using Verse.Sound;

namespace Verse
{
	public abstract class Verb : IExposable, ILoadReferenceable
	{
		public VerbProperties verbProps;

		public Thing caster;

		public ThingWithComps ownerEquipment;

		public HediffComp_VerbGiver ownerHediffComp;

		public int loadID = -1;

		public VerbState state;

		protected LocalTargetInfo currentTarget = (Thing)null;

		protected int burstShotsLeft;

		protected int ticksToNextBurstShot;

		protected bool surpriseAttack;

		protected bool canFreeInterceptNow = true;

		public Action castCompleteCallback;

		private static List<IntVec3> tempLeanShootSources = new List<IntVec3>();

		private static List<IntVec3> tempDestList = new List<IntVec3>();

		public Pawn CasterPawn
		{
			get
			{
				return this.caster as Pawn;
			}
		}

		public bool CasterIsPawn
		{
			get
			{
				return this.caster is Pawn;
			}
		}

		protected virtual int ShotsPerBurst
		{
			get
			{
				return 1;
			}
		}

		public virtual Texture2D UIIcon
		{
			get
			{
				if (this.ownerEquipment != null)
				{
					return this.ownerEquipment.def.uiIcon;
				}
				return BaseContent.BadTex;
			}
		}

		public bool Bursting
		{
			get
			{
				return this.burstShotsLeft > 0;
			}
		}

		public float GetDamageFactorFor(Pawn pawn)
		{
			if (pawn != null)
			{
				if (this.ownerHediffComp != null)
				{
					return PawnCapacityUtility.CalculatePartEfficiency(this.ownerHediffComp.Pawn.health.hediffSet, this.ownerHediffComp.parent.Part, true, null);
				}
				if (this.verbProps.linkedBodyPartsGroup != null)
				{
					return PawnCapacityUtility.CalculateNaturalPartsAverageEfficiency(pawn.health.hediffSet, this.verbProps.linkedBodyPartsGroup);
				}
			}
			return 1f;
		}

		public bool IsStillUsableBy(Pawn pawn)
		{
			if (this.ownerEquipment != null && !pawn.equipment.AllEquipmentListForReading.Contains(this.ownerEquipment))
			{
				return false;
			}
			if (this.ownerHediffComp != null)
			{
				bool flag = false;
				List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
				int num = 0;
				while (num < hediffs.Count)
				{
					if (hediffs[num] != this.ownerHediffComp.parent)
					{
						num++;
						continue;
					}
					flag = true;
					break;
				}
				if (!flag)
				{
					return false;
				}
			}
			if (this.GetDamageFactorFor(pawn) == 0.0)
			{
				return false;
			}
			return true;
		}

		public virtual void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.loadID, "loadID", 0, false);
			Scribe_Values.Look<VerbState>(ref this.state, "state", VerbState.Idle, false);
			Scribe_TargetInfo.Look(ref this.currentTarget, "currentTarget");
			Scribe_Values.Look<int>(ref this.burstShotsLeft, "burstShotsLeft", 0, false);
			Scribe_Values.Look<int>(ref this.ticksToNextBurstShot, "ticksToNextBurstShot", 0, false);
		}

		public string GetUniqueLoadID()
		{
			return "Verb_" + this.loadID;
		}

		public bool TryStartCastOn(LocalTargetInfo castTarg, bool surpriseAttack = false, bool canFreeIntercept = true)
		{
			if (this.caster == null)
			{
				Log.Error("Verb " + this.GetUniqueLoadID() + " needs caster to work (possibly lost during saving/loading).");
				return false;
			}
			if (!this.caster.Spawned)
			{
				return false;
			}
			if (this.state != VerbState.Bursting && this.CanHitTarget(castTarg))
			{
				if (this.verbProps.CausesTimeSlowdown && castTarg.HasThing && (castTarg.Thing.def.category == ThingCategory.Pawn || (castTarg.Thing.def.building != null && castTarg.Thing.def.building.IsTurret)) && castTarg.Thing.Faction == Faction.OfPlayer && this.caster.HostileTo(Faction.OfPlayer))
				{
					Find.TickManager.slower.SignalForceNormalSpeed();
				}
				this.surpriseAttack = surpriseAttack;
				this.canFreeInterceptNow = canFreeIntercept;
				this.currentTarget = castTarg;
				if (this.CasterIsPawn && this.verbProps.warmupTime > 0.0)
				{
					ShootLine newShootLine = default(ShootLine);
					if (this.TryFindShootLineFromTo(this.caster.Position, castTarg, out newShootLine))
					{
						this.CasterPawn.Drawer.Notify_WarmingCastAlongLine(newShootLine, this.caster.Position);
						float statValue = this.CasterPawn.GetStatValue(StatDefOf.AimingDelayFactor, true);
						int ticks = (this.verbProps.warmupTime * statValue).SecondsToTicks();
						this.CasterPawn.stances.SetStance(new Stance_Warmup(ticks, castTarg, this));
						goto IL_01aa;
					}
					return false;
				}
				this.WarmupComplete();
				goto IL_01aa;
			}
			return false;
			IL_01aa:
			return true;
		}

		public virtual void WarmupComplete()
		{
			this.burstShotsLeft = this.ShotsPerBurst;
			this.state = VerbState.Bursting;
			this.TryCastNextBurstShot();
		}

		public void VerbTick()
		{
			if (this.state == VerbState.Bursting)
			{
				if (!this.caster.Spawned)
				{
					this.Reset();
				}
				else
				{
					this.ticksToNextBurstShot--;
					if (this.ticksToNextBurstShot <= 0)
					{
						this.TryCastNextBurstShot();
					}
				}
			}
		}

		protected void TryCastNextBurstShot()
		{
			LocalTargetInfo target = this.currentTarget;
			if (this.TryCastShot())
			{
				if (this.verbProps.muzzleFlashScale > 0.0099999997764825821)
				{
					MoteMaker.MakeStaticMote(this.caster.Position, this.caster.Map, ThingDefOf.Mote_ShotFlash, this.verbProps.muzzleFlashScale);
				}
				if (this.verbProps.soundCast != null)
				{
					this.verbProps.soundCast.PlayOneShot(new TargetInfo(this.caster.Position, this.caster.Map, false));
				}
				if (this.verbProps.soundCastTail != null)
				{
					this.verbProps.soundCastTail.PlayOneShotOnCamera(this.caster.Map);
				}
				if (this.CasterIsPawn)
				{
					if (this.CasterPawn.thinker != null)
					{
						this.CasterPawn.mindState.Notify_EngagedTarget();
					}
					if (this.CasterPawn.mindState != null)
					{
						this.CasterPawn.mindState.Notify_AttackedTarget(target);
					}
					if (!this.CasterPawn.Spawned)
						return;
				}
				this.burstShotsLeft--;
			}
			else
			{
				this.burstShotsLeft = 0;
			}
			if (this.burstShotsLeft > 0)
			{
				this.ticksToNextBurstShot = this.verbProps.ticksBetweenBurstShots;
				if (this.CasterIsPawn)
				{
					this.CasterPawn.stances.SetStance(new Stance_Cooldown(this.verbProps.ticksBetweenBurstShots + 1, this.currentTarget, this));
				}
			}
			else
			{
				this.state = VerbState.Idle;
				if (this.CasterIsPawn)
				{
					this.CasterPawn.stances.SetStance(new Stance_Cooldown(this.verbProps.AdjustedCooldownTicks(this.ownerEquipment), this.currentTarget, this));
				}
				if ((object)this.castCompleteCallback != null)
				{
					this.castCompleteCallback();
				}
			}
		}

		protected abstract bool TryCastShot();

		public void Notify_PickedUp()
		{
			this.Reset();
		}

		public virtual void Reset()
		{
			this.state = VerbState.Idle;
			this.currentTarget = (Thing)null;
			this.burstShotsLeft = 0;
			this.ticksToNextBurstShot = 0;
			this.castCompleteCallback = null;
			this.surpriseAttack = false;
		}

		public virtual void Notify_EquipmentLost()
		{
			if (this.CasterIsPawn)
			{
				Pawn casterPawn = this.CasterPawn;
				if (casterPawn.Spawned)
				{
					Stance_Warmup stance_Warmup = casterPawn.stances.curStance as Stance_Warmup;
					if (stance_Warmup != null && stance_Warmup.verb == this)
					{
						casterPawn.stances.CancelBusyStanceSoft();
					}
				}
			}
		}

		public virtual float HighlightFieldRadiusAroundTarget()
		{
			return 0f;
		}

		public bool CanHitTarget(LocalTargetInfo targ)
		{
			return this.CanHitTargetFrom(this.caster.Position, targ);
		}

		public virtual bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
		{
			if (targ.Thing != null && targ.Thing == this.caster)
			{
				return this.verbProps.targetParams.canTargetSelf;
			}
			if (this.CasterIsPawn && this.CasterPawn.apparel != null)
			{
				List<Apparel> wornApparel = this.CasterPawn.apparel.WornApparel;
				for (int i = 0; i < wornApparel.Count; i++)
				{
					if (!wornApparel[i].AllowVerbCast(root, this.caster.Map, targ))
					{
						return false;
					}
				}
			}
			ShootLine shootLine = default(ShootLine);
			return this.TryFindShootLineFromTo(root, targ, out shootLine);
		}

		public bool TryFindShootLineFromTo(IntVec3 root, LocalTargetInfo targ, out ShootLine resultingLine)
		{
			if (targ.HasThing && targ.Thing.Map != this.caster.Map)
			{
				resultingLine = default(ShootLine);
				return false;
			}
			if (this.verbProps.MeleeRange)
			{
				resultingLine = new ShootLine(root, targ.Cell);
				return ReachabilityImmediate.CanReachImmediate(root, targ, this.caster.Map, PathEndMode.Touch, null);
			}
			CellRect cellRect = (!targ.HasThing) ? CellRect.SingleCell(targ.Cell) : targ.Thing.OccupiedRect();
			float num = cellRect.ClosestDistSquaredTo(root);
			if (!(num > this.verbProps.range * this.verbProps.range) && !(num < this.verbProps.minRange * this.verbProps.minRange))
			{
				if (!this.verbProps.NeedsLineOfSight)
				{
					resultingLine = new ShootLine(root, targ.Cell);
					return true;
				}
				IntVec3 dest = default(IntVec3);
				if (this.CasterIsPawn)
				{
					if (this.CanHitFromCellIgnoringRange(root, targ, out dest))
					{
						resultingLine = new ShootLine(root, dest);
						return true;
					}
					ShootLeanUtility.LeanShootingSourcesFromTo(root, cellRect.ClosestCellTo(root), this.caster.Map, Verb.tempLeanShootSources);
					for (int i = 0; i < Verb.tempLeanShootSources.Count; i++)
					{
						IntVec3 intVec = Verb.tempLeanShootSources[i];
						if (this.CanHitFromCellIgnoringRange(intVec, targ, out dest))
						{
							resultingLine = new ShootLine(intVec, dest);
							return true;
						}
					}
				}
				else
				{
					CellRect.CellRectIterator iterator = this.caster.OccupiedRect().GetIterator();
					while (!iterator.Done())
					{
						IntVec3 current = iterator.Current;
						if (this.CanHitFromCellIgnoringRange(current, targ, out dest))
						{
							resultingLine = new ShootLine(current, dest);
							return true;
						}
						iterator.MoveNext();
					}
				}
				resultingLine = new ShootLine(root, targ.Cell);
				return false;
			}
			resultingLine = new ShootLine(root, targ.Cell);
			return false;
		}

		private bool CanHitFromCellIgnoringRange(IntVec3 sourceCell, LocalTargetInfo targ, out IntVec3 goodDest)
		{
			if (targ.Thing != null)
			{
				if (targ.Thing.Map != this.caster.Map)
				{
					goodDest = IntVec3.Invalid;
					return false;
				}
				ShootLeanUtility.CalcShootableCellsOf(Verb.tempDestList, targ.Thing);
				for (int i = 0; i < Verb.tempDestList.Count; i++)
				{
					if (this.CanHitCellFromCellIgnoringRange(sourceCell, Verb.tempDestList[i], targ.Thing.def.Fillage == FillCategory.Full))
					{
						goodDest = Verb.tempDestList[i];
						return true;
					}
				}
			}
			else if (this.CanHitCellFromCellIgnoringRange(sourceCell, targ.Cell, false))
			{
				goodDest = targ.Cell;
				return true;
			}
			goodDest = IntVec3.Invalid;
			return false;
		}

		private bool CanHitCellFromCellIgnoringRange(IntVec3 sourceSq, IntVec3 targetLoc, bool includeCorners = false)
		{
			if (this.verbProps.mustCastOnOpenGround && (!targetLoc.Standable(this.caster.Map) || this.caster.Map.thingGrid.CellContains(targetLoc, ThingCategory.Pawn)))
			{
				return false;
			}
			if (this.verbProps.requireLineOfSight)
			{
				if (!includeCorners)
				{
					if (!GenSight.LineOfSight(sourceSq, targetLoc, this.caster.Map, true, null, 0, 0))
					{
						return false;
					}
				}
				else if (!GenSight.LineOfSightToEdges(sourceSq, targetLoc, this.caster.Map, true, null))
				{
					return false;
				}
			}
			return true;
		}

		public override string ToString()
		{
			string str = this.verbProps.label.NullOrEmpty() ? ((this.ownerHediffComp == null) ? ((this.ownerEquipment == null) ? ((this.verbProps.linkedBodyPartsGroup == null) ? "unknown" : this.verbProps.linkedBodyPartsGroup.defName) : this.ownerEquipment.def.label) : this.ownerHediffComp.Def.label) : this.verbProps.label;
			return base.GetType().ToString() + "(" + str + ")";
		}
	}
}