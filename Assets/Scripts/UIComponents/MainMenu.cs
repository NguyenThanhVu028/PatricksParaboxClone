using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] float defaultTransitionTime = 0.25f;
    [SerializeField] string mapSelectionScene = "";
    private Coroutine changeSceneCoroutine = null;

    private float TransitionTime
    {
        get
        {
            if (gameData != null) return gameData.EnterTime / 1000f;
            return defaultTransitionTime;
        }
    }
    private SaveAndLoadManager.GameData gameData;

    private void Start()
    {
        if (SaveAndLoadManager.Instance != null)
        {
            gameData = SaveAndLoadManager.Instance.GeneralGameData;
        }
    }
    public void RequestStartGame()
    {
        if (changeSceneCoroutine != null) return;
        changeSceneCoroutine = StartCoroutine(StartGameCoroutine());
    }
    public void StartGame()
    {
        SceneManager.LoadSceneAsync(mapSelectionScene);
    }
    public void RequestQuitGame()
    {
        if (changeSceneCoroutine != null) return;
        changeSceneCoroutine = StartCoroutine(QuitGameCoroutine());
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    private IEnumerator StartGameCoroutine()
    {
        yield return new WaitForSeconds(TransitionTime);
        changeSceneCoroutine = null;
        StartGame();
    }
    private IEnumerator QuitGameCoroutine()
    {
        yield return new WaitForSeconds(TransitionTime);
        changeSceneCoroutine = null;
        QuitGame();
    }
}
