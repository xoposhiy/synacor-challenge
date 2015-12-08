using System;
using System.Drawing;
using System.Linq;

namespace tasks
{
	class Program
	{
		static void Main(string[] args)
		{
			//Coins();
			//Teleport();
			Valut();
		}

		static readonly string[,] map = {
				{"*", "8", "-", "1"},
				{"4", "*", "11", "*"},
				{"+", "4", "-", "18"},
				{"22", "-", "9", "*"}
		};
		static readonly Size[] d = { new Size(1, 0), new Size(-1, 0), new Size(0, 1), new Size(0, -1) };


		private static void Valut()
		{
			var pos = new Point(3, 0);
			var vault = new Point(0, 3);
			//Console.WriteLine(map[pos.X, pos.Y]);
			var r = new Random();
			var path = Enumerable.Range(0, 1000000).Select(i => Walk(pos, vault, r)).Where(p => p != null).OrderBy(p => p.Length).FirstOrDefault();

			Console.WriteLine(path);
		}
		private static string Walk(Point p1, Point p2, Random r)
		{
			var n = 22;
			var path = "";
			var e = "22";
			var op = "";
			while (path.Length < 14 && p1 != p2)
			{
				var index = r.Next(4);
				var next = p1 + d[index];
				if (next.X < 0 || next.X > 3 || next.Y < 0 || next.Y > 3 || (next.X == 3 && next.Y==0)) continue;
				path += "SNEW"[index];
				p1 = next;
				var cell = map[p1.X, p1.Y];
				e += cell;
				if (path.Length%2 == 0)
                {
	                var value = int.Parse(cell);
	                if (op == "+") n += value;
					if (op == "-") n -= value;
					if (op == "*") n *= value;
				}
				else
				{
					op = cell;
				}
			}
//			Console.WriteLine(path + " " + n);
//			Console.WriteLine(e + " " + n);
			return n == 30 && p1 == p2 ? path : null;

		}

		private static void Teleport()
		{
			var max = 32768;
			for (int r8 = 1; r8 < max; r8++)
			{
				int[,] dp = new int[5, max];
				for (int r2 = 0; r2 < max; r2++)
					dp[0, r2] = (r2 + 1)%max;
				for (int r1 = 1; r1 <= 4; r1++)
				{
					dp[r1, 0] = dp[r1 - 1, r8];
					for (int r2 = 1; r2 < max; r2++)
						dp[r1, r2] = dp[r1 - 1, dp[r1, r2 - 1]];
//					for (int r2 = 0; r2 < max; r2++)
//						Console.Write(dp[r1, r2] + " ");
//					Console.WriteLine();
				}
				if (dp[4, 1] == 6)
					Console.WriteLine("f(" + r8 + ") = " + dp[4, 1]);
				//25734
			}
		}
		private static void Coins()
		{
			var all = new[] { 2, 7, 3, 9, 5 };
			var q =
				from a in all
				from b in all
				from c in all
				from d in all
				from e in all
				let res = new[] { a, b, c, d, e }
				where res.Distinct().Count() == 5
				where a + b * c * c + d * d * d - e == 399
				select res;
			Console.WriteLine(string.Join(" ", q.First()));
		}

	}
}
