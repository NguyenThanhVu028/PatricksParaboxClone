using UnityEngine;

public class GeneralTest : MonoBehaviour
{
    [ContextMenu("Test Get Color Palette")]
    public void TestGetColorPalette()
    {
        var colorPalette = ResourcesUtils.Load<ColorPalette>(ResourceType.ScriptableObjects, GameConfigs.MainColorPalette);
        Debug.Log(colorPalette);
    }
}
