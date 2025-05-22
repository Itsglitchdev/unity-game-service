using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{

    public static string targetSceneName;

    public static void Load(SceneName scene, SceneName targetScene)
    {
        targetSceneName = targetScene.ToString();
        SceneManager.LoadScene(scene.ToString());
    }

    public static void LoadScene(SceneName scene)
    {
        SceneManager.LoadScene(scene.ToString());
    }

    public static void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

   
}
