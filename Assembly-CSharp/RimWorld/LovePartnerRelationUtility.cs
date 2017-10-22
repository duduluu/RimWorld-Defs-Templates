using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class LovePartnerRelationUtility
	{
		private const float MinAgeToGenerateWithLovePartnerRelation = 14f;

		public static bool HasAnyLovePartner(Pawn pawn)
		{
			return pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, null) != null || pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, null) != null || pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, null) != null;
		}

		public static bool IsLovePartnerRelation(PawnRelationDef relation)
		{
			return relation == PawnRelationDefOf.Lover || relation == PawnRelationDefOf.Fiance || relation == PawnRelationDefOf.Spouse;
		}

		public static bool IsExLovePartnerRelation(PawnRelationDef relation)
		{
			return relation == PawnRelationDefOf.ExLover || relation == PawnRelationDefOf.ExSpouse;
		}

		public static bool HasAnyLovePartnerOfTheSameGender(Pawn pawn)
		{
			return pawn.relations.DirectRelations.Find((Predicate<DirectPawnRelation>)((DirectPawnRelation x) => LovePartnerRelationUtility.IsLovePartnerRelation(x.def) && x.otherPawn.gender == pawn.gender)) != null;
		}

		public static bool HasAnyExLovePartnerOfTheSameGender(Pawn pawn)
		{
			return pawn.relations.DirectRelations.Find((Predicate<DirectPawnRelation>)((DirectPawnRelation x) => LovePartnerRelationUtility.IsExLovePartnerRelation(x.def) && x.otherPawn.gender == pawn.gender)) != null;
		}

		public static bool HasAnyLovePartnerOfTheOppositeGender(Pawn pawn)
		{
			return pawn.relations.DirectRelations.Find((Predicate<DirectPawnRelation>)((DirectPawnRelation x) => LovePartnerRelationUtility.IsLovePartnerRelation(x.def) && x.otherPawn.gender != pawn.gender)) != null;
		}

		public static bool HasAnyExLovePartnerOfTheOppositeGender(Pawn pawn)
		{
			return pawn.relations.DirectRelations.Find((Predicate<DirectPawnRelation>)((DirectPawnRelation x) => LovePartnerRelationUtility.IsExLovePartnerRelation(x.def) && x.otherPawn.gender != pawn.gender)) != null;
		}

		public static Pawn ExistingLovePartner(Pawn pawn)
		{
			Pawn firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, null);
			if (firstDirectRelationPawn != null)
			{
				return firstDirectRelationPawn;
			}
			firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, null);
			if (firstDirectRelationPawn != null)
			{
				return firstDirectRelationPawn;
			}
			firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, null);
			if (firstDirectRelationPawn != null)
			{
				return firstDirectRelationPawn;
			}
			return null;
		}

		public static bool LovePartnerRelationExists(Pawn first, Pawn second)
		{
			return first.relations.DirectRelationExists(PawnRelationDefOf.Lover, second) || first.relations.DirectRelationExists(PawnRelationDefOf.Fiance, second) || first.relations.DirectRelationExists(PawnRelationDefOf.Spouse, second);
		}

		public static bool ExLovePartnerRelationExists(Pawn first, Pawn second)
		{
			return first.relations.DirectRelationExists(PawnRelationDefOf.ExSpouse, second) || first.relations.DirectRelationExists(PawnRelationDefOf.ExLover, second);
		}

		public static void GiveRandomExLoverOrExSpouseRelation(Pawn first, Pawn second)
		{
			PawnRelationDef def = (!(Rand.Value < 0.5)) ? PawnRelationDefOf.ExSpouse : PawnRelationDefOf.ExLover;
			first.relations.AddDirectRelation(def, second);
		}

		public static Pawn GetPartnerInMyBed(Pawn pawn)
		{
			if (((pawn.CurJob != null) ? pawn.jobs.curDriver.layingDown : LayingDownState.NotLaying) != 0)
			{
				Building_Bed building_Bed = pawn.CurrentBed();
				if (building_Bed == null)
				{
					return null;
				}
				if (building_Bed.SleepingSlotsCount <= 1)
				{
					return null;
				}
				if (!LovePartnerRelationUtility.HasAnyLovePartner(pawn))
				{
					return null;
				}
				foreach (Pawn curOccupant in building_Bed.CurOccupants)
				{
					if (curOccupant != pawn && LovePartnerRelationUtility.LovePartnerRelationExists(pawn, curOccupant))
					{
						return curOccupant;
					}
				}
				return null;
			}
			return null;
		}

		public static Pawn ExistingMostLikedLovePartner(Pawn p, bool allowDead)
		{
			DirectPawnRelation directPawnRelation = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(p, allowDead);
			if (directPawnRelation != null)
			{
				return directPawnRelation.otherPawn;
			}
			return null;
		}

		public static DirectPawnRelation ExistingMostLikedLovePartnerRel(Pawn p, bool allowDead)
		{
			if (!p.RaceProps.IsFlesh)
			{
				return null;
			}
			DirectPawnRelation directPawnRelation = null;
			int num = -2147483648;
			List<DirectPawnRelation> directRelations = p.relations.DirectRelations;
			for (int i = 0; i < directRelations.Count; i++)
			{
				if ((allowDead || !directRelations[i].otherPawn.Dead) && LovePartnerRelationUtility.IsLovePartnerRelation(directRelations[i].def))
				{
					int num2 = p.relations.OpinionOf(directRelations[i].otherPawn);
					if (directPawnRelation == null || num2 > num)
					{
						directPawnRelation = directRelations[i];
						num = num2;
					}
				}
			}
			return directPawnRelation;
		}

		public static float GetLovinMtbHours(Pawn pawn, Pawn partner)
		{
			if (!pawn.Dead && !partner.Dead)
			{
				if (DebugSettings.alwaysDoLovin)
				{
					return 0.1f;
				}
				if (!pawn.needs.food.Starving && !partner.needs.food.Starving)
				{
					if (!(pawn.health.hediffSet.BleedRateTotal > 0.0) && !(partner.health.hediffSet.BleedRateTotal > 0.0))
					{
						float num = LovePartnerRelationUtility.LovinMtbSinglePawnFactor(pawn);
						if (num <= 0.0)
						{
							return -1f;
						}
						float num2 = LovePartnerRelationUtility.LovinMtbSinglePawnFactor(partner);
						if (num2 <= 0.0)
						{
							return -1f;
						}
						float num3 = 12f;
						num3 *= num;
						num3 *= num2;
						num3 /= Mathf.Max(pawn.relations.SecondaryRomanceChanceFactor(partner), 0.1f);
						num3 /= Mathf.Max(partner.relations.SecondaryRomanceChanceFactor(pawn), 0.1f);
						num3 *= GenMath.LerpDouble(-100f, 100f, 1.3f, 0.7f, (float)pawn.relations.OpinionOf(partner));
						return num3 * GenMath.LerpDouble(-100f, 100f, 1.3f, 0.7f, (float)partner.relations.OpinionOf(pawn));
					}
					return -1f;
				}
				return -1f;
			}
			return -1f;
		}

		private static float LovinMtbSinglePawnFactor(Pawn pawn)
		{
			float num = 1f;
			num = (float)(num / (1.0 - pawn.health.hediffSet.PainTotal));
			float level = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness);
			if (level < 0.5)
			{
				num = (float)(num / (level * 2.0));
			}
			return num / GenMath.FlatHill(0f, 14f, 16f, 25f, 80f, 0.2f, pawn.ageTracker.AgeBiologicalYearsFloat);
		}

		public static void TryToShareBed(Pawn first, Pawn second)
		{
			if (!LovePartnerRelationUtility.TryToShareBed_Int(first, second))
			{
				LovePartnerRelationUtility.TryToShareBed_Int(second, first);
			}
		}

		private static bool TryToShareBed_Int(Pawn bedOwner, Pawn otherPawn)
		{
			Building_Bed ownedBed = bedOwner.ownership.OwnedBed;
			if (ownedBed != null && ownedBed.AnyUnownedSleepingSlot)
			{
				otherPawn.ownership.ClaimBedIfNonMedical(ownedBed);
				return true;
			}
			return false;
		}

		public static float LovePartnerRelationGenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request, bool ex)
		{
			if (generated.ageTracker.AgeBiologicalYearsFloat < 14.0)
			{
				return 0f;
			}
			if (other.ageTracker.AgeBiologicalYearsFloat < 14.0)
			{
				return 0f;
			}
			if (generated.gender == other.gender && (!other.story.traits.HasTrait(TraitDefOf.Gay) || !request.AllowGay))
			{
				return 0f;
			}
			if (generated.gender != other.gender && other.story.traits.HasTrait(TraitDefOf.Gay))
			{
				return 0f;
			}
			float num = 1f;
			if (ex)
			{
				int num2 = 0;
				List<DirectPawnRelation> directRelations = other.relations.DirectRelations;
				for (int i = 0; i < directRelations.Count; i++)
				{
					if (LovePartnerRelationUtility.IsExLovePartnerRelation(directRelations[i].def))
					{
						num2++;
					}
				}
				num = Mathf.Pow(0.2f, (float)num2);
			}
			else if (LovePartnerRelationUtility.HasAnyLovePartner(other))
			{
				return 0f;
			}
			float num3 = (float)((generated.gender != other.gender) ? 1.0 : 0.0099999997764825821);
			float generationChanceAgeFactor = LovePartnerRelationUtility.GetGenerationChanceAgeFactor(generated);
			float generationChanceAgeFactor2 = LovePartnerRelationUtility.GetGenerationChanceAgeFactor(other);
			float generationChanceAgeGapFactor = LovePartnerRelationUtility.GetGenerationChanceAgeGapFactor(generated, other, ex);
			float num4 = 1f;
			if (generated.GetRelations(other).Any((Func<PawnRelationDef, bool>)((PawnRelationDef x) => x.familyByBloodRelation)))
			{
				num4 = 0.01f;
			}
			float num5 = 1f;
			num5 = ((!request.FixedMelanin.HasValue) ? PawnSkinColors.GetMelaninCommonalityFactor(other.story.melanin) : ChildRelationUtility.GetMelaninSimilarityFactor(request.FixedMelanin.Value, other.story.melanin));
			return num * generationChanceAgeFactor * generationChanceAgeFactor2 * generationChanceAgeGapFactor * num3 * num5 * num4;
		}

		private static float GetGenerationChanceAgeFactor(Pawn p)
		{
			float value = GenMath.LerpDouble(14f, 27f, 0f, 1f, p.ageTracker.AgeBiologicalYearsFloat);
			return Mathf.Clamp(value, 0f, 1f);
		}

		private static float GetGenerationChanceAgeGapFactor(Pawn p1, Pawn p2, bool ex)
		{
			float num = Mathf.Abs(p1.ageTracker.AgeBiologicalYearsFloat - p2.ageTracker.AgeBiologicalYearsFloat);
			if (ex)
			{
				float num2 = LovePartnerRelationUtility.MinPossibleAgeGapAtMinAgeToGenerateAsLovers(p1, p2);
				if (num2 >= 0.0)
				{
					num = Mathf.Min(num, num2);
				}
				float num3 = LovePartnerRelationUtility.MinPossibleAgeGapAtMinAgeToGenerateAsLovers(p2, p1);
				if (num3 >= 0.0)
				{
					num = Mathf.Min(num, num3);
				}
			}
			if (num > 40.0)
			{
				return 0f;
			}
			float value = GenMath.LerpDouble(0f, 20f, 1f, 0.001f, num);
			return Mathf.Clamp(value, 0.001f, 1f);
		}

		private static float MinPossibleAgeGapAtMinAgeToGenerateAsLovers(Pawn p1, Pawn p2)
		{
			float num = (float)(p1.ageTracker.AgeChronologicalYearsFloat - 14.0);
			if (num < 0.0)
			{
				Log.Warning("at < 0");
				return 0f;
			}
			float num2 = PawnRelationUtility.MaxPossibleBioAgeAt(p2.ageTracker.AgeBiologicalYearsFloat, p2.ageTracker.AgeChronologicalYearsFloat, num);
			float num3 = PawnRelationUtility.MinPossibleBioAgeAt(p2.ageTracker.AgeBiologicalYearsFloat, num);
			if (num2 < 0.0)
			{
				return -1f;
			}
			if (num2 < 14.0)
			{
				return -1f;
			}
			if (num3 <= 14.0)
			{
				return 0f;
			}
			return (float)(num3 - 14.0);
		}

		public static void TryToShareChildrenForGeneratedLovePartner(Pawn generated, Pawn other, PawnGenerationRequest request, float extraChanceFactor)
		{
			if (generated.gender != other.gender)
			{
				List<Pawn> list = other.relations.Children.ToList();
				for (int i = 0; i < list.Count; i++)
				{
					Pawn pawn = list[i];
					float num = 1f;
					if (generated.gender == Gender.Male)
					{
						num = ChildRelationUtility.ChanceOfBecomingChildOf(pawn, generated, other, default(PawnGenerationRequest?), new PawnGenerationRequest?(request), default(PawnGenerationRequest?));
					}
					else if (generated.gender == Gender.Female)
					{
						num = ChildRelationUtility.ChanceOfBecomingChildOf(pawn, other, generated, default(PawnGenerationRequest?), default(PawnGenerationRequest?), new PawnGenerationRequest?(request));
					}
					num *= extraChanceFactor;
					if (Rand.Value < num)
					{
						if (generated.gender == Gender.Male)
						{
							pawn.SetFather(generated);
						}
						else if (generated.gender == Gender.Female)
						{
							pawn.SetMother(generated);
						}
					}
				}
			}
		}

		public static void ChangeSpouseRelationsToExSpouse(Pawn pawn)
		{
			while (true)
			{
				Pawn firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, null);
				if (firstDirectRelationPawn != null)
				{
					pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Spouse, firstDirectRelationPawn);
					pawn.relations.AddDirectRelation(PawnRelationDefOf.ExSpouse, firstDirectRelationPawn);
					continue;
				}
				break;
			}
		}

		public static Pawn GetMostDislikedNonPartnerBedOwner(Pawn p)
		{
			Building_Bed ownedBed = p.ownership.OwnedBed;
			if (ownedBed == null)
			{
				return null;
			}
			Pawn pawn = null;
			int num = 0;
			for (int i = 0; i < ownedBed.owners.Count; i++)
			{
				if (ownedBed.owners[i] != p && !LovePartnerRelationUtility.LovePartnerRelationExists(p, ownedBed.owners[i]))
				{
					int num2 = p.relations.OpinionOf(ownedBed.owners[i]);
					if (pawn == null || num2 < num)
					{
						pawn = ownedBed.owners[i];
						num = num2;
					}
				}
			}
			return pawn;
		}

		public static float IncestOpinionOffsetFor(Pawn other, Pawn pawn)
		{
			float num = 0f;
			List<DirectPawnRelation> directRelations = other.relations.DirectRelations;
			for (int i = 0; i < directRelations.Count; i++)
			{
				if (LovePartnerRelationUtility.IsLovePartnerRelation(directRelations[i].def) && directRelations[i].otherPawn != pawn && !directRelations[i].otherPawn.Dead)
				{
					foreach (PawnRelationDef relation in other.GetRelations(directRelations[i].otherPawn))
					{
						float incestOpinionOffset = relation.incestOpinionOffset;
						if (incestOpinionOffset < num)
						{
							num = incestOpinionOffset;
						}
					}
				}
			}
			return num;
		}
	}
}