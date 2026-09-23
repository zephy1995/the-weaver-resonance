using UnityEngine;

/// <summary>
/// MGSManager - Mental Guard System
/// Single bar = health + sanity + stamina
/// Handles threshold states and Reality Fade triggers
/// </summary>
public class MGSManager : MonoBehaviour
{
    [Header("MGS Settings")]
    [SerializeField] private float maxMGS = 100f;
    [SerializeField] private float passiveDrainRate = 1f;
    [SerializeField] private float enemyHitDamage = 15f;
    
    [Header("Thresholds")]
    [SerializeField] private float strainThreshold = 60f;
    [SerializeField] private float fadeThreshold = 30f;
    
    // State
    private float currentMGS;
    private MGSState currentState;
    private bool isCollapsed = false;
    
    // References
    private RealityFadeEffect realityFade;
    private DreamWarrenManager dreamWarren;
    
    // Events
    public System.Action<MGSState> OnStateChanged;
    public System.Action<float> OnMGSChanged; // 0-1 normalized
    public System.Action OnCollapse;
    
    public enum MGSState
    {
        Stable,     // 100-60
        Strain,     // 60-30
        RealityFade,// 30-10
        Collapse    // 0
    }
    
    void Start()
    {
        currentMGS = maxMGS;
        currentState = MGSState.Stable;
        realityFade = FindObjectOfType<RealityFadeEffect>();
        dreamWarren = FindObjectOfType<DreamWarrenManager>();
        UpdateState();
    }
    
    void Update()
    {
        if (isCollapsed) return;
        
        DrainMGS(passiveDrainRate * Time.deltaTime);
        
        MGSState newState = GetStateFromMGS();
        if (newState != currentState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(currentState);
            HandleStateEffects();
        }
        
        if (currentMGS <= 0 && !isCollapsed)
        {
            Collapse();
        }
    }
    
    MGSState GetStateFromMGS()
    {
        if (currentMGS <= 0) return MGSState.Collapse;
        if (currentMGS <= fadeThreshold) return MGSState.RealityFade;
        if (currentMGS <= strainThreshold) return MGSState.Strain;
        return MGSState.Stable;
    }
    
    void HandleStateEffects()
    {
        switch (currentState)
        {
            case MGSState.RealityFade:
                realityFade?.TriggerFade(true);
                break;
            case MGSState.Strain:
                realityFade?.TriggerFade(false);
                break;
            case MGSState.Stable:
                realityFade?.TriggerFade(false);
                break;
        }
    }
    
    public void DrainMGS(float amount)
    {
        if (isCollapsed) return;
        currentMGS = Mathf.Max(0, currentMGS - amount);
        OnMGSChanged?.Invoke(currentMGS / maxMGS);
    }
    
    public void RestoreMGS(float amount)
    {
        if (isCollapsed) return;
        currentMGS = Mathf.Min(maxMGS, currentMGS + amount);
        OnMGSChanged?.Invoke(currentMGS / maxMGS);
    }
    
    public void Collapse()
    {
        isCollapsed = true;
        currentMGS = 0;
        OnMGSChanged?.Invoke(0);
        OnCollapse?.Invoke();
        dreamWarren?.EnterDreamWarren();
    }
    
    public void RecoverFromDream(float restoreAmount)
    {
        currentMGS = Mathf.Min(maxMGS, restoreAmount);
        isCollapsed = false;
        currentState = GetStateFromMGS();
        OnMGSChanged?.Invoke(currentMGS / maxMGS);
        OnStateChanged?.Invoke(currentState);
    }
    
    public float CurrentMGS => currentMGS;
    public float MaxMGS => maxMGS;
    public MGSState CurrentState => currentState;
    public bool IsCollapsed => isCollapsed;
}
