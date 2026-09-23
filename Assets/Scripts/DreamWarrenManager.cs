using UnityEngine;
using System.Collections;

/// <summary>
/// DreamWarrenManager - Recovery phase after collapse
/// Attach to empty GameObject
/// </summary>
public class DreamWarrenManager : MonoBehaviour
{
    [Header("Dream Settings")]
    [SerializeField] private float minDuration = 20f;
    [SerializeField] private float maxDuration = 40f;
    [SerializeField] private float lumeBunnyMGSRestore = 5f;
    [SerializeField] private float guardianRegenRate = 2f;
    [SerializeField] private float guardianRadius = 3f;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject lumeBunnyPrefab;
    [SerializeField] private GameObject guardianPrefab;
    [SerializeField] private GameObject dreamEnvironmentPrefab;
    
    [Header("UI")]
    [SerializeField] private GameObject dreamUI;
    [SerializeField] private GameObject gameplayUI;
    
    // State
    private bool isInDream = false;
    private float dreamTimer;
    private float currentMGSRestore = 0f;
    private MGSManager mgsManager;
    private PlayerController player;
    private GameObject guardianInstance;
    private GameObject dreamEnvInstance;
    
    // Events
    public System.Action OnDreamEntered;
    public System.Action<float> OnDreamExited; // Returns restored MGS amount
    
    void Start()
    {
        mgsManager = FindObjectOfType<MGSManager>();
        player = FindObjectOfType<PlayerController>();
    }
    
    public void EnterDreamWarren()
    {
        if (isInDream) return;
        
        isInDream = true;
        dreamTimer = Random.Range(minDuration, maxDuration);
        currentMGSRestore = 0f;
        
        SetupDreamEnvironment();
        
        if (dreamUI != null) dreamUI.SetActive(true);
        if (gameplayUI != null) gameplayUI.SetActive(false);
        
        FindObjectOfType<EnemySpawner>()?.ClearAllEnemies();
        
        OnDreamEntered?.Invoke();
        
        StartCoroutine(DreamLoop());
    }
    
    void SetupDreamEnvironment()
    {
        if (dreamEnvironmentPrefab != null)
        {
            dreamEnvInstance = Instantiate(dreamEnvironmentPrefab);
        }
        
        if (guardianPrefab != null)
        {
            Vector3 guardianPos = player != null ? player.transform.position : Vector3.zero;
            guardianInstance = Instantiate(guardianPrefab, guardianPos, Quaternion.identity);
        }
        
        SpawnLumeBunnies();
    }
    
    void SpawnLumeBunnies()
    {
        int bunnyCount = Random.Range(5, 10);
        for (int i = 0; i < bunnyCount; i++)
        {
            Vector2 spawnPos = Random.insideUnitCircle * 8f;
            if (player != null)
                spawnPos += (Vector2)player.transform.position;
            
            GameObject bunny = Instantiate(lumeBunnyPrefab, spawnPos, Quaternion.identity);
            LumeBunny lb = bunny.GetComponent<LumeBunny>();
            if (lb != null)
            {
                lb.OnTapped += () => OnBunnyTapped(lb);
            }
        }
    }
    
    void OnBunnyTapped(LumeBunny bunny)
    {
        currentMGSRestore += lumeBunnyMGSRestore;
        bunny.Collect();
    }
    
    IEnumerator DreamLoop()
    {
        float elapsed = 0f;
        
        while (elapsed < dreamTimer)
        {
            elapsed += Time.deltaTime;
            
            if (guardianInstance != null && player != null)
            {
                float distToGuardian = Vector2.Distance(
                    player.transform.position,
                    guardianInstance.transform.position
                );
                
                if (distToGuardian <= guardianRadius)
                {
                    currentMGSRestore += guardianRegenRate * Time.deltaTime;
                }
            }
            
            currentMGSRestore = Mathf.Min(currentMGSRestore, 100f);
            
            yield return null;
        }
        
        ExitDreamWarren();
    }
    
    void ExitDreamWarren()
    {
        isInDream = false;
        
        float restoreAmount = Mathf.Max(70f, currentMGSRestore);
        mgsManager?.RecoverFromDream(restoreAmount);
        
        CleanupDreamEnvironment();
        
        if (dreamUI != null) dreamUI.SetActive(false);
        if (gameplayUI != null) gameplayUI.SetActive(true);
        
        FindObjectOfType<SectorManager>()?.ResetSector();
        
        OnDreamExited?.Invoke(restoreAmount);
    }
    
    void CleanupDreamEnvironment()
    {
        if (dreamEnvInstance != null)
            Destroy(dreamEnvInstance);
        
        if (guardianInstance != null)
            Destroy(guardianInstance);
        
        LumeBunny[] bunnies = FindObjectsOfType<LumeBunny>();
        foreach (var bunny in bunnies)
        {
            Destroy(bunny.gameObject);
        }
    }
    
    public bool IsInDream => isInDream;
    public float DreamTimeRemaining => isInDream ? dreamTimer : 0f;
}

/// <summary>
/// LumeBunny - Collectible in Dream Warren
/// Attach to bunny prefab
/// </summary>
public class LumeBunny : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private float glowIntensity = 2f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;
    
    private Vector3 startPos;
    private bool isCollected = false;
    private SpriteRenderer spriteRenderer;
    
    public System.Action OnTapped;
    
    void Start()
    {
        startPos = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            Material glowMat = new Material(Shader.Find("Sprites/Default"));
            glowMat.EnableKeyword("_EMISSION");
            glowMat.SetColor("_EmissionColor", Color.white * glowIntensity);
            spriteRenderer.material = glowMat;
        }
    }
    
    void Update()
    {
        if (isCollected) return;
        
        float yOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = startPos + new Vector3(0, yOffset, 0);
    }
    
    void OnMouseDown()
    {
        if (isCollected) return;
        OnTapped?.Invoke();
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;
        
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            OnTapped?.Invoke();
        }
    }
    
    public void Collect()
    {
        if (isCollected) return;
        isCollected = true;
        
        StartCoroutine(CollectAnimation());
    }
    
    IEnumerator CollectAnimation()
    {
        float timer = 0f;
        Vector3 startScale = transform.localScale;
        
        while (timer < 0.3f)
        {
            timer += Time.deltaTime;
            float t = timer / 0.3f;
            transform.localScale = startScale * (1f - t);
            yield return null;
        }
        
        Destroy(gameObject);
    }
}
