using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class DrugPolicyDatabase : IExposable
	{
		private List<DrugPolicy> policies = new List<DrugPolicy>();

		public List<DrugPolicy> AllPolicies
		{
			get
			{
				return this.policies;
			}
		}

		public DrugPolicyDatabase()
		{
			this.GenerateStartingDrugPolicies();
		}

		public void ExposeData()
		{
			Scribe_Collections.Look<DrugPolicy>(ref this.policies, "policies", LookMode.Deep, new object[0]);
		}

		public DrugPolicy DefaultDrugPolicy()
		{
			if (this.policies.Count == 0)
			{
				this.MakeNewDrugPolicy();
			}
			return this.policies[0];
		}

		public AcceptanceReport TryDelete(DrugPolicy policy)
		{
			foreach (Pawn allMapsCaravansAndTravelingTransportPod in PawnsFinder.AllMapsCaravansAndTravelingTransportPods)
			{
				if (allMapsCaravansAndTravelingTransportPod.drugs != null && allMapsCaravansAndTravelingTransportPod.drugs.CurrentPolicy == policy)
				{
					return new AcceptanceReport("DrugPolicyInUse".Translate(allMapsCaravansAndTravelingTransportPod));
				}
			}
			this.policies.Remove(policy);
			return AcceptanceReport.WasAccepted;
		}

		public DrugPolicy MakeNewDrugPolicy()
		{
			int uniqueId = (!this.policies.Any()) ? 1 : (this.policies.Max((Func<DrugPolicy, int>)((DrugPolicy o) => o.uniqueId)) + 1);
			DrugPolicy drugPolicy = new DrugPolicy(uniqueId, "DrugPolicy".Translate() + " " + uniqueId.ToString());
			this.policies.Add(drugPolicy);
			return drugPolicy;
		}

		private void GenerateStartingDrugPolicies()
		{
			DrugPolicy drugPolicy = this.MakeNewDrugPolicy();
			drugPolicy.label = "SocialDrugs".Translate();
			drugPolicy[ThingDefOf.Beer].allowedForJoy = true;
			drugPolicy[ThingDefOf.SmokeleafJoint].allowedForJoy = true;
			DrugPolicy drugPolicy2 = this.MakeNewDrugPolicy();
			drugPolicy2.label = "NoDrugs".Translate();
			DrugPolicy drugPolicy3 = this.MakeNewDrugPolicy();
			drugPolicy3.label = "Unrestricted".Translate();
			for (int i = 0; i < drugPolicy3.Count; i++)
			{
				if (drugPolicy3[i].drug.IsPleasureDrug)
				{
					drugPolicy3[i].allowedForJoy = true;
				}
			}
			DrugPolicy drugPolicy4 = this.MakeNewDrugPolicy();
			drugPolicy4.label = "OneDrinkPerDay".Translate();
			drugPolicy4[ThingDefOf.Beer].allowedForJoy = true;
			drugPolicy4[ThingDefOf.Beer].allowScheduled = true;
			drugPolicy4[ThingDefOf.Beer].takeToInventory = 1;
			drugPolicy4[ThingDefOf.Beer].daysFrequency = 1f;
			drugPolicy4[ThingDefOf.SmokeleafJoint].allowedForJoy = true;
		}
	}
}