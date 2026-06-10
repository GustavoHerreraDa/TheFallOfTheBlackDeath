using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Render Feature de dithering estilo Obra Dinn para URP.
/// Aplica cuantización de luminancia con blue noise triplanar,
/// detección de bordes Sobel y paleta de dos colores configurable.
/// </summary>
public class DitherFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class DitherSettings
    {
        public bool enabled = true;
        public Shader shader;
        public Texture2D blueNoiseTex;

        [Header("Dithering")]
        public float noiseScale = 256.0f;
        [Range(1, 4)] public int bitDepth = 1;
        [Range(0f, 1f)] public float lumThreshold = 0.05f; // Corregido: antes sin usar
        [Range(0f, 1f)] public float contrast = 0.5f;
        [Range(0f, 1f)] public float brightness = 0.5f;

        [Header("Triplanar")]
        [Range(1f, 16f)] public float triplanarSharpness = 4f; // Nuevo: blend nítido vs suave

        [Header("Bordes")]
        [Range(0f, 5f)] public float edgeStrength = 1.0f;
        [Range(0f, 1f)] public float edgeThreshold = 0.1f;

        [Header("Animación")]
        public bool animatedNoise = false;

        [Header("Paleta Obra Dinn")]
        public Color colorDark  = new Color(0.08f, 0.06f, 0.05f); // Tinta oscura
        public Color colorLight = new Color(0.95f, 0.92f, 0.86f); // Papel crema
    }

    public DitherSettings settings = new DitherSettings();

    DitherPass pass;

    // ─────────────────────────────────────────────────────────────
    //  Inner Pass
    // ─────────────────────────────────────────────────────────────
    class DitherPass : ScriptableRenderPass
    {
        DitherSettings settings;
        Material material;
        RTHandle tempHandle;

        // Seguimiento del shader para recrear el material si cambia en el Inspector
        Shader lastShader;

        // IDs de propiedades cacheados (evita string lookup en cada frame)
        static readonly int ID_BlueNoiseTex      = Shader.PropertyToID("_BlueNoiseTex");
        static readonly int ID_NoiseScale         = Shader.PropertyToID("_NoiseScale");
        static readonly int ID_BitDepth           = Shader.PropertyToID("_BitDepth");
        static readonly int ID_LumThreshold       = Shader.PropertyToID("_LumThreshold");
        static readonly int ID_Contrast           = Shader.PropertyToID("_Contrast");
        static readonly int ID_Brightness         = Shader.PropertyToID("_Brightness");
        static readonly int ID_TriplanarSharpness = Shader.PropertyToID("_TriplanarSharpness");
        static readonly int ID_EdgeStrength       = Shader.PropertyToID("_EdgeStrength");
        static readonly int ID_EdgeThreshold      = Shader.PropertyToID("_EdgeThreshold");
        static readonly int ID_DitherTime         = Shader.PropertyToID("_DitherTime");
        static readonly int ID_ColorDark          = Shader.PropertyToID("_ColorDark");
        static readonly int ID_ColorLight         = Shader.PropertyToID("_ColorLight");

        public DitherPass(DitherSettings settings)
        {
            this.settings = settings;
            TryCreateMaterial();
        }

        // Separa la creación del material para poder rehacerlo si el shader cambia
        void TryCreateMaterial()
        {
            if (settings.shader == null) return;
            if (material != null) CoreUtils.Destroy(material);
            material = CoreUtils.CreateEngineMaterial(settings.shader);
            lastShader = settings.shader;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            // Recrear material si el shader cambió en el Inspector (editor workflow)
            if (settings.shader != lastShader)
                TryCreateMaterial();

            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(
                ref tempHandle, desc,
                FilterMode.Point, TextureWrapMode.Clamp,
                name: "_TempDitherTex"
            );
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // Filtro de cámara: solo Game y SceneView
            var camType = renderingData.cameraData.cameraType;
            if (camType != CameraType.Game && camType != CameraType.SceneView) return;

            // Validación de recursos
            if (!settings.enabled)          return;
            if (settings.blueNoiseTex == null) return;
            if (material == null)            return;

            var source = renderingData.cameraData.renderer.cameraColorTargetHandle;

            // Validación de handles (URP moderno puede devolver handles nulos en ciertos estados)
            if (source          == null || source.rt          == null) return;
            if (tempHandle      == null || tempHandle.rt      == null) return;

            var cmd = CommandBufferPool.Get("DitherPass");
            try
            {
                // ── Setear propiedades con IDs cacheados ──
                material.SetTexture(ID_BlueNoiseTex,      settings.blueNoiseTex);
                material.SetFloat  (ID_NoiseScale,         settings.noiseScale);
                material.SetInt    (ID_BitDepth,           settings.bitDepth);
                material.SetFloat  (ID_LumThreshold,       settings.lumThreshold);
                material.SetFloat  (ID_Contrast,           settings.contrast);
                material.SetFloat  (ID_Brightness,         settings.brightness);
                material.SetFloat  (ID_TriplanarSharpness, settings.triplanarSharpness);
                material.SetFloat  (ID_EdgeStrength,       settings.edgeStrength);
                material.SetFloat  (ID_EdgeThreshold,      settings.edgeThreshold);
                material.SetVector (ID_ColorDark,          (Vector4)(Vector3)(Vector4)settings.colorDark);
                material.SetVector (ID_ColorLight,         (Vector4)(Vector3)(Vector4)settings.colorLight);

                // Animación basada en frame (sin crawling suave)
                // El shader usa floor(_DitherTime) para desplazamiento por frame
                float ditherTime = settings.animatedNoise ? (float)Time.frameCount : 0.0f;
                material.SetFloat(ID_DitherTime, ditherTime);

                // Source → Temp (efecto) → Source (resultado)
                Blitter.BlitCameraTexture(cmd, source, tempHandle, material, 0);
                Blitter.BlitCameraTexture(cmd, tempHandle, source);

                context.ExecuteCommandBuffer(cmd);
            }
            finally
            {
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }
        }

        public void Dispose()
        {
            tempHandle?.Release();
            CoreUtils.Destroy(material); // Correcto: usar CoreUtils para materiales de engine
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  ScriptableRendererFeature lifecycle
    // ─────────────────────────────────────────────────────────────
    public override void Create()
    {
        pass = new DitherPass(settings)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // No aplicar a cámaras Overlay (evita doble procesamiento en setups multicámara)
        if (renderingData.cameraData.renderType == CameraRenderType.Overlay) return;
        if (!settings.enabled) return; // Check temprano para no encolar el pase

        renderer.EnqueuePass(pass);
    }

    protected override void Dispose(bool disposing)
    {
        pass?.Dispose();
    }
}