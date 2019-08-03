using UnityEngine;
using UnityEngine.UI;

public class OtherPanel : MonoBehaviour
{
    [Header("Restart Panel")]
    public GameObject restartPanel;
    public Button yesRestButton;
    public Button noRestButton;

    public void ActiveRestPanel(bool active)
    {
        restartPanel.SetActive(active);
    }

    [Space(10)]
    [Header("Back Menu Panel")]
    public GameObject backMenuPanel;
    public Button yesBackButton;
    public Button noBackButton;

    public void ActiveBackPanel(bool active)
    {
        backMenuPanel.SetActive(active);
    }

    [Space(10)]
    [Header("Lost Panel")]
    public GameObject lostPanel;
    public Button retryLostButton;
    public Button backMenuLostButton;

    [Space(10)]
    [Header("Won Panel")]
    public GameObject wonPanel;
    public GameObject[] wonStars;
    public Text scoreText;
    public Button retryWonButton;
    public Button backMenuWonButton;
    public Button nextLevelButton;

    [Space(10)]
    [Header("Dialog Panel")]
    public GameObject dialogPanel;
    public Image charaAvatar;
    public Text charaName;
    public Text dialogText;
    public Button nextDialogButton;

    public void ActiveDialogPanel(bool active)
    {
        dialogPanel.SetActive(active);
    }
}
