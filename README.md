# DiagLauncher

Lanceur d'applications de **diagnostic et reprogrammation automobile** pour Windows.  
Interface plein écran style tablette de diagnostic, thème sombre/clair, reconnaissance automatique des logiciels.

---

## Fonctionnalités

- **Plein écran au démarrage** — interface type kiosque, pensée pour rester ouverte en atelier
- **Grille de tuiles** avec icône extraite de l'exécutable, couleur personnalisable et badge catégorie
- **4 catégories** — Diagnostic, Cartographie, Reprogrammation, Outils
- **Reconnaissance automatique** de ~90 logiciels auto au moment de l'ajout (VCDS, KSuite, WinOLS, ISTA, XENTRY, DiagBox…)
- **Thème sombre / clair / système** — suit automatiquement le thème Windows si "Système" est sélectionné
- **Lancement au démarrage Windows** — on/off dans les paramètres
- **Apps portables bundlées** — les exécutables déposés dans `assets/` sont enregistrés automatiquement au premier lancement
- **Config persistée** en JSON dans `%AppData%\DiagLauncher\`
- **Clic droit** sur une tuile → Modifier / Supprimer

---

## Logiciels reconnus automatiquement

| Catégorie | Logiciels |
|-----------|-----------|
| **Cartographie** | KSuite, KESS3, K-TAG, WinOLS, ECM Titanium, PCMFlash, MPPS, BitEdit, Swiftec, CMDFlash, MMFlasher, AutoTuner, FoxFlash, HP Tuners, EFILive, TunerPro, Magic Motorsport Flex, Dimsport New Genius… |
| **Diagnostic** | VCDS, ODIS, ISTA/Rheingold, INPA, NCS Expert, E-Sys, XENTRY/DAS, Renault CLIP, DDT2000, DiagBox, PP2000, IDS Ford, FORScan, GDS2, SDD JLR, Techstream Toyota, HDS Honda, PIWIS Porsche, MultiEcuScan, Launch X431, Autel MaxiSys… |
| **Reprogrammation** | VVDI Xhorse, Yanhua ACDP, CarProg, DigiProg, OBDSTAR, Tango Key Programmer… |
| **Outils** | TeamViewer, AnyDesk, HxD, 010 Editor, Zadig, J2534… |

---

## Installation

### Méthode 1 — Archive portable (recommandée)

1. Télécharger `DiagLauncher_v1.0.0_win-x64.zip` depuis les [Releases](../../releases)
2. Extraire dans le dossier de votre choix (ex. `C:\DiagLauncher\`)
3. Lancer `DiagLauncher.exe`

> **Aucun prérequis** — le runtime .NET est inclus dans l'archive.

### Méthode 2 — Compiler depuis les sources

**Prérequis :** [.NET SDK 3.1+](https://dotnet.microsoft.com/download)

```bash
git clone https://github.com/lamziiii/DiagLauncher.git
cd DiagLauncher
dotnet run --project DiagLauncher/DiagLauncher.csproj
```

### Méthode 3 — Générer le setup d'installation

**Prérequis supplémentaire :** [Inno Setup 6](https://jrsoftware.org/isdownload.php)

```bash
build.bat
# → génère installer/output/DiagLauncher_Setup.exe
```

---

## Ajouter des applications

### Manuellement
Cliquer sur **+ Ajouter** dans la barre du haut → parcourir l'exécutable.  
Le nom et la catégorie sont pré-remplis automatiquement si le logiciel est reconnu.

### Apps portables (bundlées)
Déposer l'exécutable dans le dossier `assets/` à côté de `DiagLauncher.exe`.  
Il sera enregistré automatiquement au prochain lancement.

---

## Raccourcis

| Action | Raccourci |
|--------|-----------|
| Fermer l'application | `Alt + F4` |
| Modifier une tuile | Clic droit → Modifier |
| Supprimer une tuile | Clic droit → Supprimer |

---

## Structure du projet

```
DiagLauncher/
├── assets/                        # Apps portables bundlées
├── installer/
│   └── DiagLauncher.iss           # Script Inno Setup
├── DiagLauncher/
│   ├── Models/                    # AppEntry, AppSettings
│   ├── ViewModels/                # MainViewModel (MVVM)
│   ├── Views/                     # Dialogs (Ajout, Paramètres)
│   ├── Services/
│   │   ├── AppRecognitionService  # Base ~90 logiciels auto
│   │   ├── BundledAppsService     # Scan dossier assets/
│   │   ├── ConfigService          # JSON %AppData%
│   │   ├── StartupService         # Registre Windows
│   │   └── ThemeService           # Thème système Windows
│   ├── Converters/                # HexToBrush, ExeToIcon…
│   └── Resources/
│       ├── DarkTheme.xaml
│       └── LightTheme.xaml
└── build.bat                      # Script publish + setup
```

---

## Licence

Projet personnel — usage libre.
