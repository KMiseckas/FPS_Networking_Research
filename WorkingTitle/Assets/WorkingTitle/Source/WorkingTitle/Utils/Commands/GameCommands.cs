using SimpleCommands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorkingTitle
{
    public class GameCommands : CommandDefinitions
    {
        private static SCCommand<int> TEST_GENERIC = new SCCommand<int>("test", "Test generics out", "test <int>", (val) =>
        {
            Debug.LogWarning("TEST WORKS FOR GENERIC CHEAT");
        });

        protected override void DefineCommands()
        {
            AddCommand(TEST_GENERIC, (command, data) => { (command as SCCommand<int>).Execute(int.Parse(data[0])); });

        }
    }
}
