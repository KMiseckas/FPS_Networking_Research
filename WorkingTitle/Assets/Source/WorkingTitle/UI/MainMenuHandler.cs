using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WorkingTitle.GameFlow;

namespace WorkingTitle
{
    public class MainMenuHandler : MonoBehaviour
    {
        [SerializeField]
        private Button _JoinBtn;

        [SerializeField]
        private Button _HostBtn;

        [SerializeField]
        private Button _ServerBtn;

        [SerializeField]
        private Button _ExitBtn;

        [SerializeField]
        private InputField _IPField;

        [SerializeField]
        private InputField _MinPlayersField;

        [SerializeField]
        private InputField _MaxPlayersField;

        [SerializeField]
        private Dropdown _MapDropdown;

        [Scene]
        [SerializeField]
        private string _ServerScene;

        private void OnEnable()
        {
            List<string> mapDisplayNames = new List<string>();

            List<MapDefinitions.MapData> mapDataList = GameCore.Instance.MapDefinitions.MapDataList;

            foreach(MapDefinitions.MapData mapData in mapDataList)
            {
                mapDisplayNames.Add(mapData.DisplayName);
            }

            _MapDropdown.AddOptions(mapDisplayNames);

            _JoinBtn.onClick.AddListener(OnJoinClick);
            _HostBtn.onClick.AddListener(OnHostClick);
            _ServerBtn.onClick.AddListener(OnServerClick);
            _ExitBtn.onClick.AddListener(OnExitClick);
        }

        private void OnDisable()
        {
            _JoinBtn.onClick.RemoveListener(OnJoinClick);
            _HostBtn.onClick.RemoveListener(OnHostClick);
            _ServerBtn.onClick.RemoveListener(OnServerClick);
            _ExitBtn.onClick.RemoveListener(OnExitClick);
        }

        private void OnJoinClick()
        {
            GameCore.Instance.Join(_IPField.text == "" ? "localhost" : _IPField.text);
        }

        private void OnHostClick()
        {
            int.TryParse(_MinPlayersField.text, out int minPlayerResult);
            int.TryParse(_MaxPlayersField.text, out int maxPlayerResult);

            GameCore.Instance.Host(minPlayerResult, maxPlayerResult, _MapDropdown.options[_MapDropdown.value].text);
        }

        private void OnServerClick()
        {
            int.TryParse(_MinPlayersField.text, out int minPlayerResult);
            int.TryParse(_MaxPlayersField.text, out int maxPlayerResult);

            GameCore.Instance.StartServer(minPlayerResult, maxPlayerResult, _MapDropdown.options[_MapDropdown.value].text);
        }

        private void OnExitClick()
        {
            Application.Quit();
        }
    }
}
