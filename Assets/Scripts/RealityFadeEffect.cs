using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// RealityFadeEffect - Visual/audio distortion at low MGS
/// Requires Post-Processing Stack v2
/// </summary>
public class RealityFadeEffect : MonoBehaviour
{
    [Header("Visual Effects")]
    [SerializeField] private float ghostIntensity = 0.5f;
    [SerializeField] private float chromaticAberrationIntensity = 0.8f;
    [SerializeField] private float vignetteIntensity = 0.6f;
    [SerializeField] private float inputDelayMin = 0.1f;
    [SerializeField] private float inputDelayMax = 0.3f;
    
    [Header("Audio")]
    [SerializeField] private AudioLowPassFilter lowPassFilter;
    [SerializeField] private float lowPassCutoff = 1000f;
    
    [Header("UI")]
    [SerializeField] private CanvasGroup uiCanvas;
    [SerializeField] private float uiFlickerRate = 0.1f;
    
    // Post-processing
    private PostProcessVolume postProcessVolume;
    private ChromaticAberration chromaticAberration;
    private Vignette vignette;
    private MotionBlur motionBlur;
    
    // State
    private bool isFading = false;
    private float inputDelayTimer = 0f;
    private float currentInputDelay = 0f;
    private float uiFlickerTimer = 0f;
    
    void Start()
    {
        postProcessVolume = GetComponent<PostProcessVolume>();
        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGetSettings(out chromaticAberration);
            postProcessVolume.profile.TryGetSettings(out vignette);
            postProcessVolume.profile.TryGetSettings(out motionBlur);
        }
        
        if (lowPassFilter == null)
            lowPassFilter = GetComponent<AudioLowPassFilter>();
        
        SetEffectsActive(false);
    }
    
    void Update()
    {
        if (!isFading) return;
        
        if (inputDelayTimer > 0)
        {
            inputDelayTimer -= Time.deltaTime;
        }
        
        uiFlickerTimer -= Time.deltaTime;
        if (uiFlickerTimer <= 0)
        {
            uiFlickerTimer = uiFlickerRate;
            if (uiCanvas != null)
            {
                uiCanvas.alpha = Random.value > 0.5f ? 1f : 0.3f;
            }
        }
    }
    
    public void TriggerFade(bool active)
    {
        isFading = active;
        SetEffectsActive(active);
        
        if (active)
        {
            currentInputDelay = Random.Range(inputDelayMin, inputDelayMax);
            inputDelayTimer = currentInputDelay;
        }
        else
        {
            if (uiCanvas != null)
                uiCanvas.alpha = 1f;
        }
    }
    
    void SetEffectsActive(bool active)
    {
        if (chromaticAberration != null)
            chromaticAberration.intensity.value = active ? chromaticAberrationIntensity : 0f;
        
        if (vignette != null)
            vignette.intensity.value = active ? vignetteIntensity : 0.3f;
        
        if (motionBlur != null)
            motionBlur.intensity.value = active ? ghostIntensity : 0f;
        
        if (lowPassFilter != null)
            lowPassFilter.cutoffFrequency = active ? lowPassCutoff : 22000f;
    }
    
    public bool IsInputDelayed()
    {
        return isFading && inputDelayTimer > 0;
    }
    
    public float CurrentInputDelay => currentInputDelay;
}
