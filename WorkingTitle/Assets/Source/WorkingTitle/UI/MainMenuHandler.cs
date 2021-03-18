using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

        [Scene]
        [SerializeField]
        private string _ServerScene;

        private void OnEnable()
        {
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
            string ipAddress = _IPField.text == "" ? "localhost" : _IPField.text;

            GameManager.Instance.NetworkManager.networkAddress = ipAddress;
            GameManager.Instance.NetworkManager.StartClient();
        }

        private void OnHostClick()
        {
            GameManager.Instance.NetworkManager.onlineScene = _ServerScene;
            GameManager.Instance.NetworkManager.StartHost();
        }

        private void OnServerClick()
        {
            GameManager.Instance.NetworkManager.onlineScene = _ServerScene;
            GameManager.Instance.NetworkManager.StartServer();
        }

        private void OnExitClick()
        {
            Application.Quit();
        }
    }
}
