using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CubesManager))]
public class CubesManagerEditor : Editor
{
    private CubesManager cubesManager;
    private void OnEnable()
    {
        cubesManager = (CubesManager)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }

    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
    static void OnDrawGizmos(CubesManager cubesManager, GizmoType gizmoType)
    {
        Camera sceneCam = SceneView.lastActiveSceneView.camera;
        if (sceneCam == null) return;

        float dist = Mathf.Abs(sceneCam.transform.position.z - cubesManager.transform.position.z);

        // Only draw if within range
        if (dist > 10) return;

        var allCubes = cubesManager.AllCubes;

        Handles.Label(cubesManager.transform.position, "Cubes manager details:");
        for (int i = 0; i < allCubes.Count; i++)
        {
            CubeDetails cubeDetails = allCubes[i];
            if (cubeDetails == null) continue;
            Vector3 gizmosPos = cubesManager.transform.position + Vector3.down * (i + 1) * 1.0f;
            Handles.Label(gizmosPos, $"ID: {cubeDetails.ID} | Cube: {(cubeDetails.Cube != null ? cubeDetails.Cube.name : "null")}");
        }
    }
}
