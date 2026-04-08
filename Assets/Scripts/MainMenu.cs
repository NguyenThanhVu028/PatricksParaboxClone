using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] float transitionTime = 0.25f;
    [SerializeField] string mapSelectionScene = "";
    private Coroutine changeSceneCoroutine = null;
    public void RequestStartGame()
    {
        if (changeSceneCoroutine != null) return;
        changeSceneCoroutine = StartCoroutine(StartGameCoroutine());
    }
    private void StartGame()
    {
        SceneManager.LoadSceneAsync(mapSelectionScene);
    }
    public void RequestQuitGame()
    {
        if (changeSceneCoroutine != null) return;
        changeSceneCoroutine = StartCoroutine(QuitGameCoroutine());
    }
    private void QuitGame()
    {
        Application.Quit();
    }
    private IEnumerator StartGameCoroutine()
    {
        yield return new WaitForSeconds(transitionTime);
        changeSceneCoroutine = null;
        StartGame();
    }
    private IEnumerator QuitGameCoroutine()
    {
        yield return new WaitForSeconds(transitionTime);
        changeSceneCoroutine = null;
        QuitGame();
    }
}
