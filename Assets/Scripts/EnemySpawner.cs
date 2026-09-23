using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// EnemySpawner - Manages drift enemy spawning
/// Attach to empty GameObject
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject swarmerPrefab;
    [SerializeField] private GameObject breakerPrefab;
    [SerializeField] private GameObject phantomPrefab;
    
    [Header("Spawn Settings")]
    [SerializeField] private float baseSpawnRate = 3f;
    [SerializeField] private float spawnRateIncrease = 0.1f;
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform[] anchorTransforms;
    
    // State
    private float spawnTimer;
    private float currentSpawnRate;
    private int waveNumber = 1;
    private int anchorsCompleted = 0;
    private List<DriftEnemy> activeEnemies = new List<DriftEnemy>();
    
    void Start()
    {
        if (playerTransform == null)
            playerTransform = FindObjectOfType<PlayerController>()?.transform;
        
        currentSpawnRate = baseSpawnRate;
        spawnTimer = currentSpawnRate;
    }
    
    void Update()
    {
        spawnTimer -= Time.deltaTime;
        
        if (spawnTimer <= 0)
        {
            SpawnWave();
            spawnTimer = currentSpawnRate;
        }
    }
    
    void SpawnWave()
    {
        int enemyCount = Mathf.Min(1 + waveNumber / 3, 5);
        
        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy();
        }
        
        waveNumber++;
        currentSpawnRate = Mathf.Max(0.5f, baseSpawnRate - (waveNumber * spawnRateIncrease));
    }
    
    void SpawnEnemy()
    {
        GameObject prefab = swarmerPrefab;
        float rand = Random.value;
        
        if (waveNumber > 3 && rand < 0.3f)
            prefab = breakerPrefab;
        else if (waveNumber > 5 && rand < 0.2f)
            prefab = phantomPrefab;
        
        Vector2 spawnPos = GetSpawnPosition();
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        
        DriftEnemy driftEnemy = enemy.GetComponent<DriftEnemy>();
        if (driftEnemy != null)
        {
            driftEnemy.Initialize(playerTransform, GetTargetAnchor());
            activeEnemies.Add(driftEnemy);
            driftEnemy.OnDestroyed += () => activeEnemies.Remove(driftEnemy);
        }
    }
    
    Vector2 GetSpawnPosition()
    {
        float angle = Random.Range(0f, Mathf.PI * 2);
        return (Vector2)playerTransform.position + new Vector2(
            Mathf.Cos(angle) * spawnRadius,
            Mathf.Sin(angle) * spawnRadius
        );
    }
    
    Transform GetTargetAnchor()
    {
        if (anchorTransforms != null && anchorTransforms.Length > 0)
        {
            List<Transform> incomplete = new List<Transform>();
            foreach (var anchor in anchorTransforms)
            {
                Anchor a = anchor.GetComponent<Anchor>();
                if (a != null && !a.IsCompleted)
                    incomplete.Add(anchor);
            }
            
            if (incomplete.Count > 0)
                return incomplete[Random.Range(0, incomplete.Count)];
        }
        
        return playerTransform;
    }
    
    public void OnAnchorCompleted()
    {
        anchorsCompleted++;
        currentSpawnRate = Mathf.Max(0.5f, currentSpawnRate - 0.2f);
    }
    
    public void ClearAllEnemies()
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }
        activeEnemies.Clear();
    }
    
    public List<DriftEnemy> ActiveEnemies => activeEnemies;
}

/// <summary>
/// Base class for all drift enemies
/// </summary>
public class DriftEnemy : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float damage = 15f;
    [SerializeField] protected float attackRange = 0.5f;
    
    protected Transform target;
    protected Transform anchorTarget;
    protected float currentSpeed;
    protected bool isPhased = false;
    protected bool isBlocked = false;
    
    public System.Action OnDestroyed;
    
    protected virtual void Start()
    {
        currentSpeed = moveSpeed;
    }
    
    public virtual void Initialize(Transform player, Transform anchor)
    {
        target = player;
        anchorTarget = anchor;
    }
    
    protected virtual void Update()
    {
        if (isPhased) return;
        
        MoveTowardTarget();
        CheckAttack();
    }
    
    protected virtual void MoveTowardTarget()
    {
        Transform moveTarget = anchorTarget != null ? anchorTarget : target;
        if (moveTarget == null) return;
        
        Vector2 direction = (moveTarget.position - transform.position).normalized;
        transform.position += (Vector3)(direction * currentSpeed * Time.deltaTime);
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    protected virtual void CheckAttack()
    {
        Transform attackTarget = anchorTarget != null ? anchorTarget : target;
        if (attackTarget == null) return;
        
        float distance = Vector2.Distance(transform.position, attackTarget.position);
        if (distance <= attackRange)
        {
            Attack(attackTarget);
        }
    }
    
    protected virtual void Attack(Transform attackTarget)
    {
        PlayerController player = attackTarget.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damage);
        }
        
        Anchor anchor = attackTarget.GetComponent<Anchor>();
        if (anchor != null)
        {
            anchor.ResetAnchor();
        }
        
        Destroy(gameObject);
    }
    
    public void ApplySlow(float factor)
    {
        currentSpeed = moveSpeed * factor;
    }
    
    public void OnBlockedByGate(ToriiGate gate)
    {
        isBlocked = true;
        currentSpeed = 0;
        
        gate.TakeHit(damage);
        
        Invoke(nameof(ResumeMovement), 0.5f);
    }
    
    void ResumeMovement()
    {
        isBlocked = false;
        currentSpeed = moveSpeed;
    }
    
    protected virtual void OnDestroy()
    {
        OnDestroyed?.Invoke();
    }
}
