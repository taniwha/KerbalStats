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
using UnityEngine;

using KSP.IO;

namespace KerbalStats.Genome {

	public class Genome: IKerbalExt
	{
		public class Data
		{
			public Random random;
			public GenePair[] genes;

			public Data (Random.State state, GenePair[] genes)
			{
				random = new Random ();
				if (state != null) {
					random.Load (state);
				}
				this.genes = genes;
			}

			public Data ()
			{
				random = new Random ();///< \todo seed from kerbal
				genes = new GenePair[traits.Length];
			}

			public static Data Load (ConfigNode node)
			{
				var s = Genome.ReadState (node);
				var g = Genome.ReadGenes (node);
				return new Data (s, g);
			}

			public void Save (ConfigNode node)
			{
				Genome.WriteState (random.Save (), node);
				Genome.WriteGenes (genes, node);
			}
		}

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
			var prefab_list = dbase.GetConfigNodes ("ProgenyPrefab");
			foreach (var prefab_kerbals in prefab_list) {
				foreach (var kerbal in prefab_kerbals.GetNodes ("Kerbal")) {
					if (!kerbal.HasValue ("name")
						|| !kerbal.HasNode ("genome")) {
						continue;
					}
					var name = kerbal.GetValue ("name");
					Debug.LogFormat ("[KS Genome] prefab {0}", name);
					var genome = ReadGenes (kerbal.GetNode ("genome"));
					prefabs[name] = new Dictionary<string, GenePair> ();
					foreach (var gene in genome) {
						// gene will be null if a trait is missing from the
						// config
						if (gene != null) {
							prefabs[name][gene.trait.name] = gene;
						}
					}
				}
			}
		}

		void onGameStateCreated (Game game)
		{
			if (loading_kerbals != null) {
				foreach (var kerbal in loading_kerbals) {
					var data = kerbal[ModuleName] as Data;
					RebuildGenes (kerbal.kerbal, data);
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

		public static void RebuildGenes (ProtoCrewMember kerbal, Data data)
		{
			for (int i = 0; i < traits.Length; i++) {
				if (data.genes[i] == null) {
					data.genes[i] = traits[i].CreateGene (kerbal, data.random);
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
			var data = new Data();
			kerbal[ModuleName] = data;
			if (kerbal.kerbal.name != null) {
				RebuildGenes (kerbal.kerbal, data);
			} else {
				if (loading_kerbals == null) {
					loading_kerbals = new List<KerbalExt> ();
				}
				loading_kerbals.Add (kerbal);
			}
		}

		public static Data GetGenes (ProtoCrewMember pcm)
		{
			KerbalExt kerbal = KerbalStats.current[pcm];
			return kerbal[extname] as Data;
		}

		public void RemoveKerbal (KerbalExt kerbal)
		{
		}

		public void Load (KerbalExt kerbal, ConfigNode node)
		{
			if (node.HasNode (ModuleName)) {
				node = node.GetNode (ModuleName);
				var state = ReadState (node);
				var genes = ReadGenes (node);
				var data = new Data (state, genes);
				kerbal[ModuleName] = data;
				RebuildGenes (kerbal.kerbal, data);
			} else {
				AddKerbal (kerbal);
			}
		}

		public static void WriteState (Random.State state, ConfigNode node)
		{
			string val = Convert.ToBase64String (state.state);
			val = val.Replace ('/', '.');
			val = val.Replace ('=', '%');
			node.AddValue ("state", val);
		}

		public static Random.State ReadState (ConfigNode node)
		{
			if (!node.HasValue ("state")) {
				return null;
			}
			string val = node.GetValue ("state");
			val = val.Replace ('.', '/');
			val = val.Replace ('%', '=');
			var bytes = Convert.FromBase64String (val);
			return new Random.State (bytes);
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
			var data = kerbal[ModuleName] as Data;
			var gen = new ConfigNode (ModuleName);
			node.AddNode (gen);
			WriteState (data.random.Save (), gen);
			WriteGenes (data.genes, gen);
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

		public static Data Combine (Data kerbal1, Data kerbal2)
		{
			var data = new Data ();
			for (int i = 0; i < data.genes.Length; i++) {
				data.genes[i] = GenePair.Combine (kerbal1.genes[i], kerbal2.genes[i], data.random);
			}
			return data;
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
