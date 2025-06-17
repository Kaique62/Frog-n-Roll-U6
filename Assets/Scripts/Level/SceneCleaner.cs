using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneCleaner : MonoBehaviour
{
    /// <summary>
    /// Descarrega todas as cenas carregadas, exceto a atual.
    /// </summary>
    public void UnloadAllLoadedScenesExceptActive()
    {
        Debug.Log("ClearScene");
        Scene activeScene = SceneManager.GetActiveScene();

        StartCoroutine(UnloadScenesRoutine(activeScene));
    }

    private IEnumerator UnloadScenesRoutine(Scene activeScene)
    {
        int sceneCount = SceneManager.sceneCount;

        for (int i = 0; i < sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            if (scene != activeScene && scene.isLoaded)
            {
                AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(scene);

                if (unloadOp != null)
                {
                    yield return unloadOp;
                }
            }
        }

        Debug.Log("Todas as cenas secundárias foram descarregadas.");
    }
}
