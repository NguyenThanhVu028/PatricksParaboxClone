using UnityEditor;

[CustomEditor(typeof(CustomAnimatedTexture))]
public class CustomTextureAnimationEditor : Editor
{
    CustomAnimatedTexture customTextureAnimation;
    private void OnEnable()
    {
        customTextureAnimation = (CustomAnimatedTexture)target;
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
