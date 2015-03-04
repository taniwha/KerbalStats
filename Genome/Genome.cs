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

namespace KerbalStats.Genome {

	public class Genome: IKerbalExt
	{
		Dictionary<string, GenePair[]> kerbal_genome;
		Trait[] traits;

		public Genome ()
		{
			traits = new Trait[] {
				new Gender (),
				new Stupidity (),
				new Courage (),
				new BadAss (),
			};
			Clear ();
		}
			
		public string name
		{
			get {
				return "genome";
			}
		}

		public void AddKerbal (ProtoCrewMember kerbal)
		{
			var genes = new GenePair[traits.Length];
			genes[0] = traits[0].CreateGene (KerbalExt.Get (kerbal, "gender"));
			genes[1] = traits[1].CreateGene (kerbal.stupidity.ToString ("G9"));
			genes[2] = traits[2].CreateGene (kerbal.courage.ToString ("G9"));
			genes[3] = traits[3].CreateGene (kerbal.isBadass.ToString ());
			kerbal_genome[kerbal.name] = genes;
		}

		public void RemoveKerbal (ProtoCrewMember kerbal)
		{
		}

		public void Load (ProtoCrewMember kerbal, ConfigNode node)
		{
			if (node.HasNode (name)) {
				node = node.GetNode (name);
				string[] pairs = node.GetValues ("genepair");
				GenePair[] genes = new GenePair[pairs.Length];
				for (int i = 0; i < pairs.Length; i++) {
					genes[i] = new GenePair (pairs[i]);
				}
				kerbal_genome[kerbal.name] = genes;
			} else {
				AddKerbal (kerbal);
			}
		}

		public void Save (ProtoCrewMember kerbal, ConfigNode node)
		{
			if (kerbal_genome.ContainsKey (kerbal.name)) {
				var gen = new ConfigNode (name);
				node.AddNode (gen);
				var genes = kerbal_genome[kerbal.name];
				for (int i = 0; i < genes.Length; i++) {
					gen.AddValue ("genepair", genes[i].ToString ());
				}
			}
		}

		public void Clear ()
		{
			kerbal_genome = new Dictionary<string, GenePair[]>();
		}

		public string Get (ProtoCrewMember kerbal, string parms)
		{
			return "";
		}
	}

	[KSPAddon (KSPAddon.Startup.MainMenu, true)]
	public class KSGenomeInit : MonoBehaviour
	{
		void Awake ()
		{
			KerbalExt.AddModule (new Genome ());
		}
	}
}
