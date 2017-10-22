using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class TaleData_Thing : TaleData
	{
		public int thingID;

		public ThingDef thingDef;

		public ThingDef stuff;

		public string title;

		public QualityCategory quality;

		public override void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.thingID, "thingID", 0, false);
			Scribe_Defs.Look<ThingDef>(ref this.thingDef, "thingDef");
			Scribe_Defs.Look<ThingDef>(ref this.stuff, "stuff");
			Scribe_Values.Look<string>(ref this.title, "title", (string)null, false);
			Scribe_Values.Look<QualityCategory>(ref this.quality, "quality", QualityCategory.Awful, false);
		}

		public override IEnumerable<Rule> GetRules(string prefix)
		{
			yield return (Rule)new Rule_String(prefix + "_label", this.thingDef.label);
			yield return (Rule)new Rule_String(prefix + "_labelDefinite", Find.ActiveLanguageWorker.WithDefiniteArticle(this.thingDef.label));
			yield return (Rule)new Rule_String(prefix + "_labelIndefinite", Find.ActiveLanguageWorker.WithIndefiniteArticle(this.thingDef.label));
			if (this.stuff != null)
			{
				yield return (Rule)new Rule_String(prefix + "_stuffLabel", this.stuff.label);
			}
			if (this.title != null)
			{
				yield return (Rule)new Rule_String(prefix + "_title", this.title);
			}
			yield return (Rule)new Rule_String(prefix + "_quality", this.quality.GetLabel());
		}

		public static TaleData_Thing GenerateFrom(Thing t)
		{
			TaleData_Thing taleData_Thing = new TaleData_Thing();
			taleData_Thing.thingID = t.thingIDNumber;
			taleData_Thing.thingDef = t.def;
			taleData_Thing.stuff = t.Stuff;
			t.TryGetQuality(out taleData_Thing.quality);
			CompArt compArt = t.TryGetComp<CompArt>();
			if (compArt != null && compArt.Active)
			{
				taleData_Thing.title = compArt.Title;
			}
			return taleData_Thing;
		}

		public static TaleData_Thing GenerateRandom()
		{
			ThingDef thingDef = (from d in DefDatabase<ThingDef>.AllDefs
			where d.comps != null && d.comps.Any((Predicate<CompProperties>)((CompProperties cp) => cp.compClass == typeof(CompArt)))
			select d).RandomElement();
			ThingDef thingDef2 = GenStuff.RandomStuffFor(thingDef);
			Thing thing = ThingMaker.MakeThing(thingDef, thingDef2);
			ArtGenerationContext source = (ArtGenerationContext)((Rand.Value < 0.5) ? 1 : 0);
			CompQuality compQuality = thing.TryGetComp<CompQuality>();
			if (compQuality != null && (int)compQuality.Quality < (int)thing.TryGetComp<CompArt>().Props.minQualityForArtistic)
			{
				compQuality.SetQuality(thing.TryGetComp<CompArt>().Props.minQualityForArtistic, source);
			}
			thing.TryGetComp<CompArt>().InitializeArt(source);
			return TaleData_Thing.GenerateFrom(thing);
		}
	}
}