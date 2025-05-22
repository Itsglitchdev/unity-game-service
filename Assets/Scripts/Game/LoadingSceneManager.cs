using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class LoadingSceneManager : MonoBehaviour
{
    private const float minimumLoadingTime = 2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(LoadTargetScene());
    }

    private IEnumerator LoadTargetScene()
    {
        string targetScene = SceneLoader.targetSceneName;
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetScene);
        asyncLoad.allowSceneActivation = false;
        float timer = 0f;
        
        while (timer < minimumLoadingTime || asyncLoad.progress < 0.9f)
        {
            timer += Time.deltaTime;
            
            float timeProgress = timer / minimumLoadingTime;
            float actualProgress = asyncLoad.progress / 0.9f;
            float displayProgress = Mathf.Max(timeProgress, actualProgress);
            displayProgress = Mathf.Clamp01(displayProgress);

            yield return null;
        }
        
        asyncLoad.allowSceneActivation = true;
    }
}
