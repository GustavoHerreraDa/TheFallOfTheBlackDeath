using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DitherFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class DitherSettings
    {
        public Shader shader;
        public Texture2D ditherTex;
        public Texture2D rampTex;
        public bool useScrolling = false;
        public FilterMode filterMode = FilterMode.Bilinear;
    }

    public DitherSettings settings = new DitherSettings();

    class DitherPass : ScriptableRenderPass
    {
        private Material material;
        private DitherSettings settings;
        [System.Obsolete]
        private RenderTargetHandle tempTexture;

        public DitherPass(DitherSettings settings)
        {
            this.settings = settings;
            if (settings.shader)
                material = new Material(settings.shader);
            tempTexture.Init("_TempDitherTex");
        }

        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null) return;

            var cmd = CommandBufferPool.Get("DitherPass");
            ref var cameraData = ref renderingData.cameraData;

            // ✅ Ahora sí podemos usarlo
            var source = cameraData.renderer.cameraColorTargetHandle;

            material.SetTexture("_NoiseTex", settings.ditherTex);
            material.SetTexture("_ColorRampTex", settings.rampTex);

            float xOffset = 0f, yOffset = 0f;
            var cam = cameraData.camera;
            if (settings.useScrolling)
            {
                var euler = cam.transform.eulerAngles;
                xOffset = 4.0f * euler.y / cam.fieldOfView;
                yOffset = -2.0f * cam.aspect * euler.x / cam.fieldOfView;
            }
            material.SetFloat("_XOffset", xOffset);
            material.SetFloat("_YOffset", yOffset);

            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            cmd.GetTemporaryRT(tempTexture.id, desc, settings.filterMode);
            Blit(cmd, source, tempTexture.Identifier(), material);
            Blit(cmd, tempTexture.Identifier(), source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            cmd.ReleaseTemporaryRT(tempTexture.id);
        }
    }
    DitherPass pass;

    public override void Create()
    {
        pass = new DitherPass(settings)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (pass == null)
            return;

        // No llames a renderer.cameraColorTargetHandle aquí
        renderer.EnqueuePass(pass);
    }
}
