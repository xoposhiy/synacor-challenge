using System;

namespace vm
{
	abstract class VMCommand
	{
		protected VMCommand(string name, int arity)
		{
			Name = name;
			Arity = arity;
		}

		public string Name { get; }
		public int Arity { get; }

		public abstract int Execute(VM vm);
	}
	class VMJump : VMCommand
	{
		private readonly Func<int> action;

		public VMJump(string name, int arity, Func<int> action) : base(name, arity)
		{
			this.action = action;
		}

		public override int Execute(VM vm)
		{
			return action();
		}
	}
	class VMAction : VMCommand
	{
		private readonly Action action;

		public VMAction(string name, int arity, Action action) : base(name, arity)
		{
			this.action = action;
		}

		public override int Execute(VM vm)
		{
			action();
			return vm.ip + Arity + 1;
		}
	}
}