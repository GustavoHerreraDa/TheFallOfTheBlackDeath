using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DitherFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class DitherSettings
    {
        public bool enabled = true;
        public Shader shader;
        public Texture2D blueNoiseTex;
        public float noiseScale = 256.0f;
        [Range(1, 4)] public int bitDepth = 1;
        [Range(0, 1)] public float contrast = 0.5f;
        [Range(0, 1)] public float brightness = 0.5f;
        [Range(0, 1)] public float threshold = 0.5f;
        [Range(0, 5)] public float edgeStrength = 1.0f;
        [Range(0, 1)] public float edgeThreshold = 0.1f;
        public bool animatedNoise = false;
    }

    public DitherSettings settings = new DitherSettings();
    DitherPass pass;

    class DitherPass : ScriptableRenderPass
    {
        private Material material;
        private DitherSettings settings;
        private RTHandle tempTextureHandle; // EL NUEVO SISTEMA

        public DitherPass(DitherSettings settings)
        {
            this.settings = settings;
            if (settings.shader) material = new Material(settings.shader);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            // Requerir Depth y Normals para el shader
            ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);
        }

        // Se llama cuando la cámara se configura (antes de renderizar)
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0; // No necesitamos profundidad
            
            // Reasignar el RTHandle automáticamente si la resolución cambia
            RenderingUtils.ReAllocateIfNeeded(ref tempTextureHandle, desc, FilterMode.Point, TextureWrapMode.Clamp, name: "_TempDitherTex");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!settings.enabled || material == null) return;

            var cmd = CommandBufferPool.Get("DitherPass");
            
            // En URP moderno, accedemos al source así:
            var source = renderingData.cameraData.renderer.cameraColorTargetHandle;
            var camera = renderingData.cameraData.camera;

            // --- LÓGICA DE OBRA DINN MEJORADA ---
            material.SetTexture("_BlueNoiseTex", settings.blueNoiseTex);
            material.SetFloat("_NoiseScale", settings.noiseScale);
            material.SetInt("_BitDepth", settings.bitDepth);
            material.SetFloat("_Contrast", settings.contrast);
            material.SetFloat("_Brightness", settings.brightness);
            material.SetFloat("_Threshold", settings.threshold);
            material.SetFloat("_EdgeStrength", settings.edgeStrength);
            material.SetFloat("_EdgeThreshold", settings.edgeThreshold);
            material.SetFloat("_DitherTime", settings.animatedNoise ? Time.time : 0.0f);

            // Blit usando RTHandles (Source -> Temp -> Source)
            Blitter.BlitCameraTexture(cmd, source, tempTextureHandle, material, 0);
            Blitter.BlitCameraTexture(cmd, tempTextureHandle, source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            tempTextureHandle?.Release();
        }
    }

    public override void Create()
    {
        pass = new DitherPass(settings)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.renderType == CameraRenderType.Overlay) return;
        renderer.EnqueuePass(pass);
    }

    protected override void Dispose(bool disposing)
    {
        pass?.Dispose();
    }
}