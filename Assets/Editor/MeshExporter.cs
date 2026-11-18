using UnityEngine;
using UnityEditor;

public static class MeshExporter
{
    [UnityEditor.MenuItem("Tools/Export Mesh")]
    private static void ExportMesh()
    {
        GameObject selected = Selection.activeGameObject;

        if (selected == null)
            return;

        MeshFilter meshFilter = selected.GetComponent<MeshFilter>();

        if (meshFilter == null || meshFilter.sharedMesh == null)
            return;

        string path = EditorUtility.SaveFilePanelInProject(
            "Save Mesh",
            meshFilter.sharedMesh.name,
            "asset",
            "Select a destination folder"
        );

        if (string.IsNullOrEmpty(path))
            return;

        Mesh mesh = Object.Instantiate(meshFilter.sharedMesh);
        mesh.name = System.IO.Path.GetFileNameWithoutExtension(path);

        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();

        EditorGUIUtility.PingObject(mesh);
    }
}