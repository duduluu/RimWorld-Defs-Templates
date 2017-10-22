using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public abstract class BuildableDef : Def
	{
		public List<StatModifier> statBases;

		public Traversability passability;

		public int pathCost;

		public bool pathCostIgnoreRepeat = true;

		public float fertility = -1f;

		public List<ThingCountClass> costList;

		public int costStuffCount = -1;

		public List<StuffCategoryDef> stuffCategories;

		public TerrainAffordance terrainAffordanceNeeded = TerrainAffordance.Light;

		public List<ThingDef> buildingPrerequisites;

		public List<ResearchProjectDef> researchPrerequisites;

		public int placingDraggableDimensions;

		public EffecterDef repairEffect;

		public EffecterDef constructEffect;

		public Rot4 defaultPlacingRot = Rot4.North;

		public float resourcesFractionWhenDeconstructed = 0.75f;

		[Unsaved]
		public ThingDef blueprintDef;

		[Unsaved]
		public ThingDef installBlueprintDef;

		[Unsaved]
		public ThingDef frameDef;

		public string uiIconPath;

		public AltitudeLayer altitudeLayer = AltitudeLayer.Item;

		[Unsaved]
		public Texture2D uiIcon = BaseContent.BadTex;

		[Unsaved]
		public Graphic graphic = BaseContent.BadGraphic;

		public bool menuHidden;

		public float specialDisplayRadius;

		public List<Type> placeWorkers;

		[NoTranslate]
		public DesignationCategoryDef designationCategory;

		public KeyBindingDef designationHotKey;

		[Unsaved]
		private List<PlaceWorker> placeWorkersInstantiatedInt;

		public virtual IntVec2 Size
		{
			get
			{
				return new IntVec2(1, 1);
			}
		}

		public bool MadeFromStuff
		{
			get
			{
				return !this.stuffCategories.NullOrEmpty();
			}
		}

		public abstract Color IconDrawColor
		{
			get;
		}

		public Material DrawMatSingle
		{
			get
			{
				if (this.graphic == null)
				{
					return null;
				}
				return this.graphic.MatSingle;
			}
		}

		public float Altitude
		{
			get
			{
				return Altitudes.AltitudeFor(this.altitudeLayer);
			}
		}

		public List<PlaceWorker> PlaceWorkers
		{
			get
			{
				if (this.placeWorkers == null)
				{
					return null;
				}
				this.placeWorkersInstantiatedInt = new List<PlaceWorker>();
				List<Type>.Enumerator enumerator = this.placeWorkers.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Type current = enumerator.Current;
						this.placeWorkersInstantiatedInt.Add((PlaceWorker)Activator.CreateInstance(current));
					}
				}
				finally
				{
					((IDisposable)(object)enumerator).Dispose();
				}
				return this.placeWorkersInstantiatedInt;
			}
		}

		public bool ForceAllowPlaceOver(BuildableDef other)
		{
			if (this.PlaceWorkers == null)
			{
				return false;
			}
			for (int i = 0; i < this.PlaceWorkers.Count; i++)
			{
				if (this.PlaceWorkers[i].ForceAllowPlaceOver(other))
				{
					return true;
				}
			}
			return false;
		}

		public override void PostLoad()
		{
			base.PostLoad();
			LongEventHandler.ExecuteWhenFinished((Action)delegate
			{
				if (!this.uiIconPath.NullOrEmpty())
				{
					this.uiIcon = ContentFinder<Texture2D>.Get(this.uiIconPath, true);
				}
				else if ((UnityEngine.Object)this.DrawMatSingle != (UnityEngine.Object)null && (UnityEngine.Object)this.DrawMatSingle != (UnityEngine.Object)BaseContent.BadMat)
				{
					this.uiIcon = (Texture2D)this.DrawMatSingle.mainTexture;
				}
			});
		}

		public override void ResolveReferences()
		{
			base.ResolveReferences();
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
		}

		public override string ToString()
		{
			return base.defName;
		}

		public override int GetHashCode()
		{
			return base.defName.GetHashCode();
		}
	}
}