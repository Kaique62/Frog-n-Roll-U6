using UnityEngine;
using UnityEngine.SceneManagement;

public class KeyBinds : MonoBehaviour
{
    void closeKeyBindMenu() {
        SceneManager.UnloadSceneAsync("PauseMenu");
        //Time.timeScale = 1f;
    }
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
