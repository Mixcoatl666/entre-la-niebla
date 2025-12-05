using UnityEngine;

/// <summary>
/// Configurador automático de Canvas para que la UI se vea correcta en todas las resoluciones
/// Especialmente útil para móviles con diferentes aspect ratios
/// </summary>
[RequireComponent(typeof(Canvas))]
public class CanvasScalerConfig : MonoBehaviour
{
    [Header("Configuración de Referencia")]
    [Tooltip("Resolución de referencia (diseñaste tu UI para esto)")]
    public Vector2 resolucionReferencia = new Vector2(1920f, 1080f);
    
    [Header("Modo de Escalado")]
    [Tooltip("Scale With Screen Size = Recomendado para juegos con UI")]
    public UnityEngine.UI.CanvasScaler.ScaleMode scaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
    
    [Tooltip("Match: 0 = Width, 0.5 = Balance, 1 = Height")]
    [Range(0f, 1f)]
    public float match = 0.5f;
    
    [Header("Orientación")]
    [Tooltip("¿Forzar orientación específica?")]
    public bool forzarOrientacion = true;
    
    [Tooltip("Orientación del juego")]
    public ScreenOrientation orientacionDeseada = ScreenOrientation.LandscapeLeft;
    
    [Header("Safe Area (para móviles con notch)")]
    [Tooltip("¿Ajustar por Safe Area? (iPhone X, notches, etc.)")]
    public bool usarSafeArea = true;
    
    [Tooltip("Panel que debe ajustarse al Safe Area (opcional)")]
    public RectTransform panelPrincipal;
    
    [Header("Debug")]
    public bool mostrarDebug = true;
    
    private UnityEngine.UI.CanvasScaler canvasScaler;
    private Canvas canvas;

    void Awake()
    {
        ConfigurarCanvas();
        ConfigurarOrientacion();
        
        if (usarSafeArea)
        {
            AplicarSafeArea();
        }
    }
    
    void Start()
    {
        if (mostrarDebug)
        {
            MostrarInfoResolucion();
        }
    }
    
    /// <summary>
    /// Configura el Canvas Scaler para que escale correctamente
    /// </summary>
    void ConfigurarCanvas()
    {
        canvas = GetComponent<Canvas>();
        canvasScaler = GetComponent<UnityEngine.UI.CanvasScaler>();
        
        if (canvasScaler == null)
        {
            canvasScaler = gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            Debug.Log("[CanvasScalerConfig] CanvasScaler agregado automáticamente");
        }
        
        // Configurar Canvas Scaler
        canvasScaler.uiScaleMode = scaleMode;
        canvasScaler.referenceResolution = resolucionReferencia;
        canvasScaler.matchWidthOrHeight = match;
        canvasScaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        
        // Configuración adicional para mejor calidad
        canvasScaler.referencePixelsPerUnit = 100f;
        
        if (mostrarDebug)
        {
            Debug.Log($"[CanvasScalerConfig] Canvas configurado: {resolucionReferencia.x}x{resolucionReferencia.y}, Match: {match}");
        }
    }
    
    /// <summary>
    /// Fuerza la orientación de pantalla
    /// </summary>
    void ConfigurarOrientacion()
    {
        if (!forzarOrientacion) return;
        
        Screen.orientation = orientacionDeseada;
        
        // Bloquear auto-rotación
        Screen.autorotateToLandscapeLeft = orientacionDeseada == ScreenOrientation.LandscapeLeft;
        Screen.autorotateToLandscapeRight = orientacionDeseada == ScreenOrientation.LandscapeRight;
        Screen.autorotateToPortrait = orientacionDeseada == ScreenOrientation.Portrait;
        Screen.autorotateToPortraitUpsideDown = orientacionDeseada == ScreenOrientation.PortraitUpsideDown;
        
        if (mostrarDebug)
        {
            Debug.Log($"[CanvasScalerConfig] Orientación forzada a: {orientacionDeseada}");
        }
    }
    
    /// <summary>
    /// Ajusta el panel principal al Safe Area (para móviles con notch)
    /// </summary>
    void AplicarSafeArea()
    {
        if (panelPrincipal == null)
        {
            // Intentar encontrar el panel principal automáticamente
            panelPrincipal = transform.GetChild(0) as RectTransform;
        }
        
        if (panelPrincipal == null)
        {
            if (mostrarDebug)
            {
                Debug.LogWarning("[CanvasScalerConfig] No hay panel principal asignado para Safe Area");
            }
            return;
        }
        
        Rect safeArea = Screen.safeArea;
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        
        panelPrincipal.anchorMin = anchorMin;
        panelPrincipal.anchorMax = anchorMax;
        
        if (mostrarDebug)
        {
            Debug.Log($"[CanvasScalerConfig] Safe Area aplicada: {safeArea}");
        }
    }
    
    /// <summary>
    /// Muestra información de depuración sobre la resolución
    /// </summary>
    void MostrarInfoResolucion()
    {
        Debug.Log("=== INFORMACIÓN DE RESOLUCIÓN ===");
        Debug.Log($"Resolución Actual: {Screen.width}x{Screen.height}");
        Debug.Log($"Resolución de Referencia: {resolucionReferencia.x}x{resolucionReferencia.y}");
        Debug.Log($"Aspect Ratio Actual: {(float)Screen.width / Screen.height:F2}");
        Debug.Log($"Aspect Ratio Referencia: {resolucionReferencia.x / resolucionReferencia.y:F2}");
        Debug.Log($"DPI: {Screen.dpi}");
        Debug.Log($"Orientación: {Screen.orientation}");
        Debug.Log($"Safe Area: {Screen.safeArea}");
        Debug.Log($"Plataforma: {Application.platform}");
        Debug.Log("================================");
    }
    
    /// <summary>
    /// Método público para cambiar el match en runtime
    /// </summary>
    public void SetMatch(float newMatch)
    {
        match = Mathf.Clamp01(newMatch);
        if (canvasScaler != null)
        {
            canvasScaler.matchWidthOrHeight = match;
        }
    }
    
    /// <summary>
    /// Método público para cambiar la orientación en runtime
    /// </summary>
    public void CambiarOrientacion(ScreenOrientation nuevaOrientacion)
    {
        orientacionDeseada = nuevaOrientacion;
        ConfigurarOrientacion();
    }

#if UNITY_EDITOR
    // Botón en el Inspector para probar diferentes resoluciones
    [ContextMenu("Probar Resolución 16:9 (1920x1080)")]
    void ProbarResolucion16_9()
    {
        UnityEditor.GameViewSizeGroupType currentGroup = UnityEditor.GameViewSizeGroupType.Standalone;
        Debug.Log("Cambia manualmente a 1920x1080 en Game View para probar");
    }
    
    [ContextMenu("Probar Resolución 18:9 (2160x1080)")]
    void ProbarResolucion18_9()
    {
        Debug.Log("Cambia manualmente a 2160x1080 en Game View para probar");
    }
    
    [ContextMenu("Probar Resolución 19.5:9 (2340x1080)")]
    void ProbarResolucion19_5_9()
    {
        Debug.Log("Cambia manualmente a 2340x1080 en Game View para probar");
    }
#endif
}
