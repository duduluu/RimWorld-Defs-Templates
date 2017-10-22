using System;
using System.Collections.Generic;

namespace Verse
{
	public abstract class ChoiceLetter : LetterWithTimeout
	{
		public string title;

		public string text;

		public bool radioMode;

		protected abstract IEnumerable<DiaOption> Choices
		{
			get;
		}

		protected DiaOption Reject
		{
			get
			{
				DiaOption diaOption = new DiaOption("RejectLetter".Translate());
				diaOption.action = (Action)delegate
				{
					Find.LetterStack.RemoveLetter(this);
				};
				diaOption.resolveTree = true;
				return diaOption;
			}
		}

		protected DiaOption Postpone
		{
			get
			{
				DiaOption diaOption = new DiaOption("PostponeLetter".Translate());
				diaOption.resolveTree = true;
				if (base.TimeoutActive && base.disappearAtTick <= Find.TickManager.TicksGame + 1)
				{
					diaOption.Disable((string)null);
				}
				return diaOption;
			}
		}

		protected DiaOption OK
		{
			get
			{
				DiaOption diaOption = new DiaOption("OK".Translate());
				diaOption.action = (Action)delegate
				{
					Find.LetterStack.RemoveLetter(this);
				};
				diaOption.resolveTree = true;
				return diaOption;
			}
		}

		protected DiaOption JumpToLocation
		{
			get
			{
				DiaOption diaOption = new DiaOption("JumpToLocation".Translate());
				diaOption.action = (Action)delegate
				{
					CameraJumper.TryJumpAndSelect(base.lookTarget);
					Find.LetterStack.RemoveLetter(this);
				};
				diaOption.resolveTree = true;
				if (!base.lookTarget.IsValid)
				{
					diaOption.Disable((string)null);
				}
				return diaOption;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<string>(ref this.title, "title", (string)null, false);
			Scribe_Values.Look<string>(ref this.text, "text", (string)null, false);
			Scribe_Values.Look<bool>(ref this.radioMode, "radioMode", false, false);
		}

		protected override string GetMouseoverText()
		{
			return this.text;
		}

		public override void OpenLetter()
		{
			DiaNode diaNode = new DiaNode(this.text);
			diaNode.options.AddRange(this.Choices);
			WindowStack windowStack = Find.WindowStack;
			bool flag = this.radioMode;
			windowStack.Add(new Dialog_NodeTree(diaNode, false, flag, this.title));
		}
	}
}