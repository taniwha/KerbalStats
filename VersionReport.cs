using System;
using UnityEngine;
using KSPAPIExtensions;
using System.Reflection;

using KSP.IO;

namespace KerbalStats {

	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class KSVersionReport : MonoBehaviour
	{
		static string version = null;

		public static string GetVersion ()
		{
			if (version != null) {
				return version;
			}
			var asm = Assembly.GetCallingAssembly ();
			var title = SystemUtils.GetAssemblyTitle (asm);
			version = title + " " + SystemUtils.GetAssemblyVersionString (asm);
			return version;
		}

		void Start ()
		{
			Debug.Log (GetVersion ());
			Destroy (this);
		}
	}
}
