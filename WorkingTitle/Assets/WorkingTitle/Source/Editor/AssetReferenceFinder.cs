﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class AssetReferenceFinder : EditorWindow
{
    #region Constants

    private static readonly string[] FileExtensions = new string[]
    {
        ".unity",
        ".prefab",
        ".asset",
        ".controller",
        ".anim",
        ".mat",
        ".cs.meta"
    };

    #endregion

    #region Fields

    private Object _searchTarget;
    private readonly List<ReferenceObject> _referenceObjects = new List<ReferenceObject>();
    private readonly Stack<Object> _searchStack = new Stack<Object>();
    private Vector2 _scrollPosition;
    private bool _initialised;

    #endregion

    #region Unity Messages

    /// <summary>
    ///     Called when the window is enabled
    /// </summary>
    private void OnEnable()
    {
        this._searchTarget = Selection.activeObject;

        if (this._searchTarget)
        {
            this.Search(true);
        }
    }

    /// <summary>
    ///     Called every GUI render frame
    /// </summary>
    private void OnGUI()
    {
        if (EditorSettings.serializationMode == SerializationMode.ForceText)
        {
            var assetStackText = new StringBuilder();

            this.ValidateStack();

            foreach (var reference in this._searchStack)
            {
                assetStackText.Insert(0, reference.name + "/");
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(this._searchStack.Count < 2);

            if (GUILayout.Button("Back", GUILayout.Width(75)))
            {
                this._searchStack.Pop();
                this._searchTarget = this._searchStack.Peek();

                this.Search(false);
            }

            EditorGUILayout.TextField(assetStackText.ToString());

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            this._searchTarget = EditorGUILayout.ObjectField("Asset", this._searchTarget, typeof(Object), false);

            EditorGUI.BeginDisabledGroup(this._searchTarget == null);

            if (GUILayout.Button("Search"))
            {
                this._searchStack.Clear();

                this.Search(true);
            }

            if (GUILayout.Button("Delete"))
            {
                var assetToDelete = this._searchStack.Pop();

                this._searchTarget = this._searchStack.Count > 0 ? this._searchStack.Peek() : null;

                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(assetToDelete));
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                if (this._searchTarget)
                {
                    this.Search(false);
                }
            }

            EditorGUI.EndDisabledGroup();

            this.DisplayReferenceObject();
        }
        else
        {
            GUILayout.Label("This tool requires the serialization mode of assets to be set to 'Force Text'");
        }
    }

    #endregion

    #region Functions

    /// <summary>
    ///     Creates and displays the window
    /// </summary>
    [MenuItem("Assets/Find References In Project")]
    private static void Init()
    {
        var window = (AssetReferenceFinder)GetWindow(typeof(AssetReferenceFinder), false, "Asset Reference Finder");

        if(window._initialised)
        {
            var selectedObjectExists = window._searchTarget != null;

            window._searchTarget = Selection.activeObject;

            if (selectedObjectExists)
            {
                window._searchStack.Clear();
                window.Search(true);
            }
        }

        window._initialised = true;
    }

    /// <summary>
    ///     Validates the stack
    /// </summary>
    private void ValidateStack()
    {
        while (this._searchStack.Count > 0 && this._searchStack.Peek() == null)
        {
            this._searchStack.Pop();
        }
    }

    /// <summary>
    ///     Searches the project assets
    /// </summary>
    /// <param name="addToStack"></param>
    private void Search(bool addToStack)
    {
        var pathToAsset = AssetDatabase.GetAssetPath(this._searchTarget);
        var guidToFind = AssetDatabase.AssetPathToGUID(pathToAsset);

        this._referenceObjects.Clear();

        pathToAsset = AssetDatabase.GUIDToAssetPath(guidToFind);

        if (!string.IsNullOrEmpty(pathToAsset))
        {
            var searchText = string.Format("guid: {0}", guidToFind);

            this._searchTarget = AssetDatabase.LoadAssetAtPath<Object>(pathToAsset);

            if (addToStack)
            {
                this._searchStack.Push(this._searchTarget);
            }

            foreach (var fileExtension in FileExtensions)
            {
                var assetPaths = Directory.GetFiles(Application.dataPath, string.Format("*{0}", fileExtension), SearchOption.AllDirectories);

                foreach (var assetPath in assetPaths)
                {
                    var text = File.ReadAllText(assetPath);
                    var lines = text.Split('\n');

                    foreach (var line in lines)
                    {
                        if (line.Contains(searchText))
                        {
                            string pathToReferenceAsset;
                            string path;
                            Object asset = null;
                            ReferenceObject referenceObject = null;

                            pathToReferenceAsset = assetPath.Replace(Application.dataPath, string.Empty);
                            pathToReferenceAsset = pathToReferenceAsset.Replace(".meta", string.Empty);

                            path = "Assets" + pathToReferenceAsset;
                            path = path.Replace(@"\", "/");

                            asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                            referenceObject = this._referenceObjects.Find(x => x.Target == asset);

                            if (referenceObject == null)
                            {
                                this._referenceObjects.Add(new ReferenceObject(asset, path));
                            }

                            break;
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogError("no asset found for GUID: " + guidToFind);
        }
    }

    /// <summary>
    ///     Display reference objects
    /// </summary>
    private void DisplayReferenceObject()
    {
        this._scrollPosition = EditorGUILayout.BeginScrollView(this._scrollPosition);

        foreach (var referenceObject in this._referenceObjects)
        {
            var target = referenceObject.Target;

            if(target == null)
            {
                target = AssetDatabase.LoadAssetAtPath<Object>(referenceObject.Path);
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);

            if (target == null)
            {
                EditorGUILayout.TextField("Asset does not exist");
            }
            else
            {
                EditorGUILayout.ObjectField(new GUIContent(referenceObject.Name, referenceObject.Path), target, typeof(Object), false);
            }
            
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(target == null);

            if (GUILayout.Button("Search", GUILayout.Width(75)))
            {
                this._searchTarget = target;

                this.Search(true);

                break;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.EndScrollView();
    }

    #endregion

    #region Classes

    private class ReferenceObject
    {
        #region Fields

        public string Name { get; }
        public Object Target { get; }
        public string Path { get; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Creates an instance of <see cref="ReferenceObject"/>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="path"></param>
        public ReferenceObject(Object target, string path)
        {
            this.Name = target.name;
            this.Target = target;
            this.Path = path;
        }

        #endregion
    }

    #endregion
}