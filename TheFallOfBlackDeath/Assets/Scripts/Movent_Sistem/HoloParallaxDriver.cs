// ============================================================
//  HoloParallaxDriver.cs
//  Calcula el vector de parallax a partir de la posición de la
//  cámara principal y lo inyecta al shader cada frame.
//
//  Uso:
//    Añade este script al mismo GameObject que tiene el material
//    HoloButton_Diegetic (o a cualquier objeto con acceso al
//    renderer). Asigna los materiales holoMaterials en el Inspector.
//    El script funciona tanto con Renderer (mundo 3D) como con
//    Image de UI (Canvas en World Space).
//
//  Requiere: el shader HoloButton_Diegetic v4 o superior.
// ============================================================
 
using UnityEngine;
 
public class HoloParallaxDriver : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────
 
    [Header("Materiales holográficos a actualizar")]
    [Tooltip("Asigna aquí todos los materiales de instancia que usen HoloButton_Diegetic.")]
    [SerializeField] private Material[] _holoMaterials;
 
    [Header("Referencia de la cámara (opcional — usa Camera.main si está vacío)")]
    [SerializeField] private Camera _camera;
 
    [Header("Punto de referencia del objeto (centro 'neutro' del parallax)")]
    [Tooltip("Normalmente es la posición del propio botón en world space.")]
    [SerializeField] private Transform _pivotTransform;
 
    [Header("Amortiguación (suavizado del movimiento)")]
    [Tooltip("Cuanto más alto, más lento y suave responde el parallax. 0 = sin suavizado.")]
    [SerializeField, Range(0f, 20f)] private float _damping = 8f;
 
    [Header("Límite de desplazamiento máximo en UV")]
    [Tooltip("Clampea el vector para que capas profundas no salgan del quad visible.")]
    [SerializeField] private float _maxUVOffset = 0.12f;
 
    [Header("Debug")]
    [SerializeField] private bool _drawGizmo = true;
 
    // ── IDs de propiedades (hash = más rápido que strings en SetVector) ────
    private static readonly int PropParallaxVec = Shader.PropertyToID("_ParallaxVec");
 
    // ── Estado interno ─────────────────────────────────────────────────────
    private Vector2 _currentVec   = Vector2.zero;
    private Vector2 _targetVec    = Vector2.zero;
    private Vector3 _worldCenter  = Vector3.zero;
 
    // ── Inicialización ─────────────────────────────────────────────────────
    private void Awake()
    {
        if (_camera == null)
            _camera = Camera.main;
 
        if (_pivotTransform == null)
            _pivotTransform = transform;
 
        // Si no se asignaron materiales manualmente, buscamos en el Renderer
        if (_holoMaterials == null || _holoMaterials.Length == 0)
        {
            var r = GetComponent<Renderer>();
            if (r != null)
                _holoMaterials = r.materials; // Unity instancia automáticamente
        }
    }
 
    // ── Update ─────────────────────────────────────────────────────────────
    private void LateUpdate()
    {
        if (_camera == null || _holoMaterials == null) return;
 
        // ── 1. Obtener posición de cámara en espacio de vista del objeto ────
        //
        // Queremos un vector 2D que represente "cuánto se ha desviado la
        // cámara del punto de vista neutro (frontal directo al botón)".
        //
        // Strategy:
        //   a) Calcular el vector cámara → pivot en world space.
        //   b) Proyectarlo sobre los ejes locales del pivot (right, up).
        //   c) Normalizar por la distancia para que el efecto sea consistente
        //      independientemente de qué tan cerca/lejos esté la cámara.
 
        Vector3 camPos    = _camera.transform.position;
        Vector3 pivotPos  = _pivotTransform.position;
 
        // Vector desde el pivot hacia la cámara
        Vector3 toCam = camPos - pivotPos;
 
        // Distancia (para normalizar el efecto con la distancia)
        float distance = Mathf.Max(toCam.magnitude, 0.1f);
 
        // Componentes laterales: proyección sobre ejes locales del pivot
        // Usamos los ejes del pivot (no de la cámara) para que el efecto
        // sea relativo a la orientación del botón holográfico.
        float offsetX = Vector3.Dot(toCam, _pivotTransform.right) / distance;
        float offsetY = Vector3.Dot(toCam, _pivotTransform.up)    / distance;
 
        // ── 2. Remap y clamp ────────────────────────────────────────────────
        // offsetX/Y están en [-1, 1] por la normalización con distance.
        // Los escalamos a UV space y los clampamos para evitar que capas
        // muy profundas salgan del quad.
        _targetVec = Vector2.ClampMagnitude(
            new Vector2(offsetX, offsetY),
            _maxUVOffset
        );
 
        // ── 3. Suavizado (damping) ──────────────────────────────────────────
        // Lerp independiente de framerate usando Time.deltaTime.
        // _damping = 0 → sin suavizado (respuesta instantánea)
        // _damping = 15 → respuesta muy suave (cámara lenta)
        if (_damping > 0f)
        {
            _currentVec = Vector2.Lerp(
                _currentVec,
                _targetVec,
                1f - Mathf.Exp(-_damping * Time.deltaTime)
            );
        }
        else
        {
            _currentVec = _targetVec;
        }
 
        // ── 4. Pasar al shader ──────────────────────────────────────────────
        // Vector4 porque SetVector necesita 4 componentes.
        // Z y W quedan en 0 — reservados para extensiones futuras (ej: rotación).
        var vec4 = new Vector4(_currentVec.x, _currentVec.y, 0f, 0f);
 
        foreach (var mat in _holoMaterials)
        {
            if (mat != null)
                mat.SetVector(PropParallaxVec, vec4);
        }
    }
 
    // ── Gizmo de debug ─────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        if (!_drawGizmo || _pivotTransform == null) return;
 
        // Dibuja el vector parallax actual como una flecha en el editor
        Gizmos.color = Color.cyan;
        Vector3 origin = _pivotTransform.position;
        Vector3 offset = _pivotTransform.right   * _currentVec.x
                       + _pivotTransform.up      * _currentVec.y;
        Gizmos.DrawLine(origin, origin + offset);
        Gizmos.DrawSphere(origin + offset, 0.02f);
 
        // Dibuja el límite máximo como un círculo
        Gizmos.color = new Color(0f, 1f, 1f, 0.2f);
        // Aproximamos el círculo con líneas del gizmo
        for (int i = 0; i < 32; i++)
        {
            float a1 = i       / 32f * Mathf.PI * 2f;
            float a2 = (i + 1) / 32f * Mathf.PI * 2f;
            Vector3 p1 = origin + (_pivotTransform.right * Mathf.Cos(a1) +
                                   _pivotTransform.up    * Mathf.Sin(a1)) * _maxUVOffset;
            Vector3 p2 = origin + (_pivotTransform.right * Mathf.Cos(a2) +
                                   _pivotTransform.up    * Mathf.Sin(a2)) * _maxUVOffset;
            Gizmos.DrawLine(p1, p2);
        }
    }
}