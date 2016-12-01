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
using UnityEngine;
using System.Collections.Generic;

namespace KerbalStats {
	public class KerbalExt
	{
		ConfigNode node;
		Dictionary<string, object> module_data;

		public ProtoCrewMember kerbal { get; private set; }

		public object this[string mod]
		{
			get {
				object data;
				module_data.TryGetValue (mod, out data);
				return data;
			}
			set {
				module_data[mod] = value;
			}
		}

		public KerbalExt ()
		{
			node = new ConfigNode ();
			module_data = new Dictionary<string, object>();
		}

		public void NewKerbal (ProtoCrewMember pcm)
		{
			kerbal = pcm;
			var modules = KerbalStats.current.kerbalext_modules;
			foreach (var mod in modules.Values) {
				mod.AddKerbal (this);
			}
		}

		public void Load (ProtoCrewMember pcm, ConfigNode ext)
		{
			kerbal = pcm;
			var modules = KerbalStats.current.kerbalext_modules;
			if (ext.HasValue ("name")) {
				var name = ext.GetValue ("name");
				ext.RemoveValue ("name");
				if (name != kerbal.name) {
					Debug.LogWarning (String.Format ("kerbal name mismatch: pcm = '{0}' ext = '{1}'", kerbal.name, name));
				}
			}
			ext.CopyTo (node, "KerbalExt");
			foreach (var mod in modules.Values) {
				mod.Load (this, node);
			}
			for (int i = 0; i < node.nodes.Count; ) {
				if (modules.ContainsKey (node.nodes[i].name)) {
					node.RemoveNodes (node.nodes[i].name);
					continue;
				}
				i++;
			}
			for (int i = 0; i < node.values.Count; ) {
				if (modules.ContainsKey (node.values[i].name)) {
					node.RemoveValues (node.values[i].name);
					continue;
				}
				i++;
			}
		}

		public void Save (ConfigNode ext)
		{
			var modules = KerbalStats.current.kerbalext_modules;
			node.CopyTo (ext, "KerbalExt");
			foreach (var mod in modules.Values) {
				mod.Save (this, ext);
			}
		}

		internal static void Clear ()
		{
			var modules = KerbalStats.current.kerbalext_modules;
			foreach (var mod in modules.Values) {
				mod.Clear ();
			}
		}

		public static string Get (ProtoCrewMember pcm, string parms)
		{
			var modules = KerbalStats.current.kerbalext_modules;
			KerbalExt kerbal = KerbalStats.current[pcm];
			string system = parms;
			if (parms.Contains (":")) {
				int index = parms.IndexOf (":");
				system = parms.Substring (0, index);
				parms = parms.Substring (index + 1);
			} else {
				parms = "";
			}
			if (!modules.ContainsKey (system)) {
				Debug.LogError ("[KS] KerbalExt.Get: no such module: " + system);
				return null;
			}
			return modules[system].Get (kerbal, parms);
		}
	}
}
