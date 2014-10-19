namespace KerbalStats {
	public class KerbalExt
	{
		ConfigNode node;
		public ConfigNode CopyNode ()
		{
			var n = new ConfigNode ();
			node.CopyTo (n, "KerbalExt");
			return n;
		}

		public bool HasNode (string name)
		{
			return node.HasNode (name);
		}

		public ConfigNode GetNode (string name)
		{
			return node.GetNode (name);
		}

		public void AddNode (ConfigNode n)
		{
			node.AddNode (n);
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
}
