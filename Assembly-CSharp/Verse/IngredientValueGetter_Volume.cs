using System;
using System.Linq;

namespace Verse
{
	public class IngredientValueGetter_Volume : IngredientValueGetter
	{
		public override float ValuePerUnitOf(ThingDef t)
		{
			if (t.IsStuff)
			{
				return t.VolumePerUnit;
			}
			return 1f;
		}

		public override string BillRequirementsDescription(RecipeDef r, IngredientCount ing)
		{
			if (ing.filter.AllowedThingDefs.Any((Func<ThingDef, bool>)((ThingDef td) => td.smallVolume)) && !ing.filter.AllowedThingDefs.Any((Func<ThingDef, bool>)((ThingDef td) => td.smallVolume && !r.GetPremultipliedSmallIngredients().Contains(td))))
			{
				return "BillRequires".Translate((float)(ing.GetBaseCount() * 10.0), ing.filter.Summary);
			}
			return "BillRequires".Translate(ing.GetBaseCount(), ing.filter.Summary);
		}

		public override string ExtraDescriptionLine(RecipeDef r)
		{
			if (r.ingredients.Any((Predicate<IngredientCount>)((IngredientCount ing) => ing.filter.AllowedThingDefs.Any((Func<ThingDef, bool>)((ThingDef td) => td.smallVolume && !r.GetPremultipliedSmallIngredients().Contains(td))))))
			{
				return "BillRequiresMayVary".Translate(10.ToStringCached());
			}
			return (string)null;
		}
	}
}