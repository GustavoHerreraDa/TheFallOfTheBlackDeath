using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DitherFeatures : ScriptableRendererFeature
{
    [System.Serializable]
    public class DitherSettings
    {
        public bool enabled = true;
        public Shader shader;
        public Texture2D ditherTex;
        public Texture2D rampTex;
        public float noiseScale = 256.0f; // Controlar escala desde el inspector
        [Range(0, 1)] public float spread = 0.5f; // Controlar intensidad
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
            // Enviamos la matriz inversa de vista para calcular la dirección del rayo en el shader
            material.SetMatrix("_InverseView", camera.cameraToWorldMatrix);
            material.SetTexture("_NoiseTex", settings.ditherTex);
            material.SetTexture("_ColorRampTex", settings.rampTex);
            material.SetFloat("_NoiseScale", settings.noiseScale);
            
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