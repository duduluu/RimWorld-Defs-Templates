using System;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Building_MarriageSpot : Building
	{
		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			if (stringBuilder.Length != 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(this.UsableNowStatus());
			return stringBuilder.ToString();
		}

		private string UsableNowStatus()
		{
			if (!this.AnyCoupleForWhichIsValid())
			{
				StringBuilder stringBuilder = new StringBuilder();
				Pair<Pawn, Pawn> pair = default(Pair<Pawn, Pawn>);
				if (this.TryFindAnyFiancesCouple(out pair))
				{
					if (!MarriageSpotUtility.IsValidMarriageSpotFor(base.Position, pair.First, pair.Second, stringBuilder))
					{
						return "MarriageSpotNotUsable".Translate(stringBuilder);
					}
				}
				else if (!MarriageSpotUtility.IsValidMarriageSpot(base.Position, base.Map, stringBuilder))
				{
					return "MarriageSpotNotUsable".Translate(stringBuilder);
				}
			}
			return "MarriageSpotUsable".Translate();
		}

		private bool AnyCoupleForWhichIsValid()
		{
			return base.Map.mapPawns.FreeColonistsSpawned.Any((Func<Pawn, bool>)delegate(Pawn p)
			{
				Pawn firstDirectRelationPawn = p.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, (Predicate<Pawn>)((Pawn x) => x.Spawned));
				return firstDirectRelationPawn != null && MarriageSpotUtility.IsValidMarriageSpotFor(base.Position, p, firstDirectRelationPawn, null);
			});
		}

		private bool TryFindAnyFiancesCouple(out Pair<Pawn, Pawn> fiances)
		{
			foreach (Pawn item in base.Map.mapPawns.FreeColonistsSpawned)
			{
				Pawn firstDirectRelationPawn = item.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, (Predicate<Pawn>)((Pawn x) => x.Spawned));
				if (firstDirectRelationPawn != null)
				{
					fiances = new Pair<Pawn, Pawn>(item, firstDirectRelationPawn);
					return true;
				}
			}
			fiances = default(Pair<Pawn, Pawn>);
			return false;
		}
	}
}