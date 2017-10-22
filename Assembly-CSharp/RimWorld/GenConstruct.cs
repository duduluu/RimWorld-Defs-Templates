using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class GenConstruct
	{
		public static Blueprint_Build PlaceBlueprintForBuild(BuildableDef sourceDef, IntVec3 center, Map map, Rot4 rotation, Faction faction, ThingDef stuff)
		{
			Blueprint_Build blueprint_Build = (Blueprint_Build)ThingMaker.MakeThing(sourceDef.blueprintDef, null);
			blueprint_Build.SetFactionDirect(faction);
			blueprint_Build.stuffToUse = stuff;
			GenSpawn.Spawn(blueprint_Build, center, map, rotation, false);
			return blueprint_Build;
		}

		public static Blueprint_Install PlaceBlueprintForInstall(MinifiedThing itemToInstall, IntVec3 center, Map map, Rot4 rotation, Faction faction)
		{
			Blueprint_Install blueprint_Install = (Blueprint_Install)ThingMaker.MakeThing(itemToInstall.InnerThing.def.installBlueprintDef, null);
			blueprint_Install.SetThingToInstallFromMinified(itemToInstall);
			blueprint_Install.SetFactionDirect(faction);
			GenSpawn.Spawn(blueprint_Install, center, map, rotation, false);
			return blueprint_Install;
		}

		public static Blueprint_Install PlaceBlueprintForReinstall(Building buildingToReinstall, IntVec3 center, Map map, Rot4 rotation, Faction faction)
		{
			Blueprint_Install blueprint_Install = (Blueprint_Install)ThingMaker.MakeThing(buildingToReinstall.def.installBlueprintDef, null);
			blueprint_Install.SetBuildingToReinstall(buildingToReinstall);
			blueprint_Install.SetFactionDirect(faction);
			GenSpawn.Spawn(blueprint_Install, center, map, rotation, false);
			return blueprint_Install;
		}

		public static bool CanBuildOnTerrain(BuildableDef entDef, IntVec3 c, Map map, Rot4 rot, Thing thingToIgnore = null)
		{
			TerrainDef terrainDef = entDef as TerrainDef;
			if (terrainDef != null && !c.GetTerrain(map).changeable)
			{
				return false;
			}
			CellRect cellRect = GenAdj.OccupiedRect(c, rot, entDef.Size);
			cellRect.ClipInsideMap(map);
			CellRect.CellRectIterator iterator = cellRect.GetIterator();
			while (!iterator.Done())
			{
				TerrainDef terrainDef2 = map.terrainGrid.TerrainAt(iterator.Current);
				if (!terrainDef2.affordances.Contains(entDef.terrainAffordanceNeeded))
				{
					return false;
				}
				List<Thing> thingList = iterator.Current.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					if (thingList[i] != thingToIgnore)
					{
						TerrainDef terrainDef3 = thingList[i].def.entityDefToBuild as TerrainDef;
						if (terrainDef3 != null && !terrainDef3.affordances.Contains(entDef.terrainAffordanceNeeded))
						{
							return false;
						}
					}
				}
				iterator.MoveNext();
			}
			return true;
		}

		public static Thing MiniToInstallOrBuildingToReinstall(Blueprint b)
		{
			Blueprint_Install blueprint_Install = b as Blueprint_Install;
			if (blueprint_Install != null)
			{
				return blueprint_Install.MiniToInstallOrBuildingToReinstall;
			}
			return null;
		}

		public static bool CanConstruct(Thing t, Pawn p, bool forced = false)
		{
			Blueprint blueprint = t as Blueprint;
			if (blueprint != null)
			{
				Thing thingToIgnore = GenConstruct.MiniToInstallOrBuildingToReinstall(blueprint);
				if (blueprint.FirstBlockingThing(p, thingToIgnore, false) != null)
				{
					return false;
				}
			}
			if (!p.CanReserveAndReach(t, PathEndMode.Touch, (!forced) ? p.NormalMaxDanger() : Danger.Deadly, 1, -1, null, forced))
			{
				return false;
			}
			if (t.IsBurning())
			{
				return false;
			}
			return true;
		}

		public static int AmountNeededByOf(IConstructible c, ThingDef resDef)
		{
			List<ThingCountClass>.Enumerator enumerator = c.MaterialsNeeded().GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					ThingCountClass current = enumerator.Current;
					if (current.thingDef == resDef)
					{
						return current.count;
					}
				}
			}
			finally
			{
				((IDisposable)(object)enumerator).Dispose();
			}
			return 0;
		}

		public static AcceptanceReport CanPlaceBlueprintAt(BuildableDef entDef, IntVec3 center, Rot4 rot, Map map, bool godMode = false, Thing thingToIgnore = null)
		{
			CellRect cellRect = GenAdj.OccupiedRect(center, rot, entDef.Size);
			CellRect.CellRectIterator iterator = cellRect.GetIterator();
			while (!iterator.Done())
			{
				IntVec3 current = iterator.Current;
				if (!current.InBounds(map))
				{
					return new AcceptanceReport("OutOfBounds".Translate());
				}
				if (current.InNoBuildEdgeArea(map) && !DebugSettings.godMode)
				{
					return "TooCloseToMapEdge".Translate();
				}
				iterator.MoveNext();
			}
			if (center.Fogged(map))
			{
				return "CannotPlaceInUndiscovered".Translate();
			}
			List<Thing> thingList = center.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing != thingToIgnore && thing.Position == center && thing.Rotation == rot)
				{
					if (thing.def == entDef)
					{
						return new AcceptanceReport("IdenticalThingExists".Translate());
					}
					if (thing.def.entityDefToBuild == entDef)
					{
						if (thing is Blueprint)
						{
							return new AcceptanceReport("IdenticalBlueprintExists".Translate());
						}
						return new AcceptanceReport("IdenticalThingExists".Translate());
					}
				}
			}
			ThingDef thingDef = entDef as ThingDef;
			if (thingDef != null && thingDef.hasInteractionCell)
			{
				IntVec3 c = Thing.InteractionCellWhenAt(thingDef, center, rot, map);
				if (!c.InBounds(map))
				{
					return new AcceptanceReport("InteractionSpotOutOfBounds".Translate());
				}
				List<Thing> list = map.thingGrid.ThingsListAtFast(c);
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j] != thingToIgnore)
					{
						if (list[j].def.passability == Traversability.Impassable)
						{
							return new AcceptanceReport("InteractionSpotBlocked".Translate(list[j].LabelNoCount).CapitalizeFirst());
						}
						Blueprint blueprint = list[j] as Blueprint;
						if (blueprint != null && blueprint.def.entityDefToBuild.passability == Traversability.Impassable)
						{
							return new AcceptanceReport("InteractionSpotWillBeBlocked".Translate(blueprint.LabelNoCount).CapitalizeFirst());
						}
					}
				}
			}
			if (entDef.passability != 0)
			{
				foreach (IntVec3 item in GenAdj.CellsAdjacentCardinal(center, rot, entDef.Size))
				{
					if (item.InBounds(map))
					{
						thingList = item.GetThingList(map);
						for (int k = 0; k < thingList.Count; k++)
						{
							Thing thing2 = thingList[k];
							ThingDef thingDef2;
							if (thing2 != thingToIgnore)
							{
								thingDef2 = null;
								Blueprint blueprint2 = thing2 as Blueprint;
								if (blueprint2 != null)
								{
									ThingDef thingDef3 = blueprint2.def.entityDefToBuild as ThingDef;
									if (thingDef3 != null)
									{
										thingDef2 = thingDef3;
										goto IL_0316;
									}
									continue;
								}
								thingDef2 = thing2.def;
								goto IL_0316;
							}
							continue;
							IL_0316:
							if (thingDef2.hasInteractionCell && cellRect.Contains(Thing.InteractionCellWhenAt(thingDef2, thing2.Position, thing2.Rotation, thing2.Map)))
							{
								return new AcceptanceReport("WouldBlockInteractionSpot".Translate(entDef.label, thingDef2.label).CapitalizeFirst());
							}
						}
					}
				}
			}
			TerrainDef terrainDef = entDef as TerrainDef;
			if (terrainDef != null)
			{
				if (map.terrainGrid.TerrainAt(center) == terrainDef)
				{
					return new AcceptanceReport("TerrainIsAlready".Translate(terrainDef.label));
				}
				if (map.designationManager.DesignationAt(center, DesignationDefOf.SmoothFloor) != null)
				{
					return new AcceptanceReport("SpaceBeingSmoothed".Translate());
				}
			}
			if (!GenConstruct.CanBuildOnTerrain(entDef, center, map, rot, thingToIgnore))
			{
				return new AcceptanceReport("TerrainCannotSupport".Translate());
			}
			if (!godMode)
			{
				CellRect.CellRectIterator iterator2 = cellRect.GetIterator();
				while (!iterator2.Done())
				{
					thingList = iterator2.Current.GetThingList(map);
					for (int l = 0; l < thingList.Count; l++)
					{
						Thing thing3 = thingList[l];
						if (thing3 != thingToIgnore && !GenConstruct.CanPlaceBlueprintOver(entDef, thing3.def))
						{
							return new AcceptanceReport("SpaceAlreadyOccupied".Translate());
						}
					}
					iterator2.MoveNext();
				}
			}
			if (entDef.PlaceWorkers != null)
			{
				for (int m = 0; m < entDef.PlaceWorkers.Count; m++)
				{
					AcceptanceReport result = entDef.PlaceWorkers[m].AllowsPlacing(entDef, center, rot, thingToIgnore);
					if (!result.Accepted)
					{
						return result;
					}
				}
			}
			return AcceptanceReport.WasAccepted;
		}

		public static BuildableDef BuiltDefOf(ThingDef def)
		{
			return (def.entityDefToBuild == null) ? def : def.entityDefToBuild;
		}

		public static bool CanPlaceBlueprintOver(BuildableDef newDef, ThingDef oldDef)
		{
			if (oldDef.EverHaulable)
			{
				return true;
			}
			TerrainDef terrainDef = newDef as TerrainDef;
			if (terrainDef != null)
			{
				if (oldDef.category == ThingCategory.Building && !terrainDef.affordances.Contains(oldDef.terrainAffordanceNeeded))
				{
					return false;
				}
				if ((oldDef.IsBlueprint || oldDef.IsFrame) && !terrainDef.affordances.Contains(oldDef.entityDefToBuild.terrainAffordanceNeeded))
				{
					return false;
				}
			}
			ThingDef thingDef = newDef as ThingDef;
			BuildableDef buildableDef = GenConstruct.BuiltDefOf(oldDef);
			ThingDef thingDef2 = buildableDef as ThingDef;
			if (oldDef == ThingDefOf.SteamGeyser && !newDef.ForceAllowPlaceOver(oldDef))
			{
				return false;
			}
			if (oldDef.category == ThingCategory.Plant && oldDef.passability == Traversability.Impassable && thingDef != null && thingDef.category == ThingCategory.Building && !thingDef.building.canPlaceOverImpassablePlant)
			{
				return false;
			}
			if (oldDef.category != ThingCategory.Building && !oldDef.IsBlueprint && !oldDef.IsFrame)
			{
				return true;
			}
			if (thingDef != null)
			{
				if (!thingDef.IsEdifice())
				{
					if (oldDef.building != null && !oldDef.building.canBuildNonEdificesUnder)
					{
						return false;
					}
					if (thingDef.EverTransmitsPower && oldDef.EverTransmitsPower)
					{
						return false;
					}
					return true;
				}
				if (thingDef.IsEdifice() && oldDef != null && oldDef.category == ThingCategory.Building && !oldDef.IsEdifice())
				{
					if (thingDef.building != null && !thingDef.building.canBuildNonEdificesUnder)
					{
						return false;
					}
					return true;
				}
				if (thingDef2 != null && thingDef2 == ThingDefOf.Wall && thingDef.building != null && thingDef.building.canPlaceOverWall)
				{
					return true;
				}
				if (newDef != ThingDefOf.PowerConduit && buildableDef == ThingDefOf.PowerConduit)
				{
					return true;
				}
			}
			if (newDef is TerrainDef && buildableDef is ThingDef && ((ThingDef)buildableDef).CoexistsWithFloors)
			{
				return true;
			}
			if (buildableDef is TerrainDef && !(newDef is TerrainDef))
			{
				return true;
			}
			return false;
		}

		public static bool BlocksFramePlacement(Blueprint blue, Thing t)
		{
			if (t.def.category == ThingCategory.Plant)
			{
				if (t.def.plant.harvestWork >= 200.0)
				{
					return true;
				}
				return false;
			}
			if (((!(blue.def.entityDefToBuild is TerrainDef)) ? blue.def.entityDefToBuild.passability : Traversability.Standable) != 0)
			{
				if (t.def == ThingDefOf.SteamGeyser && blue.def.entityDefToBuild.ForceAllowPlaceOver(t.def))
				{
					return false;
				}
				ThingDef thingDef = blue.def.entityDefToBuild as ThingDef;
				if (thingDef != null)
				{
					if (thingDef.EverTransmitsPower && t.def == ThingDefOf.PowerConduit && thingDef != ThingDefOf.PowerConduit)
					{
						return false;
					}
					if (t.def == ThingDefOf.Wall && thingDef.building != null && thingDef.building.canPlaceOverWall)
					{
						return false;
					}
				}
				if (t.def.IsEdifice() && thingDef.IsEdifice())
				{
					return true;
				}
				if (t.def.category != ThingCategory.Pawn && (t.def.category != ThingCategory.Item || blue.def.entityDefToBuild.passability != Traversability.Impassable))
				{
					if ((int)t.def.Fillage >= 1 && thingDef != null && (int)thingDef.Fillage >= 1)
					{
						return true;
					}
					return false;
				}
				return true;
			}
			return false;
		}

		public static bool TerrainCanSupport(CellRect rect, Map map, ThingDef thing)
		{
			CellRect.CellRectIterator iterator = rect.GetIterator();
			while (!iterator.Done())
			{
				if (!iterator.Current.SupportsStructureType(map, thing.terrainAffordanceNeeded))
				{
					return false;
				}
				iterator.MoveNext();
			}
			return true;
		}
	}
}