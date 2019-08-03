using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OptionUI : MonoBehaviour
{
    public Button backButton;
    public Toggle soundsToggle;
    public Toggle musicsToggle;

    public void BackToMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
