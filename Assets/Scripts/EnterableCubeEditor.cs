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
        if (GUILayout.Button("Reset Cubes ID Grid"))
        {
            targetEnterableCube.ReGenerateCubesIDGrid();
            EditorUtility.SetDirty(targetEnterableCube);
        }
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.LabelField("Cubes ID Grid");
        for(int row = 0; row < targetEnterableCube.Tiling; row++)
        {
            EditorGUILayout.BeginHorizontal();
            for(int column = 0; column < targetEnterableCube.Tiling; column++)
            {
                //targetEnterableCube.CubesIDGrid[row, column] = EditorGUILayout.IntField(targetEnterableCube.CubesIDGrid[row, column], GUILayout.MaxWidth(30));
                targetEnterableCube.SetCubeIDInGrid(row, column, EditorGUILayout.IntField(targetEnterableCube.GetCubeIDInGrid(row, column), GUILayout.MaxWidth(30)));
                //EditorUtility.SetDirty(targetEnterableCube);
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
