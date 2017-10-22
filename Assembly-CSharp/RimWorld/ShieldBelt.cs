using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class ShieldBelt : Apparel
	{
		private const float MinDrawSize = 1.2f;

		private const float MaxDrawSize = 1.55f;

		private const float MaxDamagedJitterDist = 0.05f;

		private const int JitterDurationTicks = 8;

		private float energy;

		private int ticksToReset = -1;

		private int lastKeepDisplayTick = -9999;

		private Vector3 impactAngleVect;

		private int lastAbsorbDamageTick = -9999;

		private int StartingTicksToReset = 3200;

		private float EnergyOnReset = 0.2f;

		private float EnergyLossPerDamage = 0.033f;

		private int KeepDisplayingTicks = 1000;

		private float ApparelScorePerEnergyMax = 0.25f;

		private static readonly Material BubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);

		private float EnergyMax
		{
			get
			{
				return this.GetStatValue(StatDefOf.EnergyShieldEnergyMax, true);
			}
		}

		private float EnergyGainPerTick
		{
			get
			{
				return (float)(this.GetStatValue(StatDefOf.EnergyShieldRechargeRate, true) / 60.0);
			}
		}

		public float Energy
		{
			get
			{
				return this.energy;
			}
		}

		public ShieldState ShieldState
		{
			get
			{
				if (this.ticksToReset > 0)
				{
					return ShieldState.Resetting;
				}
				return ShieldState.Active;
			}
		}

		private bool ShouldDisplay
		{
			get
			{
				Pawn wearer = base.Wearer;
				if (!wearer.Dead && !wearer.Downed)
				{
					if (wearer.IsPrisonerOfColony && (wearer.MentalStateDef == null || !wearer.MentalStateDef.IsAggro))
					{
						return false;
					}
					if (wearer.Drafted)
					{
						return true;
					}
					if (wearer.Faction.HostileTo(Faction.OfPlayer))
					{
						return true;
					}
					if (Find.TickManager.TicksGame < this.lastKeepDisplayTick + this.KeepDisplayingTicks)
					{
						return true;
					}
					return false;
				}
				return false;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.energy, "energy", 0f, false);
			Scribe_Values.Look<int>(ref this.ticksToReset, "ticksToReset", -1, false);
			Scribe_Values.Look<int>(ref this.lastKeepDisplayTick, "lastKeepDisplayTick", 0, false);
		}

		public override IEnumerable<Gizmo> GetWornGizmos()
		{
			if (Find.Selector.SingleSelectedThing == base.Wearer)
			{
				yield return (Gizmo)new Gizmo_EnergyShieldStatus
				{
					shield = this
				};
			}
		}

		public override float GetSpecialApparelScoreOffset()
		{
			return this.EnergyMax * this.ApparelScorePerEnergyMax;
		}

		public override void Tick()
		{
			base.Tick();
			if (base.Wearer == null)
			{
				this.energy = 0f;
			}
			else if (this.ShieldState == ShieldState.Resetting)
			{
				this.ticksToReset--;
				if (this.ticksToReset <= 0)
				{
					this.Reset();
				}
			}
			else if (this.ShieldState == ShieldState.Active)
			{
				this.energy += this.EnergyGainPerTick;
				if (this.energy > this.EnergyMax)
				{
					this.energy = this.EnergyMax;
				}
			}
		}

		public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
		{
			if (this.ShieldState == ShieldState.Active)
			{
				if (dinfo.Instigator != null && !dinfo.Instigator.Position.AdjacentTo8WayOrInside(base.Wearer.Position))
				{
					goto IL_0049;
				}
				if (dinfo.Def.isExplosive)
					goto IL_0049;
			}
			return false;
			IL_0049:
			if (dinfo.Instigator != null)
			{
				AttachableThing attachableThing = dinfo.Instigator as AttachableThing;
				if (attachableThing != null && attachableThing.parent == base.Wearer)
				{
					return false;
				}
			}
			this.energy -= (float)dinfo.Amount * this.EnergyLossPerDamage;
			if (dinfo.Def == DamageDefOf.EMP)
			{
				this.energy = -1f;
			}
			if (this.energy < 0.0)
			{
				this.Break();
			}
			else
			{
				this.AbsorbedDamage(dinfo);
			}
			return true;
		}

		public void KeepDisplaying()
		{
			this.lastKeepDisplayTick = Find.TickManager.TicksGame;
		}

		private void AbsorbedDamage(DamageInfo dinfo)
		{
			SoundDefOf.EnergyShieldAbsorbDamage.PlayOneShot(new TargetInfo(base.Wearer.Position, base.Wearer.Map, false));
			this.impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
			Vector3 loc = base.Wearer.TrueCenter() + this.impactAngleVect.RotatedBy(180f) * 0.5f;
			float num = Mathf.Min(10f, (float)(2.0 + (float)dinfo.Amount / 10.0));
			MoteMaker.MakeStaticMote(loc, base.Wearer.Map, ThingDefOf.Mote_ExplosionFlash, num);
			int num2 = (int)num;
			for (int num3 = 0; num3 < num2; num3++)
			{
				MoteMaker.ThrowDustPuff(loc, base.Wearer.Map, Rand.Range(0.8f, 1.2f));
			}
			this.lastAbsorbDamageTick = Find.TickManager.TicksGame;
			this.KeepDisplaying();
		}

		private void Break()
		{
			SoundDefOf.EnergyShieldBroken.PlayOneShot(new TargetInfo(base.Wearer.Position, base.Wearer.Map, false));
			MoteMaker.MakeStaticMote(base.Wearer.TrueCenter(), base.Wearer.Map, ThingDefOf.Mote_ExplosionFlash, 12f);
			for (int i = 0; i < 6; i++)
			{
				Vector3 loc = base.Wearer.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle((float)Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f);
				MoteMaker.ThrowDustPuff(loc, base.Wearer.Map, Rand.Range(0.8f, 1.2f));
			}
			this.energy = 0f;
			this.ticksToReset = this.StartingTicksToReset;
		}

		private void Reset()
		{
			if (base.Wearer.Spawned)
			{
				SoundDefOf.EnergyShieldReset.PlayOneShot(new TargetInfo(base.Wearer.Position, base.Wearer.Map, false));
				MoteMaker.ThrowLightningGlow(base.Wearer.TrueCenter(), base.Wearer.Map, 3f);
			}
			this.ticksToReset = -1;
			this.energy = this.EnergyOnReset;
		}

		public override void DrawWornExtras()
		{
			if (this.ShieldState == ShieldState.Active && this.ShouldDisplay)
			{
				float num = Mathf.Lerp(1.2f, 1.55f, this.energy);
				Vector3 vector = base.Wearer.Drawer.DrawPos;
				vector.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
				int num2 = Find.TickManager.TicksGame - this.lastAbsorbDamageTick;
				if (num2 < 8)
				{
					float num3 = (float)((float)(8 - num2) / 8.0 * 0.05000000074505806);
					vector += this.impactAngleVect * num3;
					num -= num3;
				}
				float angle = (float)Rand.Range(0, 360);
				Vector3 s = new Vector3(num, 1f, num);
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
				Graphics.DrawMesh(MeshPool.plane10, matrix, ShieldBelt.BubbleMat, 0);
			}
		}

		public override bool AllowVerbCast(IntVec3 root, Map map, LocalTargetInfo targ)
		{
			return ReachabilityImmediate.CanReachImmediate(root, targ, map, PathEndMode.Touch, null);
		}
	}
}