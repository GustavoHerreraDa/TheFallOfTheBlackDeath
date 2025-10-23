using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DitherRenderFeature : ScriptableRendererFeature
{
    class DitherRenderPass : ScriptableRenderPass
    {
        private Material material;
        private RTHandle tempRT;
        private string profilerTag = "DitherEffect";
        private static readonly int NoiseTexID = Shader.PropertyToID("_NoiseTex");
        private static readonly int ColorRampTexID = Shader.PropertyToID("_ColorRampTex");
        private static readonly int XOffsetID = Shader.PropertyToID("_XOffset");
        private static readonly int YOffsetID = Shader.PropertyToID("_YOffset");
        private static readonly int NoiseScaleID = Shader.PropertyToID("_NoiseScale");

        public DitherRenderPass(Material mat)
        {
            material = mat;
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public void Setup(RenderTextureDescriptor desc)
        {
            RenderingUtils.ReAllocateIfNeeded(ref tempRT, desc, FilterMode.Bilinear, name: "_TempDitherTex");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null) return;

            var cmd = CommandBufferPool.Get(profilerTag);
            var source = renderingData.cameraData.renderer.cameraColorTargetHandle;

            Blitter.BlitCameraTexture(cmd, source, tempRT, material, 0);
            Blitter.BlitCameraTexture(cmd, tempRT, source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    [System.Serializable]
    public class DitherSettings
    {
        public Texture2D noiseTex;
        public Texture2D colorRamp;
        [Range(16, 4096)] public float noiseScale = 512f;
        public bool useScrolling = false;
    }

    public DitherSettings settings = new DitherSettings();
    private DitherRenderPass pass;
    private Material material;

    public override void Create()
    {
        Shader shader = Shader.Find("Hidden/URP/DitherEffect");
        if (shader == null)
        {
            Debug.LogError("❌ Dither shader not found!");
            return;
        }

        material = new Material(shader);
        pass = new DitherRenderPass(material);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (material == null) return;

        Camera cam = renderingData.cameraData.camera;

        float xOffset = 0f;
        float yOffset = 0f;

        if (settings.useScrolling && cam != null)
        {
            var euler = cam.transform.eulerAngles;
            xOffset = 4.0f * euler.y / cam.fieldOfView;
            yOffset = -2.0f * cam.aspect * euler.x / cam.fieldOfView;
        }

        material.SetTexture("_NoiseTex", settings.noiseTex);
        material.SetTexture("_ColorRampTex", settings.colorRamp);
        material.SetFloat("_NoiseScale", settings.noiseScale);
        material.SetFloat("_XOffset", xOffset);
        material.SetFloat("_YOffset", yOffset);

        var desc = renderingData.cameraData.cameraTargetDescriptor;
        pass.Setup(desc);
        renderer.EnqueuePass(pass);
    }
}
