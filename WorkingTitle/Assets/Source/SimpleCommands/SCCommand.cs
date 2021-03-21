using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleCommands
{
    public class SCCommand : SCCommandBase
    {
        private Action _Action;

        public SCCommand(string key, string description, string format, Action action) : base(key, description, format)
        {
            _Action = action;
        }

        public void Execute()
        {
            _Action();
        }
    }

    public class SCCommand<T1> : SCCommandBase
    {
        private Action<T1> _Action;

        public SCCommand(string key, string description, string format, Action<T1> action) : base(key, description, format)
        {
            _Action = action;
        }

        public void Execute(T1 value)
        {
            _Action(value);
        }
    }

    public class SCCommand<T1, T2> : SCCommandBase
    {
        private Action<T1, T2> _Action;

        public SCCommand(string key, string description, string format, Action<T1, T2> action) : base(key, description, format)
        {
            _Action = action;
        }

        public void Execute(T1 value, T2 value2)
        {
            _Action(value, value2);
        }
    }

    public class SCCommand<T1, T2, T3> : SCCommandBase
    {
        private Action<T1, T2, T3> _Action;

        public SCCommand(string key, string description, string format, Action<T1, T2, T3> action) : base(key, description, format)
        {
            _Action = action;
        }

        public void Execute(T1 value, T2 value2, T3 value3)
        {
            _Action(value, value2, value3);
        }
    }

    public class SCCommand<T1, T2, T3, T4> : SCCommandBase
    {
        private Action<T1, T2, T3, T4> _Action;

        public SCCommand(string key, string description, string format, Action<T1, T2, T3, T4> action) : base(key, description, format)
        {
            _Action = action;
        }

        public void Execute(T1 value, T2 value2, T3 value3, T4 value4)
        {
            _Action(value, value2, value3, value4);
        }
    }
}
