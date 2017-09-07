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

namespace KerbalStats.Genome {
	public class ProtoCrewMember
	{
		public bool isBadass;
		public float courage;
		public float stupidity;
		public enum Gender {
			Male,
			Female,
		};
		public Gender gender;
	}

	public class Genome
	{
		public static GenePair Prefab (Trait trait, ProtoCrewMember kerbal)
		{
			return null;
		}
	}

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
			var random = new KerbalStats.Random ();
			var pcm = new ProtoCrewMember ();

			var dist = new ContinuousDistribution (pdf3, 0, 1, cdf3);
			for (int i = 0; i < 11; i++) {
				float x = i / 10f;
				Console.WriteLine(String.Format ("{0}; {1} {2} {3} {4}", x, pdf3(x), dist.Density(x), cdf3(x), dist.Cumulation(x)));
			}
			int [] counts = new int [11];
			for (int i = 0; i < 10000; i++) {
				float x = dist.Value (random.Range (0, 1f));
				counts[(int)(x * 10 + 0.5)]++;
			}
			for (int i = 0; i < 11; i++) {
				Console.WriteLine(String.Format ("{0}; {1}", i/10f, counts[i]));
			}

			var ddist = new DiscreteDistribution (new float[]{1,2,3,1});
			int [] dcounts = new int [4];
			for (int i = 0; i < 10000; i++) {
				int v = ddist.Value(random.Range (0, 1f));
				dcounts[v]++;
			}
			for (int i = 0; i < 4; i++) {
				Console.WriteLine(String.Format ("{0}; {1}", i, dcounts[i]));
			}

			Trait badass = new BadAss ();
			Console.WriteLine("BadAss True");
			for (int i = 0; i < 20; i++) {
				pcm.isBadass = true;
				GenePair gene = badass.CreateGene (pcm, random);
				Console.WriteLine(String.Format ("{0}, {1}: {2}", gene.a, gene.b, badass.CreateValue (gene, random)));
			}
			Console.WriteLine("BadAss False");
			for (int i = 0; i < 20; i++) {
				pcm.isBadass = false;
				GenePair gene = badass.CreateGene (pcm, random);
				Console.WriteLine(String.Format ("{0}, {1}: {2}", gene.a, gene.b, badass.CreateValue (gene, random)));
			}

			Trait courage = new Courage ();
			Console.WriteLine("Courage 0..1");
			for (int i = 0; i < 11; i++) {
				pcm.courage = i / 10f;
				GenePair gene = courage.CreateGene (pcm, random);
				Console.WriteLine(String.Format ("{0}: {1}, {2}: {3}", pcm.courage, gene.a, gene.b, courage.CreateValue (gene, random)));
			}

			Trait stupidity = new Stupidity ();
			Console.WriteLine("Stupidity 0..1");
			for (int i = 0; i < 11; i++) {
				pcm.stupidity = i / 10f;
				GenePair gene = stupidity.CreateGene (pcm, random);
				Console.WriteLine(String.Format ("{0}: {1}, {2}: {3}", pcm.stupidity, gene.a, gene.b, stupidity.CreateValue (gene, random)));
			}

			Trait gender = new Gender ();
			Console.WriteLine("Gender");
			{
				pcm.gender = ProtoCrewMember.Gender.Male;
				GenePair gene = gender.CreateGene (pcm, random);
				Console.WriteLine(String.Format ("{0}: {1}, {2}: {3}", "M", gene.a, gene.b, gender.CreateValue (gene, random)));
				pcm.gender = ProtoCrewMember.Gender.Female;
				gene = gender.CreateGene (pcm, random);
				Console.WriteLine(String.Format ("{0}: {1}, {2}: {3}", "F", gene.a, gene.b, gender.CreateValue (gene, random)));
			}
		}
	}
}
