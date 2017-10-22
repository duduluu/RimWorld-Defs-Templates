using Verse;

namespace RimWorld.Planet
{
	public class CaravanArrivalAction_Enter : CaravanArrivalAction
	{
		private MapParent mapParent;

		public override string ReportString
		{
			get
			{
				return "CaravanEntering".Translate(this.mapParent.Label);
			}
		}

		public override bool ShouldFail
		{
			get
			{
				return base.ShouldFail || this.mapParent == null || !this.mapParent.Spawned;
			}
		}

		public CaravanArrivalAction_Enter()
		{
		}

		public CaravanArrivalAction_Enter(MapParent mapParent)
		{
			this.mapParent = mapParent;
		}

		public override void Arrived(Caravan caravan)
		{
			Map map = this.mapParent.Map;
			if (map != null)
			{
				Pawn t = caravan.PawnsListForReading[0];
				CaravanDropInventoryMode dropInventoryMode = (CaravanDropInventoryMode)(map.IsPlayerHome ? 2 : 0);
				bool draftColonists = this.mapParent.Faction != null && this.mapParent.Faction.HostileTo(Faction.OfPlayer);
				CaravanEnterMapUtility.Enter(caravan, map, CaravanEnterMode.Edge, dropInventoryMode, draftColonists, null);
				if (this.mapParent.def == WorldObjectDefOf.Ambush)
				{
					Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
					Find.LetterStack.ReceiveLetter("LetterLabelCaravanEnteredAmbushMap".Translate(), "LetterCaravanEnteredAmbushMap".Translate(caravan.Label).CapitalizeFirst(), LetterDefOf.Good, (Thing)t, (string)null);
				}
				else
				{
					if (!caravan.IsPlayerControlled && this.mapParent.Faction != Faction.OfPlayer)
						return;
					Messages.Message("MessageCaravanEnteredWorldObject".Translate(caravan.Label, this.mapParent.Label).CapitalizeFirst(), (Thing)t, MessageSound.Benefit);
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<MapParent>(ref this.mapParent, "mapParent", false);
		}
	}
}