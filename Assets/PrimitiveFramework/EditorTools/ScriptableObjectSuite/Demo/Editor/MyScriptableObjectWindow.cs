using PrimitiveFactory.Framework.EditorTools;
using UnityEditor;

public class MyScriptableObjectWindow : ScriptableObjectEditorWindow<MyScriptableObject>
{
    //---- These are the 3 must-have overrides ----//
    // Name of the object (Display purposes)
    protected override string c_ObjectName { get { return "My Scriptable Object"; } }
    // Relative path from Resource Folder
    protected override string c_ObjectResourcePath { get { return "Data/MyScriptableObjects/"; } }
    // Relative path from Project Root
    protected override string c_ObjectFullPath { get { return string.Concat("Assets/Resources/", c_ObjectResourcePath); } }

    [MenuItem("Primitive/Scriptable Object Suite/Show Demo Window")]
    public static void ShowWindow()
    {
        MyScriptableObjectWindow window = GetWindow<MyScriptableObjectWindow>("MyScriptableObject Editor");
        window.Show();
    }
}
