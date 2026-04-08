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
    SerializedProperty isLeavable;

    // Rendering
    SerializedProperty minPixelToRender;
    SerializedProperty floorTexture;
    SerializedProperty normalMat;
    SerializedProperty outlineMat;
    SerializedProperty cubeMesh;
    SerializedProperty staticTilesRTDepth;
    SerializedProperty unenterableColor;
    SerializedProperty unleavableColor;

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
        isLeavable = serializedObject.FindProperty("isLeavable");

        // Rendering
        minPixelToRender = serializedObject.FindProperty("minPixelToRender");
        normalMat = serializedObject.FindProperty("normalMat");
        outlineMat = serializedObject.FindProperty("outlineMat");
        cubeMesh = serializedObject.FindProperty("cubeMesh");
        floorTexture = serializedObject.FindProperty("floorTexture");
        staticTilesRTDepth = serializedObject.FindProperty("staticTilesRTDepth");
        unenterableColor = serializedObject.FindProperty("unenterableColor");
        unleavableColor = serializedObject.FindProperty("unleavableColor");

        // Other settings
        onInit = serializedObject.FindProperty("onInit");

    }
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("General Infos", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(isEnterable);
        EditorGUILayout.PropertyField(isLeavable);
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
        EditorGUILayout.PropertyField(unenterableColor);
        EditorGUILayout.PropertyField(unleavableColor);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(onInit);

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(mapSelectionCube);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
