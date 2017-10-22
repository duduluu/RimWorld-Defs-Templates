using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Mineable : Building
	{
		private const float YieldChanceOnNonMiningKill = 0.2f;

		private float yieldPct;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.yieldPct, "yieldPct", 0f, false);
		}

		public override void PreApplyDamage(DamageInfo dinfo, out bool absorbed)
		{
			base.PreApplyDamage(dinfo, out absorbed);
			if (!absorbed)
			{
				if (base.def.building.mineableThing != null && base.def.building.mineableYieldWasteable && dinfo.Def == DamageDefOf.Mining && dinfo.Instigator != null && dinfo.Instigator is Pawn)
				{
					int num = Mathf.Min(dinfo.Amount, this.HitPoints);
					float num2 = (float)num / (float)base.MaxHitPoints;
					this.yieldPct += num2 * dinfo.Instigator.GetStatValue(StatDefOf.MiningYield, true);
				}
				absorbed = false;
			}
		}

		public void DestroyMined(Pawn pawn)
		{
			Map map = base.Map;
			base.Destroy(DestroyMode.KillFinalize);
			this.TrySpawnYield(map, this.yieldPct, true);
		}

		public override void Destroy(DestroyMode mode)
		{
			Map map = base.Map;
			base.Destroy(mode);
			if (mode == DestroyMode.KillFinalize)
			{
				this.TrySpawnYield(map, 0.2f, false);
			}
		}

		private void TrySpawnYield(Map map, float yieldChance, bool moteOnWaste)
		{
			if (base.def.building.mineableThing != null && !(Rand.Value > base.def.building.mineableDropChance))
			{
				int num = base.def.building.mineableYield;
				if (base.def.building.mineableYieldWasteable)
				{
					num = Mathf.Max(1, GenMath.RoundRandom((float)num * this.yieldPct));
				}
				Thing thing = ThingMaker.MakeThing(base.def.building.mineableThing, null);
				thing.stackCount = num;
				GenSpawn.Spawn(thing, base.Position, map);
			}
		}
	}
}