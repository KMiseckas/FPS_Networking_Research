using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorkingTitle.GameFlow.FSMSystem;

namespace WorkingTitle
{
    public class NTMainGameToPostGame : NetworkedTransition
    {
        protected override void Update()
        {
            End();
        }
    }
}
