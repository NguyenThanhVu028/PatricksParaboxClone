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

        TexturePack texturePack = ResourcesUtils.Load<TexturePack>(ResourceType.ScriptableObjects, GameConfigs.MainTexturePackName);

        testRuleTile.SetOutput("Top Left", texturePack.GetTexture(TextureName.Cube_Wall_TopLeft).GetTexture());
        testRuleTile.SetOutput("Left", texturePack.GetTexture(TextureName.Cube_Wall_Left).GetTexture());
        testRuleTile.SetOutput("Bottom Left", texturePack.GetTexture(TextureName.Cube_Wall_BottomLeft).GetTexture());
        testRuleTile.SetOutput("Bottom", texturePack.GetTexture(TextureName.Cube_Wall_Bottom).GetTexture());
        testRuleTile.SetOutput("Bottom Right", texturePack.GetTexture(TextureName.Cube_Wall_BottomRight).GetTexture());
        testRuleTile.SetOutput("Right", texturePack.GetTexture(TextureName.Cube_Wall_Right).GetTexture());
        testRuleTile.SetOutput("Top Right", texturePack.GetTexture(TextureName.Cube_Wall_TopRight).GetTexture());
        testRuleTile.SetOutput("Top", texturePack.GetTexture(TextureName.Cube_Wall_Top).GetTexture());
        testRuleTile.SetOutput("Inner Top Left", texturePack.GetTexture(TextureName.Cube_Wall_InnerTopLeft).GetTexture());
        testRuleTile.SetOutput("Inner Bottom Left", texturePack.GetTexture(TextureName.Cube_Wall_InnerBottomLeft).GetTexture());
        testRuleTile.SetOutput("Inner Bottom Right", texturePack.GetTexture(TextureName.Cube_Wall_InnerBottomRight).GetTexture());
        testRuleTile.SetOutput("Inner Top Right", texturePack.GetTexture(TextureName.Cube_Wall_InnerTopRight).GetTexture());
        testRuleTile.SetOutput("Center", texturePack.GetTexture(TextureName.Cube_Wall_Center).GetTexture());
        testRuleTile.SetDefaultTexture(texturePack.GetTexture(TextureName.Cube_Wall_Center).GetTexture());

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
