using UnityEditor;

[CustomEditor(typeof(InfinityCube))]
public class InfinityCubeEditor : Editor
{
    #region SerializedProperties
    // General Infos
    SerializedProperty mainContainerCube;
    SerializedProperty cubeType;
    SerializedProperty needInstantiating;
    SerializedProperty isEnterable;
    SerializedProperty isLeavable;

    // Rendering
    SerializedProperty minPixelToRender;
    SerializedProperty normalMat;
    SerializedProperty outlineMat;
    SerializedProperty cubeMesh;
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
    SerializedProperty onInit;

    // Infinity settings
    SerializedProperty level;
    SerializedProperty infinityTexture;
    SerializedProperty infinityTextureHeightRatio;

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
        minPixelToRender = serializedObject.FindProperty("minPixelToRender");
        normalMat = serializedObject.FindProperty("normalMat");
        outlineMat = serializedObject.FindProperty("outlineMat");
        cubeMesh = serializedObject.FindProperty("cubeMesh");
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
        onInit = serializedObject.FindProperty("onInit");

        // Infinity settings
        level = serializedObject.FindProperty("level");
        infinityTexture = serializedObject.FindProperty("infinityTexture");
        infinityTextureHeightRatio = serializedObject.FindProperty("infinityTextureHeightRatio");

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
        EditorGUILayout.PropertyField(minPixelToRender);
        EditorGUILayout.PropertyField(normalMat);
        EditorGUILayout.PropertyField(outlineMat);
        EditorGUILayout.PropertyField(cubeMesh);
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
        EditorGUILayout.LabelField("Infinity Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(level);
        EditorGUILayout.PropertyField(infinityTexture);
        EditorGUILayout.PropertyField(infinityTextureHeightRatio);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(onInit);

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(targetEnterableCube);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
