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

	/** The genome KerbalStats module
	 *
	 * Each kerbal gets a genome: a set of gene pairs representing the
	 * kerbal's traits. The core of Genome provides traits for Courage,
	 * Stupidity and BadAss, but additional traits can be added simply
	 * by implementing the Trait interface.
	 */
	public class Genome: IKerbalExt
	{
		/** The actual genome data for a kerbal.
		 */
		public class Data
		{
			/** Each kerbal gets its own random number generator
			 */
			public Random random;
			/** The kerbal's genes, one pair for each trait known to the
			 * system.
			 */
			public GenePair[] genes;

			/** Initialize from saved random state and gene pairs.
			 */
			public Data (Random.State state, GenePair[] genes)
			{
				random = new Random ();
				if (state != null) {
					random.Load (state);
				}
				this.genes = genes;
			}

			/** Initialize from scratch
			 *
			 * The gene pairs array is empty but of the correct length.
			 */
			public Data ()
			{
				random = new Random ();///< \todo seed from kerbal
				genes = new GenePair[traits.Length];
			}

			/** Load the genome from a config node.
			 */
			public static Data Load (ConfigNode node)
			{
				var s = Genome.ReadState (node);
				var g = Genome.ReadGenes (node);
				return new Data (s, g);
			}

			/** Save the genome to a config node.
			 */
			public void Save (ConfigNode node)
			{
				Genome.WriteState (random.Save (), node);
				Genome.WriteGenes (genes, node);
			}
		}

		/** Map from trait name to array index
		 */
		static Dictionary<string, int> trait_map;
		/** Array of traits (same order as in genome)
		 */
		static Trait[] traits;
		/** Predefined genomes for kerbals
		 *
		 * Outer index is kerbal name, inner is trait name. Any missing
		 * data is left to chance.
		 */
		static Dictionary<string, Dictionary<string, GenePair>> prefabs;
		/** List of kerbals being loaded.
		 *
		 * Allows delayed processing to avoid issues with partial
		 * initialization.
		 */
		static List<KerbalExt> loading_kerbals;

		/** Static constructor.
		 *
		 * Initializes the genome system.
		 */
		static Genome ()
		{
			/** Find all the defined traits by searching for classes
			 * that implement the Trait interface and have a constructor
			 * that takes no arguments.
			 *
			 * A map from trait name to index in the trait and gene pair
			 * arrays is created.
			 */
			var trait_modules = ModuleLoader.LoadModules<Trait> (new Type []{});
			traits = new Trait[trait_modules.Count];
			trait_map = new Dictionary<string, int> ();
			var parms = new object[] {};
			for (int i = 0; i < trait_modules.Count; i++) {
				traits[i] = (Trait) trait_modules[i].Invoke (parms);
				trait_map[traits[i].name] = i;
			}

			/** Load the genome prefabs
			 */
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

		/** Process loaded kerbals after the game state has stabilized.
		 */
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

		/** Create any missing genes for the kerbal.
		 *
		 * Genes may be missing due to there not being a prefab for the
		 * kerbal, no prefab trait, or an update has added genes that
		 * are not in the persistent data.
		 */
		public static void RebuildGenes (ProtoCrewMember kerbal, Data data)
		{
			for (int i = 0; i < traits.Length; i++) {
				if (data.genes[i] == null) {
					data.genes[i] = traits[i].CreateGene (kerbal, data.random);
				}
			}
		}

		/** Add a genome to the new kerbal.
		 *
		 * The adding will be delayed if the kerbal's name is null as
		 * this indicates partial initialization in KSP.
		 */
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

		/** Fetch the genome of the specified kerbal.
		 */
		public static Data GetGenes (ProtoCrewMember pcm)
		{
			KerbalExt kerbal = KerbalStats.current[pcm];
			return kerbal[extname] as Data;
		}

		public void RemoveKerbal (KerbalExt kerbal)
		{
		}

		/** Load a kerbal's genome data from persistence.
		 *
		 * Includes both random generator state and the genetic data
		 * itself.
		 */
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

		/** Write the random state using modified base-64.
		 *
		 * / and = cannot be used in config node values, so they are
		 * switched to . and %.
		 */
		public static void WriteState (Random.State state, ConfigNode node)
		{
			string val = Convert.ToBase64String (state.state);
			val = val.Replace ('/', '.');
			val = val.Replace ('=', '%');
			node.AddValue ("state", val);
		}

		/** Read the random state using modified base-64.
		 *
		 * / and = cannot be used in config node values, so they are
		 * switched to . and %.
		 */
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

		/** Write out all the gene pairs to the config node.
		 */
		public static void WriteGenes (GenePair[] genes, ConfigNode node)
		{
			for (int i = 0; i < genes.Length; i++) {
				node.AddValue (genes[i].trait.name, genes[i].ToString ());
			}
		}

		/** Read the genetic data from the config node
		 *
		 * The order of the traits in the node is irrelevant, undefined
		 * traits are dropped and missing traits will be filled in
		 * afterwards.
		 */
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

		/** Save a kerbal's genome data to persistence.
		 *
		 * Includes both random generator state and the genetic data
		 * itself.
		 */
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

		/** Produce a new genome from two parent genomes.
		 *
		 * Each gene pair is produced for the corresponding parent gene
		 * pairs using recombination. See GenePair.Combine().
		 */
		public static Data Combine (Data kerbal1, Data kerbal2)
		{
			var data = new Data ();
			for (int i = 0; i < data.genes.Length; i++) {
				data.genes[i] = GenePair.Combine (kerbal1.genes[i], kerbal2.genes[i], data.random);
			}
			return data;
		}

		/** Return the prefab gene pair for a kerbal if it exists.
		 */
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
