using UnityEngine;
using TMPro;
using System.Collections;

public class TentInteraction : MonoBehaviour
{
    public GameObject hintPanel; 
    public TextMeshProUGUI hintText;
    
    // Ovde smo formatirali tekst koristeći '\n' za novi red
    public string missionText = "To move around, use the following keys:\n\n" + 
                               "W - Move Forward\n" +
                               "A - Move Left\n" +
                               "S - Move Backward\n" +
                               "D - Move Right\n\n" +
                               "MOUSE - Control the camera\n" +
                               "RIGHT CLICK - Listen to resonators\n" +
                               "LEFT CLICK - Attack";

    private void Start()
    {
        if (hintPanel != null)
        {
            hintPanel.SetActive(false); 
        }
    }

    private void OnMouseDown()
    {
        ShowHint();
    }

    private void ShowHint()
    {
        if (hintPanel.activeSelf)
        {
            return;
        }

        hintText.text = missionText;
        hintPanel.SetActive(true);

        // Tajmer od 4 sekunde ostaje isti
        StartCoroutine(HideHintAfterDelay(4f));
    }

    private IEnumerator HideHintAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        hintPanel.SetActive(false);
    }
}