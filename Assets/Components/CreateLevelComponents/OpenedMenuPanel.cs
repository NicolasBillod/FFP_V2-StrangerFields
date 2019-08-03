using UnityEngine;
using UnityEngine.UI;

public class OpenedMenuPanel : MonoBehaviour
{
    [Header("Opened Menu")]
    public GameObject openedMenu;
    public Button closeMenuButton;
    public Button createLevelButton;
    public Button loadLevelButton;
    public Button restartButton;
    public Button BackMenuButton;

    [Space(10)]
    [Header("Create Level Panel")]
    public GameObject createLevelPanel;
    public InputField inputLevel;
    public InputField inputAttractive;
    public InputField inputRepulsive;
    public Button saveButton;
    public Button closeButton;

    [Space(10)]
    [Header("Load Level Panel")]
    public GameObject loadLevelPanel;
    public InputField inputLevelLoad;
    public Button loadButton;
    public Button closeLoadButton;

    [Space(10)]
    [Header("Restart Panel")]
    public GameObject restartPanel;
    public Button yesRestButton;
    public Button noRestButton;

    [Space(10)]
    [Header("Back Menu Panel")]
    public GameObject backMenuPanel;
    public Button yesBackButton;
    public Button noBackButton;
}
