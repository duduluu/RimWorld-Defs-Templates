using System;

namespace Verse
{
	public class Dialog_RenameZone : Dialog_Rename
	{
		private Zone zone;

		public Dialog_RenameZone(Zone zone)
		{
			this.zone = zone;
			base.curName = zone.label;
		}

		protected override AcceptanceReport NameIsValid(string name)
		{
			AcceptanceReport result = base.NameIsValid(name);
			if (!result.Accepted)
			{
				return result;
			}
			if (this.zone.Map.zoneManager.AllZones.Any((Predicate<Zone>)((Zone z) => z.label == name)))
			{
				return "NameIsInUse".Translate();
			}
			return true;
		}

		protected override void SetName(string name)
		{
			this.zone.label = base.curName;
			Messages.Message("ZoneGainsName".Translate(base.curName), MessageSound.Benefit);
		}
	}
}