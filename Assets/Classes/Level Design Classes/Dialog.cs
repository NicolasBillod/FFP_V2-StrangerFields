using System;

[Serializable]
public class Dialog
{
    public string speakerName;
    public string dialogText;

    public Dialog(string theName, string theDialog)
    {
        speakerName = theName;
        dialogText = theDialog;
    }
}
