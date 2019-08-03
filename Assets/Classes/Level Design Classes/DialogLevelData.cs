using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CreateAssetMenu(fileName = "NewDialogLevel", menuName = "Dialogs/Create A New List of Dialog for a Level")]
#endif
public class DialogLevelData : ScriptableObject
{
    public int numLevel;
    public List<Dialog> dialogs = new List<Dialog>();
    public bool beginLevel;

    private const string beginDialogPath = "Assets/Resources/DialogData/DialogBeginLevel";
    private const string endDialogPath = "Assets/Resources/DialogData/DialogEndLevel";

    public void Init (int dataNumLevel, List<Dialog> dataDialogs, bool dataBeginLevel)
    {
        numLevel = dataNumLevel;
        dialogs = dataDialogs;
        beginLevel = dataBeginLevel;
    }

	#if UNITY_EDITOR
    public static DialogLevelData CreateInstance(int theNumLevel, List<Dialog> theDialogs, bool beginLevel)
    {
        DialogLevelData dialogsLevel = ScriptableObject.CreateInstance<DialogLevelData>();
        dialogsLevel.Init(theNumLevel, theDialogs, beginLevel);

        if (beginLevel)
            AssetDatabase.CreateAsset(dialogsLevel, string.Concat(beginDialogPath, theNumLevel, ".asset"));
        else
            AssetDatabase.CreateAsset(dialogsLevel, string.Concat(endDialogPath, theNumLevel, ".asset"));

        Debug.Log(AssetDatabase.GetAssetPath(dialogsLevel));
        return dialogsLevel;
    }
	#endif
}
