using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomRendererFeature : ScriptableRendererFeature
{
    public class CustomRenderPass : ScriptableRenderPass
    {
        public static CommandBuffer CommandBuffer;
        public static UnityEvent OnExecuteCmd = new();
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer = CommandBufferPool.Get("Custom Render Pass");
            OnExecuteCmd.Invoke();
            context.ExecuteCommandBuffer(CommandBuffer);
            CommandBufferPool.Release(CommandBuffer); // Always release it back to the pool
        }
    }

    CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass();

        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game || renderingData.cameraData.cameraType == CameraType.SceneView)
        {
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }
}
