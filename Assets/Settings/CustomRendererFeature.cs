using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class CustomRendererFeature : ScriptableRendererFeature
{
    public class CustomRenderPass : ScriptableRenderPass
    {
        //public static CommandBuffer CommandBuffer;
        public static UnityEvent OnExecuteCmd = new();

        public static RasterCommandBuffer CommandBuffer;
        //public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        //{
        //    CommandBuffer = CommandBufferPool.Get("Custom Render Pass");
        //    OnExecuteCmd.Invoke();
        //    context.ExecuteCommandBuffer(CommandBuffer);
        //    CommandBufferPool.Release(CommandBuffer); // Always release it back to the pool
        //}

        public class PassData { }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Custom Render Pass", out var passData))
            {
                builder.AllowPassCulling(false);

                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                if (resourceData.activeColorTexture.IsValid())
                {
                    builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                }

                if (resourceData.activeDepthTexture.IsValid())
                {
                    builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Write);
                }

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    if (OnExecuteCmd != null)
                    {
                        CommandBuffer = context.cmd;
                        OnExecuteCmd.Invoke();

                    }
                });
            }
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
