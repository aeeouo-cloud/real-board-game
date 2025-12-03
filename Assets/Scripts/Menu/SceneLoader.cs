using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadSpecificScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    public void EndGame()
    {
        Debug.Log("goodbye! world!");
        Application.Quit();
    }
}
