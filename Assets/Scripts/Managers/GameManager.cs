using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    [SerializeField] int playerButtonCount = 0;
    [SerializeField] int normalButtonCount = 0;

    [SerializeField] UnityEvent onPlayerButtonCountReached = new();
    [SerializeField] UnityEvent onNormalButtonCountReached = new();

    [SerializeField] float onLevelCompletedDelay = 1f;
    [SerializeField] string nextLevelSceneName = "";

    private int activatedPlayerButtons = 0;
    private int activatedNormalButtons = 0;

    private Coroutine levelCompletedCoroutine = null;

    public static GameManager Instance { get => instance; }
    private void Awake()
    {
        if (instance != null && instance != this) Destroy(instance);
        instance = this;
    }

    private void Update()
    {
        if ((playerButtonCount > 0 || normalButtonCount > 0) && activatedPlayerButtons >= playerButtonCount && activatedNormalButtons >= normalButtonCount)
        {
            if (levelCompletedCoroutine == null) levelCompletedCoroutine = StartCoroutine(LevelCompletedCoroutine());
        }
        else
        {
            if (levelCompletedCoroutine != null)
            {
                StopCoroutine(levelCompletedCoroutine);
                levelCompletedCoroutine = null;
            }
        }
    }

    public void AssignPlayerButton() { playerButtonCount++; }
    public void UnAssignPlayerButton() { if (playerButtonCount > 0) playerButtonCount--; }
    public void AssignNormalButton() { normalButtonCount++; }
    public void UnAssignNormalButton() { if (normalButtonCount > 0) normalButtonCount--; }

    public void AnnouncePlayerButtonActivated()
    {
        if (activatedPlayerButtons >= playerButtonCount) return;
        activatedPlayerButtons++;
        if (activatedPlayerButtons >= playerButtonCount) onPlayerButtonCountReached.Invoke();
    }
    public void AnnouncePlayerButtonDeactivated()
    {
        if (activatedPlayerButtons <= 0) return;
        activatedPlayerButtons--;
    }
    public void AnnounceNormalButtonActivated() 
    {
        if (activatedNormalButtons >= normalButtonCount) return;
        activatedNormalButtons++;
        if (activatedNormalButtons >= normalButtonCount) onNormalButtonCountReached.Invoke();
    }
    public void AnnounceNormalButtonDeactivated() {
        if (activatedNormalButtons <= 0) return;
        activatedNormalButtons--;
    }

    IEnumerator LevelCompletedCoroutine()
    {
        yield return new WaitForSeconds(onLevelCompletedDelay);
        if (SaveAndLoadManager.Instance != null)
        {
            SaveAndLoadManager.GameData gameData = SaveAndLoadManager.Instance.GeneralGameData;
            if (gameData != null)
            {
                gameData.AddFinishedMap(SceneManager.GetActiveScene().name);
            }
        }
        if (!string.IsNullOrEmpty(nextLevelSceneName))
            SceneManager.LoadSceneAsync(nextLevelSceneName);
    }
}
