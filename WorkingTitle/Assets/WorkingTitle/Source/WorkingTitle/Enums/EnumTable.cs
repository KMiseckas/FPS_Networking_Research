using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorkingTitle.Enums
{
    public abstract class EnumTable<TKey, TValue> : ScriptableObject
      where TKey : struct, Enum
    {

        private static EnumTable<TKey, TValue> _Instance;
        protected static string _ResourcePath;

        public static EnumTable<TKey, TValue> Instance
        {
            get
            {
                if(_Instance)
                {
                    return _Instance;
                }

                return _Instance = (EnumTable<TKey, TValue>)Resources.Load(_ResourcePath);
            }
        }

        [Serializable]
        public struct Entry
        {
            public string Key;
            public TValue Value;
        }

        [SerializeField]
        public Entry[] Entries;

        [NonSerialized]
        Dictionary<TKey, TValue> _runtimeLookup;

        public void BuildRuntimeTable()
        {
            _runtimeLookup = new Dictionary<TKey, TValue>();

            for(int i = 0; i < Entries.Length; ++i)
            {
                if(Enum.TryParse<TKey>(Entries[i].Key, true, out var value))
                {
                    _runtimeLookup.Add(value, Entries[i].Value);
                }
                else
                {
                    Debug.LogError($"Old entry \"{Entries[i].Key}\" found in {typeof(TKey).Namespace} table");
                }
            }
        }

        public TValue GetValue(TKey value)
        {
            if(_runtimeLookup == null)
            {
                BuildRuntimeTable();
            }

            if(_runtimeLookup.TryGetValue(value, out var prefab))
            {
                return prefab;
            }

            Debug.LogError($"Entry for {value} not found in {typeof(TKey).Namespace} table");
            return default;
        }
    }
}