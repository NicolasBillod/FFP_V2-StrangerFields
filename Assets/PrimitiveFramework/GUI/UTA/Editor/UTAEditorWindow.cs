using PrimitiveFactory.Framework.EditorTools;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace PrimitiveFactory.Framework.UITimelineAnimation
{
    public class UTAEditorWindow : EditorWindow
    {
        [MenuItem("Primitive/UI Timeline Animation Editor")]
        public static void Open()
        {
            UTAEditorWindow window = GetWindow<UTAEditorWindow>("UTA Editor");
            window.Show();
        }

        /****************
        * Color, layout *
        ****************/
        protected static readonly Color c_ColorDefault = new Color(1f, 1f, 1f);
        protected static readonly Color c_ColorSave = new Color(0.2f, 0.9f, 0.2f);
        protected static readonly Color c_ColorNew = new Color(0.2f, 0.9f, 0.2f);
        protected static readonly Color c_ColorCurrent = new Color(0.4f, 0.4f, 1.0f);
        protected static readonly Color c_ColorDuplicate = Color.cyan;
        protected static readonly Color c_ColorDelete = new Color(1.0f, 0.0f, 0.0f);
        protected static readonly Color c_ColorInactive = new Color(0.7f, 0.7f, 0.7f);

        protected Color m_DefaultGUIColor;
        protected Color m_DefaultGUIBackgroundColor;

        protected const float c_ScrollListWidthPercent = 0.25f;

        /*******
        * Core *
        *******/
        // Data storing
        private const string c_AnimationDirectory = "Assets/FrameworkSettings/UTA/";

        // Private variables
        private UTAController m_UTAController;

        private Vector2 m_MenuScrollView = Vector2.zero;
        private Vector2 m_MainWindowScrollView = Vector2.zero;

        private UTADirector _CurrentUTADirector
        {
            get
            {
                if (TimelineEditor.playableDirector != null)
                    return TimelineEditor.playableDirector.GetComponent<UTADirector>();
                else if (Selection.activeGameObject != null)
                    return Selection.activeGameObject.GetComponent<UTADirector>();
                else
                    return null;
            }
        }

        /***************
        * Base Drawing *
        ***************/
        public void OnGUI()
        {
            m_DefaultGUIColor = GUI.color;
            m_DefaultGUIBackgroundColor = GUI.backgroundColor;

            m_UTAController = FindObjectOfType<UTAController>();
            if (m_UTAController == null)
            {
                DrawStartupHelper();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                DrawLeftMenu();
                DrawRightMenu();
                EditorGUILayout.EndHorizontal();
            }
        }

        public void OnEnable()
        {
        }

        /*****************
        * Startup Helper *
        *****************/
        private void DrawStartupHelper()
        {
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.HelpBox("It seems like you're starting up the UTA lib for the first time on this scene. Would you like to start the setup ?", MessageType.Warning);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (EditorGUITools.Button("Do it!", c_ColorNew, true, GUILayout.Width(100)))
                {
                    CreateUTAController();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void CreateUTAController()
        {
            GameObject utaControllerObject = new GameObject()
            {
                name = "UTAController"
            };
            utaControllerObject.AddComponent<UTAController>();
        }

        /************
        * Left Menu *
        ************/
        string m_SearchText = "";

        private bool _StringContains(string cheatName, string[] searchTexts)
        {
            foreach (string txt in searchTexts)
            {
                if (!cheatName.ToUpper().Contains(txt.ToUpper()))
                    return false;
            }
            return true;
        }

        private void DrawLeftMenu()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.Width(c_ScrollListWidthPercent * position.width));
            {
                EditorGUILayout.LabelField("Menu", EditorStyles.boldLabel);

                GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                {
                    m_SearchText = "";
                }
                m_SearchText = GUILayout.TextField(m_SearchText, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                string[] searchTexts = m_SearchText.Split(' ');

                // New
                GUI.backgroundColor = c_ColorNew;
                if (GUILayout.Button("New Animation"))
                {
                    AskTextPopup.Show("Create", NewObject);
                }
                GUI.backgroundColor = m_DefaultGUIBackgroundColor;

                EditorGUILayout.Space();

                // List of all objects
                List<UTADirector> directors = GetAllUTADirectors();
                m_MenuScrollView = EditorGUILayout.BeginScrollView(m_MenuScrollView, false, false);
                {
                    for (int i = 0; i < directors.Count; i++)
                    {
                        string directorName = directors[i].name;

                        if (_StringContains(directorName, searchTexts))
                        {
                            if (_CurrentUTADirector == directors[i])
                            {
                                GUI.backgroundColor = c_ColorCurrent;
                            }

                            if (GUILayout.Button(directorName))
                            {
                                SelectCurrentObject(directors[i]);
                            }
                        }
                        GUI.backgroundColor = m_DefaultGUIBackgroundColor;
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        /*************
        * Right Menu *
        *************/
        private void DrawRightMenu()
        {
            EditorGUILayout.BeginVertical("box");
            m_MainWindowScrollView = EditorGUILayout.BeginScrollView(m_MainWindowScrollView, false, false);
            {
                if (_CurrentUTADirector == null)
                {
                    EditorGUILayout.HelpBox("Select an animation or a create a new one using the left menu", MessageType.Info);
                }
                else
                {
                    GUIStyle centeredLabel = new GUIStyle(EditorStyles.boldLabel)
                    {
                        alignment = TextAnchor.UpperCenter
                    };
                    EditorGUILayout.LabelField(_CurrentUTADirector.name, centeredLabel);

                    EditorGUILayout.Space();

                    //if (EditorGUITools.Button("Rename", c_ColorSave, GUILayout.Width(150)))
                    //{
                    //    AskTextPopup.Show("Rename", RenameCurrentObject);
                    //}

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (EditorGUITools.Button("Duplicate", c_ColorDuplicate, GUILayout.Width(150)))
                    {
                        AskTextPopup.Show("Duplicate", DuplicateCurrentObject);
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (EditorGUITools.Button("Delete", c_ColorDelete, GUILayout.Width(150)))
                    {
                        if (EditorUtility.DisplayDialog("Delete animation?", "Are you sure you want to delete this animation ?", "Do it!", "Hell no!"))
                        {
                            DeleteCurrentObject();
                        }
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        /*******
        * CRUD *
        *******/
        private void NewObject(string objectName)
        {
            if (!Directory.Exists(c_AnimationDirectory))
            {
                Directory.CreateDirectory(c_AnimationDirectory);
            }

            TimelineAsset asset = ScriptableObjectUtility.CreateAsset<TimelineAsset>(string.Concat(c_AnimationDirectory, objectName, ".playable"));
            SelectCurrentObject(m_UTAController.CreateNewAnimation(asset));
        }

        private void DuplicateCurrentObject(string objectName)
        {
            // Get old references
            PlayableDirector oldPlayableDirector = _CurrentUTADirector.GetPlayableDirector();
            PlayableAsset oldPlayableAsset = oldPlayableDirector.playableAsset;

            // Duplicate asset
            PlayableAsset newPlayableAsset = ScriptableObjectUtility.DuplicateAsset(
                oldPlayableAsset,
                string.Concat(c_AnimationDirectory, objectName, ".playable")
            );

            // Duplicate game object
            GameObject cloneGameObject = GameObject.Instantiate(_CurrentUTADirector.gameObject);
            cloneGameObject.name = objectName;
            PlayableDirector newPlayableDirector = cloneGameObject.GetComponent<PlayableDirector>();
            newPlayableDirector.playableAsset = newPlayableAsset;

            // Duplicate bindings
            IEnumerator<PlayableBinding> oldBindings = oldPlayableAsset.outputs.GetEnumerator();
            IEnumerator<PlayableBinding> newBindings = newPlayableAsset.outputs.GetEnumerator();

            while (oldBindings.MoveNext())
            {
                var oldBindings_sourceObject = oldBindings.Current.sourceObject;

                newBindings.MoveNext();

                var newBindings_sourceObject = newBindings.Current.sourceObject;

                newPlayableDirector.SetGenericBinding(
                    newBindings_sourceObject,
                    oldPlayableDirector.GetGenericBinding(oldBindings_sourceObject)
                );
            }

            // Reparent and select
            cloneGameObject.transform.SetParent(oldPlayableDirector.transform.parent);
            SelectCurrentObject(cloneGameObject.GetComponent<UTADirector>());
        }

        private void DeleteCurrentObject()
        {
            ScriptableObjectUtility.DeleteAsset(_CurrentUTADirector.GetPlayableAsset());
            DestroyImmediate(_CurrentUTADirector.gameObject);
        }

        protected void SelectCurrentObject(UTADirector director)
        {
            Selection.activeGameObject = director.gameObject;
        }


        /**************
        * UTA Getters *
        **************/
        private List<TimelineAsset> GetAllUTATimelines()
        {
            List<TimelineAsset> res = new List<TimelineAsset>();

            string[] guids = AssetDatabase.FindAssets("t:timelineasset", new string[] { "Assets" });
            foreach (string guid in guids)
            {
                TimelineAsset a = AssetDatabase.LoadAssetAtPath<TimelineAsset>(AssetDatabase.GUIDToAssetPath(guid));
                foreach (TrackAsset track in a.GetRootTracks())
                {
                    if (track.GetType().IsAssignableFrom(typeof(UTAPlayableTrack)))
                    {
                        res.Add(a);
                        break;
                    }
                }
            }

            return res;
        }

        private List<UTADirector> GetAllUTADirectors()
        {
            List<UTADirector> res = new List<UTADirector>();

            foreach (Transform child in m_UTAController.transform)
            {
                res.Add(child.GetComponent<UTADirector>());
            }

            return res;
        }

        public static void DuplicateWithBindings()
        {
            if (UnityEditor.Selection.activeGameObject == null)
                return;

            var playableDirector = UnityEditor.Selection.activeGameObject.GetComponent<PlayableDirector>();
            if (playableDirector == null)
                return;

            var playableAsset = playableDirector.playableAsset;
            if (playableAsset == null)
                return;

            var path = AssetDatabase.GetAssetPath(playableAsset);
            if (string.IsNullOrEmpty(path))
                return;

            string newPath = path.Replace(".", "(Clone).");
            if (!AssetDatabase.CopyAsset(path, newPath))
            {
                Debug.LogError("Couldn't Clone Asset");
                return;
            }

            var newPlayableAsset = AssetDatabase.LoadMainAssetAtPath(newPath) as PlayableAsset;
            var gameObject = GameObject.Instantiate(UnityEditor.Selection.activeGameObject);
            var newPlayableDirector = gameObject.GetComponent<PlayableDirector>();
            newPlayableDirector.playableAsset = newPlayableAsset;

            var oldBindings = playableAsset.outputs.GetEnumerator();
            var newBindings = newPlayableAsset.outputs.GetEnumerator();


            while (oldBindings.MoveNext())
            {
                var oldBindings_sourceObject = oldBindings.Current.sourceObject;

                newBindings.MoveNext();

                var newBindings_sourceObject = newBindings.Current.sourceObject;


                newPlayableDirector.SetGenericBinding(
                    newBindings_sourceObject,
                    playableDirector.GetGenericBinding(oldBindings_sourceObject)
                );
            }
        }
    }

    /*********
    * Popups *
    *********/
    public class AskTextPopup : PopupWindowContent
    {
        private const float c_RectWidth = 200;
        private const float c_RectHeight = 100;

        private string m_TextData;
        private string m_ButtonText;
        private Action<string> m_Callback;
        
        /*******
        * Init *
        *******/ 
        public static void Show(string buttonText, Action<string> callback)
        {
            AskTextPopup popup = new AskTextPopup();
            popup.Prepare(buttonText, callback);
            PopupWindow.Show(new Rect(Screen.width / 2 - c_RectWidth / 2, c_RectHeight, c_RectWidth, c_RectHeight), popup);
        }


        public void Prepare(string buttonText, Action<string> callback)
        {
            m_ButtonText = buttonText;
            m_Callback = callback;
        }


        /**********
        * Display *
        **********/
        public override Vector2 GetWindowSize()
        {
            return new Vector2(c_RectWidth, c_RectHeight);
        }

        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Choose a name:", EditorStyles.boldLabel);
            m_TextData = EditorGUILayout.TextField(m_TextData);
            if (EditorGUITools.Button(m_ButtonText, true))
            {
                m_Callback(m_TextData);
                editorWindow.Close();
            }
            EditorGUILayout.EndVertical();
        }
    }
}

