using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorkingTitle
{
    public class MapDefinitions : MonoBehaviour
    {
        /// <summary>
        /// Enum for all the playable maps that can be launched.
        /// </summary>
        public enum PlayableMap
        {
            Dev_Map,
        }

        /// <summary>
        /// Simple data struct to keep any map related data together in one place.
        /// </summary>
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

        /// <summary>
        /// List of all maps and their associated data.
        /// </summary>
        [SerializeField]
        private List<MapData> _MapDataList = new List<MapData>();

        public List<MapData> MapDataList => _MapDataList;

        /// <summary>
        /// Get the scene string name of the provided map enum.
        /// </summary>
        /// <param name="map">Enum of the map to retrieve the scene name for.</param>
        /// <returns>Scene name associated with the passed in map enum.</returns>
        public MapData GetMapData(PlayableMap map)
        {
            return _MapDataList.Find(MapData => MapData.Map.Equals(map));
        }

        /// <summary>
        /// Get the scene string name of the provided map enum.
        /// </summary>
        /// <param name="displayName">Display name string.</param>
        /// <returns>Scene name associated with the passed in map enum.</returns>
        public MapData GetMapData(string displayName)
        {
            return _MapDataList.Find(MapData => MapData.DisplayName.Equals(displayName));
        }
    }
}
