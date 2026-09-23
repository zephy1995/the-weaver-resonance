using UnityEngine;

/// <summary>
/// Anchor - Individual anchor node in a sector
/// Place on anchor GameObjects in the scene
/// </summary>
public class Anchor : MonoBehaviour
{
    [Header("Anchor Settings")]
    [SerializeField] private float activationRadius = 1.5f;
    [SerializeField] private float instabilityReduction = 20f;
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color activeColor = Color.cyan;
    [SerializeField] private Color completedColor = Color.green;
    
    // State
    private bool isActivated = false;
    private bool isCompleted = false;
    private SpriteRenderer spriteRenderer;
    
    // Events
    public System.Action OnActivated;
    public System.Action OnCompleted;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.color = inactiveColor;
    }
    
    public void Activate()
    {
        if (isActivated) return;
        
        isActivated = true;
        if (spriteRenderer != null)
            spriteRenderer.color = activeColor;
        
        OnActivated?.Invoke();
        CompleteActivation();
    }
    
    void CompleteActivation()
    {
        isCompleted = true;
        if (spriteRenderer != null)
            spriteRenderer.color = completedColor;
        
        OnCompleted?.Invoke();
        FindObjectOfType<SectorManager>()?.OnAnchorCompleted();
    }
    
    public void ResetAnchor()
    {
        isActivated = false;
        isCompleted = false;
        if (spriteRenderer != null)
            spriteRenderer.color = inactiveColor;
    }
    
    public bool IsActivated => isActivated;
    public bool IsCompleted => isCompleted;
    public float ActivationRadius => activationRadius;
    public float InstabilityReduction => instabilityReduction;
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}

/// <summary>
/// SectorManager - Manages all anchors in a sector
/// Attach to an empty GameObject in the scene
/// </summary>
public class SectorManager : MonoBehaviour
{
    [Header("Sector Settings")]
    [SerializeField] private Anchor[] anchors;
    [SerializeField] private float totalInstability = 100f;
    
    // State
    private int completedAnchors = 0;
    private float currentInstability;
    private bool isStabilized = false;
    
    // Events
    public System.Action OnSectorStabilized;
    public System.Action<float> OnInstabilityChanged; // 0-1 normalized
    
    void Start()
    {
        if (anchors == null || anchors.Length == 0)
            anchors = FindObjectsOfType<Anchor>();
        
        currentInstability = totalInstability;
        
        foreach (var anchor in anchors)
        {
            anchor.OnCompleted += OnAnchorCompleted;
        }
    }
    
    public void OnAnchorCompleted()
    {
        completedAnchors++;
        
        float reduction = totalInstability / anchors.Length;
        currentInstability = Mathf.Max(0, currentInstability - reduction);
        OnInstabilityChanged?.Invoke(currentInstability / totalInstability);
        
        if (completedAnchors >= anchors.Length)
        {
            StabilizeSector();
        }
    }
    
    void StabilizeSector()
    {
        isStabilized = true;
        OnSectorStabilized?.Invoke();
        Debug.Log("SECTOR STABILIZED!");
    }
    
    public void ResetSector()
    {
        completedAnchors = 0;
        currentInstability = totalInstability;
        isStabilized = false;
        
        foreach (var anchor in anchors)
        {
            anchor.ResetAnchor();
        }
        
        OnInstabilityChanged?.Invoke(1f);
    }
    
    public bool IsStabilized => isStabilized;
    public int TotalAnchors => anchors.Length;
    public int CompletedAnchors => completedAnchors;
    public float CurrentInstability => currentInstability;
}
