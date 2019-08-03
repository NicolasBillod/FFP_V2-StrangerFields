using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PrimitiveFactory.Framework.EditorTools
{
    public static class EditorGUITools
    {
        public delegate void Callback();
        public delegate GUIContent ItemDelegate<T>(int index, T value);
        public delegate bool ItemFilter<T>(T value);

        public static Quaternion QuaternionField(String label, Quaternion value, params GUILayoutOption[] options)
        {
            if (EditorGUITools.Foldout(label, false))
            {
                EditorGUILayout.BeginHorizontal(options);
                EditorGUILayout.LabelField("X:");
                value.x = EditorGUILayout.FloatField(value.x);
                EditorGUILayout.LabelField("Y:");
                value.y = EditorGUILayout.FloatField(value.y);
                EditorGUILayout.LabelField("Z:");
                value.z = EditorGUILayout.FloatField(value.z);
                EditorGUILayout.LabelField("W:");
                value.w = EditorGUILayout.FloatField(value.w);
                EditorGUILayout.EndHorizontal();
            }
            return value;
        }

        public static void HorizontalSeparator()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        /// <summary>
        /// Button helpers
        /// </summary>
        /// <param name="text"></param>
        /// <param name="enabled"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static bool Button(String text, bool enabled, params GUILayoutOption[] options)
        {
            return Button(text, enabled, "button", options);
        }

        public static bool Button(String text, bool enabled, GUIStyle style, params GUILayoutOption[] options)
        {
            bool oldEnabled = GUI.enabled;

            GUI.enabled = enabled;
            bool result = GUILayout.Button(text, style, options);
            GUI.enabled = oldEnabled;
            return result;
        }

        public static bool Button(String text, Color background, params GUILayoutOption[] options)
        {
            return Button(text, background, true, options);
        }

        public static bool Button(String text, Color background, bool enabled, params GUILayoutOption[] options)
        {
            return Button(text, background, enabled, "button", options);
        }

        public static bool Button(String text, Color background, bool enabled, GUIStyle style, params GUILayoutOption[] options)
        {
            bool oldEnabled = GUI.enabled;
            Color oldBackground = GUI.backgroundColor;

            GUI.enabled = enabled;
            GUI.backgroundColor = background;
            bool result = GUILayout.Button(text, style, options);
            GUI.enabled = oldEnabled;
            GUI.backgroundColor = oldBackground;
            return result;
        }

        public static void LabelField(String text, Color color, params GUILayoutOption[] options)
        {
            LabelField(new GUIContent(text), color, EditorStyles.label, options);
        }

        public static void LabelField(String text, Color color, GUIStyle style, params GUILayoutOption[] options)
        {
            LabelField(new GUIContent(text), color, style, options);
        }

        public static void LabelField(GUIContent content, Color color, GUIStyle style, params GUILayoutOption[] options)
        {
            Color oldColor = GUI.color;

            GUI.color = color;
            EditorGUILayout.LabelField(content, style, options);
            GUI.color = oldColor;
        }

        /// <summary>
        /// Simplified foldout that store his own status himself.
        /// By default the foldout is opened.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="option"></param>
        /// <returns>Return true if the foldout is open otherwise false.</returns>
        public static bool Foldout(String text, params GUILayoutOption[] option)
        {
            return Foldout(new GUIContent(text), true, "foldout", option);
        }

        public static bool Foldout(String text, GUIStyle style, params GUILayoutOption[] option)
        {
            return Foldout(new GUIContent(text), true, style, option);
        }

        public static bool Foldout(GUIContent content, params GUILayoutOption[] option)
        {
            return Foldout(content, true, option);
        }

        public static bool Foldout(GUIContent content, GUIStyle style, params GUILayoutOption[] option)
        {
            return Foldout(content, true, style, option);
        }

        /// <summary>
        /// Simplified foldout that store his own status himself.
        /// The first time the foldout state (open or closed) is defined by initiallyOpened.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="initiallyOpened">Define if the foldout is initially opened or not.</param>
        /// <param name="option"></param>
        /// <returns>Return true if the foldout is open otherwise false.</returns>
        public static bool Foldout(String text, bool initiallyOpened, params GUILayoutOption[] option)
        {
            return Foldout(new GUIContent(text), initiallyOpened, "foldout", option);
        }

        public static bool Foldout(String text, bool initiallyOpened, GUIStyle style, params GUILayoutOption[] option)
        {
            return Foldout(new GUIContent(text), initiallyOpened, style, option);
        }

        public static bool Foldout(GUIContent content, bool initiallyOpened, params GUILayoutOption[] option)
        {
            return Foldout(content, initiallyOpened, "foldout", option);
        }

        public static bool Foldout(GUIContent content, bool initiallyOpened, GUIStyle style, params GUILayoutOption[] option)
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            FoldoutState state = GUIUtility.GetStateObject(typeof(FoldoutState), controlID) as FoldoutState;

            if (state == null || state.initialized == false)
            {
                state = GUIUtility.GetStateObject(typeof(FoldoutState), controlID) as FoldoutState;
                state.opened = initiallyOpened;
                state.initialized = true;
            }
            state.opened = EditorGUILayout.Foldout(state.opened, content, style);
            return state.opened;
        }

        #region Foldout details
        private class FoldoutState
        {
            public bool opened = false;
            public bool initialized = false;
        }
        #endregion

        public static String DirectoryPathField(String path, params GUILayoutOption[] option)
        {
            return DirectoryPathField(path, EditorStyles.textField, option);
        }

        public static String DirectoryPathField(String path, GUIStyle style, params GUILayoutOption[] option)
        {
            String result = path;

            GUILayout.BeginHorizontal(option);
            GUI.tooltip = path;
            GUILayout.Label(new GUIContent(Path.GetFileName(path), path), style, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("...", EditorStyles.miniButton, GUILayout.Width(20)))
            {
                String newPath = EditorUtility.OpenFolderPanel("Select a directory...", path, "");

                if (newPath != null)
                {
                    result = newPath;
                }
            }
            if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(20)))
            {
                result = null;
            }
            GUILayout.EndHorizontal();
            return result;
        }

        public static String FilePathField(String initialDirectory, String extension, params GUILayoutOption[] option)
        {
            return FilePathField(null, initialDirectory, extension, EditorStyles.textField, option);
        }

        public static String FilePathField(String initialDirectory, String extension, GUIStyle style, params GUILayoutOption[] option)
        {
            return FilePathField(null, initialDirectory, extension, style, option);
        }

        public static String FilePathField(String label, String initialDirectory, String extension, params GUILayoutOption[] option)
        {
            return FilePathField(label, initialDirectory, extension, EditorStyles.textField, option);
        }

        public static String FilePathField(String label, String initialDirectory, String extension, GUIStyle style, params GUILayoutOption[] option)
        {
            EditorGUI.BeginChangeCheck();
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            PathFieldState state = (PathFieldState)GUIUtility.GetStateObject(typeof(PathFieldState), controlID);

            if (state.currentDirectory == null)
            {
                state.currentDirectory = initialDirectory;
            }
            GUILayout.BeginHorizontal(option);
            GUI.tooltip = state.currentFilePath;
            if (label != null)
            {
                GUILayout.Label(label);
            }
            GUILayout.Label(new GUIContent(Path.GetFileName(state.currentFilePath), state.currentFilePath), style, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("...", EditorStyles.miniButton, GUILayout.Width(20)))
            {
                String newPath = EditorUtility.OpenFilePanel("Select a file...", state.currentDirectory, extension);

                if (String.IsNullOrEmpty(newPath) == false)
                {
                    state.currentDirectory = Path.GetDirectoryName(newPath);
                    state.currentFilePath = newPath;
                }
            }
            if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(20)))
            {
                state.currentFilePath = null;
            }
            GUILayout.EndHorizontal();
            return state.currentFilePath;
        }

        private class PathFieldState
        {
            public String currentFilePath;
            public String currentDirectory;
        }

        public class ExtensionFilters
        {
            public ExtensionFilters()
            {
                m_datas = new String[] { };
            }

            public ExtensionFilters(String name, params String[] extensions)
            {
                StringBuilder builder = new StringBuilder();

                for (var i = 0; i < extensions.Length; ++i)
                {
                    if (i > 0)
                    {
                        builder.Append(",");
                    }
                    builder.Append(extensions[i]);
                }
                m_datas = new String[] { name, builder.ToString() };
            }

            public ExtensionFilters Combine(ExtensionFilters other)
            {
                // TODO: improve: do not duplicate name, combines extensions instead
                if (other != this)
                {
                    int total = m_datas.Length + other.m_datas.Length;
                    int i = 0;
                    String[] newDatas = new String[total];

                    while (i < m_datas.Length)
                    {
                        newDatas[i] = m_datas[i];
                        ++i;
                    }
                    while (i < other.m_datas.Length)
                    {
                        newDatas[i] = other.m_datas[i];
                        ++i;
                    }
                    m_datas = newDatas;
                }
                return this;
            }

            public String[] GetExtensions()
            {
                return m_datas;
            }

            private String[] m_datas;
        }


        /// <summary>
        /// Display a warning box if the current scene is not the requiered scene.
        /// </summary>
        /// <param name="displayedSceneName">The scene name displayed to the user</param>
        /// <param name="scenePath">The complete scene path</param>
        /// <param name="onChangeScene">Callback called just after the scene change</param>
        /// <returns>true if the current scene is not the expected scene</returns>
        public static bool RequiredSceneBox(String displayedSceneName, String scenePath, Callback onChangeScene = null)
        {
            Scene currentScene = EditorSceneManager.GetActiveScene();
            String sceneName = Path.GetFileNameWithoutExtension(scenePath);

            if (currentScene.name != sceneName)
            {
                EditorGUILayout.HelpBox("You are not in the " + displayedSceneName + " scene", MessageType.Warning);
                if (GUILayout.Button("Change scene"))
                {
                    EditorSceneManager.OpenScene(scenePath);
                    if (onChangeScene != null)
                    {
                        onChangeScene();
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Draw a list view with selectable items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values">The list or the array of values (and finally anything that implements IEnumerable<T>) to display</param>
        /// <param name="currentIndex">The current index (-1 to hide the selection rectangle)</param>
        /// <param name="itemDelegate">Delegate used to convert each value to an instance of GUIContent</param>
        /// <returns>The index of the selected item</returns>
        public static int ListView<T>(IEnumerable<T> values, int currentIndex, ItemDelegate<T> itemDelegate, params GUILayoutOption[] options)
        {
            return ListView<T>(values, currentIndex, itemDelegate, null, options);
        }

        /// <summary>
        /// Draw a list view with selectable items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values">The list or the array of values (and finally anything that implements IEnumerable<T>) to display</param>
        /// <param name="currentIndex">The current index (-1 to hide the selection rectangle)</param>
        /// <param name="itemDelegate">Delegate used to convert each value to an instance of GUIContent</param>
        /// <param name="itemFilter">Delegate used to filter values</param>
        /// <returns>The index of the selected item</returns>
        public static int ListView<T>(IEnumerable<T> values, int currentIndex, ItemDelegate<T> itemDelegate, ItemFilter<T> itemFilter, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            ListViewState state = GUIUtility.GetStateObject(typeof(ListViewState), controlID) as ListViewState;

            if (state.selectionColor == null)
            {
                state.selectionColor = new Texture2D(1, 1);
                state.selectionColor.wrapMode = TextureWrapMode.Repeat;
                state.selectionColor.SetPixel(0, 0, Color.white);
            }
            EditorGUILayout.BeginVertical(EditorStyles.textArea, options);
            state.scrollPosition = EditorGUILayout.BeginScrollView(state.scrollPosition, false, false, options);
            EditorGUILayout.BeginVertical(options);
            IEnumerator<T> iterator = values.GetEnumerator();
            int index = 0;

            state.currentIndex = currentIndex;
            while (iterator.MoveNext())
            {
                if (itemFilter == null || itemFilter(iterator.Current))
                {
                    GUIContent content = itemDelegate(index, iterator.Current);
                    Rect rectangle = GUILayoutUtility.GetRect(content, EditorStyles.label);

                    if (Event.current.GetTypeForControl(controlID) == EventType.Repaint)
                    {
                        if (currentIndex == index)
                        {
                            DrawSelectionRectangle(state, rectangle);
                        }
                        EditorStyles.label.Draw(rectangle, content, false, true, true, false);
                    }
                    else if (Event.current.GetTypeForControl(controlID) == EventType.MouseDown && rectangle.Contains(Event.current.mousePosition))
                    {
                        state.currentIndex = index;
                        GUIUtility.hotControl = controlID;
                        //GUI.changed = true;
                        // The line above is disabled because MightyScriptableObjectEditorWindow
                        // use GUI.changed to check if something has been modified...
                        Event.current.Use();
                    }
                    else if (Event.current.GetTypeForControl(controlID) == EventType.MouseUp && GUIUtility.hotControl == controlID)
                    {
                        GUIUtility.hotControl = 0;
                    }
                }
                ++index;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUI.EndChangeCheck();
            // index is now the count of items present in the container
            UpdateCurrentSelection(state, index);
            return state.currentIndex;
        }

        #region ListView details
        private class ListViewState
        {
            public Vector2 scrollPosition;
            public int currentIndex;
            public Texture2D selectionColor;
        }

        static private void UpdateCurrentSelection(ListViewState state, int count)
        {
            if (count == 0)
            {
                state.currentIndex = -1;
            }
            else if (state.currentIndex >= count)
            {
                state.currentIndex = Mathf.Max(count - 1, state.currentIndex);
            }
        }

        static private void DrawSelectionRectangle(ListViewState state, Rect itemPosition)
        {
            Color contentColor = GUI.contentColor;
            Color color = GUI.color;

            GUI.contentColor = Color.white;
            GUI.color = GUI.skin.settings.selectionColor;
            GUI.DrawTexture(itemPosition, state.selectionColor);
            GUI.contentColor = contentColor;
            GUI.color = color;
        }
        #endregion
    }
}
