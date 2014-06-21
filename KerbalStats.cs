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
			Debug.Log (String.Format ("[KS] OnLoad"));
		}

		public override void OnSave(ConfigNode config)
		{
			Debug.Log (String.Format ("[KS] OnLoad: {0}", config));
		}
		
		public override void OnAwake ()
		{
			enabled = false;
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
