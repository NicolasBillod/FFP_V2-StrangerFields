using UnityEngine;
using UnityEngine.UI;

public class GamePanelLD : MonoBehaviour
{
    [Header("Global")]
    public GameObject gamePanel;

    [Space(10)]
    [Header("Buttons Panel")]
    public Button undoButton;
    public Button redoButton;
    public Button switchCameraButton;
    public Button fireButton;

    [Space(10)]
    [Header("Add Field Panel")]
    public Button attractiveButton;
    public Text attractiveRemaining;
    public Button repulsiveButton;
    public Text repulsiveRemaining;
    public Button attractiveButtonLD;
    public Text attractiveLDRemaining;
    public Button repulsiveButtonLD;
    public Text repulsiveLDRemaining;

    [Space(10)]
    [Header("Ship Panel")]
    public GameObject shipPanel;
    public Slider speedSlider;
    public Text speedText;
    public Toggle movableToggle;

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
    public Button deleteBmButton;

    [Space(10)]
    [Header("Object Panel")]
    public GameObject objectPanel;
    public Text objectName;
    public Image objectImage;
    public Button deleteObjButton;
}
