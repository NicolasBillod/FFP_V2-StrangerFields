using UnityEngine;
using UnityEngine.UI;

public class UICreateDialog : MonoBehaviour
{
    [Header("Menu Panel")]
    public GameObject menuPanel;
    public Button openMenuButton;
    public Button closeMenuButton;
    public Button createDialogButton;
    public Button backMenuButton;

    [Space(10)]
    [Header("Create Dialog Panel")]
    public GameObject createDialogPanel;
    public InputField numLevel;
    public Toggle isBeginDialog;
    public Button createButton;
    public Button closeButton;

    [Space(10)]
    [Header("Back Menu Panel")]
    public GameObject backMenuPanel;
    public Button yesButton;
    public Button noButton;
}
