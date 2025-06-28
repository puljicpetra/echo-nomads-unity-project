using UnityEngine;
using TMPro;
using System.Collections; // <-- OBAVEZNO dodajte ovu liniju!

public class TekstRez : MonoBehaviour
{
    public GameObject hintPanel; 
    public TextMeshProUGUI hintText;
    public string missionText = "The sound holds the key to finding Hush. Seek out what amplifies the echoes.";

    // Nije nam više potrebna 'isPanelActive' promenljiva za ovu logiku

    private void Start()
    {
        // Dobra je praksa osigurati da je panel ugašen na početku
        if (hintPanel != null)
        {
            hintPanel.SetActive(false); 
        }
    }

    private void OnMouseDown()
    {
        // Pozivamo našu novu metodu koja će prikazati panel
        ShowHint();
    }

    // Ova metoda prikazuje hint i pokreće tajmer za gašenje
    private void ShowHint()
    {
        // Ako je panel već aktivan, ne radimo ništa (sprečava višestruke klikove)
        if (hintPanel.activeSelf)
        {
            return;
        }

        // Podesimo tekst i aktiviramo panel
        hintText.text = missionText;
        hintPanel.SetActive(true);

        // Pokrećemo korutinu koja će ugasiti panel nakon 5 sekundi
        StartCoroutine(HideHintAfterDelay(4f));
    }

    // Ovo je Korutina - metoda koja čeka određeno vreme
    private IEnumerator HideHintAfterDelay(float delay)
    {
        // Čekaj 'delay' sekundi
        yield return new WaitForSeconds(delay);

        // Nakon što je čekanje završeno, ugasi panel
        hintPanel.SetActive(false);
    }
}