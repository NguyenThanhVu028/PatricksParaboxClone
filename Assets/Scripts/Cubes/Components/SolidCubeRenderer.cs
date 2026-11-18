using System;
using UnityEngine;

public class SolidCubeRenderer : BaseRenderer
{
    private const string DEBUG_PREFIX = "[SolidCubeRenderer]";

    private TexturePack _cachedTexturePack = null;
    private ColorPalette _cachedColorPalette = null;
    private Material _cachedMaterial = null;
    private Mesh _cachedMesh = null;
    public override void Render(float minSortingOrder, float maxSortingOrder, Rect rect)
    {
        RecursiveCamera.Instance?.AddRenderCall(new RecursiveCamera.RenderCall()
        {
            SortingOrder = minSortingOrder,
            Callback = () =>
            {
                DrawBase(rect);
            }
        });
    }

    public void DrawBase(Rect rect)
    {
        if (_cachedTexturePack == null)
        {
            _cachedTexturePack = ResourcesUtils.Load<TexturePack>(ResourceType.ScriptableObjects, GameConfigs.MainTexturePackName);
        }

        if (_cachedTexturePack == null)
        {
            Debug.LogWarning($"{DEBUG_PREFIX} No TexturePack found!");
            return;
        }

        if (_cachedMesh == null)
        {
            _cachedMesh = ResourcesUtils.Load<Mesh>(ResourceType.Meshes, GameConfigs.MainMeshName);
        }

        if (_cachedMesh == null)
        {
            Debug.LogWarning($"{DEBUG_PREFIX} No Mesh found!");
            return;
        }

        if (_cachedMaterial == null)
        {
            _cachedMaterial = ResourcesUtils.Load<Material>(ResourceType.Materials, GameConfigs.OutlineCubeMaterialName);
        }

        if (_cachedMaterial == null)
        {
            Debug.LogWarning($"{DEBUG_PREFIX} No Material found!");
            return;
        }

        if (_cachedColorPalette == null)
        {
            _cachedColorPalette = ResourcesUtils.Load<ColorPalette>(ResourceType.ScriptableObjects, GameConfigs.MainColorPaletteName);
        }

        if (_cachedColorPalette == null)
        {
            Debug.LogWarning($"{DEBUG_PREFIX} No ColorPalette found!");
        }

        RenderingUtils.RenderMesh(new RenderingUtils.RenderRequestData()
        {
            Mesh = _cachedMesh,
            Material = _cachedMaterial,
            Texture = _cachedTexturePack.GetTexture(TextureName.Cube_SolidCube).GetTexture(),
            Rect = rect,
            Color = _cachedColorPalette != null ? _cachedColorPalette.GetColor((ThisObject as BaseCube).Color) : Color.white,
        });

    }
}
