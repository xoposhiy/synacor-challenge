using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vm
{
	class Program
	{
		static void Main(string[] args)
		{
			var filename = args.Length > 0 ? args[0] : "challenge.bin";

			var history = args.Length > 1 ? File.ReadAllText(args[1]) : "";
			var data = File.ReadAllBytes(filename);
			var code = new List<int>();
			for (var i = 0; i < data.Length; i += 2)
				code.Add(data[i] + 256 * data[i + 1]);
			try
			{
				var vm = new VM(code, history);
				File.WriteAllLines("asm.txt", vm.Disassemble());
				vm.Run();
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e.ToString());
			}
		}
	}
}
