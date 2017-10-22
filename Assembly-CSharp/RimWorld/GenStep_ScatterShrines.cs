using RimWorld.BaseGen;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class GenStep_ScatterShrines : GenStep_ScatterRuinsSimple
	{
		private const int MarginCells = 1;

		private static readonly IntRange ShrinesCountX = new IntRange(1, 4);

		private static readonly IntRange ShrinesCountZ = new IntRange(1, 4);

		private static readonly IntRange ExtraHeightRange = new IntRange(0, 8);

		protected override bool CanScatterAt(IntVec3 c, Map map)
		{
			if (!base.CanScatterAt(c, map))
			{
				return false;
			}
			Building edifice = c.GetEdifice(map);
			if (edifice != null && edifice.def.building.isNaturalRock)
			{
				return true;
			}
			return false;
		}

		protected override void ScatterAt(IntVec3 loc, Map map, int stackCount = 1)
		{
			int randomInRange = GenStep_ScatterShrines.ShrinesCountX.RandomInRange;
			int randomInRange2 = GenStep_ScatterShrines.ShrinesCountZ.RandomInRange;
			int randomInRange3 = GenStep_ScatterShrines.ExtraHeightRange.RandomInRange;
			IntVec2 standardAncientShrineSize = SymbolResolver_AncientShrinesGroup.StandardAncientShrineSize;
			int num = 1;
			int num2 = randomInRange * standardAncientShrineSize.x + (randomInRange - 1) * num;
			int num3 = randomInRange2 * standardAncientShrineSize.z + (randomInRange2 - 1) * num;
			int num4 = num2 + 2;
			int num5 = num3 + 2 + randomInRange3;
			CellRect rect = new CellRect(loc.x, loc.z, num4, num5);
			rect.ClipInsideMap(map);
			if (rect.Width == num4 && rect.Height == num5)
			{
				foreach (IntVec3 cell in rect.Cells)
				{
					List<Thing> list = map.thingGrid.ThingsListAt(cell);
					int num6 = 0;
					while (num6 < list.Count)
					{
						if (list[num6].def != ThingDefOf.AncientCryptosleepCasket)
						{
							num6++;
							continue;
						}
						return;
					}
				}
				if (base.CanPlaceAncientBuildingInRange(rect, map))
				{
					ResolveParams resolveParams = new ResolveParams
					{
						rect = rect,
						disableSinglePawn = new bool?(true),
						disableHives = new bool?(true),
						ancientTempleEntranceHeight = new int?(randomInRange3)
					};
					RimWorld.BaseGen.BaseGen.globalSettings.map = map;
					RimWorld.BaseGen.BaseGen.symbolStack.Push("ancientTemple", resolveParams);
					RimWorld.BaseGen.BaseGen.Generate();
					RectTrigger rectTrigger = (RectTrigger)ThingMaker.MakeThing(ThingDefOf.RectTrigger, null);
					rectTrigger.Rect = rect.ExpandedBy(1).ClipInsideMap(map);
					rectTrigger.letter = LetterMaker.MakeLetter("LetterLabelAncientShrineWarning".Translate(), "AncientShrineWarning".Translate(), LetterDefOf.BadNonUrgent, new TargetInfo(rect.CenterCell, map, false));
					rectTrigger.destroyIfUnfogged = true;
					GenSpawn.Spawn(rectTrigger, rect.CenterCell, map);
				}
			}
		}
	}
}