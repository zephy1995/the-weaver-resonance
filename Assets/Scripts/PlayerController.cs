using UnityEngine;

/// <summary>
/// PlayerController - Handles movement, anchor activation, and gate placement
/// Attach to the Player GameObject
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float stopDistance = 0.1f;
    
    [Header("Anchor Activation")]
    [SerializeField] private float activationTime = 2f;
    [SerializeField] private float activationDrainRate = 3f;
    
    [Header("Gate Placement")]
    [SerializeField] private GameObject gatePrefab;
    [SerializeField] private int maxGates = 2;
    [SerializeField] private float gateCooldown = 8f;
    
    // State
    private Vector2 targetPosition;
    private bool isMoving = false;
    private bool isActivating = false;
    private float activationProgress = 0f;
    private Anchor currentAnchor;
    private int activeGates = 0;
    private float gateCooldownTimer = 0f;
    private MGSManager mgsManager;
    
    // Events
    public System.Action<float> OnActivationProgress;
    public System.Action OnActivationComplete;
    public System.Action OnActivationInterrupted;
    
    void Start()
    {
        mgsManager = FindObjectOfType<MGSManager>();
        targetPosition = transform.position;
    }
    
    void Update()
    {
        HandleInput();
        HandleMovement();
        HandleActivation();
        HandleCooldowns();
    }
    
    void HandleInput()
    {
        // Mobile: Drag to move / Desktop: Click to move
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Vector2 inputPos = GetInputPosition();
            
            // Check if clicking on anchor
            Collider2D hit = Physics2D.OverlapPoint(inputPos);
            if (hit != null && hit.GetComponent<Anchor>() != null)
            {
                return;
            }
            
            targetPosition = inputPos;
            isMoving = true;
            isActivating = false;
        }
        
        // Hold for anchor activation
        if (Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Stationary))
        {
            Vector2 inputPos = GetInputPosition();
            Collider2D hit = Physics2D.OverlapPoint(inputPos);
            
            if (hit != null && hit.GetComponent<Anchor>() != null)
            {
                Anchor anchor = hit.GetComponent<Anchor>();
                if (!anchor.IsActivated && !isActivating)
                {
                    StartActivation(anchor);
                }
            }
        }
        
        // Release stops activation
        if (Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
        {
            if (isActivating)
            {
                InterruptActivation();
            }
        }
        
        // Gate placement
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Space))
        {
            TryPlaceGate();
        }
    }
    
    Vector2 GetInputPosition()
    {
        if (Input.touchCount > 0)
            return Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
    
    void HandleMovement()
    {
        if (!isMoving) return;
        
        Vector2 currentPos = transform.position;
        float distance = Vector2.Distance(currentPos, targetPosition);
        
        if (distance <= stopDistance)
        {
            isMoving = false;
            return;
        }
        
        Vector2 direction = (targetPosition - currentPos).normalized;
        transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
    }
    
    void StartActivation(Anchor anchor)
    {
        isActivating = true;
        currentAnchor = anchor;
        activationProgress = 0f;
    }
    
    void HandleActivation()
    {
        if (!isActivating) return;
        
        float distanceToAnchor = Vector2.Distance(transform.position, currentAnchor.transform.position);
        if (distanceToAnchor > currentAnchor.ActivationRadius)
        {
            InterruptActivation();
            return;
        }
        
        activationProgress += Time.deltaTime;
        float progress = activationProgress / activationTime;
        OnActivationProgress?.Invoke(progress);
        
        mgsManager?.DrainMGS(activationDrainRate * Time.deltaTime);
        
        if (activationProgress >= activationTime)
        {
            CompleteActivation();
        }
    }
    
    void CompleteActivation()
    {
        isActivating = false;
        currentAnchor.Activate();
        OnActivationComplete?.Invoke();
        currentAnchor = null;
    }
    
    void InterruptActivation()
    {
        isActivating = false;
        activationProgress = 0f;
        OnActivationProgress?.Invoke(0f);
        OnActivationInterrupted?.Invoke();
        currentAnchor = null;
    }
    
    void TryPlaceGate()
    {
        if (activeGates >= maxGates || gateCooldownTimer > 0) return;
        
        Vector2 placePos = transform.position;
        GameObject gate = Instantiate(gatePrefab, placePos, Quaternion.identity);
        gate.GetComponent<ToriiGate>().Initialize(this);
        
        activeGates++;
        gateCooldownTimer = gateCooldown;
    }
    
    public void OnGateDestroyed()
    {
        activeGates = Mathf.Max(0, activeGates - 1);
    }
    
    void HandleCooldowns()
    {
        if (gateCooldownTimer > 0)
            gateCooldownTimer -= Time.deltaTime;
    }
    
    public void TakeDamage(float damage)
    {
        mgsManager?.DrainMGS(damage);
        
        if (isActivating)
        {
            InterruptActivation();
        }
    }
    
    public bool IsActivating => isActivating;
    public float GateCooldownProgress => 1f - (gateCooldownTimer / gateCooldown);
}
