using System.Xml;

namespace Verse
{
	public static class Scribe_References
	{
		public static void Look<T>(ref T refee, string label, bool saveDestroyedThings = false) where T : ILoadReferenceable
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (refee == null)
				{
					Scribe.saver.WriteElement(label, "null");
				}
				else
				{
					Thing thing = ((object)refee) as Thing;
					if (thing != null && Scribe_References.CheckSaveReferenceToDestroyedThing(thing, label, saveDestroyedThings))
						return;
					if (UnityData.isDebugBuild && thing != null)
					{
						if (!thing.def.HasThingIDNumber)
						{
							Log.Error("Trying to cross-reference save Thing which lacks ID number: " + refee);
							Scribe.saver.WriteElement(label, "null");
							return;
						}
						if (thing.IsSaveCompressible())
						{
							Log.Error("Trying to save a reference to a thing that will be compressed away: " + refee);
							Scribe.saver.WriteElement(label, "null");
							return;
						}
					}
					string uniqueLoadID = refee.GetUniqueLoadID();
					Scribe.saver.WriteElement(label, uniqueLoadID);
					Scribe.saver.loadIDsErrorsChecker.RegisterReferenced((ILoadReferenceable)(object)refee, label);
				}
			}
			else if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				if (Scribe.loader.curParent != null && Scribe.loader.curParent.GetType().IsValueType)
				{
					Log.Warning("Trying to load reference of an object of type " + typeof(T) + " with label " + label + ", but our current node is a value type. The reference won't be loaded properly. curParent=" + Scribe.loader.curParent);
				}
				XmlNode xmlNode = Scribe.loader.curXmlParent[label];
				string targetLoadID = (xmlNode == null) ? null : xmlNode.InnerText;
				Scribe.loader.crossRefs.loadIDs.RegisterLoadIDReadFromXml(targetLoadID, typeof(T), label);
			}
			else if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				refee = Scribe.loader.crossRefs.TakeResolvedRef<T>(label);
			}
		}

		public static bool CheckSaveReferenceToDestroyedThing(Thing th, string label, bool saveDestroyedThings)
		{
			if (!th.Destroyed)
			{
				return false;
			}
			if (!saveDestroyedThings)
			{
				Scribe.saver.WriteElement(label, "null");
				return true;
			}
			if (th.Discarded)
			{
				Log.Warning("Trying to save reference to a discarded thing " + th + " with saveDestroyedThings=true. This means that it's not deep-saved anywhere and is no longer managed by anything in the code, so saving its reference will always fail. , label=" + label);
				Scribe.saver.WriteElement(label, "null");
				return true;
			}
			return false;
		}
	}
}