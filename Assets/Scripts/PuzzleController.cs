using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro; // Ova linija mora biti tu

public class PuzzleController : MonoBehaviour
{
    [Header("UI Elementi")]
    public GameObject puzzlePanel;
    public GameObject porukaUspjeh;
    public TextMeshProUGUI instructionText; // Polje za vaš tekst 'igraupute'

    [Header("Gumbi - Poredaj ih ovdje redom od 0 do 4")]
    public List<GameObject> gumbi;

    private readonly List<int> tocanRedoslijed = new List<int> { 0, 1, 2, 3, 4 };
    private List<int> unosIgraca = new List<int>();

    void Start()
    {
        PromijesajGumbe();
        // Na početku, ugasimo sve
        puzzlePanel.SetActive(false);
        porukaUspjeh.SetActive(false);
        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && (puzzlePanel.activeInHierarchy || porukaUspjeh.activeInHierarchy))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                ZatvoriSve();
            }
        }

        // Proverava tipku 'X' za zatvaranje
        if (puzzlePanel.activeInHierarchy && Input.GetKeyDown(KeyCode.X))
        {
            ZatvoriSve();
        }
    }
    
    private void OnMouseDown()
    {
        if (puzzlePanel.activeInHierarchy)
        {
            ZatvoriSve();
        }
        else
        {
            if (!porukaUspjeh.activeInHierarchy)
            {
                OtvoriPuzzle();
            }
        }
    }

    public void GumbPritisnut(int indexGumba)
    {
        unosIgraca.Add(indexGumba);

        if (unosIgraca[unosIgraca.Count - 1] == tocanRedoslijed[unosIgraca.Count - 1])
        {
            if (unosIgraca.Count == tocanRedoslijed.Count)
            {
                RijesenPuzzle();
            }
        }
        else
        {
            Debug.Log("Pogrešan redoslijed! Resetiram.");
            unosIgraca.Clear();
        }
    }

    // --- KLJUČNA IZMENA JE OVDE ---
    public void ZatvoriSve()
    {
        puzzlePanel.SetActive(false);
        porukaUspjeh.SetActive(false);
        // Kada se sve zatvara, gasimo i tekst sa uputstvima
        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(false);
        }
        unosIgraca.Clear();
    }

    // --- I OVDE ---
    private void OtvoriPuzzle()
    {
        puzzlePanel.SetActive(true);
        porukaUspjeh.SetActive(false);
        
        if (instructionText != null) 
        {
            // Prvo upalimo tekstualni objekat
            instructionText.gameObject.SetActive(true);
            // Onda mu postavimo tekst
            instructionText.text = "Match the resonator's color sequence.\n\nPress 'X' to close.";
        }
        
        unosIgraca.Clear();
    }

    private void RijesenPuzzle()
    {
        puzzlePanel.SetActive(false);
        unosIgraca.Clear();
        // Kada se reši, gasimo i tekst sa uputstvima
        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(false);
        }
        StartCoroutine(PrikaziPorukuNaVrijeme());
    }

    private IEnumerator PrikaziPorukuNaVrijeme()
    {
        porukaUspjeh.SetActive(true);
        yield return new WaitForSeconds(4f);
        if (porukaUspjeh.activeInHierarchy)
        {
            porukaUspjeh.SetActive(false);
        }
    }

    private void PromijesajGumbe()
    {
        for (int i = 0; i < gumbi.Count; i++)
        {
            GameObject temp = gumbi[i];
            int randomIndex = Random.Range(i, gumbi.Count);
            gumbi[i] = gumbi[randomIndex];
            gumbi[randomIndex] = temp;
        }

        foreach (var gumb in gumbi)
        {
            gumb.transform.SetAsLastSibling();
        }
    }
}