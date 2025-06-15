using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PuzzleController : MonoBehaviour
{
    [Header("UI Elementi")]
    public GameObject puzzlePanel;
    public GameObject porukaUspjeh;

    [Header("Gumbi - Poredaj ih ovdje redom od 0 do 4")]
    public List<GameObject> gumbi;

    // Točan redoslijed rješenja.
    private readonly List<int> tocanRedoslijed = new List<int> { 0, 1, 2, 3, 4 };
    
    private List<int> unosIgraca = new List<int>();

    // --- Glavne Unity Funkcije ---

    void Start()
    {
        PromijesajGumbe();
        puzzlePanel.SetActive(false);
        porukaUspjeh.SetActive(false);
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
    }

    // --- Logika ---

    // NOVO: OnMouseDown sada radi kao prekidač
    private void OnMouseDown()
    {
        // Ako je puzzle panel aktivan...
        if (puzzlePanel.activeInHierarchy)
        {
            // ...zatvori ga.
            ZatvoriSve();
        }
        // Inače (ako nije aktivan)...
        else
        {
            // ...otvori ga (ali samo ako ni poruka nije aktivna).
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
    
    public void ZatvoriSve()
    {
        puzzlePanel.SetActive(false);
        porukaUspjeh.SetActive(false);
        unosIgraca.Clear();
    }

    // --- Pomoćne Funkcije ---

    private void OtvoriPuzzle()
    {
        puzzlePanel.SetActive(true);
        porukaUspjeh.SetActive(false);
        unosIgraca.Clear();
    }

    // NOVO: RijesenPuzzle sada pokreće korutinu
    private void RijesenPuzzle()
    {
        puzzlePanel.SetActive(false);
        unosIgraca.Clear();
        StartCoroutine(PrikaziPorukuNaVrijeme()); // Pokreni korutinu umjesto da direktno pališ poruku
    }

    // NOVO: Korutina koja prikazuje poruku na 4 sekunde
    private IEnumerator PrikaziPorukuNaVrijeme()
    {
        // 1. Pokaži poruku
        porukaUspjeh.SetActive(true);

        // 2. Čekaj 4 sekunde
        yield return new WaitForSeconds(4f);

        // 3. Sakrij poruku (samo ako je igrač u međuvremenu nije ručno zatvorio)
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