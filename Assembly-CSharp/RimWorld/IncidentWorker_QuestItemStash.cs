using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_QuestItemStash : IncidentWorker
	{
		private const float ChanceToRevealSitePart = 0.5f;

		private const float AIPersonaCoreConsiderChance = 0.25f;

		private static readonly IntRange TimeoutDaysRange = new IntRange(10, 30);

		private static readonly IntRange ThingsCountRange = new IntRange(5, 9);

		private static readonly FloatRange TotalMarketValueRange = new FloatRange(2500f, 3200f);

		private static readonly IntRange FeeRange = new IntRange(100, 750);

		private static readonly IntRange NeurotrainersCountRange = new IntRange(3, 5);

		private List<Pair<ItemCollectionGeneratorDef, ItemCollectionGeneratorParams>> possibleItemCollectionGenerators = new List<Pair<ItemCollectionGeneratorDef, ItemCollectionGeneratorParams>>();

		private List<SitePartDef> possibleSitePartsInt = new List<SitePartDef>();

		private List<SitePartDef> PossibleSiteParts
		{
			get
			{
				this.possibleSitePartsInt.Clear();
				this.possibleSitePartsInt.Add(SitePartDefOf.Manhunters);
				this.possibleSitePartsInt.Add(SitePartDefOf.Outpost);
				this.possibleSitePartsInt.Add(SitePartDefOf.Turrets);
				this.possibleSitePartsInt.Add(null);
				return this.possibleSitePartsInt;
			}
		}

		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			if (!base.CanFireNowSub(target))
			{
				return false;
			}
			SitePartDef sitePartDef;
			if (!SiteMaker.TryFindNewRandomSitePartFor(SiteCoreDefOf.ItemStash, null, this.PossibleSiteParts, null, out sitePartDef, true, null))
			{
				return false;
			}
			IEnumerable<SitePartDef> arg_41_0;
			if (sitePartDef != null)
			{
				IEnumerable<SitePartDef> enumerable = Gen.YieldSingle<SitePartDef>(sitePartDef);
				arg_41_0 = enumerable;
			}
			else
			{
				arg_41_0 = null;
			}
			IEnumerable<SitePartDef> parts = arg_41_0;
			int num;
			Faction faction;
			return Find.FactionManager.RandomAlliedFaction(false, false, false) != null && TileFinder.TryFindNewSiteTile(out num) && SiteMaker.TryFindRandomFactionFor(SiteCoreDefOf.ItemStash, parts, out faction, true, null);
		}

		public override bool TryExecute(IncidentParms parms)
		{
			Faction faction = Find.FactionManager.RandomAlliedFaction(false, false, false);
			if (faction == null)
			{
				return false;
			}
			int tile;
			if (!TileFinder.TryFindNewSiteTile(out tile))
			{
				return false;
			}
			SitePartDef sitePart;
			if (!SiteMaker.TryFindNewRandomSitePartFor(SiteCoreDefOf.ItemStash, null, this.PossibleSiteParts, null, out sitePart, true, null))
			{
				return false;
			}
			IEnumerable<SitePartDef> arg_7C_0;
			if (sitePart != null)
			{
				IEnumerable<SitePartDef> enumerable = Gen.YieldSingle<SitePartDef>(sitePart);
				arg_7C_0 = enumerable;
			}
			else
			{
				arg_7C_0 = null;
			}
			IEnumerable<SitePartDef> parts = arg_7C_0;
			Faction siteFaction;
			if (!SiteMaker.TryFindRandomFactionFor(SiteCoreDefOf.ItemStash, parts, out siteFaction, true, null))
			{
				return false;
			}
			int days = IncidentWorker_QuestItemStash.TimeoutDaysRange.RandomInRange;
			List<Thing> items = this.GenerateItems(siteFaction);
			string letterText = this.GetLetterText(faction, items, days, this.GetSitePartInfoKey(sitePart));
			if (Rand.Chance(this.FeeDemandChance(faction)))
			{
				Map map = TradeUtility.PlayerHomeMapWithMostLaunchableSilver();
				int fee = IncidentWorker_QuestItemStash.FeeRange.RandomInRange;
				DiaNode diaNode = new DiaNode(letterText + "\n\n" + "ItemStashQuestFeeDemand".Translate(new object[]
				{
					faction.leader.LabelShort,
					fee
				}).CapitalizeFirst());
				DiaOption diaOption = new DiaOption("ItemStashQuest_Accept".Translate());
				diaOption.action = delegate
				{
					Site o2 = this.CreateSite(tile, sitePart, days, siteFaction, items);
					CameraJumper.TryJumpAndSelect(o2);
					TradeUtility.LaunchSilver(map, fee);
				};
				diaOption.resolveTree = true;
				if (map == null || !TradeUtility.ColonyHasEnoughSilver(map, fee))
				{
					diaOption.Disable("NeedSilverLaunchable".Translate(new object[]
					{
						fee
					}));
				}
				diaNode.options.Add(diaOption);
				DiaOption diaOption2 = new DiaOption("ItemStashQuest_Reject".Translate());
				diaOption2.action = delegate
				{
					for (int i = 0; i < items.Count; i++)
					{
						items[i].Destroy(DestroyMode.Vanish);
					}
				};
				diaOption2.resolveTree = true;
				diaNode.options.Add(diaOption2);
				Dialog_NodeTree dialog_NodeTree = new Dialog_NodeTree(diaNode, true, true, "ItemStashQuestTitle".Translate());
				dialog_NodeTree.closeOnEscapeKey = false;
				Find.WindowStack.Add(dialog_NodeTree);
			}
			else
			{
				Site o = this.CreateSite(tile, sitePart, days, siteFaction, items);
				Find.LetterStack.ReceiveLetter(this.def.letterLabel, letterText, this.def.letterType, o, null);
			}
			return true;
		}

		private string GetSitePartInfoKey(SitePartDef sitePart)
		{
			if (sitePart == null)
			{
				return "ItemStashSitePart_Nothing";
			}
			if (sitePart == SitePartDefOf.Manhunters)
			{
				return "ItemStashSitePart_Manhunters";
			}
			if (sitePart == SitePartDefOf.Outpost)
			{
				return "ItemStashSitePart_Outpost";
			}
			if (sitePart == SitePartDefOf.Turrets)
			{
				return "ItemStashSitePart_Turrets";
			}
			throw new InvalidOperationException("Site part not handled: " + sitePart);
		}

		private List<Thing> GenerateItems(Faction siteFaction)
		{
			this.CalculatePossibleItemCollectionGenerators(siteFaction);
			Pair<ItemCollectionGeneratorDef, ItemCollectionGeneratorParams> pair = this.possibleItemCollectionGenerators.RandomElement<Pair<ItemCollectionGeneratorDef, ItemCollectionGeneratorParams>>();
			return pair.First.Worker.Generate(pair.Second);
		}

		private void CalculatePossibleItemCollectionGenerators(Faction siteFaction)
		{
			TechLevel techLevel = (siteFaction == null) ? TechLevel.Spacer : siteFaction.def.techLevel;
			this.possibleItemCollectionGenerators.Clear();
			if (Rand.Chance(0.25f) && techLevel >= ThingDefOf.AIPersonaCore.techLevel)
			{
				ItemCollectionGeneratorDef aIPersonaCores = ItemCollectionGeneratorDefOf.AIPersonaCores;
				ItemCollectionGeneratorParams second = default(ItemCollectionGeneratorParams);
				second.count = 1;
				this.possibleItemCollectionGenerators.Add(new Pair<ItemCollectionGeneratorDef, ItemCollectionGeneratorParams>(aIPersonaCores, second));
			}
			if (techLevel >= ThingDefOf.Neurotrainer.techLevel)
			{
				ItemCollectionGeneratorDef neurotrainers = ItemCollectionGeneratorDefOf.Neurotrainers;
				ItemCollectionGeneratorParams second2 = default(ItemCollectionGeneratorParams);
				second2.count = IncidentWorker_QuestItemStash.NeurotrainersCountRange.RandomInRange;
				this.possibleItemCollectionGenerators.Add(new Pair<ItemCollectionGeneratorDef, ItemCollectionGeneratorParams>(neurotrainers, second2));
			}
			ItemCollectionGeneratorParams second3 = default(ItemCollectionGeneratorParams);
			second3.count = IncidentWorker_QuestItemStash.ThingsCountRange.RandomInRange;
			second3.totalMarketValue = IncidentWorker_QuestItemStash.TotalMarketValueRange.RandomInRange;
			second3.techLevel = techLevel;
			this.possibleItemCollectionGenerators.Add(new Pair<ItemCollectionGeneratorDef, ItemCollectionGeneratorParams>(ItemCollectionGeneratorDefOf.Weapons, second3));
			this.possibleItemCollectionGenerators.Add(new Pair<ItemCollectionGeneratorDef, ItemCollectionGeneratorParams>(ItemCollectionGeneratorDefOf.RawResources, second3));
			this.possibleItemCollectionGenerators.Add(new Pair<ItemCollectionGeneratorDef, ItemCollectionGeneratorParams>(ItemCollectionGeneratorDefOf.Apparel, second3));
		}

		private Site CreateSite(int tile, SitePartDef sitePart, int days, Faction siteFaction, List<Thing> items)
		{
			Site site = (Site)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Site);
			site.Tile = tile;
			site.core = SiteCoreDefOf.ItemStash;
			if (sitePart != null)
			{
				site.parts.Add(sitePart);
			}
			site.SetFaction(siteFaction);
			site.GetComponent<TimeoutComp>().StartTimeout(days * 60000);
			site.GetComponent<ItemStashContentsComp>().contents.TryAddRange(items);
			Find.WorldObjects.Add(site);
			return site;
		}

		private string GetLetterText(Faction alliedFaction, List<Thing> items, int days, string sitePartTextKey)
		{
			string text;
			if (Rand.Chance(0.5f))
			{
				text = sitePartTextKey.Translate(new object[]
				{
					alliedFaction.leader.LabelShort
				}).CapitalizeFirst();
			}
			else
			{
				text = "ItemStashSitePart_Unknown".Translate(new object[]
				{
					alliedFaction.leader.LabelShort
				}).CapitalizeFirst();
			}
			return string.Format(this.def.letterText, new object[]
			{
				alliedFaction.leader.LabelShort,
				alliedFaction.def.leaderTitle,
				alliedFaction.Name,
				GenLabel.ThingsLabel(items).TrimEndNewlines(),
				days.ToString(),
				text
			}).CapitalizeFirst();
		}

		private float FeeDemandChance(Faction faction)
		{
			FactionRelation factionRelation = faction.RelationWith(Faction.OfPlayer, false);
			float x = Mathf.Max(factionRelation.goodwill, 0f);
			return GenMath.LerpDouble(0f, 100f, 0.5f, 0f, x);
		}
	}
}