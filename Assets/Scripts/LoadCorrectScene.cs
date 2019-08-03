using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadCorrectScene : MonoBehaviour
{
    public string sceneName;

    public void LoadSceneLevel()
    {
        if (sceneName != string.Empty)
            SceneManager.LoadScene(sceneName);
    }
}
