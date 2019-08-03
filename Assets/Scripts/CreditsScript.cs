using UnityEngine;
using UnityEngine.SceneManagement;
using PrimitiveFactory.Framework.UITimelineAnimation;

/// <summary>
/// 
/// </summary>
public class CreditsScript : MonoBehaviour
{
    public void Start()
    {
        UTAController.Instance.PlayAnimation("A_Credits_Apparition");
    }

    public void BackMenu()
    {
        UTAController.Instance.PlayAnimation("A_Credits_Disparition");
    }

    public void LoadMenuScene()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
