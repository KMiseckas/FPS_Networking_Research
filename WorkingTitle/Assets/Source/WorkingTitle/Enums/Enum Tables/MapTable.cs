using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorkingTitle.Enums
{
    public enum PlayableMap
    {
        Dev_Map,
    }

    [Serializable]
    public struct MapData
    {
        [SerializeField]
        private PlayableMap _Map;

        [SerializeField]
        private string _DisplayName;

        [Scene]
        [SerializeField]
        private string _Scene;

        public PlayableMap Map => _Map;
        public string DisplayName => _DisplayName;
        public string Scene => _Scene;
    }

    public class MapTable : EnumTable<PlayableMap, MapData>
    {
        static MapTable()
        {
            _ResourcePath = "Enum Tables/MapTable";
        }

        public static new MapTable Instance
        {
            get => (MapTable)EnumTable<PlayableMap, MapData>.Instance;
        }
    }
}
