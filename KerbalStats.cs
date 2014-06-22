using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using KSPAPIExtensions;

using KSP.IO;

namespace KerbalStats {
	public class KerbalStats : ScenarioModule
	{
		public class KerbalExt
		{
			static string[] male_names = {
				"Adam", "Al", "Alan", "Archibald", "Bill", "Bob", "Buzz",
				"Carson", "Chad", "Charlie", "Chris", "Chuck", "Dean", "Ed",
				"Edan", "Edlu", "Frank", "Franklin", "Gus", "Hans", "Jack",
				"James", "Jebediah", "Jim", "Kirk", "Kurt", "Lars", "Luke",
				"Mac", "Matt", "Phil", "Randall", "Scott", "Sean", "Steve",
				"Tom", "Will"
			};
			static string[] female_endings = {
				"gee", "les", "nie", "one", "ree", "rett", "rie", "ski",
				"sy", "win",
			};
			static string[] male_endings = {
				"zer",
				"zon",
				"zor",
			};
			ConfigNode node;
			public ConfigNode GetNode ()
			{
				var n = new ConfigNode ();
				node.CopyTo (n, "KerbalExt");
				return n;
			}

			public void SetAttribute (string  attr, string val)
			{
				if (node.HasValue (attr)) {
					node.SetValue (attr, val);
				} else {
					node.AddValue (attr, val);
				}
			}
			public string GetAttribute (string  attr)
			{
				return node.GetValue (attr);
			}

			static string PickGender (string name)
			{
				int end = name.LastIndexOf (" ");
				name = name.Substring (0, end);
				if (male_names.Contains (name)) {
					Debug.Log (String.Format ("[KS] Male fn: {0}", name));
					return "M";
				}
				foreach (string suf in female_endings) {
					if (name.EndsWith (suf)) {
						Debug.Log (String.Format ("[KS] Female e: {0}", name));
						return "F";
					}
				}
				foreach (string suf in male_endings) {
					if (name.EndsWith (suf)) {
						Debug.Log (String.Format ("[KS] Male e: {0}", name));
						return "M";
					}
				}
				if (UnityEngine.Random.Range (0, 10) < 2) {
					Debug.Log (String.Format ("[KS] Female r: {0}", name));
					return "F";
				}
				Debug.Log (String.Format ("[KS] Male r: {0}", name));
				return "M";
			}

			public KerbalExt (ProtoCrewMember pcm)
			{
				node = new ConfigNode ();
				SetAttribute ("gender", PickGender (pcm.name));
			}

			public KerbalExt (ConfigNode kerbal)
			{
				node = new ConfigNode ();
				kerbal.CopyTo (node, "KerbalExt");
			}
		}

		List<KerbalExt> Roster;
		List<KerbalExt> Applicants;

		public static KerbalStats current
		{
			get {
				var game = HighLogic.CurrentGame;
				return game.scenarios.Select (s => s.moduleRef).OfType<KerbalStats> ().SingleOrDefault ();

			}
		}

		public static void Create (Game game)
		{
			if (!game.scenarios.Any (p => p.moduleName == typeof (KerbalStats).Name)) {
				//Debug.Log (String.Format ("[KS] Create"));
				var proto = game.AddProtoScenarioModule (typeof (KerbalStats), GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.SPH, GameScenes.TRACKSTATION, GameScenes.FLIGHT);
				proto.Load (ScenarioRunner.fetch);
			}
		}

		public override void OnLoad (ConfigNode config)
		{
			var game = HighLogic.CurrentGame;
			if (game == null) {
				return;
			}
			Debug.Log (String.Format ("[KS] OnLoad"));
			var roster = config.GetNode ("Roster");

			if (roster == null) {
				foreach (var pcm in game.CrewRoster) {
					Roster.Add (new KerbalExt (pcm));
				}
			} else {
				foreach (var kerbal in roster.GetNodes ("KerbalExt")) {
					Roster.Add (new KerbalExt (kerbal));
				}
			}
			var applicants = config.GetNode ("Applicants");
			if (applicants == null) {
				foreach (var pcm in game.CrewRoster.Applicants) {
					Applicants.Add (new KerbalExt (pcm));
				}
			} else {
				foreach (var kerbal in applicants.GetNodes ("KerbalExt")) {
					Applicants.Add (new KerbalExt (kerbal));
				}
			}
		}

		public override void OnSave(ConfigNode config)
		{
			Debug.Log (String.Format ("[KS] OnSave: {0}", config));

			var roster = config.AddNode (new ConfigNode ("Roster"));
			foreach (var kerbal in Roster) {
				roster.AddNode (kerbal.GetNode ());
			}

			ConfigNode applicants = config.AddNode (new ConfigNode ("Applicants"));
			foreach (var kerbal in Applicants) {
				applicants.AddNode (kerbal.GetNode ());
			}
		}

		public override void OnAwake ()
		{
			enabled = false;
			Roster = new List<KerbalExt> ();
			Applicants = new List<KerbalExt> ();
		}

	}

	// Fun magic to get a custom scenario into a game automatically.

	public class KerbalStatsCreator
	{
		public static KerbalStatsCreator me;
		void onGameStateCreated (Game game)
		{
			Debug.Log (String.Format ("[KS] onGameStateCreated"));
			KerbalStats.Create (game);
		}

		public KerbalStatsCreator ()
		{
			GameEvents.onGameStateCreated.Add (onGameStateCreated);
		}
	}

	[KSPAddon(KSPAddon.Startup.Instantly, false)]
	public class KerbalStatsCreatorSpawn : MonoBehaviour
	{

		void Start ()
		{
			Debug.Log (String.Format ("[KS] KerbalStatsCreatorSpawn.Start"));
			KerbalStatsCreator.me = new KerbalStatsCreator ();
			enabled = false;
		}
	}
}
