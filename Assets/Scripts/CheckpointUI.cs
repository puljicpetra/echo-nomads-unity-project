using UnityEngine;
using UnityEngine.UI;

public class CheckpointUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text checkpointNameText;
    [SerializeField] private Text distanceText;
    [SerializeField] private GameObject uiPanel;
    
    [Header("Settings")]
    [SerializeField] private bool showDistance = true;
    [SerializeField] private bool showOnlyWhenChanged = false;
    [SerializeField] private float displayDuration = 3f; // How long to show UI after change
    
    private Checkpoint currentCheckpoint;
    private GameObject playerObject;
    private float hideTimer;
    private bool isVisible = false;

    void Start()
    {
        // Find player
        playerObject = GameObject.FindGameObjectWithTag("Player");
        
        // Subscribe to checkpoint events
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.OnActiveCheckpointChanged += OnActiveCheckpointChanged;
        }
        
        // Initial UI state
        if (uiPanel != null)
        {
            uiPanel.SetActive(!showOnlyWhenChanged);
        }
    }

    void Update()
    {
        if (currentCheckpoint != null)
        {
            UpdateUI();
        }
        
        // Handle auto-hide timer
        if (showOnlyWhenChanged && isVisible)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0f)
            {
                HideUI();
            }
        }
    }

    void OnActiveCheckpointChanged(Checkpoint newCheckpoint)
    {
        currentCheckpoint = newCheckpoint;
        
        if (showOnlyWhenChanged)
        {
            ShowUI();
            hideTimer = displayDuration;
        }
        
        UpdateUI();
    }

    void UpdateUI()
    {
        if (currentCheckpoint == null) return;
        
        // Update checkpoint name
        if (checkpointNameText != null)
        {
            string displayName = string.IsNullOrEmpty(currentCheckpoint.CheckpointId) 
                ? "Unnamed Checkpoint" 
                : currentCheckpoint.CheckpointId;
            checkpointNameText.text = $"Active: {displayName}";
        }
        
        // Update distance
        if (distanceText != null && showDistance && playerObject != null)
        {
            float distance = Vector3.Distance(
                playerObject.transform.position, 
                currentCheckpoint.transform.position
            );
            distanceText.text = $"Distance: {distance:F1}m";
        }
    }

    void ShowUI()
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(true);
            isVisible = true;
        }
    }

    void HideUI()
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
            isVisible = false;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.OnActiveCheckpointChanged -= OnActiveCheckpointChanged;
        }
    }

    // Public methods for external control
    public void SetShowDistance(bool show)
    {
        showDistance = show;
        if (distanceText != null)
        {
            distanceText.gameObject.SetActive(show);
        }
    }

    public void SetShowOnlyWhenChanged(bool showOnly)
    {
        showOnlyWhenChanged = showOnly;
        if (!showOnly && uiPanel != null)
        {
            uiPanel.SetActive(true);
            isVisible = true;
        }
    }
}
