using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CloneCube), true)]
public class CloneCubeEditor : Editor
{
    #region SerializedProperties
    // General Infos
    SerializedProperty mainContainerCube;
    SerializedProperty cubeType;
    SerializedProperty needInstantiating;
    SerializedProperty isEnterable;
    SerializedProperty isLeavable;

    // Rendering
    SerializedProperty isHorizFlipped;
    SerializedProperty minPixelToRender;
    SerializedProperty normalMat;
    SerializedProperty outlineMat;
    SerializedProperty cubeMesh;
    SerializedProperty possessableFaceTexture;
    SerializedProperty unenterableColor;
    SerializedProperty unleavableColor;
    SerializedProperty enableOcclusionCulling;
    SerializedProperty surfaceEffectsAnimationIDs;

    // Cube status
    SerializedProperty parent;
    SerializedProperty previousParents; // Record self cube's previous parent to record history
    SerializedProperty relativeScale;
    SerializedProperty relativePosition;

    // Other settings
    SerializedProperty possessingTime;
    SerializedProperty onInit;

    #endregion

    CloneCube targetEnterableCube;
    private void OnEnable()
    {
        targetEnterableCube = (CloneCube)target;

        mainContainerCube = serializedObject.FindProperty("mainContainerCube");
        cubeType = serializedObject.FindProperty("cubeType");
        needInstantiating = serializedObject.FindProperty("needInstantiating");
        isEnterable = serializedObject.FindProperty("isEnterable");
        isLeavable = serializedObject.FindProperty("isLeavable");

        // Rendering
        isHorizFlipped = serializedObject.FindProperty("isHorizFlipped");
        minPixelToRender = serializedObject.FindProperty("minPixelToRender");
        normalMat = serializedObject.FindProperty("normalMat");
        outlineMat = serializedObject.FindProperty("outlineMat");
        cubeMesh = serializedObject.FindProperty("cubeMesh");
        possessableFaceTexture = serializedObject.FindProperty("possessableFaceTexture");
        unenterableColor = serializedObject.FindProperty("unenterableColor");
        unleavableColor = serializedObject.FindProperty("unleavableColor");
        enableOcclusionCulling = serializedObject.FindProperty("enableOcclusionCulling");
        surfaceEffectsAnimationIDs = serializedObject.FindProperty("surfaceEffectsAnimationIDs");

        // Cube status
        parent = serializedObject.FindProperty("parent");
        previousParents = serializedObject.FindProperty("previousParents"); ;
        relativeScale = serializedObject.FindProperty("relativeScale");
        relativePosition = serializedObject.FindProperty("relativePosition");

        // Other settings
        possessingTime = serializedObject.FindProperty("possessingTime");
        onInit = serializedObject.FindProperty("onInit");

    }
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("General Infos", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(mainContainerCube);
        EditorGUILayout.PropertyField(isEnterable);
        EditorGUILayout.PropertyField(isLeavable);
        EditorGUILayout.PropertyField(cubeType);
        EditorGUILayout.PropertyField(needInstantiating);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(isHorizFlipped);
        EditorGUILayout.PropertyField(minPixelToRender);
        EditorGUILayout.PropertyField(normalMat);
        EditorGUILayout.PropertyField(outlineMat);
        EditorGUILayout.PropertyField(cubeMesh);
        EditorGUILayout.PropertyField(possessableFaceTexture);
        EditorGUILayout.PropertyField(unenterableColor);
        EditorGUILayout.PropertyField(unleavableColor);
        EditorGUILayout.PropertyField(enableOcclusionCulling);
        EditorGUILayout.PropertyField(surfaceEffectsAnimationIDs);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(parent);
        EditorGUILayout.PropertyField(previousParents);
        EditorGUILayout.PropertyField(relativeScale);
        EditorGUILayout.PropertyField(relativePosition);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(possessingTime);
        EditorGUILayout.PropertyField(onInit);

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(targetEnterableCube);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
