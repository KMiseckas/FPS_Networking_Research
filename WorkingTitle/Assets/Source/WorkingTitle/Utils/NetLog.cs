using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorkingTitle
{
    public static class NetLog
    {
        public static void ClientLog(NetworkBehaviour networkBehaviour, string message)
        {
            Log("Client", networkBehaviour, message);
        }

        public static void ServerLog(NetworkBehaviour networkBehaviour, string message)
        {
            Log("Server", networkBehaviour, message);
        }

        private static void Log(string caller, NetworkBehaviour networkBehaviour, string message)
        {
            Debug.Log($"{caller} - {message} - {networkBehaviour}");
        }
    }
}
