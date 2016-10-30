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

		public KerbalExt ()
		{
			node = new ConfigNode ();
		}

		public void NewKerbal (ProtoCrewMember pcm)
		{
			var modules = KerbalStats.current.kerbalext_modules;
			foreach (var mod in modules.Values) {
				mod.AddKerbal (pcm);
			}
		}

		public void Load (ProtoCrewMember kerbal, ConfigNode ext)
		{
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
				mod.Load (kerbal, node);
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

		public void Save (ProtoCrewMember kerbal, ConfigNode ext)
		{
			var modules = KerbalStats.current.kerbalext_modules;
			node.CopyTo (ext, "KerbalExt");
			foreach (var mod in modules.Values) {
				mod.Save (kerbal, ext);
			}
		}

		internal static void Clear ()
		{
			var modules = KerbalStats.current.kerbalext_modules;
			foreach (var mod in modules.Values) {
				mod.Clear ();
			}
		}

		public static string Get (ProtoCrewMember kerbal, string parms)
		{
			var modules = KerbalStats.current.kerbalext_modules;
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
