using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    [SerializeField] int playerButtonCount = 0;
    [SerializeField] int normalButtonCount = 0;

    private int activatedPlayerButtons = 0;
    private int activatedNormalButtons = 0;

    public static GameManager Instance;
    private void Awake()
    {
        if (instance != null && instance != this) Destroy(instance);
        instance = this;
    }

    public void AssignPlayerButton() { playerButtonCount++; }
    public void AssignNormalButton() { normalButtonCount++; }

    public bool AnnouncePlayerButtonActivated()
    {
        activatedPlayerButtons++;
        return activatedPlayerButtons >= playerButtonCount;
    }
    public bool AnnounceNormalButtonActivated() {
        activatedNormalButtons++;
        return activatedNormalButtons >= normalButtonCount;
    }
}
