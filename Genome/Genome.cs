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
		Dictionary<string, int> trait_map;
		Trait[] traits;

		static Genome instance;

		public Genome (KerbalStats ks)
		{
			instance = this;
			var trait_modules = ModuleLoader.LoadModules (typeof (Trait), new Type []{});
			traits = new Trait[trait_modules.Count];
			trait_map = new Dictionary<string, int> ();
			var parms = new object[] {};
			for (int i = 0; i < trait_modules.Count; i++) {
				traits[i] = (Trait) trait_modules[i].Invoke (parms);
				trait_map[traits[i].name] = i;
			}
			Clear ();
		}

		~Genome ()
		{
			instance = null;
		}
			
		public string name
		{
			get {
				return "genome";
			}
		}

		public static void RebuildGenes (ProtoCrewMember kerbal, GenePair[] genes)
		{
			for (int i = 0; i < instance.traits.Length; i++) {
				if (genes[i] == null) {
					genes[i] = instance.traits[i].CreateGene (kerbal);
				}
			}
		}

		public void AddKerbal (ProtoCrewMember kerbal)
		{
			if (kerbal_genome.ContainsKey (kerbal.name)) {
				// already added via an initialization race
				return;
			}
			var genes = new GenePair[traits.Length];
			RebuildGenes (kerbal, genes);
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
				RebuildGenes (kerbal, kerbal_genome[kerbal.name]);
			} else {
				AddKerbal (kerbal);
			}
		}

		public static void WriteGenes (GenePair[] genes, ConfigNode node)
		{
			for (int i = 0; i < genes.Length; i++) {
				node.AddValue (genes[i].trait.name, genes[i].ToString ());
			}
		}

		public static GenePair[] ReadGenes (ConfigNode node)
		{
			var pairs = node.values;
			GenePair[] genes = new GenePair[instance.traits.Length];
			for (int i = 0; i < pairs.Count; i++) {
				var trait_name = pairs[i].name;
				var trait_value = pairs[i].value;
				if (instance.trait_map.ContainsKey (trait_name)) {
					var ind = instance.trait_map[trait_name];
					var trait = instance.traits[ind];
					genes[ind] = new GenePair (trait, trait_value);
				}
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

		public void Shutdown ()
		{
			instance = null;
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
}
