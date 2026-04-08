using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapSelectionCube))]
public class MapSelectionCubeEditor : Editor
{
    #region SerializedProperties
    // General Infos
    SerializedProperty cubeType;
    SerializedProperty colorPalette;
    SerializedProperty cubeColor;
    SerializedProperty needInstantiating;
    SerializedProperty isEnterable;
    SerializedProperty isExitable;

    // Rendering
    SerializedProperty minPixelToRender;
    SerializedProperty floorTexture;
    SerializedProperty normalMat;
    SerializedProperty outlineMat;
    SerializedProperty cubeMesh;
    SerializedProperty staticTilesRTDepth;

    // Other settings
    SerializedProperty onInit;

    // Grid settings

    #endregion

    MapSelectionCube mapSelectionCube;
    private void OnEnable()
    {
        mapSelectionCube = (MapSelectionCube)target;

        cubeType = serializedObject.FindProperty("cubeType");
        colorPalette = serializedObject.FindProperty("colorPalette");
        cubeColor = serializedObject.FindProperty("cubeColor");
        needInstantiating = serializedObject.FindProperty("needInstantiating");
        isEnterable = serializedObject.FindProperty("isEnterable");
        isExitable = serializedObject.FindProperty("isExitable");

        // Rendering
        minPixelToRender = serializedObject.FindProperty("minPixelToRender");
        normalMat = serializedObject.FindProperty("normalMat");
        outlineMat = serializedObject.FindProperty("outlineMat");
        cubeMesh = serializedObject.FindProperty("cubeMesh");
        floorTexture = serializedObject.FindProperty("floorTexture");
        staticTilesRTDepth = serializedObject.FindProperty("staticTilesRTDepth");

        // Other settings
        onInit = serializedObject.FindProperty("onInit");

    }
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("General Infos", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(isEnterable);
        EditorGUILayout.PropertyField(isExitable);
        EditorGUILayout.PropertyField(cubeType);
        EditorGUILayout.PropertyField(colorPalette);
        EditorGUILayout.PropertyField(cubeColor);
        EditorGUILayout.PropertyField(needInstantiating);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(minPixelToRender);
        EditorGUILayout.PropertyField(normalMat);
        EditorGUILayout.PropertyField(outlineMat);
        EditorGUILayout.PropertyField(cubeMesh);
        EditorGUILayout.PropertyField(floorTexture);
        EditorGUILayout.PropertyField(staticTilesRTDepth);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(onInit);

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(mapSelectionCube);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
