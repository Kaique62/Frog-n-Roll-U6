using UnityEngine;

public class OpenURLButton : MonoBehaviour
{
    /// <summary>
    /// Abre o link desejado no navegador padrão.
    /// </summary>
    /// <param name="url">https://z1c4z.github.io/Frog-and-roll/</param>
    public void OpenSite(string url)
    {
        Application.OpenURL(url);
    }
}
