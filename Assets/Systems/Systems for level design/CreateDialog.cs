using System.Collections.Generic;
using FYFY;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateDialog : FSystem
{
    private Family _UICreateDialogFamily = FamilyManager.getFamily(new AllOfComponents(typeof(UICreateDialog)));
    
    private UICreateDialog _interface;

    //Path XML
    private string dataBeginPath = "JsonDialog/DialogBeginLevel";
    private string dataEndPath = "JsonDialog/DialogEndLevel";

    public CreateDialog()
    {
        _interface = _UICreateDialogFamily.First().GetComponent<UICreateDialog>();

        _interface.openMenuButton.onClick.AddListener(() => OpenCloseMenuPanel(true));
        _interface.closeMenuButton.onClick.AddListener(() => OpenCloseMenuPanel(false));
        _interface.createDialogButton.onClick.AddListener(() => OpenCloseCreatePanel(true));
        _interface.backMenuButton.onClick.AddListener(() => OpenCloseBackMenuPanel(true));

#if UNITY_EDITOR
        _interface.createButton.onClick.AddListener(() => CreateDialogData(_interface.numLevel.text, _interface.isBeginDialog.isOn));
#endif
        _interface.closeButton.onClick.AddListener(() => OpenCloseCreatePanel(false));

        _interface.yesButton.onClick.AddListener(() => BackMenuButton());
        _interface.noButton.onClick.AddListener(() => OpenCloseBackMenuPanel(false));
    }

    private void OpenCloseMenuPanel(bool isActive)
    {
        _interface.menuPanel.SetActive(isActive);
        _interface.openMenuButton.gameObject.SetActive(!isActive);
    }

    private void OpenCloseCreatePanel(bool isActive)
    {
        _interface.createDialogPanel.SetActive(isActive);
        OpenCloseMenuPanel(false);
    }

    private void OpenCloseBackMenuPanel(bool isActive)
    {
        _interface.backMenuPanel.SetActive(isActive);
        OpenCloseMenuPanel(false);
    }

    private void BackMenuButton()
    {
        SceneManager.LoadScene("MenuScene");
    }

#if UNITY_EDITOR
    private void CreateDialogData(string numLevel, bool beginLevel)
    {
        TextAsset asset;

        if (beginLevel)
            asset = Resources.Load(string.Concat(dataBeginPath, numLevel)) as TextAsset;
        else
            asset = Resources.Load(string.Concat(dataEndPath, numLevel)) as TextAsset;

        DialogsList dialogsListData = new DialogsList();
        dialogsListData = JsonUtility.FromJson<DialogsList>(asset.text);

        List<Dialog> dialogList = new List<Dialog>();

        foreach (Dialogs aDialog in dialogsListData.Dialogs)
        {
            string speaker = aDialog.speakerName + "_" + aDialog.feeling;
            Dialog theDialog = new Dialog(speaker, aDialog.dialogText);
            dialogList.Add(theDialog);
        }

        DialogLevelData dialogData = DialogLevelData.CreateInstance(int.Parse(numLevel), dialogList, beginLevel);
    }
#endif
}