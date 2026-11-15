using UnityEngine;

public class TestMyRuleTile : MonoBehaviour
{
    [SerializeField] RenderTexture _outputRenderTexture;
    [SerializeField] Material _material;

    [ContextMenu("Test")]
    public void Test()
    {
        MyRuleTile<TestWall> testRuleTile = new(_material);
        TestObject[,] testGrid = new TestObject[,]
        {
            {new TestWall(), new TestWall(), new TestWall(), new TestWall(), new TestWall()},
            {new TestWall(), new OtherObject(), new OtherObject(), new TestWall(), new TestWall()},
            {new OtherObject(), new OtherObject(), new OtherObject(), new TestWall(), new OtherObject()},
            {new TestWall(), new OtherObject(), new TestWall(), new OtherObject(), new OtherObject()},
            {new OtherObject(), new OtherObject(), new OtherObject(), new TestWall(), new TestWall()}
        };

        // testRuleTile.SetOutput("Top Left", ResourcesUtils.Load<Texture2D>(ResourceTypes.Sprites, "TopLeft"));
        // testRuleTile.SetOutput("Left", ResourcesUtils.Load<Texture2D>(ResourceTypes.Sprites, "Left"));
        // testRuleTile.SetOutput("Bottom Left", ResourcesUtils.Load<Texture2D>(ResourceTypes.Sprites, "BottomLeft"));
        // testRuleTile.SetOutput("Bottom", ResourcesUtils.Load<Texture2D>(ResourceTypes.Sprites, "Bottom"));
        // testRuleTile.SetOutput("Bottom Right", ResourcesUtils.Load<Texture2D>(ResourceTypes.Sprites, "BottomRight"));
        // testRuleTile.SetOutput("Right", ResourcesUtils.Load<Texture2D>(ResourceTypes.Sprites, "Right"));
        // testRuleTile.SetOutput("Top Right", ResourcesUtils.Load<Texture2D>(ResourceTypes.Sprites, "TopRight"));
        // testRuleTile.SetOutput("Top", ResourcesUtils.Load<Texture2D>(ResourceTypes.Sprites, "Top"));
        // testRuleTile.SetOutput("Inner Top Left", ResourcesUtils.Load<Texture2D>(ResourceTypes.Sprites, "InnerTopLeft"));
        // testRuleTile.SetOutput("Inner Bottom Left", ResourcesUtils.Load<Texture2D>(ResourceTypes.Sprites, "InnerBottomLeft"));
        // testRuleTile.SetOutput("Inner Bottom Right", ResourcesUtils.Load<Texture2D>(ResourceTypes.Sprites, "InnerBottomRight"));
        // testRuleTile.SetOutput("Inner Top Right", ResourcesUtils.Load<Texture2D>(ResourceTypes.Sprites, "InnerTopRight"));
        // testRuleTile.SetOutput("Center", ResourcesUtils.Load<Texture2D>(ResourceTypes.Sprites, "Center"));
        // testRuleTile.SetDefaultTexture(ResourcesUtils.Load<Texture2D>(ResourceTypes.Sprites, "Center"));

        testRuleTile.DrawOnRenderTexture(testGrid, 2, new(16, 16), ref _outputRenderTexture);
    }

    private class TestObject
    {

    }

    private class TestWall : TestObject
    {

    }

    private class OtherObject : TestObject
    {

    }
}
