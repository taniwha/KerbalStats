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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using KSP.UI.Screens;

namespace KerbalStats.Progeny {
	using Locations;
	using Zygotes;

	[KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] {
			GameScenes.SPACECENTER,
			GameScenes.EDITOR,
			GameScenes.FLIGHT,
			GameScenes.TRACKSTATION,
		})
	]
	public class ProgenyScenario : ScenarioModule
	{
		Dictionary<string, Embryo> embryos;
		Dictionary<string, Juvenile> juveniles;
		Dictionary<string, Male> males;
		Dictionary<string, Female> females;

		List<string> pending_reports;
		int lastReportDay;
		double lastReportUT;

		public static EventData<ProgenyScenario> onProgenyScenarioLoaded = new EventData<ProgenyScenario> ("onProgenyScenarioLoaded");

#region Zygote list access
		public List<Embryo> Embryos
		{
			get {
				return embryos.Values.ToList ();
			}
		}

		public List<Juvenile> Juveniles
		{
			get {
				return juveniles.Values.ToList ();
			}
		}

		public List<Male> Males
		{
			get {
				return males.Values.ToList ();
			}
		}

		public List<Female> Females
		{
			get {
				return females.Values.ToList ();
			}
		}
#endregion

#region Location support
		public LocationTracker locations { get; private set; }

		public Location GetLocation (string location, object parm = null)
		{
			return locations.location (location, parm);
		}

		public Location ParseLocation (string locstring)
		{
			return locations.Parse (locstring);
		}
#endregion

		public static ProgenyScenario current { get; private set; }

#region Zygote ID handling
		uint zygote_id;

		public static uint bit_reverse (uint x)
		{
			uint y = 0;

			for (int i = 0; i < 32; i++) {
				y <<= 1;
				y |= x & 1;
				x >>= 1;
			}
			return y;
		}

		public static uint grey (uint x)
		{
			return x ^ (x >> 1);
		}

		public static uint rgrey (uint x)
		{
			for (var m = x >> 1; m != 0; m >>= 1) {
				x ^= m;
			}
			return x;
		}
#endregion

#region Mature
		public void Mature (Embryo embryo)
		{
			embryos.Remove (embryo.id);
			var juvenile = new Juvenile (embryo);
			juveniles[juvenile.id] = juvenile;
			var mother = females[juvenile.mother_id];
			juvenile.SetLocation (mother.location);
		}

		public void Mature (Juvenile juvenile)
		{
			juveniles.Remove (juvenile.id);
			if (juvenile.isFemale) {
				var female = new Female (juvenile);
				females[female.id] = female;
			} else {
				var male = new Male (juvenile);
				males[male.id] = male;
			}
		}
#endregion

		public void ReportPregnancy (Female female)
		{
			pending_reports.Add (female.id);
		}

		public Embryo GetEmbryo (string id)
		{
			return embryos[id];
		}

		public void AddEmbryo (Embryo embryo)
		{
			embryos[embryo.id] = embryo;
		}

		public void AddKerbal (Zygote kerbal)
		{
			if (kerbal is Female) {
				females[kerbal.id] = kerbal as Female;
			} else {
				males[kerbal.id] = kerbal as Male;
			}
		}

		public Zygote GetZygote (string id)
		{
			if (embryos.ContainsKey (id)) {
				return embryos[id];
			} else if (juveniles.ContainsKey (id)) {
				return juveniles[id];
			} else if (females.ContainsKey (id)) {
				return females[id];
			} else if (males.ContainsKey (id)) {
				return males[id];
			}
			return null;
		}

		public Zygote GetKerbal (string id)
		{
			if (females.ContainsKey (id)) {
				return females[id];
			} else if (males.ContainsKey (id)) {
				return males[id];
			}
			return null;
		}

		public string NextZygoteID ()
		{
			var id = bit_reverse (grey (++zygote_id));
			return id.ToString("x");
		}

#region Load/Save
		void LoadZygotes (ConfigNode config)
		{
			var zygote_list = config.nodes;
			foreach (ConfigNode z in zygote_list) {
				switch (z.name) {
					case "embryo":
						var embryo = new Embryo (z);
						embryos[embryo.id] = embryo;
						break;
					case "juvenile":
						var juvenile = new Juvenile (z);
						juveniles[juvenile.id] = juvenile;
						break;
					case "female":
						var female = new Female (z);
						females[female.id] = female;
						break;
					case "male":
						var male = new Male (z);
						males[male.id] = male;
						break;
				}
			}
		}

		public override void OnLoad (ConfigNode config)
		{
			//Debug.Log("[ProgenyScenario] OnLoad");
			ProgenySettings.Load (config);
			var ids = config.GetValue ("zygote_id");
			uint id = 0;
			uint.TryParse (ids, out id);
			zygote_id = rgrey (bit_reverse (id));

			pending_reports.Clear();
			if (config.HasNode ("pending_reports")) {
				var node = config.GetNode ("pending_reports");
				for (int i = 0; i < node.values.Count; i++) {
					string value = node.values[i].value;
					switch (node.values[i].name) {
					case "id":
						string rid = value;
						pending_reports.Add (rid);
						break;
					case "lastReportDay":
						int.TryParse (value, out lastReportDay);
						break;
					case "lastReportUT":
						double.TryParse (value, out lastReportUT);
						break;
					}
				}
			}

			LoadZygotes (config);
			onProgenyScenarioLoaded.Fire (this);
		}

		public override void OnSave (ConfigNode config)
		{
			ProgenySettings.Save (config);
			var id = bit_reverse (grey (zygote_id));
			config.AddValue ("zygote_id", id);

			{
				var node = config.AddNode ("pending_reports");
				node.AddValue ("lastReportDay", lastReportDay);
				node.AddValue ("lastReportUT", lastReportUT);
				foreach (string rid in pending_reports) {
					node.AddValue ("id", rid);
				}
			}

			foreach (var embryo in embryos.Values) {
				var node = config.AddNode ("embryo");
				embryo.Save (node);
			}

			foreach (var juvenile in juveniles.Values) {
				var node = config.AddNode ("juvenile");
				juvenile.Save (node);
			}

			foreach (var female in females.Values) {
				var node = config.AddNode ("female");
				female.Save (node);
			}

			foreach (var male in males.Values) {
				var node = config.AddNode ("male");
				male.Save (node);
			}
		}
#endregion

		public override void OnAwake ()
		{
			//Debug.Log("[ProgenyScenario] OnAwake");
			current = this;

			locations = new LocationTracker ();

			embryos = new Dictionary<string, Embryo> ();
			juveniles = new Dictionary<string, Juvenile> ();
			males = new Dictionary<string, Male> ();
			females = new Dictionary<string, Female> ();

			pending_reports = new List<string> ();
		}

		void OnDestroy ()
		{
			current = null;
		}

		static string[] ShuffledIds (string[] ids)
		{
			int len = ids.Length;
			var kv = new List<KeyValuePair<float, string>> ();
			for (int i = 0; i < len; i++) {
				kv.Add (new KeyValuePair<float, string>(UnityEngine.Random.Range(0, 1f), ids[i]));
			}
			var skv = (from item in kv orderby item.Key select item).ToArray ();
			string [] shuffled = new string[len];
			for (int i = 0; i < len; i++) {
				shuffled[i] = skv[i].Value;
			}
			return shuffled;
		}

		internal IEnumerator ScanFemales ()
		{
			while (true) {
				//Debug.Log(String.Format ("[KS Progeny] ScanFemales"));
				string[] ids = ShuffledIds (females.Keys.ToArray ());
				yield return null;
				for (int i = 0; i < ids.Length; i++) {
					if (!females.ContainsKey (ids[i])) {
						// the kerbal was removed so just skip to the next one
						continue;
					}
					//Debug.Log(String.Format ("[KS Progeny] ScanFemales: {0}", ids[i]));
					females[ids[i]].Update ();
					yield return null;
				}
			}
		}

		void Start ()
		{
			StartCoroutine (ScanFemales ());
		}

		void GenerateReport (double UT, int day)
		{
			string date = KSPUtil.PrintDate (UT, false);
			string title = "Population Status Report";
			string body = "<b>" + date + "</b>\n\n";
			var color = MessageSystemButton.MessageButtonColor.BLUE;
			var icon = MessageSystemButton.ButtonIcons.MESSAGE;

			foreach (string id in pending_reports) {
				Zygote zyg = GetZygote (id);

				if (zyg == null) {
					continue;
				}
				if (zyg is Adult && (zyg as Adult).kerbal != null) {
					name = (zyg as Adult).kerbal.name;
				} else {
					name = zyg.id;
				}
				body += "    " + name + "\n";
			}
			pending_reports.Clear ();

			var message = new MessageSystem.Message (title, body, color, icon);
			MessageSystem.Instance.AddMessage (message);
		}

		void CheckPendingReports ()
		{
			if (pending_reports.Count < 1 || MessageSystem.Instance == null) {
				return;
			}

			double delay;

			float rate = TimeWarp.CurrentRate;
			if (rate >= 100000f) {
				delay = ProgenySettings.ReportPeriod2;
			} else if (rate > 1000) {
				// Make reports a little slower to avoid message spam
				delay = ProgenySettings.ReportPeriod * 2;
			} else {
				delay = ProgenySettings.ReportPeriod;
			}

			var dtFmt = KSPUtil.dateTimeFormatter;
			double UT = Planetarium.GetUniversalTime ();
			int day = (int) (UT / dtFmt.Day);

			if (day != lastReportDay && UT - lastReportUT >= delay) {
				GenerateReport (UT, day);
				lastReportDay = day;
				// pretend the report was on the dot to avoid it creaping
				// forward through the day
				lastReportUT = day * dtFmt.Day;
			}
		}

		void Update ()
		{
			CheckPendingReports ();
		}
	}
}
