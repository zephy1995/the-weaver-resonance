using UnityEngine;

/// <summary>
/// ToriiGate - Defensive gate placement
/// Attach to gate prefab
/// </summary>
public class ToriiGate : MonoBehaviour
{
    [Header("Gate Settings")]
    [SerializeField] private float duration = 8f;
    [SerializeField] private float slowRadius = 3f;
    [SerializeField] private float slowFactor = 0.5f;
    [SerializeField] private int maxHits = 3;
    
    // State
    private float timer;
    private int currentHits;
    private PlayerController owner;
    private Collider2D gateCollider;
    
    // Visual
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        gateCollider = GetComponent<Collider2D>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
        
        timer = duration;
    }
    
    public void Initialize(PlayerController player)
    {
        owner = player;
    }
    
    void Update()
    {
        timer -= Time.deltaTime;
        
        if (spriteRenderer != null)
        {
            float alpha = timer / duration;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }
        
        if (timer <= 0)
        {
            DestroyGate();
        }
        
        SlowEnemiesInRadius();
    }
    
    void SlowEnemiesInRadius()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, slowRadius);
        foreach (var col in enemies)
        {
            DriftEnemy enemy = col.GetComponent<DriftEnemy>();
            if (enemy != null)
            {
                enemy.ApplySlow(slowFactor);
            }
        }
    }
    
    public void TakeHit(float damage)
    {
        currentHits++;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
            Invoke(nameof(RestoreColor), 0.1f);
        }
        
        if (currentHits >= maxHits)
        {
            DestroyGate();
        }
    }
    
    void RestoreColor()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }
    
    void DestroyGate()
    {
        owner?.OnGateDestroyed();
        Destroy(gameObject);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, slowRadius);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        DriftEnemy enemy = other.GetComponent<DriftEnemy>();
        if (enemy != null)
        {
            enemy.OnBlockedByGate(this);
        }
    }
}
