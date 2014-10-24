using System.Collections.Generic;

namespace KerbalStats {
	public class KerbalExt
	{
		static Dictionary<string, IKerbalStats> modules = new Dictionary<string, IKerbalStats> ();

		public static void AddModule (IKerbalStats mod)
		{
			if (!modules.ContainsKey (mod.name)) {
				modules[mod.name] = mod;
			}
		}

		ConfigNode node;

		public KerbalExt ()
		{
			node = new ConfigNode ();
		}

		public void NewKerbal (ProtoCrewMember pcm)
		{
			foreach (var mod in modules.Values) {
				mod.AddKerbal (pcm);
			}
		}

		public void Load (ProtoCrewMember kerbal, ConfigNode ext)
		{
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
				if (modules.ContainsKey (node.nodes[i].name)) {
					node.RemoveValues (node.nodes[i].name);
					continue;
				}
				i++;
			}
		}

		public void Save (ProtoCrewMember kerbal, ConfigNode ext)
		{
			node.CopyTo (ext, "KerbalExt");
			foreach (var mod in modules.Values) {
				mod.Save (kerbal, node);
			}
		}
	}
}
