using UnityEngine;
using TMPro;

public class TentInteraction : MonoBehaviour
{
    public GameObject hintPanel; 
    public TextMeshProUGUI hintText;
    public string missionText = "The sound holds the key to finding Hush. Seek out what amplifies the echoes.";

    private bool isPanelActive = false;

    private void Start()
    {
        hintPanel.SetActive(false); 
    }

    private void OnMouseDown()
    {
        isPanelActive = !isPanelActive;

        if (isPanelActive)
        {
            hintPanel.SetActive(true);
            hintText.text = missionText;
        }
        else
        {
            hintPanel.SetActive(false);
        }
    }
}
