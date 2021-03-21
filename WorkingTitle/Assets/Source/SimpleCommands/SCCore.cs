using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace SimpleCommands
{
    public class SCCore : MonoBehaviour
    {
        private const string OUTPUT_START = "> ";
        private const string ACTION_MAP_NAME = "Console";
        private const float COMMAND_TEXT_SIZE = 20;
        private const float COMMAND_VIEW_PADDING = 10;

        [Header("Console Settings")]
        [FormerlySerializedAs("console_screen_position")]
        [SerializeField]
        private Vector2 _ConsoleAnchorPosition = new Vector2(10, 10);

        [FormerlySerializedAs("output_console_background")]
        [SerializeField]
        private Color _ConsoleOutputBackground;

        [FormerlySerializedAs("command_field_background")]
        [SerializeField]
        private Color _CommandFieldBackground;

        [FormerlySerializedAs("default_output_text_colour")]
        [SerializeField]
        private Color _OutputTextColour = Color.white;

        [FormerlySerializedAs("default_command_text_colour")]
        [SerializeField]
        private Color _CommandTextColour = Color.white;

        [FormerlySerializedAs("output_console_height")]
        [SerializeField]
        [Range(50, 400)]
        private float _OutputConsoleHeight = 300;

        [FormerlySerializedAs("history_size_output")]
        [SerializeField]
        [Min(10)]
        private int _OutputHistoryCap = 200;

        [FormerlySerializedAs("history_size_commands")]
        [SerializeField]
        [Min(1)]
        private int _CommandHistoryCap = 20;

        private PlayerInput _Input;

        private String _CommandInput;

        private Vector2 _ScrollPosition;

        private bool _IsConsoleVisible = false;

        private LinkedList<string> _OutputConsoleLines = new LinkedList<string>();

        private LinkedList<string> _CommandHistory = new LinkedList<string>();

        private LinkedListNode<string> _CurrentlyDisplayedCommand;

        private Texture2D _OutputConsoleTexture;

        private Texture2D _CommandConsoleTexture;

        [SerializeField]
        private CommandDefinitions _Definitions;

        private bool _NewOutputAdded;

        private static SCCore _Instance;

        private static object _Lock = new object();

        public static SCCore Instance
        {
            get
            {
                lock(_Lock)
                {
                    if(_Instance == null)
                    {
                        _Instance = FindObjectOfType<SCCore>();
                    }

                    return _Instance;
                }
            }
        }

        private void Awake()
        {
            if(_Instance != null)
            {
                Destroy(this.gameObject);
                return;
            }

            _Instance = this;

            DontDestroyOnLoad(this);

            SetupConsoleTextures();

            _Input = GetComponent<PlayerInput>();

            HookInput();
        }

        private void SetupConsoleTextures()
        {
            _OutputConsoleTexture = new Texture2D(1, 1);
            _CommandConsoleTexture = new Texture2D(1, 1);

            for(int y = 0; y < _OutputConsoleTexture.height; ++y)
            {
                for(int x = 0; x < _OutputConsoleTexture.width; ++x)
                {
                    _OutputConsoleTexture.SetPixel(x, y, _ConsoleOutputBackground);
                    _CommandConsoleTexture.SetPixel(x, y, _CommandFieldBackground);
                }
            }

            _OutputConsoleTexture.Apply();
            _CommandConsoleTexture.Apply();
        }

        /// <summary>
        /// Hook any input.
        /// </summary>
        private void HookInput()
        {
            BindAction(ToggleConsole, "Toggle");
            BindAction(IssueCommand, "Issue");
            BindAction(PreviousCommand, "Previous");
            BindAction(NextCommand, "Next");
        }

        /// <summary>
        ///     Gets an action from the player console input
        /// </summary>
        /// <param name="actionMapName"></param>
        /// <param name="actionName"></param>
        private InputAction GetAction(string actionMapName, string actionName)
        {
            InputActionMap actionMap = _Input.actions.FindActionMap(actionMapName, true);

            return actionMap.FindAction(actionName, true);
        }

        /// <summary>
        ///     Binds an action to a player input action
        /// </summary>
        /// <param name="actionMapName"></param>
        /// <param name="actionName"></param>
        /// <param name="callback"></param>
        private void BindAction(Action<InputAction.CallbackContext> callback, string actionName, string actionMapName = ACTION_MAP_NAME)
        {
            InputAction action = GetAction(actionMapName, actionName);

            action.performed += callback;
        }

        private void ToggleConsole(InputAction.CallbackContext obj)
        {
            _IsConsoleVisible = !_IsConsoleVisible;
        }

        private void IssueCommand(InputAction.CallbackContext obj)
        {
            if(_CommandInput == null || _CommandInput == "")
                return;

            _CommandHistory.AddFirst(_CommandInput);
            _CurrentlyDisplayedCommand = _CommandHistory.First;

            if(_CommandHistory.Count > _CommandHistoryCap)
            {
                _CommandHistory.RemoveLast();
            }

            string[] splitCommand = _CommandInput.Split(' ');

            string commandKey = splitCommand[0];
            string[] data = null;

            if(splitCommand.Length > 1)
            {
                data = new string[splitCommand.Length - 1];

                for(int i = 1; i < splitCommand.Length; i++)
                {
                    data[i - 1] = splitCommand[i];
                }
            }

            if(!_Definitions.ExecuteCommand(commandKey.ToLower(), data, out string message))
            {
                string failedMessage = "Failed command: " + message;

                Debug.LogWarning(failedMessage);
                AddConsoleOutput(failedMessage);
            }
            else
            {
                AddConsoleOutput(message);
            }

            _CommandInput = "";
        }

        private void PreviousCommand(InputAction.CallbackContext obj)
        {
            if(_CurrentlyDisplayedCommand == null)
                return;

            _CommandInput = _CurrentlyDisplayedCommand.Value;

            LinkedListNode<string> temp = _CurrentlyDisplayedCommand;
            _CurrentlyDisplayedCommand = _CurrentlyDisplayedCommand.Next == null ? temp : _CurrentlyDisplayedCommand.Next;
        }

        private void NextCommand(InputAction.CallbackContext obj)
        {
            if(_CurrentlyDisplayedCommand == null)
                return;

            LinkedListNode<string> temp = _CurrentlyDisplayedCommand;

            string commandString = "";

            if(_CurrentlyDisplayedCommand.Previous != null)
            {
                _CurrentlyDisplayedCommand = _CurrentlyDisplayedCommand.Previous;
                commandString = _CurrentlyDisplayedCommand.Value;
            }

            _CommandInput = commandString;
        }

        public static void AddConsoleOutput(string output)
        {
            Instance._OutputConsoleLines.AddLast(OUTPUT_START + output);

            if(Instance._OutputConsoleLines.Count > Instance._OutputHistoryCap)
            {
                Instance._OutputConsoleLines.RemoveFirst();
            }

            Instance._NewOutputAdded = true;
        }

        private void OnGUI()
        {
            if(!_IsConsoleVisible)
                return;

            CreateConsoleOutput();
            CreateCommandField();
        }

        private void CreateConsoleOutput()
        {
            float width = Screen.width - (_ConsoleAnchorPosition.x * 2);

            Rect dimensions = new Rect(_ConsoleAnchorPosition.x, _ConsoleAnchorPosition.y, width, _OutputConsoleHeight);

            GUIStyle boxStyle = new GUIStyle();
            boxStyle.normal.background = _OutputConsoleTexture;

            GUI.Box(dimensions, "", boxStyle);
         
            Rect scrollViewRect = new Rect(0, 0, 0, COMMAND_TEXT_SIZE * _OutputConsoleLines.Count);

            _ScrollPosition = GUI.BeginScrollView(dimensions, _ScrollPosition, scrollViewRect);

            GUI.contentColor = _OutputTextColour;

            LinkedListNode<string> nextNode = _OutputConsoleLines.First;
            int iteration = 0;

            Rect labelRect = default;

            while(nextNode != null)
            {
                labelRect = new Rect(5, (20 * iteration), width, COMMAND_TEXT_SIZE);

                GUI.Label(labelRect, nextNode.Value);

                iteration++;
                nextNode = nextNode.Next;
            }

            if(labelRect != null && _NewOutputAdded)
            {
                GUI.ScrollTo(labelRect);
            }

            GUI.EndScrollView();

            _NewOutputAdded = false;
        }

        private void CreateCommandField()
        {
            float y = _OutputConsoleHeight + COMMAND_VIEW_PADDING + _ConsoleAnchorPosition.y;
            float width = Screen.width - (_ConsoleAnchorPosition.x * 2);

            Rect dimensions = new Rect(_ConsoleAnchorPosition.x, y, width, COMMAND_TEXT_SIZE);

            GUIStyle boxStyle = new GUIStyle();
            boxStyle.normal.background = _CommandConsoleTexture;

            GUI.Box(dimensions, "", boxStyle);

            GUI.backgroundColor = Color.clear;
            GUI.color = _CommandTextColour;
            _CommandInput = GUI.TextField(dimensions, _CommandInput);
        }
    }
}
