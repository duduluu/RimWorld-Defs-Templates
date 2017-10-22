using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class ThinkNode_ConditionalAnyUndownedColonistSpawnedNearby : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.Spawned && pawn.Map.mapPawns.FreeColonistsSpawned.Any((Func<Pawn, bool>)((Pawn x) => !x.Downed));
		}
	}
}