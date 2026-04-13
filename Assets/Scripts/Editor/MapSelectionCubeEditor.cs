using TMPro;
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
    SerializedProperty triggerButtonPrefab;

    // Map info
    SerializedProperty mapIndex;
    SerializedProperty mapName;
    SerializedProperty openMapDelayTime;
    SerializedProperty mapIndexText;
    SerializedProperty mapIndexTextPadding;
    SerializedProperty dependentMapSelectionCubes;

    // Rendering
    SerializedProperty minPixelToRender;
    SerializedProperty floorTexture;
    SerializedProperty normalMat;
    SerializedProperty outlineMat;
    SerializedProperty cubeMesh;
    SerializedProperty staticTilesRTDepth;
    SerializedProperty unenterableColor;
    SerializedProperty unleavableColor;
    SerializedProperty enableOcclusionCulling;

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
        triggerButtonPrefab = serializedObject.FindProperty("triggerButtonPrefab");

        // Map info
        mapIndex = serializedObject.FindProperty("mapIndex");
        mapName = serializedObject.FindProperty("mapName");
        openMapDelayTime = serializedObject.FindProperty("openMapDelayTime");
        mapIndexText = serializedObject.FindProperty("mapIndexText");
        mapIndexTextPadding = serializedObject.FindProperty("mapIndexTextPadding");
        dependentMapSelectionCubes = serializedObject.FindProperty("dependentMapSelectionCubes");

        // Rendering
        minPixelToRender = serializedObject.FindProperty("minPixelToRender");
        normalMat = serializedObject.FindProperty("normalMat");
        outlineMat = serializedObject.FindProperty("outlineMat");
        cubeMesh = serializedObject.FindProperty("cubeMesh");
        floorTexture = serializedObject.FindProperty("floorTexture");
        staticTilesRTDepth = serializedObject.FindProperty("staticTilesRTDepth");
        unenterableColor = serializedObject.FindProperty("unenterableColor");
        unleavableColor = serializedObject.FindProperty("unleavableColor");
        enableOcclusionCulling = serializedObject.FindProperty("enableOcclusionCulling");

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
        EditorGUILayout.PropertyField(triggerButtonPrefab);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Map Infos", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(mapIndex);
        EditorGUILayout.PropertyField(mapName);
        EditorGUILayout.PropertyField(openMapDelayTime);
        EditorGUILayout.PropertyField(mapIndexText);
        EditorGUILayout.PropertyField(mapIndexTextPadding);
        EditorGUILayout.PropertyField(dependentMapSelectionCubes);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(minPixelToRender);
        EditorGUILayout.PropertyField(normalMat);
        EditorGUILayout.PropertyField(outlineMat);
        EditorGUILayout.PropertyField(cubeMesh);
        EditorGUILayout.PropertyField(floorTexture);
        EditorGUILayout.PropertyField(staticTilesRTDepth);
        EditorGUILayout.PropertyField(unenterableColor);
        EditorGUILayout.PropertyField(unleavableColor);
        EditorGUILayout.PropertyField(enableOcclusionCulling);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(onInit);

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(mapSelectionCube);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
