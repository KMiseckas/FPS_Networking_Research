using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleCommands
{
    public abstract class SCCommandBase
    {
        public readonly string CommandKey;

        public readonly string CommandDesc;

        public readonly string Format;

        public SCCommandBase(string key, string description, string format)
        {
            CommandKey = key;
            CommandDesc = description;
            Format = format;
        }
    }
}
