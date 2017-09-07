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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace KerbalStats {

	public class Random
	{
		System.Random random;

		public Random ()
		{
			random = new System.Random ();
		}

		public Random (int seed)
		{
			random = new System.Random (seed);
		}

		public int Range (int min, int max)
		{
			return random.Next(min, max);
		}

		public float NextFloat ()
		{
			return (float) random.NextDouble ();
		}

		public float Range (float min, float max)
		{
			return (NextFloat () * (max - min) + min);
		}

		public State Save ()
		{
			var binaryFormatter = new BinaryFormatter();
			using (var temp = new MemoryStream()) {
				binaryFormatter.Serialize(temp, random);
				return new State (temp.ToArray ());
			}
		}

		public void Load (State state)
		{
			var binaryFormatter = new BinaryFormatter();
			using (var temp = new MemoryStream(state.state)) {
				random = (System.Random) binaryFormatter.Deserialize(temp);
			}
		}

		public class State
		{
			public readonly byte[] state;
			public State (byte[] state)
			{
				this.state = state;
			}
		}
	}
}
