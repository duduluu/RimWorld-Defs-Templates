using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Need_Food : Need
	{
		private const float BaseFoodFallPerTick = 2.66666666E-05f;

		private const float MalnutritionSeverityPerDay = 0.17f;

		private const float MalnutritionSeverityPerInterval = 0.00113333331f;

		private int lastNonStarvingTick = -99999;

		public bool Starving
		{
			get
			{
				return this.CurCategory == HungerCategory.Starving;
			}
		}

		public float PercentageThreshUrgentlyHungry
		{
			get
			{
				return (float)(base.pawn.RaceProps.FoodLevelPercentageWantEat * 0.40000000596046448);
			}
		}

		public float PercentageThreshHungry
		{
			get
			{
				return (float)(base.pawn.RaceProps.FoodLevelPercentageWantEat * 0.800000011920929);
			}
		}

		public float NutritionBetweenHungryAndFed
		{
			get
			{
				return (float)((1.0 - this.PercentageThreshHungry) * this.MaxLevel);
			}
		}

		public HungerCategory CurCategory
		{
			get
			{
				if (base.CurLevelPercentage <= 0.0)
				{
					return HungerCategory.Starving;
				}
				if (base.CurLevelPercentage < this.PercentageThreshUrgentlyHungry)
				{
					return HungerCategory.UrgentlyHungry;
				}
				if (base.CurLevelPercentage < this.PercentageThreshHungry)
				{
					return HungerCategory.Hungry;
				}
				return HungerCategory.Fed;
			}
		}

		public float FoodFallPerTick
		{
			get
			{
				return this.FoodFallPerTickAssumingCategory(this.CurCategory);
			}
		}

		public int TicksUntilHungryWhenFed
		{
			get
			{
				return Mathf.CeilToInt(this.NutritionBetweenHungryAndFed / this.FoodFallPerTickAssumingCategory(HungerCategory.Fed));
			}
		}

		public override int GUIChangeArrow
		{
			get
			{
				return -1;
			}
		}

		public override float MaxLevel
		{
			get
			{
				return base.pawn.BodySize * base.pawn.ageTracker.CurLifeStage.foodMaxFactor;
			}
		}

		public float NutritionWanted
		{
			get
			{
				return this.MaxLevel - this.CurLevel;
			}
		}

		private float HungerRate
		{
			get
			{
				float num = base.pawn.ageTracker.CurLifeStage.hungerRateFactor * base.pawn.RaceProps.baseHungerRate;
				List<Hediff> hediffs = base.pawn.health.hediffSet.hediffs;
				for (int i = 0; i < hediffs.Count; i++)
				{
					HediffStage curStage = hediffs[i].CurStage;
					if (curStage != null)
					{
						num *= curStage.hungerRateFactor;
					}
				}
				return num;
			}
		}

		public int TicksStarving
		{
			get
			{
				return Mathf.Max(0, Find.TickManager.TicksGame - this.lastNonStarvingTick);
			}
		}

		public Need_Food(Pawn pawn) : base(pawn)
		{
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.lastNonStarvingTick, "lastNonStarvingTick", -99999, false);
		}

		private float FoodFallPerTickAssumingCategory(HungerCategory cat)
		{
			switch (cat)
			{
			case HungerCategory.Fed:
			{
				return (float)(2.6666666599339806E-05 * this.HungerRate);
			}
			case HungerCategory.Hungry:
			{
				return (float)(2.6666666599339806E-05 * this.HungerRate * 0.5);
			}
			case HungerCategory.UrgentlyHungry:
			{
				return (float)(2.6666666599339806E-05 * this.HungerRate * 0.25);
			}
			case HungerCategory.Starving:
			{
				return (float)(2.6666666599339806E-05 * this.HungerRate * 0.15000000596046448);
			}
			default:
			{
				return 999f;
			}
			}
		}

		public override void NeedInterval()
		{
			if (!base.IsFrozen)
			{
				this.CurLevel -= (float)(this.FoodFallPerTick * 150.0);
			}
			if (!this.Starving)
			{
				this.lastNonStarvingTick = Find.TickManager.TicksGame;
			}
			if (!base.IsFrozen)
			{
				if (this.Starving)
				{
					HealthUtility.AdjustSeverity(base.pawn, HediffDefOf.Malnutrition, 0.00113333331f);
				}
				else
				{
					HealthUtility.AdjustSeverity(base.pawn, HediffDefOf.Malnutrition, -0.00113333331f);
				}
			}
		}

		public override void SetInitialLevel()
		{
			if (base.pawn.RaceProps.Humanlike)
			{
				base.CurLevelPercentage = 0.8f;
			}
			else
			{
				base.CurLevelPercentage = Rand.Range(0.5f, 0.9f);
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				this.lastNonStarvingTick = Find.TickManager.TicksGame;
			}
		}

		public override string GetTipString()
		{
			return base.LabelCap + ": " + base.CurLevelPercentage.ToStringPercent() + " (" + this.CurLevel.ToString("0.##") + " / " + this.MaxLevel.ToString("0.##") + ")\n" + base.def.description;
		}

		public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = 2147483647, float customMargin = -1f, bool drawArrows = true, bool doTooltip = true)
		{
			if (base.threshPercents == null)
			{
				base.threshPercents = new List<float>();
			}
			base.threshPercents.Clear();
			base.threshPercents.Add(this.PercentageThreshHungry);
			base.threshPercents.Add(this.PercentageThreshUrgentlyHungry);
			base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip);
		}
	}
}