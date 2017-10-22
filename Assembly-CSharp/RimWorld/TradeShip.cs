using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class TradeShip : PassingShip, ITrader, IThingHolder
	{
		public TraderKindDef def;

		private ThingOwner things;

		private List<Pawn> soldPrisoners = new List<Pawn>();

		private int randomPriceFactorSeed = -1;

		private static List<string> tmpExtantNames = new List<string>();

		public override string FullTitle
		{
			get
			{
				return base.name + " (" + this.def.label + ")";
			}
		}

		public int Silver
		{
			get
			{
				return this.CountHeldOf(ThingDefOf.Silver, null);
			}
		}

		public IThingHolder ParentHolder
		{
			get
			{
				return base.Map;
			}
		}

		public TraderKindDef TraderKind
		{
			get
			{
				return this.def;
			}
		}

		public int RandomPriceFactorSeed
		{
			get
			{
				return this.randomPriceFactorSeed;
			}
		}

		public string TraderName
		{
			get
			{
				return base.name;
			}
		}

		public bool CanTradeNow
		{
			get
			{
				return !base.Departed;
			}
		}

		public float TradePriceImprovementOffsetForPlayer
		{
			get
			{
				return 0f;
			}
		}

		public Faction Faction
		{
			get
			{
				return null;
			}
		}

		public IEnumerable<Thing> Goods
		{
			get
			{
				for (int i = 0; i < this.things.Count; i++)
				{
					Pawn p = this.things[i] as Pawn;
					if (p == null || !this.soldPrisoners.Contains(p))
					{
						yield return this.things[i];
					}
				}
			}
		}

		public TradeShip()
		{
		}

		public TradeShip(TraderKindDef def)
		{
			this.def = def;
			this.things = new ThingOwner<Thing>(this);
			TradeShip.tmpExtantNames.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				TradeShip.tmpExtantNames.AddRange(from x in maps[i].passingShipManager.passingShips
				select x.name);
			}
			base.name = NameGenerator.GenerateName(RulePackDefOf.NamerTraderGeneral, TradeShip.tmpExtantNames, false);
			this.randomPriceFactorSeed = Rand.RangeInclusive(1, 10000000);
			base.loadID = Find.World.uniqueIDsManager.GetNextPassingShipID();
		}

		public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
		{
			foreach (Thing item in TradeUtility.AllLaunchableThings(base.Map))
			{
				yield return item;
			}
			foreach (Pawn item2 in TradeUtility.AllSellableColonyPawns(base.Map))
			{
				yield return (Thing)item2;
			}
		}

		public void GenerateThings()
		{
			ItemCollectionGeneratorParams parms = new ItemCollectionGeneratorParams
			{
				traderDef = this.def,
				forTile = base.Map.Tile
			};
			this.things.TryAddRange(ItemCollectionGeneratorDefOf.TraderStock.Worker.Generate(parms), true);
		}

		public override void PassingShipTick()
		{
			base.PassingShipTick();
			for (int num = this.things.Count - 1; num >= 0; num--)
			{
				Pawn pawn = this.things[num] as Pawn;
				if (pawn != null)
				{
					pawn.Tick();
					if (pawn.Dead)
					{
						this.things.Remove(pawn);
					}
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look<TraderKindDef>(ref this.def, "def");
			Scribe_Deep.Look<ThingOwner>(ref this.things, "things", new object[1]
			{
				this
			});
			Scribe_Collections.Look<Pawn>(ref this.soldPrisoners, "soldPrisoners", LookMode.Reference, new object[0]);
			Scribe_Values.Look<int>(ref this.randomPriceFactorSeed, "randomPriceFactorSeed", 0, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.soldPrisoners.RemoveAll((Predicate<Pawn>)((Pawn x) => x == null));
			}
		}

		public override void TryOpenComms(Pawn negotiator)
		{
			if (this.CanTradeNow)
			{
				Find.WindowStack.Add(new Dialog_Trade(negotiator, this));
				LessonAutoActivator.TeachOpportunity(ConceptDefOf.BuildOrbitalTradeBeacon, OpportunityType.Critical);
				string empty = string.Empty;
				string empty2 = string.Empty;
				PawnRelationUtility.Notify_PawnsSeenByPlayer(this.Goods.OfType<Pawn>(), ref empty, ref empty2, "LetterRelatedPawnsTradeShip".Translate(), false);
				if (!empty2.NullOrEmpty())
				{
					Find.LetterStack.ReceiveLetter(empty, empty2, LetterDefOf.Good, (string)null);
				}
				TutorUtility.DoModalDialogIfNotKnown(ConceptDefOf.TradeGoodsMustBeNearBeacon);
			}
		}

		public override void Depart()
		{
			base.Depart();
			this.things.ClearAndDestroyContentsOrPassToWorld(DestroyMode.Vanish);
			this.soldPrisoners.Clear();
		}

		public override string GetCallLabel()
		{
			return base.name + " (" + this.def.label + ")";
		}

		public int CountHeldOf(ThingDef thingDef, ThingDef stuffDef = null)
		{
			Thing thing = this.HeldThingMatching(thingDef, stuffDef);
			if (thing != null)
			{
				return thing.stackCount;
			}
			return 0;
		}

		public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			Thing thing = toGive.SplitOff(countToGive);
			thing.PreTraded(TradeAction.PlayerSells, playerNegotiator, this);
			Thing thing2 = TradeUtility.ThingFromStockToMergeWith(this, thing);
			if (thing2 != null)
			{
				if (!thing2.TryAbsorbStack(thing, false))
				{
					thing.Destroy(DestroyMode.Vanish);
				}
			}
			else
			{
				Pawn pawn = thing as Pawn;
				if (pawn != null && pawn.RaceProps.Humanlike)
				{
					this.soldPrisoners.Add(pawn);
				}
				this.things.TryAdd(thing, false);
			}
		}

		public void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			Thing thing = toGive.SplitOff(countToGive);
			thing.PreTraded(TradeAction.PlayerBuys, playerNegotiator, this);
			Pawn pawn = thing as Pawn;
			if (pawn != null)
			{
				this.soldPrisoners.Remove(pawn);
			}
			TradeUtility.SpawnDropPod(DropCellFinder.TradeDropSpot(base.Map), base.Map, thing);
		}

		private Thing HeldThingMatching(ThingDef thingDef, ThingDef stuffDef)
		{
			for (int i = 0; i < this.things.Count; i++)
			{
				if (this.things[i].def == thingDef && this.things[i].Stuff == stuffDef)
				{
					return this.things[i];
				}
			}
			return null;
		}

		public void ChangeCountHeldOf(ThingDef thingDef, ThingDef stuffDef, int count)
		{
			Thing thing = this.HeldThingMatching(thingDef, stuffDef);
			if (thing == null)
			{
				Log.Error("Changing count of thing trader doesn't have: " + thingDef);
			}
			thing.stackCount += count;
		}

		public override string ToString()
		{
			return this.FullTitle;
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return this.things;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
		}
	}
}