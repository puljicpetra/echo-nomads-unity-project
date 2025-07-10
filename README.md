[Hrvatski](#hrvatski) | [English](#english)

---

## Hrvatski
<a name="hrvatski"></a>

# 🔊 Echo Nomads

**Avanturistička puzzle igra u kojoj je zvuk vaše najjače oružje i sredstvo preživljavanja.**

Ovaj repozitorij je **fork** originalnog timskog projekta razvijenog za kolegij "Dizajn i programiranje računalnih igara". Njegova svrha je arhiviranje i detaljan prikaz mog individualnog doprinosa na razvoju igre.

### O Igri
Kao mladi pripadnik nomadskog plemena koje koristi rezonanciju za interakciju s okolinom, igrač istražuje misteriozni svijet u kojem je zvuk moć, a tišina najveća prijetnja. Cilj je otkriti tajnu izgubljenih zvučnih ključeva i vratiti balans u svijet koji polako tone u bešumnu propast. Igra se oslanja na atmosferu, istraživanje i rješavanje zagonetki pomoću zvuka kao glavne mehanike.

### Ključna Obilježja
- **Interakcija putem zvuka:** Koristite rezonantni štap za aktivaciju drevnih mehanizama i otkrivanje skrivenih puteva.
- **Mistična priča:** Otkrijte sudbinu Echo Nomada i tajnu "Bešumnih entiteta" (The Hush) koji proždiru zvuk.
- **Istraživanje svijeta:** Putujte kroz tri jedinstvena okruženja: od prostrane dnevne pustinje, preko misterioznih noćnih klanaca, do zagonetnog drevnog hrama.
- **Jedinstvena atmosfera:** Minimalističko sučelje (bez HUD-a) za potpuno uranjanje u svijet.

### 📜 Koncept i Dizajn Igre
Ključni dokumenti koji su definirali viziju i mehanike igre nalaze se u `docs` mapi:
- **[Game Design Document (GDD)](docs/Game_Design_Document.pdf)** - Detaljan opis koncepta, priče, mehanika i dizajna razina.
- **[Vizualni Koncept (Poster)](docs/Visual_Concept_Poster.pdf)** - Vizualni prikaz koncepta igre na jednoj stranici.

### 🎮 Gameplay Prikaz
📺 [YouTube Gameplay Video](https://youtu.be/RA-mUEzCPck?si=kk_472JH-pWirzME)

### Akademski Kontekst
- **Fakultet:** Fakultet informatike u Puli
- **Kolegij:** Dizajn i programiranje računalnih igara
- **Nositelj:** Izv. prof. dr. sc. Tihomir Orehovački
- **Asistent:** Robert Šajina, mag. Inf.

### Tim i Zaduženja
| Član tima        | Glavna Uloga                                       |
|------------------|----------------------------------------------------|
| Tin Pritišanac   | Glavni Gameplay Programer / Sistemski Dizajner       |
| Moira Čekada     | Dizajner Razina / Gameplay Implementator           |
| **Petra Puljić** | **Voditelj Tima / Dizajner Atmosfere & UI**          |

---
### Moj specifični doprinos (Petra Puljić)

Kao autorica originalnog koncepta igre koji je odabran za timsku produkciju, preuzela sam ulogu **voditelja tima**, uz primarna zaduženja dizajnera atmosfere i korisničkog sučelja. Moji glavni doprinosi uključuju:

#### 🎨 **Korisničko Sučelje i Narativni Okvir**
- **Glavni izbornik (Main Menu):** Dizajn i implementacija kompletnog glavnog izbornika.
- **Uvodna i odjavna cutscena:** Kreiranje i integracija animiranih sekvenci koje uvode igrača u priču i zaokružuju iskustvo na kraju.

#### 🏜️ **Dizajn Svijeta i Atmosfera (World Building)**
- **Gradnja Hrama (Scena 3):** Dizajn, modeliranje i postavljanje elemenata za finalnu scenu igre - Izgubljeni Hram.
- **Atmosfera i Nebo (Skybox):** Postavljanje i prilagodba Skyboxa u noćnoj sceni (Scena 2) i finalnoj sceni (Scena 3) kako bi se postigao željeni ugođaj.
- **Osvjetljenje i Magla (Scena 2):** Implementacija glavnog svjetla i atmosferske magle u noćnoj sceni za postizanje mističnog i napetog osjećaja.
- **Dodavanje detalja:** Postavljanje manjih objekata (kamenje, vegetacija) i svih potrebnih kolidera za ispravnu interakciju igrača sa svijetom.

#### 🎮 **Gameplay mehanike i Triggeri**
- **Račvanje puta (Scena 2):** Dizajn i implementacija sustava triggera koji vode igrača.
  - **"Dobar put":** Svjetleće gljive se aktiviraju i osvjetljavaju put.
  - **"Loš put":** Aktivira se zid koji blokira prolaz i pojavljuje se "The Hush" entitet kao prepreka.

### Korištene Tehnologije
- **Engine:** Unity (Verzija 6000.0.40f)
- **Skriptiranje:** C#

---
---

## English
<a name="english"></a>

# 🔊 Echo Nomads

**An adventure-puzzle game where sound is your strongest weapon and means of survival.**

This repository is a **fork** of the original team project developed for the "Design and Programming of Computer Games" course. Its purpose is to archive and provide a detailed showcase of my individual contributions to the game's development.

### About The Game
As a young member of a nomadic tribe that uses resonance to interact with the environment, the player explores a mysterious world where sound is power, and silence is the greatest threat. The goal is to uncover the secret of the lost sonic keys and restore balance to a world slowly descending into a soundless void. The game relies on atmosphere, exploration, and puzzle-solving using sound as the core mechanic.

### Key Features
- **Interaction via Sound:** Use a resonant staff to activate ancient mechanisms and reveal hidden paths.
- **Mystical Story:** Uncover the fate of the Echo Nomads and the secret of "The Hush," the sound-devouring entities.
- **World Exploration:** Journey through three unique environments: from a vast daytime desert, through mysterious night-time gorges, to an enigmatic ancient temple.
- **Unique Atmosphere:** A minimalist interface (no HUD) for complete immersion into the world.

### 📜 Game Concept & Design
Key documents defining the game's vision and mechanics can be found in the `docs` folder:
- **[Game Design Document (GDD)](docs/Game_Design_Document.pdf)** - A detailed description of the concept, story, mechanics, and level design.
- **[Visual Concept (Poster)](docs/Visual_Concept_Poster.pdf)** - A one-page visual poster of the game concept.

### 🎮 Gameplay Showcase
📺 [YouTube Gameplay Video](https://youtu.be/RA-mUEzCPck?si=kk_472JH-pWirzME)

### Academic Context
- **Faculty:** Faculty of Informatics in Pula
- **Course:** Design and Programming of Computer Games
- **Professor:** Assoc. Prof. Tihomir Orehovački, PhD
- **Assistant:** Robert Šajina, M.Sc. Inf.

### Team & Roles
| Team Member      | Primary Role                               |
|------------------|--------------------------------------------|
| Tin Pritišanac   | Lead Gameplay Programmer / Systems Designer|
| Moira Čekada     | Level Designer / Gameplay Implementer      |
| **Petra Puljić** | **Team Lead / Atmosphere & UI Designer**     |

---
### My Specific Contribution (Petra Puljić)

As the author of the original game concept, which was selected for team production, I took on the role of **Team Lead** in addition to my primary responsibilities as the Atmosphere and UI Designer. My main contributions include:

#### 🎨 **User Interface and Narrative Framework**
- **Main Menu:** Design and implementation of the complete main menu system.
- **Intro & Outro Cutscenes:** Creation and integration of the animated sequences that introduce the player to the story and conclude the experience.

#### 🏜️ **World Building & Atmosphere**
- **Temple Construction (Scene 3):** Designing, modeling, and placing all elements for the game's final scene - The Lost Temple.
- **Atmosphere & Skybox:** Setting up and customizing the Skybox for the night scene (Scene 2) and the final scene (Scene 3) to achieve the desired mood.
- **Lighting & Fog (Scene 2):** Implementing the main light and atmospheric fog in the night scene to create a mystical and tense feeling.
- **World Detailing:** Placing smaller objects (rocks, vegetation) and all necessary colliders for proper player interaction with the world.

#### 🎮 **Gameplay Mechanics & Triggers**
- **Path Branching (Scene 2):** Designing and implementing the trigger system that guides the player.
  - **"Good Path":** Glowing mushrooms activate and illuminate the correct path.
  - **"Bad Path":** A wall is triggered to block the way, and a "The Hush" entity appears as an obstacle.

### Technologies Used
- **Engine:** Unity (Version 6000.0.40f)
- **Scripting:** C#
