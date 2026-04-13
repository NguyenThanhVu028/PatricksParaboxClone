using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[CustomEditor(typeof(ContainerCube))]
public class ContainerCubeEditor : Editor
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
    SerializedProperty normalMat;
    SerializedProperty outlineMat;
    SerializedProperty cubeMesh;
    SerializedProperty possessableFaceTexture;
    SerializedProperty wallSubdivision;
    SerializedProperty wallRuleTile;
    SerializedProperty floorTexture;
    SerializedProperty tileTextureSize;
    SerializedProperty staticTilesRTDepth;
    SerializedProperty unenterableColor;
    SerializedProperty unleavableColor;
    SerializedProperty enableOcclusionCulling;

    // Cube status
    SerializedProperty parent;
    SerializedProperty previousParent; // Record self cube's previous parent to record history
    SerializedProperty relativeScale;
    SerializedProperty relativePosition;

    // Other settings
    SerializedProperty possessingTime;
    SerializedProperty onInit;

    // Grid settings
    SerializedProperty childGrid;

    #endregion

    ContainerCube targetEnterableCube;
    private void OnEnable()
    {
        targetEnterableCube = (ContainerCube)target;

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
        possessableFaceTexture = serializedObject.FindProperty("possessableFaceTexture");
        wallSubdivision = serializedObject.FindProperty("wallSubdivision");
        wallRuleTile = serializedObject.FindProperty("wallRuleTile");
        floorTexture = serializedObject.FindProperty("floorTexture");
        tileTextureSize = serializedObject.FindProperty("tileTextureSize");
        staticTilesRTDepth = serializedObject.FindProperty("staticTilesRTDepth");
        unenterableColor = serializedObject.FindProperty("unenterableColor");
        unleavableColor = serializedObject.FindProperty("unleavableColor");
        enableOcclusionCulling = serializedObject.FindProperty("enableOcclusionCulling");

        // Cube status
        parent = serializedObject.FindProperty("parent");
        previousParent = serializedObject.FindProperty("previousParent"); ;
        relativeScale = serializedObject.FindProperty("relativeScale");
        relativePosition = serializedObject.FindProperty("relativePosition");

        // Other settings
        possessingTime = serializedObject.FindProperty("possessingTime");
        onInit = serializedObject.FindProperty("onInit");

        // Grid settings
        childGrid = serializedObject.FindProperty("childGrid");

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

        EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(minPixelToRender);
        EditorGUILayout.PropertyField(normalMat);
        EditorGUILayout.PropertyField(outlineMat);
        EditorGUILayout.PropertyField(cubeMesh);
        EditorGUILayout.PropertyField(possessableFaceTexture);
        EditorGUILayout.PropertyField(wallSubdivision);
        EditorGUILayout.PropertyField(wallRuleTile);
        EditorGUILayout.PropertyField(floorTexture);
        EditorGUILayout.PropertyField(tileTextureSize);
        EditorGUILayout.PropertyField(staticTilesRTDepth);
        EditorGUILayout.PropertyField(unenterableColor);
        EditorGUILayout.PropertyField(unleavableColor);
        EditorGUILayout.PropertyField(enableOcclusionCulling);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(parent);
        EditorGUILayout.PropertyField(previousParent);
        EditorGUILayout.PropertyField(relativeScale);
        EditorGUILayout.PropertyField(relativePosition);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(possessingTime);
        EditorGUILayout.PropertyField(onInit);

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(childGrid);
        if (GUILayout.Button("Reset Cubes Init Details Grid"))
        {
            targetEnterableCube.ReGenerateCubesIDGrid();
            EditorUtility.SetDirty(targetEnterableCube);
        }
        EditorGUILayout.LabelField("Cubes Init Details Grid: ");
        for(int row = 0; row < targetEnterableCube.Tiling.x; row++)
        {
            EditorGUILayout.BeginHorizontal();
            // Row headers
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("ID", GUILayout.Width(80));
            EditorGUILayout.LabelField("Is Player", GUILayout.Width(80));
            EditorGUILayout.LabelField("Can Be Player", GUILayout.Width(80));
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
            for (int column = 0; column < targetEnterableCube.Tiling.y; column++)
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
