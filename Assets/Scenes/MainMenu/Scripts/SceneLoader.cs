using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string sceneName;   // اسم السين اللي هيروح لها المستخدم

    public void LoadMyScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
