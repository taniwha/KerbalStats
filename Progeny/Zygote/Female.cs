/*
This file is part of KerbalStats.

KerbalStats is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

KerbalStats is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with KerbalStats.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

using KSP.IO;

namespace KerbalStats.Progeny {
	public class Female : Adult, IComparable<Female>
	{
		double lastUpdate;
		double UT;
		double interestTime;
		double interestTC;
		Embryo embryo;

		FemaleFSM fsm;

		public float Interest ()
		{
			if (UT < interestTime) {
				return 0;
			}
			double x = (UT - interestTime) / interestTC;
			return (float) (1 - (x + 1) * Math.Exp (-x));
		}

		public bool isInterested ()
		{
			return UnityEngine.Random.Range (0, 1f) < Interest ();
		}

		public float Fertility
		{
			get {
				return 0.5f; //FIXME
			}
		}

		public Male SelectMate (List<Male> males)
		{
			float [] male_readiness = new float[males.Count + 1];
			male_readiness[0] = 2; //FIXME
			for (int i = 0; i < males.Count; i++) {
				male_readiness[i + 1] = males[i].Interest (UT);
			}
			var dist = new Genome.DiscreteDistribution (male_readiness);
			int ind = dist.Value (UnityEngine.Random.Range (0, 1f)) - 1;
			if (ind < 0) {
				return null;
			}
			return males[ind];
		}

		public bool Mate (Male mate)
		{
			interestTime = UT + 600; //FIXME
			mate.Mate (interestTime);
			float conceive_chance = Fertility * mate.Fertility;
			if (UnityEngine.Random.Range (0, 1f) > conceive_chance) {
				return false;
			}
			embryo = new Embryo (this, mate);
			ProgenyScenario.current.AddEmbryo (embryo);
			return true;
		}

		void initialize ()
		{
			lastUpdate = Planetarium.GetUniversalTime ();
			fsm = new FemaleFSM (this);

			interestTime = 0;
			interestTC = 3600;	//FIXME
			embryo = null;
		}

		public Female (Juvenile juvenile) : base (juvenile)
		{
			initialize ();
			fsm.StartFSM ("Fertile");
		}

		public Female (ProtoCrewMember kerbal) : base (kerbal)
		{
			initialize ();
			fsm.StartFSM ("Fertile");
		}

		public Female (ConfigNode node) : base (node)
		{
			initialize ();
			if (node.HasValue ("state")) {
				fsm.StartFSM (node.GetValue ("state"));
			} else {
				fsm.StartFSM ("Fertile");
			}
			if (node.HasValue ("interestTime")) {
				double.TryParse (node.GetValue ("interestTime"), out interestTime);
			}
			if (node.HasValue ("interestTC")) {
				double.TryParse (node.GetValue ("interestTC"), out interestTC);
			}
			if (node.HasValue ("embryo")) {
				var zid = node.GetValue ("embryo");
				embryo = ProgenyScenario.current.GetEmbryo (zid);
			}
		}

		public override void Save (ConfigNode node)
		{
			base.Save (node);
			node.AddValue ("state", fsm.currentStateName);
			node.AddValue ("interestTime", interestTime.ToString ("G17"));
			node.AddValue ("interestTC", interestTC.ToString ("G17"));
			if (embryo != null) {
				node.AddValue ("embryo", embryo.id);
			}
		}

		public void Update ()
		{
			UT = Planetarium.GetUniversalTime ();
			if (UT - lastUpdate < 3600) {
				return;
			}
			fsm.UpdateFSM ();
			lastUpdate = UT;
		}

		public string State
		{
			get {
				return fsm.currentStateName + " " + Interest ();
			}
		}

		public int CompareTo (Female other)
		{
			return name.CompareTo (other.name);
		}
	}
}
