using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace vm
{
	internal class VM
	{
		public readonly List<int> mem;
		public int ip;
		public readonly int[] registers = new int[8];
		public readonly Stack<int> stack = new Stack<int>();
		public const int Modulo = 32768;
		private readonly VMCommand[] cmds = new VMCommand[22];
		private readonly StringBuilder history;

		public int A(int pos)
		{
			return Get(mem[ip + pos]);
		}
		public VM(List<int> mem, string history = "")
		{
			historyFile = DateTime.Now.ToString("hhMMss") + ".txt";
			this.history = new StringBuilder(history);
			this.mem = mem;
			cmds[0] = new VMAction("halt", 0, () => { });
			cmds[1] = new VMAction("set", 2, () => Set(1, A(2)));
			cmds[2] = new VMAction("push", 1, () => stack.Push(A(1)));
			cmds[3] = new VMAction("pop", 1, () => Set(1, stack.Pop()));
			cmds[4] = new VMAction("eq", 3, () => Set(1, A(2) == A(3) ? 1 : 0));
			cmds[5] = new VMAction("gt", 3, () => Set(1, A(2) > A(3) ? 1 : 0));
			cmds[6] = new VMJump("jmp", 1, () => A(1));
			cmds[7] = new VMJump("jt", 2, () => A(1) != 0 ? A(2) : ip + 3);
			cmds[8] = new VMJump("jf", 2, () => A(1) == 0 ? A(2) : ip + 3);
			cmds[9] = new VMAction("add", 3, () => Set(1, A(2) + A(3)));
			cmds[10] = new VMAction("mult", 3, () =>Set(1, A(2) * A(3)));
			cmds[11] = new VMAction("mod", 3, () => Set(1, A(2) % A(3)));
			cmds[12] = new VMAction("and", 3, () => Set(1, A(2) & A(3)));
			cmds[13] = new VMAction("or", 3, () => Set(1, A(2) | A(3)));
			cmds[14] = new VMAction("not", 2, () => Set(1, ~A(2) & (Modulo - 1)));
			cmds[15] = new VMAction("rmem", 2, () => Set(1, mem[A(2)]));
			cmds[16] = new VMAction("wmem", 2, () => mem[A(1)] = A(2));
			cmds[17] = new VMJump("call", 1, () => {
				stack.Push(ip + 2);
				return A(1);
			});
			cmds[18] = new VMJump("ret", 0, () => stack.Pop());
			cmds[19] = new VMAction("out", 1, () => Console.Write((char)A(1)));
			cmds[20] = new VMAction("in", 1, () => Set(1, GetCh()));
			cmds[21] = new VMAction("noop", 0, () => { });
		}

		public IEnumerable<string> Disassemble()
		{
			yield return "size = " + mem.Count;
			var ip = 0;
			while (ip < mem.Count)
			{
				var cmdIndex = mem[ip];
				if (cmdIndex < 0 || cmdIndex > 21)
				{
					yield return ip + "\t" + "DATA\t" + mem[ip];
					ip++;
					continue;
				}
				var cmd = cmds[cmdIndex];
				var args = string.Join("\t", Enumerable.Range(ip + 1, cmd.Arity).Select(i => FormatAddr(mem[i])));
				var s = ip + "\t" + (cmd.Name == "out" ? "out " + (char)mem[ip+1] : (cmd.Name + "\t" + args));
				yield return s;
				ip += cmd.Arity+1;
			}

		}

		private int historyIndex;
		private string historyFile;
		private bool dump;

		private char GetCh()
		{
			var ch = ReadChar();
			if (ch == ':')
			{
				var cmd = ReadLine();
				if (cmd == "take")
				{
					for (int addr = 2670; addr <= 2678; addr += 4)
						mem[addr] = 0;
					Console.WriteLine("all taken!");
				}
				else if (cmd.StartsWith("8="))
				{
					registers[7] = int.Parse(cmd.Substring(2));
					Console.WriteLine("registers[7] == " + Get(Modulo + 7));
				}
				else if (cmd.StartsWith("reg"))
					Console.WriteLine("registers = " + string.Join(" ", registers));
				else if (cmd.StartsWith("dump"))
				{
					dump = !dump;
					Console.WriteLine("DUMP = " + dump);
				}
				else if (cmd == "patch")
				{
					mem[5489] = 6; //jmp
					mem[5490] = 5498;
				}
			}
			if (ch == '\r') ch = ReadChar();
			return ch;
		}

		private string ReadLine()
		{
			return string.Join("", Enumerable.Range(0, int.MaxValue).Select(i => ReadChar()).TakeWhile(c => c != '\n'));
		}

		private char ReadChar()
		{
			char ch;
			if (historyIndex < history.Length)
			{
				ch = history[historyIndex];
				Console.Write(ch);
			}
			else
			{
				ch = (char)Console.Read();
				history.Append(ch);
			}
			historyIndex++;
			return ch;
		}

		private void Set(int pos, int value)
		{
			registers[mem[ip + pos] - Modulo] = value % Modulo;
		}

		private int Get(int value)
		{
			if (value <= 32767) return value;
			else return registers[value - Modulo];
		}

		public void Run()
		{
			while (mem[ip] != 0)
				ExecuteCurrentInstruction();
		}

		private void ExecuteCurrentInstruction()
		{
			try
			{
				if (ip < 0 || ip >= mem.Count)
					throw new Exception("ip out of range");
				if (mem[ip] < 0 || mem[ip] > 21)
					throw new Exception("not a command " + mem[ip]);
				var cmd = cmds[mem[ip]];
				if (cmd == null)
					throw new NotImplementedException(mem[ip] + " not implemented!");
				else
				{
					var args = string.Join("\t", Enumerable.Range(ip + 1, cmd.Arity).Select(i => FormatAddr(mem[i])));
					Dump(ip + "\t" + cmd.Name + "\t" + args);
					ip = cmd.Execute(this);
				}
			}
			catch (Exception e)
			{
				throw new Exception("IP:" + ip + " " + e.Message, e);
			}
			File.WriteAllText(historyFile, history.ToString());
		}

		private string FormatAddr(int addr)
		{
			if (addr < Modulo) return addr.ToString();
			return "reg_" + (addr - Modulo + 1);
		}

		private void Dump(string text)
		{
			if (dump)
				File.AppendAllLines("dump.txt", new[] {text});
		}
	}
}