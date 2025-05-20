# Echo Nomads - Upute za pokretanje projekta

Dobrodošli u Echo Nomads! Slijedite ove korake kako biste uspješno postavili i pokrenuli projekt na svom računalu.

## Predinstalacije

Prije nego što započnete, provjerite imate li instalirane sljedeće alate:

1.  **Git:** Alat za kontrolu verzija.
    *   Preuzmite i instalirajte s: [https://gitforwindows.org/](https://gitforwindows.org/)
2.  **Git LFS (Large File Storage):** Ekstenzija za Git za upravljanje velikim datotekama, što je često potrebno u Unity projektima.
    *   Preuzmite i instalirajte s: [https://git-lfs.com/](https://git-lfs.com/)
    *   Nakon instalacije, otvorite terminal (Git Bash, Command Prompt ili PowerShell) i pokrenite `git lfs install` kako biste inicijalizirali Git LFS na svom sustavu.
3.  **Unity Hub:** Alat za upravljanje Unity instalacijama i projektima.
    *   Preuzmite i instalirajte s: [https://unity.com/download](https://unity.com/download)
4.  **Unity Editor (Verzija 6000.0.40f1 LTS):** Specifična verzija Unity editora potrebna za ovaj projekt.
    *   Instalirajte ovu verziju putem Unity Huba. U Unity Hubu idite na karticu "Installs", kliknite "Install Editor" i odaberite verziju `6000.0.40f1 (LTS)`. Ako je ne vidite odmah, možda ćete morati potražiti u arhivi verzija unutar Unity Huba.

## Postavljanje projekta

1.  **Kloniranje projekta:**
    *   Otvorite terminal (Git Bash, Command Prompt, PowerShell).
    *   Navigirajte do direktorija gdje želite pohraniti projekt (npr., `cd C:\Projects`).
    *   Izvršite sljedeću naredbu za kloniranje repozitorija:
        ```bash
        git clone https://github.com/AlphaActual/Echo-Nomads.git
        ```
    *   Nakon kloniranja, navigirajte u novostvoreni direktorij projekta:
        ```bash
        cd Echo-Nomads
        ```
    *   Ako Git LFS nije automatski povukao velike datoteke tijekom kloniranja, pokrenite:
        ```bash
        git lfs pull
        ```

2.  **Import projekta u Unity Hub:**
    *   Otvorite Unity Hub.
    *   Idite na karticu "Projects".
    *   Kliknite na dropdown gumb "Add" te zatim "Add project from disk".
    *   Navigirajte do lokacije gdje ste klonirali repozitorij (npr., `C:\Projects\Echo-Nomads`) i odaberite mapu projekta.
    *   Unity Hub će dodati projekt na popis i automatski odabrati odgovarajuću verziju Unity Editora ako je instalirana. Ako nije, zatražit će vas da je instalirate.

## Pokretanje projekta

1.  Nakon što je projekt uspješno importiran u Unity Hub:
    *   Na kartici "Projects" u Unity Hubu, pronađite projekt "Echo Nomads".
    *   Kliknite na naziv projekta kako biste ga otvorili u Unity Editoru.
    *   Prvo otvaranje projekta može potrajati neko vrijeme jer Unity mora importirati sve assete.

Sada biste trebali imati projekt otvoren i spreman za rad!
