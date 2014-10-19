using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KerbalStats {
	public interface IKerbalStats
	{
		string name { get; }
		void AddKerbal (ProtoCrewMember kerbal);
		void RemoveKerbal (ProtoCrewMember kerbal);
		void Load (ProtoCrewMember kerbal, ConfigNode node);
		void Save (ProtoCrewMember kerbal, ConfigNode node);
	}

	public static class EnumUtil {
		public static T[] GetValues<T>() {
			return (T[])Enum.GetValues(typeof(T));
		}
	}

	[KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] {
			GameScenes.SPACECENTER,
			GameScenes.EDITOR,
			GameScenes.FLIGHT,
			GameScenes.TRACKSTATION,
			GameScenes.SPH,
		})
	]
	public class KerbalStats : ScenarioModule
	{
		static Dictionary<string, IKerbalStats> modules = new Dictionary<string, IKerbalStats> ();

		public static void AddModule (IKerbalStats mod)
		{
			if (!modules.ContainsKey (mod.name)) {
				modules[mod.name] = mod;
			}
		}

		List<KerbalExt> Roster;

		public static KerbalStats current
		{
			get {
				var game = HighLogic.CurrentGame;
				return game.scenarios.Select (s => s.moduleRef).OfType<KerbalStats> ().SingleOrDefault ();

			}
		}

		public KerbalExt this[ProtoCrewMember kerbal]
		{
			get {
				var game = HighLogic.CurrentGame;
				return Roster[game.CrewRoster.IndexOf (kerbal)];
			}
		}

		void build_roster (Game game)
		{
			var KerbalTypes = EnumUtil.GetValues<ProtoCrewMember.KerbalType>();
			var states = EnumUtil.GetValues<ProtoCrewMember.RosterStatus>();
			int num_kerbals = 0;
			var roster = game.CrewRoster;

			if (roster == null) {
				// We somehow got started before the crew roster was setup.
				return;
			}

			// KerbalRoster doesn't provide an iterator for getting all
			// kerbals at once, so count the kerbals in each type.
			foreach (var type in KerbalTypes) {
				foreach (var pcm in roster.Kerbals(type, states)) {
					num_kerbals++;
				}
			}
			// This roster will now shadow the main roster
			for (int i = 0; i < num_kerbals; i++) {
				addKerbal (roster[i]);
			}
		}

		public override void OnLoad (ConfigNode config)
		{
			var game = HighLogic.CurrentGame;

			Debug.Log (String.Format ("[KS] OnLoad"));
			var roster = config.GetNode ("Roster");

			if (roster == null) {
				build_roster (game);
			} else {
				var kerbal_list = roster.GetNodes ("KerbalExt");
				for (int i = 0; i < kerbal_list.Count(); i++) {
					var kerbal = kerbal_list[i];
					ProtoCrewMember pcm = game.CrewRoster[i];
					var ext = new KerbalExt (kerbal);
					Roster.Add (ext);
					foreach (var mod in modules.Values) {
						mod.Load (pcm, kerbal);
					}
				}
			}
		}

		public override void OnSave(ConfigNode config)
		{
			var game = HighLogic.CurrentGame;
			Debug.Log (String.Format ("[KS] OnSave: {0}", config));

			var roster = config.AddNode (new ConfigNode ("Roster"));
			for (int i = 0; i < Roster.Count; i++) {
				var kerbal = Roster[i];
				ProtoCrewMember pcm = game.CrewRoster[i];
				var node = new ConfigNode ("KerbalExt");
				roster.AddNode (node);
				foreach (var mod in modules.Values) {
					mod.Save (pcm, node);
				}
			}
		}

		void addKerbal (ProtoCrewMember kerbal)
		{
			Debug.Log (String.Format ("[KS] {0} {1} {2}", kerbal.name,
									  kerbal.rosterStatus, kerbal.type));
			KerbalExt ext = new KerbalExt ();
			Roster.Add (ext);
			foreach (var mod in modules.Values) {
				mod.AddKerbal (kerbal);
			}
		}

		void onKerbalAdded (ProtoCrewMember kerbal)
		{
			Debug.Log (String.Format ("[KS] onKerbalAdded: {0}", kerbal.name));

			addKerbal (kerbal);
		}

		void onKerbalRemoved (ProtoCrewMember kerbal)
		{
			Debug.Log (String.Format ("[KS] onKerbalRemoved: {0}", kerbal.name));
			var game = HighLogic.CurrentGame;
			var roster = game.CrewRoster;
			int index = roster.IndexOf (kerbal);
			Roster.RemoveAt (index);
		}

		public override void OnAwake ()
		{
			enabled = false;
			Roster = new List<KerbalExt> ();
			GameEvents.onKerbalAdded.Add (onKerbalAdded);
			GameEvents.onKerbalRemoved.Add (onKerbalRemoved);
		}

		void OnDestroy ()
		{
			GameEvents.onKerbalAdded.Remove (onKerbalAdded);
			GameEvents.onKerbalRemoved.Remove (onKerbalRemoved);
		}
	}

}
