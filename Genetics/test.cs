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

namespace KerbalStats.Genetics {
	public class test
	{
		static float pdf0 (float x)
		{
			return (float) ((Math.Exp (1 - x) - 1) / (Math.E - 2));
		}
		static float cdf0 (float x)
		{
			return (float) ((Math.E - Math.Exp (1 - x) - x) / (Math.E - 2));
		}
		static float pdf12 (float x)
		{
			return (float) ((3*Math.E - Math.Exp (x) - Math.Exp (1 - x) - 4) / (Math.E - 2));
		}
		static float cdf12 (float x)
		{
			return (float) (((3 * Math.E - 4) * x - Math.Exp (x) + Math.Exp (1 - x) + 1 - Math.E) / (Math.E - 2));
		}
		static float pdf3 (float x)
		{
			return (float) ((Math.Exp (x) - 1) / (Math.E - 2));
		}
		static float cdf3 (float x)
		{
			return (float) ((Math.Exp (x) - x - 1) / (Math.E - 2));
		}
		static void Main ()
		{
			var dist = new ContinuousDistribution (pdf3, 0, 1, cdf3);
			for (int i = 0; i < 11; i++) {
				float x = i / 10f;
				Console.WriteLine(String.Format ("{0}; {1} {2} {3} {4}", x, pdf3(x), dist.Density(x), cdf3(x), dist.Cumulation(x)));
			}
			int [] counts = new int [11];
			for (int i = 0; i < 10000; i++) {
				float x = dist.Value (Random.Range (0, 1f));
				counts[(int)(x * 10 + 0.5)]++;
			}
			for (int i = 0; i < 11; i++) {
				Console.WriteLine(String.Format ("{0}; {1}", i/10f, counts[i]));
			}

			var ddist = new DiscreteDistribution (new float[]{1,2,3,1});
			int [] dcounts = new int [4];
			for (int i = 0; i < 10000; i++) {
				int v = ddist.Value(Random.Range (0, 1f));
				dcounts[v]++;
			}
			for (int i = 0; i < 4; i++) {
				Console.WriteLine(String.Format ("{0}; {1}", i, dcounts[i]));
			}

			Trait badass = new BadAss ();
			Console.WriteLine("BadAss True");
			for (int i = 0; i < 20; i++) {
				GenePair gene = badass.CreateGene ("True");
				Console.WriteLine(String.Format ("{0}, {1}: {2}", gene.a, gene.b, badass.CreateValue (gene)));
			}
			Console.WriteLine("BadAss False");
			for (int i = 0; i < 20; i++) {
				GenePair gene = badass.CreateGene ("False");
				Console.WriteLine(String.Format ("{0}, {1}: {2}", gene.a, gene.b, badass.CreateValue (gene)));
			}

			Trait courage = new Courage ();
			Console.WriteLine("Courage 0..1");
			for (int i = 0; i < 11; i++) {
				float c = i / 10f;
				GenePair gene = courage.CreateGene (c.ToString ("G9"));
				Console.WriteLine(String.Format ("{0}: {1}, {2}: {3}", c, gene.a, gene.b, courage.CreateValue (gene)));
			}

			Trait stupidity = new Stupidity ();
			Console.WriteLine("Stupidity 0..1");
			for (int i = 0; i < 11; i++) {
				float c = i / 10f;
				GenePair gene = stupidity.CreateGene (c.ToString ("G9"));
				Console.WriteLine(String.Format ("{0}: {1}, {2}: {3}", c, gene.a, gene.b, stupidity.CreateValue (gene)));
			}
		}
	}
}
