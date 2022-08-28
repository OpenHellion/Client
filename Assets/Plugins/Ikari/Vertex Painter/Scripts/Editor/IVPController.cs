/// <summary>
/// Ikari Vertex Painter © 2015 Created by KirbyRawr.
/// </summary>

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace Ikari.VertexPainter
{
    public class IVPController : EditorWindow
    {
        public static bool toolChanged;

        string editorPath;
        string internalResourcesPath;

        public bool erase;

        public bool rChannel = true;
        public bool gChannel = true;
        public bool bChannel = true;
        public bool aChannel = true;

        Tool toolSelected;
        bool guiChanged;
        Mesh assetMesh;

        bool waitingForInput = false;
        string nameOfInput = "";
        Vector2 scrollMainPage;
        Vector2 scrollEditorPage;
        Vector2 scrollHelpPage;
        Vector2 scrollSelectedObjects;
        string searchText = "";
        List<Color> colorClipboard;
        float pixelHeight;
        float resizeOffset = 9;
        bool resizingWindow;

        bool tryingToClear = false;
        bool tryingToClear2 = false;
        bool tryingToClear3 = false;
        bool finalClear = false;
        bool tryingTouninstall = false;
        bool tryingTouninstall2 = false;
        bool tryingTouninstall3 = false;
        bool finaluninstallation = false;

        string orderNumber = "";
        string questionString = "";
        string suggestionString = "";
        string bugString = "";
        string[] menuOptions = new string[] { "Main Menu", "Editor Menu", "Help Menu" };
        int menuOptionsIndex = 0;

        //Editor UI
        Color headerColor = new Color(0.65f, 0.65f, 0.65f, 1);
        Color backgroundColor = new Color(0.75f, 0.75f, 0.75f);
        Texture2D alphaTexture;

        public static bool hitting;
        public static List<GameObject> editedGameObjects = new List<GameObject>();
        public static List<GameObject> selectedTransforms = new List<GameObject>();
        string colorString;
        public static IVPData IVPData;

        [MenuItem("Window/Ikari/Vertex Painter")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(IVPController));
        }

        void OnEnable()
        {
            this.titleContent = new GUIContent("IVertex Painter");
            SubscribeEvents();
            SetPaths();
            GetEditorData();
            SetUIElements();
            LoadVariables();
        }

        void SubscribeEvents()
        {
            if (SceneView.onSceneGUIDelegate != this.OnSceneGUI) SceneView.onSceneGUIDelegate += this.OnSceneGUI;
            if (EditorApplication.playmodeStateChanged != HandleOnPlayModeChanged) { EditorApplication.playmodeStateChanged += HandleOnPlayModeChanged; }
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        void SetPaths()
        {
            editorPath = IVPMethods.Path.GetEditorPath(this);
            internalResourcesPath = Path.Combine(editorPath, "InternalResources");
        }

        void GetEditorData()
        {
            string editorDataFolder = Path.Combine(internalResourcesPath, "Data");
            string editorDataPath = Path.Combine(internalResourcesPath, "Data/IVPData.asset");

            IVPData = (IVPData)AssetDatabase.LoadAssetAtPath(editorDataPath, typeof(IVPData));

            if (IVPData == null)
            {
                if(!Directory.Exists(editorDataFolder))
                {
                    Directory.CreateDirectory(editorDataFolder);
                }
                AssetDatabase.CreateAsset(CreateInstance<IVPData>(), editorDataPath);
                IVPData = AssetDatabase.LoadAssetAtPath(editorDataPath, typeof(IVPData)) as IVPData;
                IVPData.Setup();
            }
        }

        void SetUIElements()
        {
            //Colors
            if (!EditorGUIUtility.isProSkin)
            {
                headerColor = new Color(165 / 255f, 165 / 255f, 165 / 255f, 1);
                backgroundColor = new Color(193 / 255f, 193 / 255f, 193 / 255f, 1);
            }
            else
            {
                headerColor = new Color(41 / 255f, 41 / 255f, 41 / 255f, 1);
                backgroundColor = new Color(56 / 255f, 56 / 255f, 56 / 255f, 1);
            }

            //Alpha Background
            alphaTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(internalResourcesPath, "Interface/IVPAlphaTexture.png"), typeof(Texture2D));
        }

        void DrawMenu(float yOffset)
        {
            Rect menuRect = GUILayoutUtility.GetLastRect();
            menuRect = new Rect(menuRect.x, menuRect.y + yOffset, position.width + 2, 20);

            if (menuRect.Contains(Event.current.mousePosition))
            {
                GUI.color = Color.gray;
            }

            menuOptionsIndex = EditorGUI.Popup(menuRect, menuOptionsIndex, menuOptions, "Box");

            GUI.color = Color.white;

            GUILayout.Space(14);
        }

        void DrawObjectProperties()
        {
            IVPData.Foldouts.objectProperties = DrawHeaderTitle("Object Properties", IVPData.Foldouts.objectProperties, headerColor);
            if (IVPData.Foldouts.objectProperties)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Save Mode: ", GUILayout.ExpandWidth(false));
                EditorGUI.BeginChangeCheck();
                SaveMode saveModeTemp = (SaveMode)EditorGUILayout.EnumPopup(IVPData.saveMode);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(IVPData, "Save Mode");
                    IVPData.saveMode = saveModeTemp;
                    EditorUtility.SetDirty(IVPData);
                }
                GUILayout.EndHorizontal();
            }
        }

        void DrawSelectedObjectsProperties()
        {
            IVPData.Foldouts.objectsSelected = DrawHeaderTitle("Selected Objects Properties", IVPData.Foldouts.objectsSelected, headerColor);
            if (IVPData.Foldouts.objectsSelected)
            {
                //No objects selected
                if (Selection.transforms.Length == 0)
                {
                    GUILayout.Label("Select a object/s for see the properties");
                    return;
                }

                GUILayout.Space(-5);

                searchText = IVPInterface.DrawSearchBar(searchText);
                scrollSelectedObjects = GUILayout.BeginScrollView(scrollSelectedObjects, GUILayout.Height(Mathf.Clamp(resizeOffset, 110, 235)));

                for (int i = 0; i < Selection.transforms.Length; i++)
                {
                    if (Selection.transforms[i].GetComponent<MeshFilter>() == null && Selection.transforms[i].GetComponent<SkinnedMeshRenderer>() == null)
                    {
                        GUILayout.Label(string.Concat(Selection.transforms[i].name, " doesn't have a 'Mesh Filter or Skin Mesh Renderer' component attached."), "HelpBox");
                    }

                    if (Selection.transforms[i].GetComponent<Collider>() == null)
                    {
                        GUILayout.Label(string.Concat(Selection.transforms[i].name, " doesn't have a 'Collider' component attached."), "HelpBox");
                    }

                    //Search bar
                    if (searchText.Contains(Selection.gameObjects[i].name.ToLower()) || string.IsNullOrEmpty(searchText))
                    {
                        if ((Selection.transforms[i].GetComponent<MeshFilter>() != null || Selection.transforms[i].GetComponent<SkinnedMeshRenderer>() != null) && Selection.transforms[i].GetComponent<Collider>() != null)
                        {
                            if (Selection.transforms[i].GetComponent<IVPObjectData>() == null)
                            {
                                if (GUILayout.Button(Selection.transforms[i].name, EditorStyles.foldout))
                                {
                                    IVPObjectData objectData = Selection.transforms[i].gameObject.AddComponent<IVPObjectData>();
                                    objectData.editorData.foldout = true;
                                }
                            }
                            else
                            {
                                IVPObjectData objectData = Selection.transforms[i].GetComponent<IVPObjectData>();
                                objectData.editorData.foldout = EditorGUILayout.Foldout(objectData.editorData.foldout, objectData.gameObject.name);
                                if (objectData.editorData.foldout)
                                {
                                    GUILayout.BeginVertical();
                                    if (!Selection.transforms[i].GetComponent<IVPObjectData>().editorData.showingVertex)
                                    {
                                        if (GUILayout.Button("Show Vertex Colors"))
                                        {
                                            objectData.originalMaterials = Selection.transforms[i].GetComponent<Renderer>().sharedMaterials;
                                            Undo.RecordObjects(new UnityEngine.Object[] { Selection.transforms[i].GetComponent<Renderer>(), objectData }, "Show Vertex Color");

                                            Material[] materials = new Material[Selection.transforms[i].GetComponent<Renderer>().sharedMaterials.Length];
                                            for (int m = 0; m < materials.Length; m++)
                                            {
                                                materials[m] = new Material(Shader.Find("Ikari Vertex Painter/Debug/Vertex Colors"));
                                            }
                                            Selection.transforms[i].GetComponent<Renderer>().sharedMaterials = materials;
                                            EditorUtility.SetDirty(Selection.transforms[i].GetComponent<Renderer>());
                                            objectData.editorData.showingVertex = true;
                                        }
                                    }
                                    else
                                    {
                                        GUILayout.BeginHorizontal();
                                        GUILayout.Label("Show Vertex Colors: ", GUILayout.ExpandWidth(true));
                                        //Red Color Button
                                        if (GUILayout.Button("", EditorStyles.textField, GUILayout.Width(20), GUILayout.Height(20)))
                                        {
                                            if (!objectData.editorData.showingRedColor)
                                            {
                                                Undo.RecordObject(this, "Show Red Color");
                                                objectData.editorData.showingRedColor = !objectData.editorData.showingRedColor;
                                                for (int s = 0; s < Selection.transforms[i].GetComponent<Renderer>().sharedMaterials.Length; s++)
                                                {
                                                    Selection.transforms[i].GetComponent<Renderer>().sharedMaterials[s].SetFloat("_RedChannel", 1);
                                                }
                                                objectData.vertexColors[0] = Color.red;
                                            }
                                            else
                                            {
                                                Undo.RecordObject(this, "Hide Red Color");
                                                objectData.editorData.showingRedColor = !objectData.editorData.showingRedColor;
                                                for (int s = 0; s < Selection.transforms[i].GetComponent<Renderer>().sharedMaterials.Length; s++)
                                                {
                                                    Selection.transforms[i].GetComponent<Renderer>().sharedMaterials[s].SetFloat("_RedChannel", 0);
                                                }
                                                objectData.vertexColors[0] = Color.clear;
                                            }
                                        }
                                        EditorGUI.DrawRect(new Rect(GUILayoutUtility.GetLastRect().x + 1, GUILayoutUtility.GetLastRect().y + 1, 18, 18), objectData.vertexColors[0]);
                                        GUI.color = new Color(0.3f, 0, 0);
                                        GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + 3, GUILayoutUtility.GetLastRect().y + 2, GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().height), "R", EditorStyles.whiteLabel);
                                        GUI.color = Color.white;

                                        //Green Color Button
                                        if (GUILayout.Button("", EditorStyles.textField, GUILayout.Width(20), GUILayout.Height(20)))
                                        {
                                            if (!objectData.editorData.showingGreenColor)
                                            {
                                                Undo.RecordObject(this, "Show Green Color");
                                                objectData.editorData.showingGreenColor = !objectData.editorData.showingGreenColor;
                                                for (int s = 0; s < Selection.transforms[i].GetComponent<Renderer>().sharedMaterials.Length; s++)
                                                {
                                                    Selection.transforms[i].GetComponent<Renderer>().sharedMaterials[s].SetFloat("_GreenChannel", 1);
                                                }
                                                objectData.vertexColors[1] = Color.green;
                                            }
                                            else
                                            {
                                                Undo.RecordObject(this, "Hide Green Color");
                                                objectData.editorData.showingGreenColor = !objectData.editorData.showingGreenColor;
                                                for (int s = 0; s < Selection.transforms[i].GetComponent<Renderer>().sharedMaterials.Length; s++)
                                                {
                                                    Selection.transforms[i].GetComponent<Renderer>().sharedMaterials[s].SetFloat("_GreenChannel", 0);
                                                }
                                                objectData.vertexColors[1] = Color.clear;
                                            }
                                        }
                                        EditorGUI.DrawRect(new Rect(GUILayoutUtility.GetLastRect().x + 1, GUILayoutUtility.GetLastRect().y + 1, 18, 18), objectData.vertexColors[1]);
                                        GUI.color = new Color(0, 0.3f, 0);
                                        GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + 3, GUILayoutUtility.GetLastRect().y + 2, GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().height), "G", EditorStyles.whiteLabel);
                                        GUI.color = Color.white;

                                        //Blue Color Button
                                        if (GUILayout.Button("", EditorStyles.textField, GUILayout.Width(20), GUILayout.Height(20)))
                                        {
                                            if (!objectData.editorData.showingBlueColor)
                                            {
                                                Undo.RecordObject(this, "Show Blue Color");
                                                objectData.editorData.showingBlueColor = !objectData.editorData.showingBlueColor;
                                                for (int s = 0; s < Selection.transforms[i].GetComponent<Renderer>().sharedMaterials.Length; s++)
                                                {
                                                    Selection.transforms[i].GetComponent<Renderer>().sharedMaterials[s].SetFloat("_BlueChannel", 1);
                                                }
                                                objectData.vertexColors[2] = Color.blue;
                                            }
                                            else
                                            {
                                                Undo.RecordObject(this, "Hide Blue Color");
                                                objectData.editorData.showingBlueColor = !objectData.editorData.showingBlueColor;
                                                for (int s = 0; s < Selection.transforms[i].GetComponent<Renderer>().sharedMaterials.Length; s++)
                                                {
                                                    Selection.transforms[i].GetComponent<Renderer>().sharedMaterials[s].SetFloat("_BlueChannel", 0);
                                                }

                                                objectData.vertexColors[2] = Color.clear;
                                            }
                                        }
                                        EditorGUI.DrawRect(new Rect(GUILayoutUtility.GetLastRect().x + 1, GUILayoutUtility.GetLastRect().y + 1, 18, 18), objectData.vertexColors[2]);
                                        GUI.color = new Color(0, 0, 0.3f);
                                        GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + 3, GUILayoutUtility.GetLastRect().y + 2, GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().height), "B", EditorStyles.whiteLabel);
                                        GUI.color = Color.white;

                                        //Alpha Color Button
                                        GUI.DrawTexture(new Rect(GUILayoutUtility.GetLastRect().x + 24, GUILayoutUtility.GetLastRect().y, 20, 20), alphaTexture as Texture2D);
                                        GUI.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
                                        if (GUILayout.Button("", EditorStyles.textField, GUILayout.Width(20), GUILayout.Height(20)))
                                        {
                                            if (!objectData.editorData.showingAlphaColor)
                                            {
                                                Undo.RecordObject(this, "Show Alpha Color");
                                                objectData.editorData.showingAlphaColor = !objectData.editorData.showingAlphaColor;
                                                for (int s = 0; s < Selection.transforms[i].GetComponent<Renderer>().sharedMaterials.Length; s++)
                                                {
                                                    Selection.transforms[i].GetComponent<Renderer>().sharedMaterials[s].SetFloat("_AlphaChannel", 1);
                                                }
                                                objectData.vertexColors[3] = new Color(0.85f, 0.85f, 0.85f, 1f);
                                            }
                                            else
                                            {
                                                Undo.RecordObject(this, "Hide Alpha Color");
                                                objectData.editorData.showingAlphaColor = !objectData.editorData.showingAlphaColor;
                                                for (int s = 0; s < Selection.transforms[i].GetComponent<Renderer>().sharedMaterials.Length; s++)
                                                {
                                                    Selection.transforms[i].GetComponent<Renderer>().sharedMaterials[s].SetFloat("_AlphaChannel", 0);
                                                }
                                                objectData.vertexColors[3] = Color.clear;
                                            }
                                        }
                                        GUI.color = new Color(0, 0, 0.3f);
                                        GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + 3, GUILayoutUtility.GetLastRect().y + 2, GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().height), "A", EditorStyles.label);
                                        GUI.color = Color.white;
                                        GUILayout.EndHorizontal();

                                        if (GUILayout.Button("Show Normal"))
                                        {
                                            objectData = Selection.transforms[i].gameObject.GetComponent<IVPObjectData>();
                                            Undo.RecordObjects(new UnityEngine.Object[] { Selection.transforms[i].GetComponent<Renderer>(), objectData }, "Show Normal Color");
                                            Selection.transforms[i].GetComponent<Renderer>().sharedMaterials = objectData.originalMaterials;
                                            EditorUtility.SetDirty(Selection.transforms[i].GetComponent<Renderer>());
                                            objectData.editorData.showingVertex = false;
                                            Resources.UnloadUnusedAssets();
                                        }

                                    }

                                    if (Selection.transforms[i].GetComponent<IVPObjectData>().editorData.showingWireframe)
                                    {
                                        if (GUILayout.Button("Show Wireframe"))
                                        {
                                            Undo.RecordObjects(new UnityEngine.Object[] { Selection.transforms[i].GetComponent<Renderer>(), objectData }, "Show Wireframe");
                                            EditorUtility.SetSelectedRenderState(Selection.transforms[i].GetComponent<Renderer>(), EditorSelectedRenderState.Wireframe);
                                            EditorUtility.SetDirty(Selection.transforms[i].GetComponent<Renderer>());
                                            objectData.editorData.showingWireframe = false;
                                        }
                                    }

                                    if (!Selection.transforms[i].GetComponent<IVPObjectData>().editorData.showingWireframe)
                                    {
                                        if (GUILayout.Button("Hide Wireframe", GUILayout.MaxWidth(position.width)))
                                        {
                                            Undo.RecordObjects(new UnityEngine.Object[] { Selection.transforms[i].GetComponent<Renderer>(), objectData }, "Hide Wireframe");
                                            EditorUtility.SetSelectedRenderState(Selection.transforms[i].GetComponent<Renderer>(), EditorSelectedRenderState.Hidden);
                                            EditorUtility.SetDirty(Selection.transforms[i].GetComponent<Renderer>());
                                            objectData.editorData.showingWireframe = true;
                                        }
                                    }

                                    if (GUILayout.Button("Copy Vertex Color"))
                                    {
                                        colorClipboard = Selection.transforms[i].GetComponent<MeshFilter>().sharedMesh.colors.ToList();
                                    }

                                    if (colorClipboard != null)
                                    {
                                        if (GUILayout.Button("Paste Vertex Color"))
                                        {
                                            MeshFilter meshFilter = Selection.transforms[i].GetComponent<MeshFilter>();
                                            Mesh meshClone = Mesh.Instantiate(meshFilter.sharedMesh) as Mesh;
                                            meshClone.name = meshFilter.sharedMesh.name;
                                            IVPVariables.Data.ActualMesh = meshFilter.mesh = meshClone;
                                            objectData.instanceID = objectData.GetInstanceID();
                                            objectData.instance = true;


                                            if (colorClipboard.Count > meshFilter.sharedMesh.vertexCount)
                                            {
                                                int exceededVertex = colorClipboard.Count - meshFilter.sharedMesh.vertexCount;
                                                int indexList = colorClipboard.Count - exceededVertex;
                                                List<Color> tempList = colorClipboard;
                                                tempList.RemoveRange(indexList, exceededVertex);
                                                meshFilter.sharedMesh.colors = tempList.ToArray();
                                            }
                                            else
                                                meshFilter.sharedMesh.colors = colorClipboard.ToArray();
                                        }
                                    }
                                    GUILayout.EndVertical();
                                    GUILayout.Space(10);
                                }
                            }
                        }
                    }
                }

                GUILayout.Space(-2f);
                GUILayout.EndScrollView();
                GUILayout.Space(-4f);

                EditorGUIUtility.AddCursorRect(new Rect(GUILayoutUtility.GetLastRect().x - 1, GUILayoutUtility.GetLastRect().y - 5, position.width, 5), MouseCursor.ResizeVertical);
                if (new Rect(GUILayoutUtility.GetLastRect().x - 1, GUILayoutUtility.GetLastRect().y - 5, position.width, 5).Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                {
                    GUI.FocusControl("");
                    resizingWindow = true;
                }

                if (resizingWindow)
                {
                    resizeOffset = Event.current.mousePosition.y - 108;
                    if (Event.current.type == EventType.MouseUp)
                    {
                        resizingWindow = false;
                    }
                }

            }
        }

        void OnUndoRedo()
        {
            for (int i = 0; i < IVPVariables.Data.EditedObjects.Count; i++)
            {
                IVPVariables.Data.EditedObjects[i].GetComponent<MeshFilter>().sharedMesh.colors = IVPVariables.Data.EditedObjects[i].GetComponent<MeshFilter>().sharedMesh.colors;
            }
            SceneView.RepaintAll();
        }

        void OnGUI()
        {
            //Set the minimum size of the screen.
            this.minSize = new Vector2(350, 650);
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), backgroundColor);

            //GUILayout.Space(120);
            GUILayout.Space(20);

            //Draw Logo
            //GUI.DrawTexture(new Rect(GUILayoutUtility.GetLastRect().x - 25 + pixelWidth / 2, 14, 390, 90), logo);

            DrawMenu(15);

            if (menuOptionsIndex == 0)
            {
                scrollMainPage = GUILayout.BeginScrollView(scrollMainPage);

                DrawObjectProperties();
                DrawSelectedObjectsProperties();


                #region Main Menu - Color Properties
                IVPData.Foldouts.color = DrawHeaderTitle("Color Properties", IVPData.Foldouts.color, headerColor);
                if (IVPData.Foldouts.color)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label("Primary Color: ", GUILayout.ExpandWidth(false));

                    //Box containing the primary color.
                    GUI.Box(new Rect(GUILayoutUtility.GetLastRect().x + GUILayoutUtility.GetLastRect().width - 1, GUILayoutUtility.GetLastRect().y - 1, 21, 21), "");
                    //Alpha background label of primary color
                    GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + GUILayoutUtility.GetLastRect().width - 2, GUILayoutUtility.GetLastRect().y - 1, 23, 23), alphaTexture);
                    EditorGUI.DrawRect(new Rect(GUILayoutUtility.GetLastRect().x + GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().y, 19, 19), IVPData.primaryColor);

                    //A dynamic space based on the window width
                    GUILayout.Space(position.width - 233);

                    //Swap button
                    if (GUI.Button(new Rect(GUILayoutUtility.GetLastRect().x + 25, GUILayoutUtility.GetLastRect().y + 2, position.width - 255, 20), "Swap"))
                    {
                        Undo.RecordObject(this, "Swap Colors");
                        //Here we save temporaly the paintColor for the change.
                        Color tempColor = IVPData.primaryColor;
                        IVPData.primaryColor = IVPData.secondaryColor;
                        IVPData.secondaryColor = tempColor;
                    }

                    GUILayout.Label("Secondary Color: ", GUILayout.ExpandWidth(false));

                    //Box containing the color of paintMode
                    GUI.Box(new Rect(GUILayoutUtility.GetLastRect().x + GUILayoutUtility.GetLastRect().width - 1, GUILayoutUtility.GetLastRect().y - 1, 21, 21), "");
                    //Alpha background label of paintMode color
                    GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + GUILayoutUtility.GetLastRect().width - 2, GUILayoutUtility.GetLastRect().y - 1, 23, 23), alphaTexture);
                    EditorGUI.DrawRect(new Rect(GUILayoutUtility.GetLastRect().x + GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().y, 19, 19), IVPData.secondaryColor);


                    //Here we end the group of the colors.
                    GUILayout.EndHorizontal();

                    //A space of 5 pixels.
                    GUILayout.Space(6);

                    GUILayout.BeginHorizontal();
                    float colorValue = 0;
                    if (EditorGUIUtility.isProSkin)
                    {
                        colorValue = 0.64f;
                    }
                    else
                    {
                        colorValue = 0.84f;
                    }

                    //Red Color Button
                    if (GUILayout.Button("", EditorStyles.textField, GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        Undo.RecordObject(this, "Select Red Color");
                        IVPData.primaryColor = Color.red;
                    }
                    EditorGUI.DrawRect(new Rect(GUILayoutUtility.GetLastRect().x + 1, GUILayoutUtility.GetLastRect().y + 1, 18, 18), new Color(colorValue, 0, 0));
                    GUI.color = new Color(0.3f, 0, 0);
                    GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + 3, GUILayoutUtility.GetLastRect().y + 2, GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().height), "R", EditorStyles.whiteLabel);
                    GUI.color = Color.white;

                    //Green Color Button
                    if (GUILayout.Button("", EditorStyles.textField, GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        Undo.RecordObject(this, "Select Green Color");
                        IVPData.primaryColor = Color.green;
                    }
                    EditorGUI.DrawRect(new Rect(GUILayoutUtility.GetLastRect().x + 1, GUILayoutUtility.GetLastRect().y + 1, 18, 18), new Color(0, colorValue, 0));
                    GUI.color = new Color(0, 0.3f, 0);
                    GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + 3, GUILayoutUtility.GetLastRect().y + 2, GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().height), "G", EditorStyles.whiteLabel);
                    GUI.color = Color.white;

                    //Blue Color Button
                    if (GUILayout.Button("", EditorStyles.textField, GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        Undo.RecordObject(this, "Select Blue Color");
                        IVPData.primaryColor = Color.blue;
                    }
                    EditorGUI.DrawRect(new Rect(GUILayoutUtility.GetLastRect().x + 1, GUILayoutUtility.GetLastRect().y + 1, 18, 18), new Color(0, 0, colorValue));
                    GUI.color = new Color(0, 0, 0.3f);
                    GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + 3, GUILayoutUtility.GetLastRect().y + 2, GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().height), "B", EditorStyles.whiteLabel);
                    GUI.color = Color.white;

                    //Alpha Color Button
                    GUI.DrawTexture(new Rect(GUILayoutUtility.GetLastRect().x + 24, GUILayoutUtility.GetLastRect().y, 20, 20), alphaTexture);
                    GUI.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
                    if (GUILayout.Button("", EditorStyles.textField, GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        Undo.RecordObject(this, "Select Alpha Color");
                        IVPData.primaryColor = Color.clear;
                    }
                    GUI.color = new Color(0.85f, 0.85f, 0.85f, 1f);
                    GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + 3, GUILayoutUtility.GetLastRect().y + 2, GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().height), "A", EditorStyles.label);
                    GUI.color = Color.white;

                    GUILayout.BeginVertical();
                    GUILayout.Space(6);
                    //If we are on paintMode use that color for the colorField.
                    IVPData.primaryColor = EditorGUILayout.ColorField(IVPData.primaryColor);

                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }
                #endregion

                #region Main Menu - Tool Properties
                IVPData.Foldouts.tool = DrawHeaderTitle("Tool Properties", IVPData.Foldouts.tool, headerColor);
                if (IVPData.Foldouts.tool)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Paint Mode: ", GUILayout.ExpandWidth(false));
                    IVPData.paintMode = (PaintMode)EditorGUILayout.EnumPopup(IVPData.paintMode, GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Separator();

                    //Brush
                    if (IVPData.paintMode == PaintMode.Brush)
                    {
                        //Brush Size property
                        IVPData.brushSize = IVPInterface.DrawFloatField("Brush Size", IVPData.brushSize);
                        IVPData.brushSize = IVPInterface.DrawSlider(IVPData.brushSize, 0.1f, 5, "Brush Size");
                        IVPData.brushSize = Mathf.Max(0, IVPData.brushSize);

                        //Brush Hardness property
                        IVPData.brushHardness = IVPInterface.DrawFloatField("Brush Hardness", IVPData.brushHardness);
                        IVPData.brushHardness = IVPInterface.DrawSlider(IVPData.brushHardness, 0f, 1f, "Brush Hardness");
                        IVPData.brushHardness = Mathf.Clamp01(IVPData.brushHardness);

                        //Brush Strength property
                        IVPData.brushStrength = IVPInterface.DrawFloatField("Brush Strength", IVPData.brushStrength);
                        IVPData.brushStrength = IVPInterface.DrawSlider(IVPData.brushStrength, 0f, 1f, "Brush Strength");
                        IVPData.brushStrength = Mathf.Clamp01(IVPData.brushStrength);

                        //Angle Limit Property
                        IVPData.brushAngleLimit = IVPInterface.DrawFloatField("Brush Angle Limit", IVPData.brushAngleLimit);
                        IVPData.brushAngleLimit = IVPInterface.DrawSlider(IVPData.brushAngleLimit, 0f, 180f, "Brush Angle Limit");
                    }

                    //Bucket
                    if (IVPData.paintMode == PaintMode.Bucket)
                    {

                    }
                }
                #endregion

                #region Main Menu - Paint Properties
                IVPData.Foldouts.paint = DrawHeaderTitle("Paint Properties", IVPData.Foldouts.paint, headerColor);
                if (IVPData.Foldouts.paint)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Selection Mode: ", GUILayout.ExpandWidth(false));
                    IVPData.selectionMode = (SelectionMode)EditorGUILayout.EnumPopup(IVPData.selectionMode, GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();
                    //GUILayout.Label("Paint mask: ", GUILayout.ExpandWidth(false));
                    //EditorGUILayout.LayerField();
                    IVPData.layerMask = LayerMaskField(
                      "Layer Mask", IVPData.layerMask);
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Paint Layers: ", GUILayout.ExpandWidth(false));

                    //Red Channel Toggle
                    IVPInterface.DrawColorToggle(IVPMethods.Reflection.GetFieldName<IVPController>((IVPController x) => x.rChannel), "Red Channel", "R", rChannel, Color.red);

                    //Green Channel Toggle
                    IVPInterface.DrawColorToggle(IVPMethods.Reflection.GetFieldName<IVPController>((IVPController x) => x.gChannel), "Green Channel", "G", gChannel, Color.green);

                    //Blue Channel Toggle
                    IVPInterface.DrawColorToggle(IVPMethods.Reflection.GetFieldName<IVPController>((IVPController x) => x.bChannel), "Blue Channel", "B", bChannel, Color.blue);

                    //Alpha Channel Toggle
                    IVPInterface.DrawColorToggle(IVPMethods.Reflection.GetFieldName<IVPController>((IVPController x) => x.aChannel), "Alpha Channel", "A", aChannel, Color.grey);

                    GUILayout.EndHorizontal();
                }
                #endregion

                #region Main Menu - Gizmo Properties
                IVPData.Foldouts.gizmos = DrawHeaderTitle("Gizmo Properties", IVPData.Foldouts.gizmos, headerColor);
                if (IVPData.Foldouts.gizmos)
                {

                    IVPData.handleColor = EditorGUILayout.ColorField("Handle Color", IVPData.handleColor);
                    IVPData.outlineHandleColor = EditorGUILayout.ColorField("Outline Handle Color", IVPData.outlineHandleColor);


                    if (GUILayout.Button("Solid Handle", "Toggle"))
                    {
                        Undo.RecordObject(this, "Solid Handle Value");
                        IVPData.solidHandle = !IVPData.solidHandle;
                    }

                    if (IVPData.solidHandle)
                    {
                        GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + 0.5f, GUILayoutUtility.GetLastRect().y, 20, 20), "X", EditorStyles.boldLabel);
                    }

                    if (GUILayout.Button("Draw Handle Outline", "Toggle"))
                    {
                        Undo.RecordObject(this, "Draw Handle Outline Value");
                        IVPData.drawHandleOutline = !IVPData.drawHandleOutline;
                    }
                    if (IVPData.drawHandleOutline)
                        GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + 0.5f, GUILayoutUtility.GetLastRect().y, 20, 20), "X", EditorStyles.boldLabel);

                    if (GUILayout.Button("Draw Handle Angle", "Toggle"))
                    {
                        Undo.RecordObject(this, "Draw Handle Angle Value");
                        IVPData.drawHandleAngle = !IVPData.drawHandleAngle;
                    }
                    if (IVPData.drawHandleAngle)
                    {
                        GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + 0.5f, GUILayoutUtility.GetLastRect().y, 20, 20), "X", EditorStyles.boldLabel);
                    }
                }
                #endregion

                if (!IVPVariables.Interaction.Painting)
                {

                    //Button for painting
                    if (GUILayout.Button("Start Painting"))
                    {
                        IVPVariables.Interaction.Painting = true;
                        selectedTransforms.Clear();
                        editedGameObjects.Clear();
                        IVPVariables.Raycast.Raycasting = true;
                    }
                }
                else
                {
                    if (GUILayout.Button("Stop Painting"))
                    {
                        IVPVariables.Interaction.Painting = false;
                        IVPVariables.Raycast.Raycasting = false;
                        IVPVariables.Raycast.ResetHit();
                        HandleUtility.Repaint();
                    }
                }
                GUILayout.EndScrollView();
            }

            #region Editor Menu
            if (menuOptionsIndex == 1)
            {
                scrollEditorPage = GUILayout.BeginScrollView(scrollEditorPage);
                GUILayout.Space(-5);
                GUILayout.Label("");
                GUILayout.Space(-16);

                IVPData.Foldouts.hotkeys = DrawHeaderTitle("Hotkeys", IVPData.Foldouts.hotkeys, headerColor);
                if (IVPData.Foldouts.hotkeys)
                {
                    GUILayout.Space(5);
                    IVPData.Hotkeys.paint.enabled = HotkeyFunction("Paint", IVPData.Hotkeys.paint.data, IVPData.Hotkeys.paint.enabled);
                    GUILayout.Space(1);
                    IVPData.Hotkeys.erase.enabled = HotkeyFunction("Erase", IVPData.Hotkeys.erase.data, IVPData.Hotkeys.erase.enabled);
                    GUILayout.Space(1);
                    IVPData.Hotkeys.increaseSize.enabled = HotkeyFunction("Increase Size", IVPData.Hotkeys.increaseSize.data, IVPData.Hotkeys.increaseSize.enabled);
                    GUILayout.Space(1);
                    IVPData.Hotkeys.decreaseSize.enabled = HotkeyFunction("Decrease Size", IVPData.Hotkeys.decreaseSize.data, IVPData.Hotkeys.decreaseSize.enabled);
                    GUILayout.Space(1);
                    IVPData.Hotkeys.showVertexColors.enabled = HotkeyFunction("Show Vertex Colors", IVPData.Hotkeys.showVertexColors.data, IVPData.Hotkeys.showVertexColors.enabled);
                    GUILayout.Space(1);
                    IVPData.Hotkeys.copyVertexColors.enabled = HotkeyFunction("Copy Vertex Colors", IVPData.Hotkeys.copyVertexColors.data, IVPData.Hotkeys.copyVertexColors.enabled);
                    GUILayout.Space(1);
                    IVPData.Hotkeys.pasteVertexColors.enabled = HotkeyFunction("Paste Vertex Colors", IVPData.Hotkeys.pasteVertexColors.data, IVPData.Hotkeys.pasteVertexColors.enabled);

                }

                IVPData.Foldouts.uninstaller = DrawHeaderTitle("Uninstaller", IVPData.Foldouts.uninstaller, headerColor);
                if (IVPData.Foldouts.uninstaller)
                {

                    if (!tryingTouninstall || !tryingToClear)
                    {
                        GUILayout.Space(5);
                        GUILayout.Label("Since Ikari Vertex Painter uses a script on gameobjects\nfor save data the only way to delete this asset is doing a\nuninstaller.\n\nIf you want to uninstall it first you need to use the clear\nbutton, use it in all the scenes you painted, when you are\ndone use the uninstall button.\n\nBoth buttons will ask you for a confirmation and will give\nyou some info");
                        GUILayout.Space(10);
                        GUILayout.Label("1- If you wish to uninstall first press the clear button:");
                        if (GUILayout.Button("Clear Scripts"))
                        {
                            tryingTouninstall = true;
                            tryingToClear = true;
                            tryingToClear2 = true;
                        }

                        GUILayout.Label("2- If you are done with clear press the uninstall button:");
                        if (GUILayout.Button("Uninstall Ikari Vertex Painter"))
                        {
                            tryingToClear = true;
                            tryingTouninstall = true;
                            tryingTouninstall2 = true;
                        }
                    }

                    if (tryingToClear2)
                    {
                        GUILayout.Label("It seems that you are trying to clear the scripts,\nkeep in mind that the process will delete:\n\n-Saved data of objects (Not colors)");

                        if (GUILayout.Button("I don't want to clear the scripts!"))
                        {
                            tryingToClear = false;
                            tryingToClear2 = false;
                        }
                        if (GUILayout.Button("I read all the info and i won't blame the developer for\n lose anything."))
                        {
                            tryingToClear2 = false;
                            tryingToClear3 = true;
                        }
                    }

                    if (tryingToClear3)
                    {
                        GUILayout.Label("The steps for clear are:\n1- Open the scene in which you painted objects\n2- Press clear button\n3-Save scene and if you need, open other and repeat");

                        if (GUILayout.Button("I don't want to clear the scripts!"))
                        {
                            tryingToClear = false;
                            tryingToClear2 = false;
                            tryingToClear3 = false;
                            tryingTouninstall = false;
                        }
                        if (GUILayout.Button("Clear scripts from this scene"))
                        {
                            finalClear = true;
                        }
                        if (GUILayout.Button("I finished!"))
                        {
                            tryingToClear = false;
                            tryingToClear2 = false;
                            tryingToClear3 = false;
                            tryingTouninstall = false;
                        }
                    }

                    if (finalClear)
                    {
                        IVPObjectDataEditor.UninstallIt();
                        finalClear = false;
                    }

                    if (tryingTouninstall2)
                    {
                        GUILayout.Label("It seems that you are trying to delete Ikari Vertex Painter,\nkeep in mind that the process will delete:\n\n-Saved data of objects\n-The entire Ikari Vertex Painter folder\n-Shaders of Ikari Vertex Painter");

                        if (GUILayout.Button("I don't want to uninstall Ikari Vertex Painter!"))
                        {
                            tryingTouninstall = false;
                            tryingTouninstall2 = false;
                            tryingToClear = false;
                        }
                        if (GUILayout.Button("I read all the info and i won't blame the developer for\n lose anything."))
                        {
                            tryingTouninstall = true;
                            tryingTouninstall2 = false;
                            tryingTouninstall3 = true;
                        }
                    }

                    if (tryingTouninstall3)
                    {
                        GUILayout.Label("The developer will miss you :(\nPress the button if you really want to uninstall it");
                        if (GUILayout.Button("I don't want to uninstall Ikari Vertex Painter!"))
                        {
                            tryingTouninstall = false;
                            tryingTouninstall2 = false;
                            tryingTouninstall3 = false;
                            tryingToClear = false;
                        }

                        if (GUILayout.Button("Uninstall it!"))
                        {
                            tryingTouninstall = false;
                            tryingTouninstall2 = false;
                            tryingTouninstall3 = false;
                            tryingToClear = false;
                            finaluninstallation = true;
                        }
                    }

                    if (finaluninstallation)
                    {


                    }
                }
                GUILayout.EndScrollView();
            }
            #endregion

            #region Help Menu
            if (menuOptionsIndex == 2)
            {

                scrollHelpPage = GUILayout.BeginScrollView(scrollHelpPage, GUIStyle.none, "VerticalScrollBar");
                GUILayout.Space(-5);
                GUILayout.Label("");
                GUILayout.Space(-16);

                IVPData.Foldouts.questions = DrawHeaderTitle("Questions", IVPData.Foldouts.questions, headerColor);
                if (IVPData.Foldouts.questions)
                {
                    GUILayout.Space(5);

                    GUILayout.Label("Write down your question.\nPlease try to explain it with details.\nIt would be good if you add at least a smiley face :)");
                    questionString = GUILayout.TextArea(questionString);

                    GUILayout.Label("Write down your order number.\nYou can get it from the confirmation mail of the purchase");
                    orderNumber = EditorGUILayout.TextField(orderNumber);

                    if (GUILayout.Button("Make the question"))
                    {
                        string tempString = questionString.Replace("\n", "%0A");
                        Application.OpenURL("mailto:neo5icekcore@gmail.com?Subject=Question - Ikari Vertex Painter&body=" + tempString + "%0AOrderNumber: " + orderNumber);
                    }
                }

                IVPData.Foldouts.suggestions = DrawHeaderTitle("Suggestions", IVPData.Foldouts.suggestions, headerColor);
                if (IVPData.Foldouts.suggestions)
                {
                    GUILayout.Space(5);

                    GUILayout.Label("Write down your suggestion.\nPlease try to explain it with details.\nIt would be good if you add at least a smiley face :)");
                    suggestionString = GUILayout.TextArea(suggestionString);

                    GUILayout.Label("Write down your order number.\nYou can get it from the confirmation mail of the purchase");
                    orderNumber = EditorGUILayout.TextField(orderNumber);

                    if (GUILayout.Button("Make the suggestion"))
                    {
                        string tempString = suggestionString.Replace("\n", "%0A");
                        Application.OpenURL("mailto:neo5icekcore@gmail.com?Subject=Suggestion - Ikari Vertex Painter&body=" + tempString + "%0AOrderNumber: " + orderNumber);
                    }
                }

                IVPData.Foldouts.bugs = DrawHeaderTitle("Bugs", IVPData.Foldouts.bugs, headerColor);
                if (IVPData.Foldouts.bugs)
                {
                    GUILayout.Space(5);

                    GUILayout.Label("Write down the bug/error you want to report.\nPlease try to explain it with details.\nI will try to fix it as soon as i can, and thanks!");
                    bugString = GUILayout.TextArea(bugString);

                    GUILayout.Label("Write down your order number.\nYou can get it from the confirmation mail of the purchase");
                    orderNumber = EditorGUILayout.TextField(orderNumber);

                    if (GUILayout.Button("Make the bug/error report"))
                    {
                        string tempString = bugString.Replace("\n", "%0A");
                        Application.OpenURL("mailto:neo5icekcore@gmail.com?Subject=Bug Report - Ikari Vertex Painter&body=" + tempString + "%0AOrderNumber: " + orderNumber);
                    }
                }
                GUILayout.EndScrollView();
            }
            #endregion

            if (GUI.changed)
            {
                guiChanged = true;
            }
            Repaint();
        }

        private LayerMask LayerMaskField(string label, LayerMask layerMask)
        {
            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (layerName != "")
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }
            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                    maskWithoutEmpty |= (1 << i);
            }
            maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());
            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << layerNumbers[i]);
            }
            layerMask.value = mask;
            return layerMask;
        }

        void OnDisable()
        {
            UnsubscribeEvents();
            SaveVariables();
        }

        void OnDestroy()
        {
            UnsubscribeEvents();
            SaveVariables();
        }

        void UnsubscribeEvents()
        {
            if (SceneView.onSceneGUIDelegate == this.OnSceneGUI) SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            if (EditorApplication.playmodeStateChanged == HandleOnPlayModeChanged) { EditorApplication.playmodeStateChanged -= HandleOnPlayModeChanged; }
            if (Undo.undoRedoPerformed == OnUndoRedo) { Undo.undoRedoPerformed -= OnUndoRedo; }
        }

        void OnLostFocus()
        {
            if (guiChanged)
            {
                //SaveVariables();
                guiChanged = false;
            }
        }

        void OnSceneGUI(SceneView sceneView)
        {
            //Raycast
            IVPRaycast.DoRaycast();

            //Handle Gizmo
            IVPGizmos.DrawHandle();

            //Paint
            IVPPaint.DoPaint();

            //Hotkeys
            IVPHotkeys.DoHotkeys();
        }

        public void DrawHeaderTitle(string title, Color backgroundColor)
        {

            GUILayout.Space(0);
            GUI.color = backgroundColor;
            GUI.Box(new Rect(1, GUILayoutUtility.GetLastRect().y + 4, position.width, 27), "");
            GUI.color = Color.white;
            GUILayout.Space(4);
            GUILayout.Label(title, EditorStyles.largeLabel);
        }

        public bool DrawHeaderTitle(string title, bool foldoutProperty, Color backgroundColor)
        {
            GUILayout.Space(0);
            Rect lastRect = GUILayoutUtility.GetLastRect();
            GUI.Box(new Rect(1, lastRect.y + 4, position.width, 27), "");
            lastRect = GUILayoutUtility.GetLastRect();
            EditorGUI.DrawRect(new Rect(lastRect.x, lastRect.y + 5f, position.width + 1, 25f), headerColor);
            GUI.Label(new Rect(lastRect.x, lastRect.y + 10, position.width, 25), title);
            GUI.color = Color.clear;
            if (GUI.Button(new Rect(0, lastRect.y + 4, position.width, 27), ""))
            {
                foldoutProperty = !foldoutProperty;
            }
            GUI.color = Color.white;
            GUILayout.Space(30);
            if (foldoutProperty) { GUILayout.Space(5); }
            return foldoutProperty;
        }

        public bool HotkeyFunction(string nameOfAction, Event hotkeyVariable, bool hotkeyState)
        {

            GUILayout.Label(nameOfAction + ": ", GUILayout.ExpandWidth(false));
            GUILayout.BeginHorizontal();
            if (hotkeyState)
            {
                if (GUILayout.Button(hotkeyVariable.keyCode.ToString()))
                {

                    if (!waitingForInput)
                    {
                        nameOfInput = nameOfAction;
                        waitingForInput = true;
                    }
                    else
                        nameOfInput = nameOfAction;

                }

                if (GUILayout.Button("Control", EditorStyles.toggle, GUILayout.ExpandWidth(false)))
                {
                    if (!hotkeyVariable.control)
                        hotkeyVariable.control = true;
                    else
                        hotkeyVariable.control = false;

                }
                if (hotkeyVariable.control)
                    GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + 0.5f, GUILayoutUtility.GetLastRect().y, 20, 20), "X", EditorStyles.boldLabel);

                if (GUILayout.Button("Shift", EditorStyles.toggle, GUILayout.ExpandWidth(false)))
                {
                    Undo.RecordObject(this, nameOfAction + " Hotkey Shift Value");
                    if (!hotkeyVariable.shift)
                        hotkeyVariable.shift = true;
                    else
                        hotkeyVariable.shift = false;

                }
                if (hotkeyVariable.shift)
                    GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + 0.5f, GUILayoutUtility.GetLastRect().y, 20, 20), "X", EditorStyles.boldLabel);

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    if (GUILayout.Button("Alt", EditorStyles.toggle, GUILayout.ExpandWidth(false)))
                    {
                        Undo.RecordObject(this, nameOfAction + " Hotkey Alt Value");
                        if (!hotkeyVariable.alt)
                            hotkeyVariable.alt = true;
                        else
                            hotkeyVariable.alt = false;
                    }
                    if (hotkeyVariable.alt)
                        GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + 0.5f, GUILayoutUtility.GetLastRect().y, 20, 20), "X", EditorStyles.boldLabel);
                }

                if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    if (GUILayout.Button("Command", EditorStyles.toggle, GUILayout.ExpandWidth(false)))
                    {
                        Undo.RecordObject(this, nameOfAction + " Hotkey Command Value");
                        if (!hotkeyVariable.command)
                            hotkeyVariable.command = true;
                        else
                            hotkeyVariable.command = false;
                    }
                    if (hotkeyVariable.command)
                        GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + 0.5f, GUILayoutUtility.GetLastRect().y, 20, 20), "X", EditorStyles.boldLabel);
                }
            }

            if (GUILayout.Button("Enable", EditorStyles.toggle, GUILayout.ExpandWidth(false)))
            {
                Undo.RecordObject(this, nameOfAction + " Hotkey Value");
                hotkeyState = !hotkeyState;
            }
            if (hotkeyState)
                GUI.Label(new Rect(GUILayoutUtility.GetLastRect().x + 0.5f, GUILayoutUtility.GetLastRect().y, 20, 20), "X", EditorStyles.boldLabel);

            GUILayout.EndHorizontal();

            if (waitingForInput && Event.current.isKey && Event.current.type == EventType.KeyUp && nameOfInput == nameOfAction)
            {
                Undo.RecordObject(this, nameOfAction + " Hotkey Keycode Value");
                if (Event.current.keyCode != KeyCode.Escape)
                {
                    hotkeyVariable.keyCode = Event.current.keyCode;
                    waitingForInput = false;
                    Event.current.Use();
                }
                else
                {
                    hotkeyVariable.keyCode = KeyCode.None;
                    waitingForInput = false;
                    Event.current.Use();
                }
            }
            return hotkeyState;
        }

        void HandleOnPlayModeChanged()
        {
            // This method is run whenever the playmode state is changed.

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                SaveVariables();
            }
        }

        void SaveVariables()
        {
            //AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(IVPData);
        }

        void LoadVariables()
        {
            rChannel = IVPData.rChannel;
            gChannel = IVPData.gChannel;
            bChannel = IVPData.bChannel;
            aChannel = IVPData.aChannel;
        }

        void Update()
        {
            IVPVariables.Data.SelectedObjects = Selection.gameObjects;
        }
    }
}