using System.IO;
using UnityEngine;

public class TestLevelDesignCompiler : MonoBehaviour
{
    private string _filePath;
    void Start()
    {
        _filePath = Path.Combine(
            Application.dataPath,
            "Scripts/Testing/DemoLevelDesign.txt"
        );
        Debug.Log(_filePath);

        LevelDetails newLevelDetails = LevelDesignCompiler.CompileToLevelDetails(_filePath);
        newLevelDetails?.LogProperties();
    }

}
