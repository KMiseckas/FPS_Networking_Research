using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace SimpleCommands
{
    public abstract class CommandDefinitions : MonoBehaviour
    {
        private Dictionary<Type, Action<SCCommandBase, string[]>> _TypeParsers = new Dictionary<Type, Action<SCCommandBase, string[]>>();

        private Dictionary<string, SCCommandBase> _RegisteredCommands = new Dictionary<string, SCCommandBase>();

        private void Awake()
        {
            DefineCommands();
        }

        protected abstract void DefineCommands();

        protected void AddCommand(SCCommandBase commandBase, Action<SCCommandBase, string[]> executeParser = null)
        {
            if(executeParser == null)
            {
                Assert.IsTrue(commandBase is SCCommand, $"parse action cannot be null if the command uses Generics");
            }

            executeParser = executeParser == null ? ((command, data) => { (command as SCCommand).Execute(); }) : executeParser;

            _TypeParsers.Add(commandBase.GetType(), executeParser);
            _RegisteredCommands.Add(commandBase.CommandKey.ToLower(), commandBase);
        }

        public bool ExecuteCommand(string commandKey, string[] data, out string output)
        {
            if(!_RegisteredCommands.TryGetValue(commandKey, out SCCommandBase command))
            {
                output = $"Command [{commandKey}] not found.";
                return false;
            }

            if(!_TypeParsers.TryGetValue(command.GetType(), out Action<SCCommandBase, string[]> executeParser))
            {
                output = $"Command [{commandKey}] execution parser not found.";
                return false;
            }

            if(!(command is SCCommand) && data == null)
            {
                output = $"Command [{commandKey}] requires parameters to execute. Format is [{command.Format}]";
                return false;
            }

            try
            {
                executeParser(command, data);
            }
            catch
            {
                output = $"Command [{commandKey}] has failed parsing for an unknown reason.";
                return false;
            }

            output = "";
            return true;
        }

    }
}
