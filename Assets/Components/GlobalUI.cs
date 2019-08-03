using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GlobalUI : MonoBehaviour
{
    [Header("Global")]
    public Canvas mainCanvas;
    public GameObject levelPanel;
    public Text levelText;
    public Button backMenuButton;
    public Button restartButton;
    public Button autodestructionButton;
	public GameObject AIPanel;

    public void ActiveDestructButton(bool active)
    {
        autodestructionButton.gameObject.SetActive(active);
    }

    [Space(10)]
    [Header("Game Panel")]
    public GameObject gamePanel;
    public GameObject miniMap;
    public Button undoButton;
    public Button redoButton;

    public void ActiveGamePanel(bool active)
    {
        gamePanel.SetActive(active);
    }

    public void ActiveMinimap(bool active)
    {
        miniMap.SetActive(active);
    }

    [Space(10)]
    [Header("Buttons Panel")]
    public Button attractiveButton;
    //public Image attRemain;
    public Text attractiveRemaining;
    public Button repulsiveButton;
    //public Image repRemain;
    public Text repulsiveRemaining;
    public Button fireButton;

    [Space(10)]
    [Header("Ship Panel")]
    public GameObject shipPanel;
    public Slider speedSlider;
    public Text speedText;

    [Space(10)]
    [Header("Force Panel")]
    public GameObject forcePanel;
    public Image fieldImage;
    public Text typeText;
    public Slider intensitySlider;
    public Text intensityText;
    public Button deleteButton;

    [Space(10)]
    [Header("Bonus Malus Panel")]
    public GameObject bmPanel;
    public Text bmTitle;
    public Image bmImage;
    public Text bmDescription;
    public Text bmName;
}
