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

		static Genome instance;

		public Genome ()
		{
			instance = this;
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
			if (kerbal_genome.ContainsKey (kerbal.name)) {
				// already added via an initialization race
				return;
			}
			var genes = new GenePair[traits.Length];
			genes[0] = traits[0].CreateGene (kerbal.gender.ToString ());
			genes[1] = traits[1].CreateGene (kerbal.stupidity.ToString ("G9"));
			genes[2] = traits[2].CreateGene (kerbal.courage.ToString ("G9"));
			genes[3] = traits[3].CreateGene (kerbal.isBadass.ToString ());
			kerbal_genome[kerbal.name] = genes;
		}

		public static GenePair[] GetGenes (ProtoCrewMember kerbal)
		{
			if (!instance.kerbal_genome.ContainsKey (kerbal.name)) {
				instance.AddKerbal (kerbal);
			}
			return instance.kerbal_genome[kerbal.name];
		}

		public void RemoveKerbal (ProtoCrewMember kerbal)
		{
		}

		public void Load (ProtoCrewMember kerbal, ConfigNode node)
		{
			if (node.HasNode (name)) {
				node = node.GetNode (name);
				kerbal_genome[kerbal.name] = ReadGenes (node);
			} else {
				AddKerbal (kerbal);
			}
		}

		public static void WriteGenes (GenePair[] genes, ConfigNode node)
		{
			for (int i = 0; i < genes.Length; i++) {
				node.AddValue ("genepair", genes[i].ToString ());
			}
		}

		public static GenePair[] ReadGenes (ConfigNode node)
		{
			string[] pairs = node.GetValues ("genepair");
			GenePair[] genes = new GenePair[pairs.Length];
			for (int i = 0; i < pairs.Length; i++) {
				genes[i] = new GenePair (pairs[i]);
			}
			return genes;
		}

		public void Save (ProtoCrewMember kerbal, ConfigNode node)
		{
			if (kerbal_genome.ContainsKey (kerbal.name)) {
				var gen = new ConfigNode (name);
				node.AddNode (gen);
				WriteGenes (kerbal_genome[kerbal.name], gen);
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

		public static GenePair[] Combine (GenePair[] kerbal1, GenePair[] kerbal2)
		{
			var genes = new GenePair[instance.traits.Length];
			for (int i = 0; i < genes.Length; i++) {
				genes[i] = GenePair.Combine (kerbal1[i], kerbal2[i]);
			}
			return genes;
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
