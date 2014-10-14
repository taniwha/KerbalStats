using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

using KSP.IO;

namespace KerbalStats {
	public interface IKerbalStats
	{
		void AddKerbal (ProtoCrewMember kerbal, KerbalExt ext);
	}
	public class KerbalExt
	{
		ConfigNode node;
		public ConfigNode CopyNode ()
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

		public KerbalExt ()
		{
			node = new ConfigNode ();
		}

		public KerbalExt (ConfigNode kerbal)
		{
			node = new ConfigNode ();
			kerbal.CopyTo (node, "KerbalExt");
		}
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
		static List<IKerbalStats> modules = new List<IKerbalStats> ();

		public static void AddModule (IKerbalStats mod)
		{
			if (!modules.Contains (mod)) {
				modules.Add (mod);
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
				foreach (var kerbal in roster.GetNodes ("KerbalExt")) {
					Roster.Add (new KerbalExt (kerbal));
				}
			}
		}

		public override void OnSave(ConfigNode config)
		{
			Debug.Log (String.Format ("[KS] OnSave: {0}", config));

			var roster = config.AddNode (new ConfigNode ("Roster"));
			foreach (var kerbal in Roster) {
				roster.AddNode (kerbal.CopyNode ());
			}
		}

		void addKerbal (ProtoCrewMember kerbal)
		{
			Debug.Log (String.Format ("[KS] {0} {1} {2}", kerbal.name,
									  kerbal.rosterStatus, kerbal.type));
			KerbalExt ext = new KerbalExt ();
			Roster.Add (ext);
			for (int i = 0; i < modules.Count; i++) {
				modules[i].AddKerbal (kerbal, ext);
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
