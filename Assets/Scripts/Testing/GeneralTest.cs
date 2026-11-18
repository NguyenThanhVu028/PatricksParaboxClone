using UnityEngine;

public class GeneralTest : MonoBehaviour
{
    [SerializeField]
    private int _testMultipleCubeCount = 1;

    [ContextMenu("Test Get Color Palette")]
    public void TestGetColorPalette()
    {
        var colorPalette = ResourcesUtils.Load<ColorPalette>(ResourceType.ScriptableObjects, GameConfigs.MainColorPaletteName);
        Debug.Log(colorPalette);
    }

    [ContextMenu("Test Get Mesh")]
    public void TestGetMesh()
    {
        var mesh = ResourcesUtils.Load<Mesh>(ResourceType.Meshes, GameConfigs.MainMeshName);
        Debug.Log(mesh);
    }

    [ContextMenu("Test Solid Cube")]
    public void TestSolidCube()
    {
        RecursiveCamera.Instance.SingleTargetCube = new SolidCube();
        RecursiveCamera.Instance.StartRendering();
    }

    [ContextMenu("Test Multiple Cubes Render")]
    public void TestMultipleCubesRender()
    {
        RecursiveCamera.Instance.RenderMode = RecursiveCamera.CameraRenderMode.MultiCubes;
        RecursiveCamera.Instance.ClearAllCubesToRender();
        for (int i = 0; i < _testMultipleCubeCount; i++)
        {
            RecursiveCamera.Instance.AddCubeToRender(new SolidCube());
        }
        // RecursiveCamera.Instance.AddCubeToRender(new SolidCube());
        // RecursiveCamera.Instance.AddCubeToRender(new SolidCube());
        // RecursiveCamera.Instance.AddCubeToRender(new SolidCube());
        // RecursiveCamera.Instance.AddCubeToRender(new SolidCube());
        // RecursiveCamera.Instance.AddCubeToRender(new SolidCube());
        // RecursiveCamera.Instance.AddCubeToRender(new SolidCube());
        RecursiveCamera.Instance.StartRendering();
    }
}
