using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnterableCube))]
public class EnterableCubeEditor : Editor
{
    #region SerializedProperties
    SerializedProperty tiling;
    #endregion


    EnterableCube targetEnterableCube;
    private void OnEnable()
    {
        targetEnterableCube = (EnterableCube)target;
        tiling = serializedObject.FindProperty("tiling");
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.LabelField("Cubes Grid", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(tiling);
        if (GUILayout.Button("Reset Cubes Init Details Grid"))
        {
            targetEnterableCube.ReGenerateCubesIDGrid();
            EditorUtility.SetDirty(targetEnterableCube);
        }
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.LabelField("Cubes Init Details Grid: ");
        for(int row = 0; row < targetEnterableCube.Tiling; row++)
        {
            EditorGUILayout.BeginHorizontal();
            // Row headers
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("ID", GUILayout.Width(80));
            EditorGUILayout.LabelField("Is Player", GUILayout.Width(80));
            EditorGUILayout.LabelField("Can Be Player", GUILayout.Width(80));
            EditorGUILayout.LabelField("Edit details: ", GUILayout.Width(80));
            EditorGUILayout.EndVertical();
            for (int column = 0; column < targetEnterableCube.Tiling; column++)
            {
                var childCubeInitDetail = targetEnterableCube.GetChildCubeInitDetails(row, column);
                if (childCubeInitDetail == null) continue;
                EditorGUILayout.BeginVertical();
                childCubeInitDetail.CubeID = EditorGUILayout.IntField(childCubeInitDetail.CubeID, GUILayout.Width(30));
                childCubeInitDetail.IsPlayer= EditorGUILayout.Toggle(childCubeInitDetail.IsPlayer, GUILayout.Width(30));
                childCubeInitDetail.CanBePlayer = EditorGUILayout.Toggle(childCubeInitDetail.CanBePlayer, GUILayout.Width(30));
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(targetEnterableCube);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
