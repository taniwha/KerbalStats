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

namespace KerbalStats {
	public class Gender : IKerbalExt
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

		static Gender instance;

		Dictionary <string, string> kerbal_gender;

		static string PickGender (string name)
		{
			int end = name.LastIndexOf (" ");
			if (end > 0) {
				name = name.Substring (0, end);
			}
			if (male_names.Contains (name)) {
				Debug.Log (String.Format ("[KS Gender] Male fn: {0}", name));
				return "M";
			}
			foreach (string suf in female_endings) {
				if (name.EndsWith (suf)) {
					Debug.Log (String.Format ("[KS Gender] Female e: {0}", name));
					return "F";
				}
			}
			foreach (string suf in male_endings) {
				if (name.EndsWith (suf)) {
					Debug.Log (String.Format ("[KS Gender] Male e: {0}", name));
					return "M";
				}
			}
			if (UnityEngine.Random.Range (0, 10) < 2) {
				Debug.Log (String.Format ("[KS Gender] Female r: {0}", name));
				return "F";
			}
			Debug.Log (String.Format ("[KS Gender] Male r: {0}", name));
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
			if (node.HasValue (name)) {
				kerbal_gender[kerbal.name] = node.GetValue (name);
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

		public void Clear ()
		{
			kerbal_gender = new Dictionary <string, string> ();
		}

		public string Get (ProtoCrewMember kerbal, string parms)
		{
			if (kerbal_gender.ContainsKey (kerbal.name)) {
				return kerbal_gender[kerbal.name];
			}
			Debug.LogError ("[KS] Gender.Get: no such kerbal: " + kerbal.name);
			return null;
		}

		public Gender ()
		{
			instance = this;
			Clear ();
		}

		public static bool IsFemale (ProtoCrewMember kerbal)
		{
			string gender = instance.Get (kerbal, "");
			return gender != null ? gender[0] == 'F' : false;
		}

		public static bool IsMale (ProtoCrewMember kerbal)
		{
			string gender = instance.Get (kerbal, "");
			return gender != null ? gender[0] == 'M' : true;
		}
	}

	[KSPAddon (KSPAddon.Startup.MainMenu, true)]
	public class KSGenderInit : MonoBehaviour
	{
		void Awake ()
		{
			KerbalExt.AddModule (new Gender ());
			Destroy (this);
		}
	}
}
