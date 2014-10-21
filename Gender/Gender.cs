using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

using KSP.IO;

namespace KerbalStats {
	public class Gender : IKerbalStats
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

		Dictionary <string, string> kerbal_gender;

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

		public void AddKerbal (ProtoCrewMember kerbal)
		{
			kerbal_gender[kerbal.name] = PickGender (kerbal.name);
		}

		public void RemoveKerbal (ProtoCrewMember kerbal)
		{
		}

		public string name
		{
			get {
				return "gender";
			}
		}

		public void Load (ProtoCrewMember kerbal, ConfigNode node)
		{
			if (node.HasValue ("gender")) {
				kerbal_gender[kerbal.name] = node.GetValue ("gender");
			} else {
				AddKerbal (kerbal);
			}
		}

		public void Save (ProtoCrewMember kerbal, ConfigNode node)
		{
			if (kerbal_gender.ContainsKey (kerbal.name)) {
				node.AddValue ("gender", kerbal_gender[kerbal.name]);
			}
		}

		public Gender ()
		{
			kerbal_gender = new Dictionary <string, string> ();
		}
	}

	[KSPAddon (KSPAddon.Startup.MainMenu, true)]
	public class KSGenderInit : MonoBehaviour
	{
		void Awake ()
		{
			KerbalStats.AddModule (new Gender ());
			Destroy (this);
		}
	}
}
