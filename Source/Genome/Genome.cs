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
		static Dictionary<string, int> trait_map;
		static Trait[] traits;
		static Dictionary<string, Dictionary<string, GenePair>> prefabs;
		static List<KerbalExt> loading_kerbals;

		static Genome ()
		{
			var trait_modules = ModuleLoader.LoadModules<Trait> (new Type []{});
			traits = new Trait[trait_modules.Count];
			trait_map = new Dictionary<string, int> ();
			var parms = new object[] {};
			for (int i = 0; i < trait_modules.Count; i++) {
				traits[i] = (Trait) trait_modules[i].Invoke (parms);
				trait_map[traits[i].name] = i;
			}

			var dbase = GameDatabase.Instance;
			prefabs = new Dictionary<string, Dictionary<string, GenePair>> ();
			var prefab_kerbals = dbase.GetConfigNodes ("ProgenyPrefab").LastOrDefault ();
			if (prefab_kerbals == null) {
				return;
			}
			foreach (var kerbal in prefab_kerbals.GetNodes ("Kerbal")) {
				if (!kerbal.HasValue ("name") || !kerbal.HasNode ("genome")) {
					continue;
				}
				var name = kerbal.GetValue ("name");
				Debug.Log(String.Format ("[KS Genome] prefab {0}", name));
				var genome = ReadGenes (kerbal.GetNode ("genome"));
				prefabs[name] = new Dictionary<string, GenePair> ();
				foreach (var gene in genome) {
					// gene will be null if it a trait is missing from the
					// config
					if (gene != null) {
						prefabs[name][gene.trait.name] = gene;
					}
				}
			}
		}

		void onGameStateCreated (Game game)
		{
			if (loading_kerbals != null) {
				foreach (var kerbal in loading_kerbals) {
					var genes = kerbal[ModuleName] as GenePair[];
					RebuildGenes (kerbal.kerbal, genes);
				}
				loading_kerbals = null;
			}
		}

		public Genome (KerbalStats ks)
		{
			GameEvents.onGameStateCreated.Add (onGameStateCreated);
		}

		~Genome ()
		{
			GameEvents.onGameStateCreated.Remove (onGameStateCreated);
		}

		public static string extname = "genome";
			
		public string ModuleName
		{
			get {
				return extname;
			}
		}

		public static void RebuildGenes (ProtoCrewMember kerbal, GenePair[] genes)
		{
			for (int i = 0; i < traits.Length; i++) {
				if (genes[i] == null) {
					genes[i] = traits[i].CreateGene (kerbal);
				}
			}
		}

		public void AddKerbal (KerbalExt kerbal)
		{
			if (kerbal[ModuleName] != null) {
				// already added via an initialization race
				Debug.LogFormat("[Genome] AddKerbal: double add {0}",
								kerbal.kerbal.name);
				return;
			}
			var genes = new GenePair[traits.Length];
			kerbal[ModuleName] = genes;
			if (kerbal.kerbal.name != null) {
				RebuildGenes (kerbal.kerbal, genes);
			} else {
				if (loading_kerbals == null) {
					loading_kerbals = new List<KerbalExt> ();
				}
				loading_kerbals.Add (kerbal);
			}
		}

		public static GenePair[] GetGenes (ProtoCrewMember pcm)
		{
			KerbalExt kerbal = KerbalStats.current[pcm];
			return kerbal[extname] as GenePair[];
		}

		public void RemoveKerbal (KerbalExt kerbal)
		{
		}

		public void Load (KerbalExt kerbal, ConfigNode node)
		{
			if (node.HasNode (ModuleName)) {
				node = node.GetNode (ModuleName);
				var genes = ReadGenes (node);
				kerbal[ModuleName] = genes;
				RebuildGenes (kerbal.kerbal, genes);
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
			GenePair[] genes = new GenePair[traits.Length];
			for (int i = 0; i < pairs.Count; i++) {
				var trait_name = pairs[i].name;
				var trait_value = pairs[i].value;
				if (trait_map.ContainsKey (trait_name)) {
					var ind = trait_map[trait_name];
					var trait = traits[ind];
					genes[ind] = new GenePair (trait, trait_value);
				}
			}
			return genes;
		}

		public void Save (KerbalExt kerbal, ConfigNode node)
		{
			var genes = kerbal[ModuleName] as GenePair[];
			var gen = new ConfigNode (ModuleName);
			node.AddNode (gen);
			WriteGenes (genes, gen);
		}

		public void Clear ()
		{
		}

		public void Shutdown ()
		{
		}

		public string Get (KerbalExt kerbal, string parms)
		{
			return "";
		}

		public static GenePair[] Combine (GenePair[] kerbal1, GenePair[] kerbal2)
		{
			var genes = new GenePair[traits.Length];
			for (int i = 0; i < genes.Length; i++) {
				genes[i] = GenePair.Combine (kerbal1[i], kerbal2[i]);
			}
			return genes;
		}

		public static GenePair Prefab (Trait trait, ProtoCrewMember kerbal)
		{
			if (prefabs.ContainsKey (kerbal.name)
				&& prefabs[kerbal.name].ContainsKey (trait.name)) {
				var gene = prefabs[kerbal.name][trait.name];
				return gene;
			}
			return null;
		}
	}
}
