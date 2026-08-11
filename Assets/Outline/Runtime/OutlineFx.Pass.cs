using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace OutlineFx
{
    public partial class OutlineFxFeature
    {
        private static readonly int s_Alpha = Shader.PropertyToID("_Alpha");
        private static readonly int s_MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int s_Step = Shader.PropertyToID("_Step");
        private static readonly int s_Color = Shader.PropertyToID("_Color");
        private static readonly int s_Solid = Shader.PropertyToID("_Solid");

        private class Pass : ScriptableRenderPass
        {
            public OutlineFxFeature _owner;

            private class PassData
            {
                public Material material;
                public List<Outline> renderers;
                public TextureHandle buffer;
                public Vector4 step;
            }

            public void Init()
            {
                renderPassEvent = _owner._event;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if(_owner._outlineMat == null || _renderers.Count == 0) return;

                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

                TextureHandle activeColor = resourceData.activeColorTexture;
                RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
                desc.depthBufferBits = 0;
                desc.colorFormat = RenderTextureFormat.ARGB32;

                TextureHandle outlineBuffer = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "OutlineBuffer", false);

                _owner._outlineMat.SetFloat(s_Alpha, _owner._alphaCutout);
                _owner._outlineMat.SetFloat(s_Solid, _owner._solid);

                using(IUnsafeRenderGraphBuilder builder = renderGraph.AddUnsafePass<PassData>("OutlineFxDraw", out PassData passData))
                {
                    passData.material = _owner._outlineMat;
                    passData.renderers = new List<Outline>(_renderers);
                    passData.buffer = outlineBuffer;
                    passData.step = _owner._step;

                    builder.UseTexture(outlineBuffer, AccessFlags.Write);
                    builder.AllowPassCulling(false);

                    builder.SetRenderFunc((PassData data, UnsafeGraphContext context) =>
                    {
                        CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);

                        cmd.SetRenderTarget(data.buffer);
                        cmd.ClearRenderTarget(false, true, Color.clear);

                        foreach(Outline inst in data.renderers)
                        {
                            if(inst == null || inst._renderer == null) continue;

                            cmd.SetGlobalTexture(s_MainTex, inst._renderer.sharedMaterial.mainTexture);
                            cmd.SetGlobalColor(s_Color, inst.Color);
                            cmd.DrawRenderer(inst._renderer, data.material, 0, 0);
                        }
                    });
                }

                using(IUnsafeRenderGraphBuilder builder = renderGraph.AddUnsafePass<PassData>("OutlineFxBlit", out PassData passData))
                {
                    passData.material = _owner._outlineMat;
                    passData.buffer = outlineBuffer;
                    passData.step = _owner._step;

                    builder.UseTexture(outlineBuffer, AccessFlags.Read);
                    builder.UseTexture(activeColor, AccessFlags.Write);
                    builder.AllowPassCulling(false);

                    builder.SetRenderFunc((PassData data, UnsafeGraphContext context) =>
                    {
                        CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);

                        cmd.SetGlobalVector(s_Step, data.step);
                        cmd.SetGlobalTexture(s_MainTex, data.buffer);
                        cmd.SetRenderTarget(activeColor);

                        cmd.DrawMesh(ScreenMesh, Matrix4x4.identity, data.material, 0, 1);
                    });
                }

                _renderers.Clear();
            }
        }
    }
}