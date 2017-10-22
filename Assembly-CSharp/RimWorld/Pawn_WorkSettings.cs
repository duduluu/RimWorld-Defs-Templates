using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Pawn_WorkSettings : IExposable
	{
		public const int LowestPriority = 4;

		public const int DefaultPriority = 3;

		private const int MaxInitialActiveWorks = 6;

		private Pawn pawn;

		private DefMap<WorkTypeDef, int> priorities;

		private bool workGiversDirty = true;

		private List<WorkGiver> workGiversInOrderEmerg = new List<WorkGiver>();

		private List<WorkGiver> workGiversInOrderNormal = new List<WorkGiver>();

		private static List<WorkTypeDef> wtsByPrio = new List<WorkTypeDef>();

		public bool EverWork
		{
			get
			{
				return this.priorities != null;
			}
		}

		public List<WorkGiver> WorkGiversInOrderNormal
		{
			get
			{
				if (this.workGiversDirty)
				{
					this.CacheWorkGiversInOrder();
				}
				return this.workGiversInOrderNormal;
			}
		}

		public List<WorkGiver> WorkGiversInOrderEmergency
		{
			get
			{
				if (this.workGiversDirty)
				{
					this.CacheWorkGiversInOrder();
				}
				return this.workGiversInOrderEmerg;
			}
		}

		public Pawn_WorkSettings()
		{
		}

		public Pawn_WorkSettings(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_Deep.Look<DefMap<WorkTypeDef, int>>(ref this.priorities, "priorities", new object[0]);
		}

		public void EnableAndInitialize()
		{
			if (this.priorities == null)
			{
				this.priorities = new DefMap<WorkTypeDef, int>();
			}
			this.priorities.SetAll(0);
			int num = 0;
			foreach (WorkTypeDef item in from w in DefDatabase<WorkTypeDef>.AllDefs
			where !w.alwaysStartActive && !this.pawn.story.WorkTypeIsDisabled(w)
			orderby this.pawn.skills.AverageOfRelevantSkillsFor(w) descending
			select w)
			{
				this.SetPriority(item, 3);
				num++;
				if (num >= 6)
					break;
			}
			foreach (WorkTypeDef item2 in from w in DefDatabase<WorkTypeDef>.AllDefs
			where w.alwaysStartActive
			select w)
			{
				if (!this.pawn.story.WorkTypeIsDisabled(item2))
				{
					this.SetPriority(item2, 3);
				}
			}
			List<WorkTypeDef>.Enumerator enumerator3 = this.pawn.story.DisabledWorkTypes.GetEnumerator();
			try
			{
				while (enumerator3.MoveNext())
				{
					WorkTypeDef current3 = enumerator3.Current;
					this.Disable(current3);
				}
			}
			finally
			{
				((IDisposable)(object)enumerator3).Dispose();
			}
		}

		private void ConfirmInitializedDebug()
		{
			if (this.priorities == null)
			{
				Log.Error(this.pawn + " did not have work settings initialized.");
				this.EnableAndInitialize();
			}
		}

		public void SetPriority(WorkTypeDef w, int priority)
		{
			this.ConfirmInitializedDebug();
			if (priority != 0 && this.pawn.story.WorkTypeIsDisabled(w))
			{
				Log.Error("Tried to change priority on disabled worktype " + w + " for pawn " + this.pawn);
			}
			else
			{
				if (priority < 0 || priority > 4)
				{
					Log.Message("Trying to set work to invalid priority " + priority);
				}
				this.priorities[w] = priority;
				if (priority == 0)
				{
					this.pawn.mindState.Notify_WorkPriorityDisabled(w);
				}
				this.workGiversDirty = true;
			}
		}

		public int GetPriority(WorkTypeDef w)
		{
			this.ConfirmInitializedDebug();
			int num = this.priorities[w];
			if (num > 0 && !Find.PlaySettings.useWorkPriorities)
			{
				return 3;
			}
			return num;
		}

		public bool WorkIsActive(WorkTypeDef w)
		{
			this.ConfirmInitializedDebug();
			return this.GetPriority(w) > 0;
		}

		public void Disable(WorkTypeDef w)
		{
			this.ConfirmInitializedDebug();
			this.SetPriority(w, 0);
		}

		public void DisableAll()
		{
			this.ConfirmInitializedDebug();
			this.priorities.SetAll(0);
			this.workGiversDirty = true;
		}

		public void Notify_UseWorkPrioritiesChanged()
		{
			this.workGiversDirty = true;
		}

		public void Notify_GainedTrait()
		{
			if (this.priorities != null)
			{
				List<WorkTypeDef>.Enumerator enumerator = this.pawn.story.DisabledWorkTypes.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						WorkTypeDef current = enumerator.Current;
						this.Disable(current);
					}
				}
				finally
				{
					((IDisposable)(object)enumerator).Dispose();
				}
			}
		}

		private void CacheWorkGiversInOrder()
		{
			Pawn_WorkSettings.wtsByPrio.Clear();
			List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
			int num = 999;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				WorkTypeDef workTypeDef = allDefsListForReading[i];
				int priority = this.GetPriority(workTypeDef);
				if (priority > 0)
				{
					if (priority < num && workTypeDef.workGiversByPriority.Any((Predicate<WorkGiverDef>)((WorkGiverDef wg) => !wg.emergency)))
					{
						num = priority;
					}
					Pawn_WorkSettings.wtsByPrio.Add(workTypeDef);
				}
			}
			Pawn_WorkSettings.wtsByPrio.InsertionSort((Comparison<WorkTypeDef>)delegate(WorkTypeDef a, WorkTypeDef b)
			{
				float value = (float)(a.naturalPriority + (4 - this.GetPriority(a)) * 100000);
				return ((float)(b.naturalPriority + (4 - this.GetPriority(b)) * 100000)).CompareTo(value);
			});
			this.workGiversInOrderEmerg.Clear();
			for (int j = 0; j < Pawn_WorkSettings.wtsByPrio.Count; j++)
			{
				WorkTypeDef workTypeDef2 = Pawn_WorkSettings.wtsByPrio[j];
				for (int k = 0; k < workTypeDef2.workGiversByPriority.Count; k++)
				{
					WorkGiver worker = workTypeDef2.workGiversByPriority[k].Worker;
					if (worker.def.emergency && this.GetPriority(worker.def.workType) <= num)
					{
						this.workGiversInOrderEmerg.Add(worker);
					}
				}
			}
			this.workGiversInOrderNormal.Clear();
			for (int l = 0; l < Pawn_WorkSettings.wtsByPrio.Count; l++)
			{
				WorkTypeDef workTypeDef3 = Pawn_WorkSettings.wtsByPrio[l];
				for (int m = 0; m < workTypeDef3.workGiversByPriority.Count; m++)
				{
					WorkGiver worker2 = workTypeDef3.workGiversByPriority[m].Worker;
					if (!worker2.def.emergency || this.GetPriority(worker2.def.workType) > num)
					{
						this.workGiversInOrderNormal.Add(worker2);
					}
				}
			}
			this.workGiversDirty = false;
		}

		public string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("WorkSettings for " + this.pawn);
			stringBuilder.AppendLine("Cached emergency WorkGivers in order:");
			for (int i = 0; i < this.WorkGiversInOrderEmergency.Count; i++)
			{
				stringBuilder.AppendLine("   " + i + ": " + this.DebugStringFor(this.WorkGiversInOrderEmergency[i].def));
			}
			stringBuilder.AppendLine("Cached normal WorkGivers in order:");
			for (int j = 0; j < this.WorkGiversInOrderNormal.Count; j++)
			{
				stringBuilder.AppendLine("   " + j + ": " + this.DebugStringFor(this.WorkGiversInOrderNormal[j].def));
			}
			return stringBuilder.ToString();
		}

		private string DebugStringFor(WorkGiverDef wg)
		{
			return "[" + this.GetPriority(wg.workType) + " " + wg.workType.defName + "] - " + wg.defName + " (" + wg.priorityInType + ")";
		}
	}
}