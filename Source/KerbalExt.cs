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
	/** Container for extended kerbal stats.
	 *
	 * Each kerbal is given a KerbalExt object to hold any additional
	 * stats defined by the modules implementing IKerbalExt.
	 */
	public class KerbalExt
	{
		/** Backup node so stats aren't lost when a module is removed
		 *
		 * The idea is that the module may have been removed only
		 * temporarily, thus losing the stats could be a bad thing.
		 */
		ConfigNode node;
		/** Map between the IKerbalExt module and the stats it manages.
		 *
		 * The module's name is used as the key, and the data is
		 * entirely arbitrary, completely up to the module to define.
		 */
		Dictionary<string, object> module_data;

		/* The kerbal to which these stats belong.
		 */
		public ProtoCrewMember kerbal { get; private set; }

		/** Access the module's extended stats data.
		 *
		 * It is up to the caller to cast the result to the correct
		 * type, and no checking is done for type consistency when
		 * setting.
		 *
		 * \param mod   The name of the module
		 */
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

		/** Create stats for a new kerbal.
		 *
		 * Runs through all the available modules so they can create
		 * initial stats when the game spawns a new kerbal.
		 *
		 * \param pcm   The kerbal for which stats will be created.
		 */
		public void NewKerbal (ProtoCrewMember pcm)
		{
			kerbal = pcm;
			var modules = KerbalStats.current.kerbalext_modules;
			foreach (var mod in modules.Values) {
				mod.AddKerbal (this);
			}
		}

		/** Load the kerbal's stats from the given config node.
		 *
		 * \param pcm   The kerbal for which the stats will be loaded.
		 * \param ext   The node from which the stats will be loaded.
		 *              Note that this is the actual container node, ie
		 *              each module pulls its data directly from \a ext.
		 */
		public void Load (ProtoCrewMember pcm, ConfigNode ext)
		{
			kerbal = pcm;
			var modules = KerbalStats.current.kerbalext_modules;
			/** Support ancient versions of KerbalStats (prior to KSP 1.2)
			 */
			if (ext.HasValue ("name")) {
				var name = ext.GetValue ("name");
				ext.RemoveValue ("name");
				if (name != kerbal.name) {
					Debug.LogWarning (String.Format ("kerbal name mismatch: pcm = '{0}' ext = '{1}'", kerbal.name, name));
				}
			}
			/** Create the backup node, removing any values or subnodes
			 * for which a module _does_ exist. This means that missing
			 * modules do not cause stats to be lost.
			 */
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

		/** Save the kerbal's extended stats to the given node.
		 *
		 * \param ext   The container config node to which the stats
		 *              from each module will be saved directly.
		 */
		public void Save (ConfigNode ext)
		{
			var modules = KerbalStats.current.kerbalext_modules;
			/** Copy the backed-up stats to the node
			 */
			node.CopyTo (ext, "KerbalExt");
			/** Run through each module to save that module's stats to
			 * the node.
			 */
			foreach (var mod in modules.Values) {
				mod.Save (this, ext);
			}
		}

		/** Clear out the kerbal's extended stats.
		 *
		 * This is used by only the KerbalStats legacy scenario support
		 * (ie, for KSP versions prior to 1.2)
		 */
		internal static void Clear ()
		{
			var modules = KerbalStats.current.kerbalext_modules;
			foreach (var mod in modules.Values) {
				mod.Clear ();
			}
		}

		/** Implementation of the KerbalStats KerbalExt API.
		 *
		 * This can be called directly if the external mod links
		 * directly to KerbalStats, or via reflection (using
		 * KerbalStatsWrapper or independent means). See
		 * ModName.KerbalStats.KerbalExt.Get() for details.
		 */
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
