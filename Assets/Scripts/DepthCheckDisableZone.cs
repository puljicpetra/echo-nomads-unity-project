using UnityEngine;

[System.Serializable]
public class DepthCheckDisableZone : MonoBehaviour
{
    [Header("Zone Settings")]
    [SerializeField] private ZoneType zoneType = ZoneType.Box;
    [SerializeField] private Vector3 zoneSize = new Vector3(10f, 10f, 10f);
    [SerializeField] private float sphereRadius = 5f;
    [SerializeField] private bool autoRegister = true;
    
    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = Color.cyan;
    
    public enum ZoneType
    {
        Box,
        Sphere,
        Trigger // Uses collider triggers
    }
    
    private DepthChecker depthChecker;
    private bool isPlayerInZone = false;
    
    void Start()
    {
        if (autoRegister)
        {
            RegisterWithDepthChecker();
        }
        
        // Set up trigger if using trigger type
        if (zoneType == ZoneType.Trigger)
        {
            SetupTrigger();
        }
    }
    
    void RegisterWithDepthChecker()
    {
        depthChecker = FindObjectOfType<DepthChecker>();
        if (depthChecker != null)
        {
            depthChecker.RegisterDisableZone(this);
        }
        else
        {
            Debug.LogWarning($"DepthCheckDisableZone '{name}': No DepthChecker found in scene!");
        }
    }
    
    void SetupTrigger()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            // Add a box collider if none exists
            col = gameObject.AddComponent<BoxCollider>();
            ((BoxCollider)col).size = zoneSize;
        }
        
        col.isTrigger = true;
    }
    
    public bool IsPointInZone(Vector3 point)
    {
        switch (zoneType)
        {
            case ZoneType.Box:
                return IsPointInBox(point);
            case ZoneType.Sphere:
                return IsPointInSphere(point);
            case ZoneType.Trigger:
                return isPlayerInZone; // Managed by trigger events
            default:
                return false;
        }
    }
    
    bool IsPointInBox(Vector3 point)
    {
        Vector3 localPoint = transform.InverseTransformPoint(point);
        Vector3 halfSize = zoneSize * 0.5f;
        
        return Mathf.Abs(localPoint.x) <= halfSize.x &&
               Mathf.Abs(localPoint.y) <= halfSize.y &&
               Mathf.Abs(localPoint.z) <= halfSize.z;
    }
    
    bool IsPointInSphere(Vector3 point)
    {
        float distance = Vector3.Distance(transform.position, point);
        return distance <= sphereRadius;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (zoneType == ZoneType.Trigger && other.CompareTag("Player"))
        {
            isPlayerInZone = true;
            if (depthChecker != null)
            {
                depthChecker.OnPlayerEnterDisableZone(name);
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (zoneType == ZoneType.Trigger && other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            if (depthChecker != null)
            {
                depthChecker.OnPlayerExitDisableZone(name);
            }
        }
    }
    
    public void DrawGizmos()
    {
        if (!showGizmos) return;
        
        Gizmos.color = gizmoColor;
        
        switch (zoneType)
        {
            case ZoneType.Box:
            case ZoneType.Trigger:
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(Vector3.zero, zoneSize);
                break;
            case ZoneType.Sphere:
                Gizmos.DrawWireSphere(transform.position, sphereRadius);
                break;
        }
    }
    
    void OnDrawGizmos()
    {
        DrawGizmos();
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw solid version when selected
        if (!showGizmos) return;
        
        Color solidColor = gizmoColor;
        solidColor.a = 0.3f;
        Gizmos.color = solidColor;
        
        switch (zoneType)
        {
            case ZoneType.Box:
            case ZoneType.Trigger:
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(Vector3.zero, zoneSize);
                break;
            case ZoneType.Sphere:
                Gizmos.DrawSphere(transform.position, sphereRadius);
                break;
        }
    }
    
    void OnDestroy()
    {
        if (depthChecker != null)
        {
            depthChecker.UnregisterDisableZone(this);
        }
    }
    
    // Public methods for external control
    public void SetZoneSize(Vector3 newSize)
    {
        zoneSize = newSize;
        if (zoneType == ZoneType.Trigger)
        {
            BoxCollider boxCol = GetComponent<BoxCollider>();
            if (boxCol != null)
            {
                boxCol.size = zoneSize;
            }
        }
    }
    
    public void SetSphereRadius(float newRadius)
    {
        sphereRadius = newRadius;
        if (zoneType == ZoneType.Trigger)
        {
            SphereCollider sphereCol = GetComponent<SphereCollider>();
            if (sphereCol != null)
            {
                sphereCol.radius = sphereRadius;
            }
        }
    }
    
    public void EnableZone(bool enable)
    {
        enabled = enable;
        if (zoneType == ZoneType.Trigger)
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = enable;
            }
        }
    }
}
