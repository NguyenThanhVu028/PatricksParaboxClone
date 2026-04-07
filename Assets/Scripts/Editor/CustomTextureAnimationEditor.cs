using UnityEditor;

[CustomEditor(typeof(CustomTextureAnimation))]
public class CustomTextureAnimationEditor : Editor
{
    CustomTextureAnimation customTextureAnimation;
    private void OnEnable()
    {
        customTextureAnimation = (CustomTextureAnimation)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUI.BeginChangeCheck();
        var framesList = customTextureAnimation.Frames;
        customTextureAnimation.TotalDuration = 0;
        foreach( var frame in framesList )
        {
            customTextureAnimation.TotalDuration += frame.Duration;
        }
        EditorUtility.SetDirty(customTextureAnimation);
        serializedObject.ApplyModifiedProperties();
    }
}
