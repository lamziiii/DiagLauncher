using System;
using System.Collections.Generic;
using System.IO;

namespace DiagLauncher.Services
{
    /// <summary>
    /// Détecte automatiquement la catégorie et propose un nom pour une application
    /// à partir de son nom d'exécutable ou de son chemin.
    /// </summary>
    public static class AppRecognitionService
    {
        // Catégories disponibles
        public const string CAT_DIAGNOSTIC    = "Diagnostic";
        public const string CAT_CARTO         = "Cartographie";
        public const string CAT_REPROG        = "Reprogrammation";
        public const string CAT_OUTILS        = "Outils";
        public const string CAT_AUTRE         = "Autre";

        public static readonly string[] AllCategories = new[]
        {
            CAT_DIAGNOSTIC, CAT_CARTO, CAT_REPROG, CAT_OUTILS, CAT_AUTRE
        };

        // ─── Base de données de reconnaissance ────────────────────────────────────
        // Clé : fragment (minuscules) présent dans le nom exe OU dans le chemin complet
        // Valeur : (NomConvivial, Catégorie)
        private static readonly List<(string keyword, string friendlyName, string category)> _db
            = new List<(string, string, string)>
        {
            // ══ CARTOGRAPHIE / MAP EDITORS ══════════════════════════════════════════

            // Alientech
            ("ksuite",          "KSuite",                       CAT_CARTO),
            ("k-suite",         "KSuite",                       CAT_CARTO),
            ("kess",            "KESS3",                        CAT_CARTO),
            ("ktag",            "K-TAG",                        CAT_CARTO),
            ("k-tag",           "K-TAG",                        CAT_CARTO),
            ("alientech",       "Alientech Suite",              CAT_CARTO),
            ("kessv2",          "KESS V2",                      CAT_CARTO),

            // WinOLS / EVC
            ("winols",          "WinOLS",                       CAT_CARTO),
            ("win_ols",         "WinOLS",                       CAT_CARTO),

            // ECM Titanium / Dimsport
            ("ecmtitanium",     "ECM Titanium",                 CAT_CARTO),
            ("ecm_titanium",    "ECM Titanium",                 CAT_CARTO),
            ("titanium",        "ECM Titanium",                 CAT_CARTO),
            ("dimsport",        "Dimsport",                     CAT_CARTO),
            ("new_genius",      "New Genius",                   CAT_CARTO),
            ("newgenius",       "New Genius",                   CAT_CARTO),
            ("new_trasdata",    "New Trasdata",                 CAT_CARTO),
            ("trasdata",        "Trasdata",                     CAT_CARTO),

            // Magic Motorsport
            ("flex",            "Magic Flex",                   CAT_CARTO),
            ("magic_motorsport","Magic Motorsport",             CAT_CARTO),
            ("magicmotorsport", "Magic Motorsport",             CAT_CARTO),
            ("flexkeysuite",    "FlexKeySuite",                 CAT_CARTO),

            // CMDFlash / MMFlasher
            ("cmdflash",        "CMDFlash",                     CAT_CARTO),
            ("cmd_flash",       "CMDFlash",                     CAT_CARTO),
            ("mmflasher",       "MMFlasher",                    CAT_CARTO),
            ("mm_flasher",      "MMFlasher",                    CAT_CARTO),

            // PCMFlash
            ("pcmflash",        "PCMFlash",                     CAT_CARTO),
            ("pcm_flash",       "PCMFlash",                     CAT_CARTO),

            // MPPS / Galletto
            ("mpps",            "MPPS",                         CAT_CARTO),
            ("galletto",        "Galletto",                     CAT_CARTO),

            // BitEdit / Swiftec
            ("bitedit",         "BitEdit",                      CAT_CARTO),
            ("bit_edit",        "BitEdit",                      CAT_CARTO),
            ("swiftec",         "Swiftec",                      CAT_CARTO),

            // Microtronik / Byteshooter
            ("hextag",          "HexTag",                       CAT_CARTO),
            ("byteshooter",     "ByteShooter",                  CAT_CARTO),
            ("microtronik",     "Microtronik",                  CAT_CARTO),

            // Autotuner
            ("autotuner",       "Autotuner",                    CAT_CARTO),

            // BDMprog
            ("bdmprog",         "BDMprog",                      CAT_CARTO),
            ("bdm100",          "BDM100",                       CAT_CARTO),

            // X17 / Foxflash
            ("foxflash",        "FoxFlash",                     CAT_CARTO),
            ("x17",             "X17",                          CAT_CARTO),

            // Powergate
            ("powergate",       "Powergate",                    CAT_CARTO),

            // EcuEdit / EcuExplorer
            ("ecuedit",         "EcuEdit",                      CAT_CARTO),
            ("ecuexplorer",     "EcuExplorer",                  CAT_CARTO),
            ("ecuflash",        "EcuFlash (EcuEdit)",           CAT_CARTO),

            // Openflash
            ("openflash",       "OpenFlash Tablet",             CAT_CARTO),

            // Tactrix / ECUExplorer
            ("tactrix",         "Tactrix OpenPort",             CAT_CARTO),

            // EFILive
            ("efilive",         "EFILive",                      CAT_CARTO),
            ("efi_live",        "EFILive",                      CAT_CARTO),

            // HPTuners
            ("hptuners",        "HP Tuners",                    CAT_CARTO),
            ("hp_tuner",        "HP Tuners",                    CAT_CARTO),

            // TunerPro
            ("tunerpro",        "TunerPro",                     CAT_CARTO),

            // ScanMatik / Chipsoft
            ("chipsoft",        "ChipSoft",                     CAT_CARTO),
            ("scanmatik",       "ScanMatik",                    CAT_CARTO),

            // AEM / Haltech / Link / MoTeC (tuning ECU)
            ("aem_tuner",       "AEM Tuner",                    CAT_CARTO),
            ("haltech",         "Haltech",                      CAT_CARTO),
            ("linkecu",         "Link ECU",                     CAT_CARTO),
            ("motec",           "MoTeC",                        CAT_CARTO),
            ("vems",            "VEMS",                         CAT_CARTO),


            // ══ DIAGNOSTIC ═══════════════════════════════════════════════════════════

            // VAG — VCDS / ODIS
            ("vcds",            "VCDS (VAG-COM)",               CAT_DIAGNOSTIC),
            ("vag-com",         "VCDS (VAG-COM)",               CAT_DIAGNOSTIC),
            ("vagcom",          "VCDS (VAG-COM)",               CAT_DIAGNOSTIC),
            ("ross-tech",       "VCDS (Ross-Tech)",             CAT_DIAGNOSTIC),
            ("rosstech",        "VCDS (Ross-Tech)",             CAT_DIAGNOSTIC),
            ("odis",            "ODIS Service",                 CAT_DIAGNOSTIC),
            ("vas6150",         "VAS 6150 (ODIS)",              CAT_DIAGNOSTIC),

            // BMW — ISTA / INPA / NCS / E-Sys / WinKFP
            ("ista",            "ISTA (BMW)",                   CAT_DIAGNOSTIC),
            ("rheingold",       "ISTA-D Rheingold",             CAT_DIAGNOSTIC),
            ("inpa",            "INPA (BMW)",                   CAT_DIAGNOSTIC),
            ("ncsexpert",       "NCS Expert (BMW)",             CAT_DIAGNOSTIC),
            ("ncs_expert",      "NCS Expert (BMW)",             CAT_DIAGNOSTIC),
            ("winkfp",          "WinKFP (BMW)",                 CAT_DIAGNOSTIC),
            ("esys",            "E-Sys (BMW)",                  CAT_DIAGNOSTIC),
            ("e-sys",           "E-Sys (BMW)",                  CAT_DIAGNOSTIC),
            ("tool32",          "Tool32 (BMW)",                 CAT_DIAGNOSTIC),
            ("wds",             "WDS (BMW)",                    CAT_DIAGNOSTIC),

            // Mercedes — XENTRY / DAS / WIS
            ("xentry",          "XENTRY (Mercedes)",            CAT_DIAGNOSTIC),
            ("das",             "DAS (Mercedes)",               CAT_DIAGNOSTIC),
            ("wis",             "WIS (Mercedes)",               CAT_DIAGNOSTIC),
            ("vediamo",         "Vediamo (Mercedes)",           CAT_DIAGNOSTIC),
            ("dts_monaco",      "DTS Monaco (Mercedes)",        CAT_DIAGNOSTIC),
            ("dtsmonaco",       "DTS Monaco (Mercedes)",        CAT_DIAGNOSTIC),

            // Renault — CLIP / DDT2000
            ("clip",            "Renault CLIP",                 CAT_DIAGNOSTIC),
            ("ddt2000",         "DDT2000 (Renault)",            CAT_DIAGNOSTIC),
            ("ddt4all",         "DDT4All",                      CAT_DIAGNOSTIC),

            // PSA — DiagBox / PP2000 / Lexia
            ("diagbox",         "DiagBox (PSA)",                CAT_DIAGNOSTIC),
            ("pp2000",          "PP2000 (Peugeot)",             CAT_DIAGNOSTIC),
            ("lexia",           "Lexia-3 (PSA)",                CAT_DIAGNOSTIC),

            // Ford — IDS / FJDS / ForScan
            ("ids",             "IDS (Ford)",                   CAT_DIAGNOSTIC),
            ("fjds",            "FJDS (Ford)",                  CAT_DIAGNOSTIC),
            ("forscan",         "FORScan (Ford)",               CAT_DIAGNOSTIC),

            // GM / Opel — GDS2 / Tech2 / MDI
            ("gds2",            "GDS2 (GM/Opel)",               CAT_DIAGNOSTIC),
            ("tech2win",        "Tech2Win (GM)",                CAT_DIAGNOSTIC),
            ("globaldiag",      "GlobalDiag (GM)",              CAT_DIAGNOSTIC),

            // Jaguar / Land Rover — SDD / JLR Pathfinder
            ("sdd",             "SDD (JLR)",                    CAT_DIAGNOSTIC),
            ("pathfinder",      "JLR Pathfinder",               CAT_DIAGNOSTIC),

            // Toyota / Lexus — Techstream
            ("techstream",      "Techstream (Toyota)",          CAT_DIAGNOSTIC),
            ("tis_techstream",  "Techstream (Toyota)",          CAT_DIAGNOSTIC),

            // Honda — HDS
            ("hds",             "HDS (Honda)",                  CAT_DIAGNOSTIC),
            ("honda_diag",      "Honda Diagnostic",             CAT_DIAGNOSTIC),

            // Mitsubishi — MUT-III
            ("mut3",            "MUT-III (Mitsubishi)",         CAT_DIAGNOSTIC),
            ("mut-iii",         "MUT-III (Mitsubishi)",         CAT_DIAGNOSTIC),
            ("mutiii",          "MUT-III (Mitsubishi)",         CAT_DIAGNOSTIC),

            // Porsche — PIWIS
            ("piwis",           "PIWIS (Porsche)",              CAT_DIAGNOSTIC),

            // Fiat / Alfa / Lancia — Examiner / Multiecuscan
            ("multiecuscan",    "MultiEcuScan (Fiat)",          CAT_DIAGNOSTIC),
            ("examiner",        "Examiner (Fiat)",              CAT_DIAGNOSTIC),
            ("fiat_ecuscan",    "FiatEcuScan",                  CAT_DIAGNOSTIC),
            ("alfaobd",         "AlfaOBD",                      CAT_DIAGNOSTIC),

            // Hyundai / Kia — GDS
            ("gds_mobile",      "GDS Mobile (Hyundai)",         CAT_DIAGNOSTIC),
            ("kia_gds",         "GDS (Kia)",                    CAT_DIAGNOSTIC),

            // Volvo — VIDA / DICE
            ("vida",            "VIDA (Volvo)",                 CAT_DIAGNOSTIC),
            ("dice",            "DICE (Volvo)",                 CAT_DIAGNOSTIC),
            ("devtool",         "DevTool (Volvo)",              CAT_DIAGNOSTIC),

            // Subaru — SSM4
            ("ssm4",            "SSM4 (Subaru)",                CAT_DIAGNOSTIC),
            ("select_monitor",  "Select Monitor (Subaru)",      CAT_DIAGNOSTIC),

            // Mazda — IDS
            ("mazda_ids",       "Mazda IDS",                    CAT_DIAGNOSTIC),

            // Nissan / Infiniti — Consult
            ("consult",         "Consult-III (Nissan)",         CAT_DIAGNOSTIC),
            ("nissan_consult",  "Consult (Nissan)",             CAT_DIAGNOSTIC),

            // Launch / Autel / iCarsoft (multimarques)
            ("launch_x431",     "Launch X431",                  CAT_DIAGNOSTIC),
            ("x431",            "Launch X431",                  CAT_DIAGNOSTIC),
            ("autel",           "Autel MaxiSys",                CAT_DIAGNOSTIC),
            ("maxisys",         "Autel MaxiSys",                CAT_DIAGNOSTIC),
            ("icarsoft",        "iCarsoft",                     CAT_DIAGNOSTIC),
            ("autocom",         "Autocom CDP",                  CAT_DIAGNOSTIC),
            ("delphi_ds",       "Delphi DS",                    CAT_DIAGNOSTIC),
            ("wurth_wow",       "Würth WoW!",                   CAT_DIAGNOSTIC),
            ("wowdiag",         "Würth WoW!",                   CAT_DIAGNOSTIC),
            ("serprog",         "Serprog",                      CAT_DIAGNOSTIC),
            ("bosch_esi",       "Bosch ESI[tronic]",            CAT_DIAGNOSTIC),
            ("esi_tronic",      "Bosch ESI[tronic]",            CAT_DIAGNOSTIC),
            ("esitronic",       "Bosch ESI[tronic]",            CAT_DIAGNOSTIC),

            // Generic OBD
            ("obdauto",         "OBD Auto",                     CAT_DIAGNOSTIC),
            ("obdwiz",          "OBDwiz",                       CAT_DIAGNOSTIC),
            ("scantool",        "ScanTool",                     CAT_DIAGNOSTIC),
            ("elm327",          "ELM327",                       CAT_DIAGNOSTIC),
            ("torque",          "Torque Pro",                   CAT_DIAGNOSTIC),


            // ══ REPROGRAMMATION ══════════════════════════════════════════════════════

            // JTAG / BDM / Boot
            ("bdm_programmer",  "BDM Programmer",               CAT_REPROG),
            ("jtag",            "JTAG Programmer",              CAT_REPROG),
            ("tricore",         "TriCore Programmer",           CAT_REPROG),
            ("openport",        "OpenPort 2.0",                 CAT_REPROG),

            // Airbag / BSI / BCM
            ("airbagpro",       "Airbag Pro",                   CAT_REPROG),
            ("carprog",         "CarProg",                      CAT_REPROG),
            ("r270",            "R270 Programmer",              CAT_REPROG),
            ("immo_killer",     "Immo Killer",                  CAT_REPROG),

            // ODO / Cluster
            ("digiprog",        "DigiProg",                     CAT_REPROG),
            ("obdstar",         "OBDSTAR",                      CAT_REPROG),
            ("yanhua",          "Yanhua ACDP",                  CAT_REPROG),
            ("acdp",            "Yanhua ACDP",                  CAT_REPROG),

            // Key / IMMO
            ("keytool",         "Key Tool",                     CAT_REPROG),
            ("vvdi",            "VVDI (Xhorse)",                CAT_REPROG),
            ("xhorse",          "VVDI (Xhorse)",                CAT_REPROG),
            ("autel_im",        "Autel IM608",                  CAT_REPROG),
            ("tango",           "Tango Key Programmer",         CAT_REPROG),
            ("tmps",            "TPMS Tool",                    CAT_REPROG),


            // ══ OUTILS ═══════════════════════════════════════════════════════════════

            // Accès distant
            ("teamviewer",      "TeamViewer",                   CAT_OUTILS),
            ("anydesk",         "AnyDesk",                      CAT_OUTILS),
            ("rustdesk",        "RustDesk",                     CAT_OUTILS),
            ("vnc",             "VNC Viewer",                   CAT_OUTILS),
            ("rdp",             "Bureau à distance",            CAT_OUTILS),

            // Drivers / J2534
            ("j2534",           "Interface J2534",              CAT_OUTILS),
            ("passthru",        "PassThru J2534",               CAT_OUTILS),
            ("drewtech",        "Drew Technologies",            CAT_OUTILS),
            ("mongoose",        "Mongoose Pro",                 CAT_OUTILS),
            ("openports",       "OpenPort",                     CAT_OUTILS),

            // Hex editors / analyse
            ("hxd",             "HxD (Éditeur Hex)",            CAT_OUTILS),
            ("hexeditor",       "Hex Editor",                   CAT_OUTILS),
            ("hex_editor",      "Hex Editor",                   CAT_OUTILS),
            ("010editor",       "010 Editor",                   CAT_OUTILS),

            // Utilitaires
            ("notepad",         "Notepad",                      CAT_OUTILS),
            ("notepadpp",       "Notepad++",                    CAT_OUTILS),
            ("7zip",            "7-Zip",                        CAT_OUTILS),
            ("winrar",          "WinRAR",                       CAT_OUTILS),
            ("filezilla",       "FileZilla",                    CAT_OUTILS),
            ("putty",           "PuTTY",                        CAT_OUTILS),
            ("zadig",           "Zadig (drivers USB)",          CAT_OUTILS),
        };

        // ─── API publique ──────────────────────────────────────────────────────────

        public class RecognitionResult
        {
            public string SuggestedName     { get; set; }
            public string SuggestedCategory { get; set; }
            public bool   Recognized        { get; set; }
        }

        /// <summary>
        /// Analyse le chemin complet de l'exécutable et retourne nom + catégorie suggérés.
        /// </summary>
        public static RecognitionResult Recognize(string executablePath)
        {
            if (string.IsNullOrWhiteSpace(executablePath))
                return new RecognitionResult { Recognized = false };

            // On cherche dans : nom du fichier + dossier parent + chemin complet
            var fileName  = Path.GetFileNameWithoutExtension(executablePath).ToLowerInvariant();
            var directory = Path.GetDirectoryName(executablePath)?.ToLowerInvariant() ?? "";
            var combined  = $"{fileName} {directory}";

            foreach (var (keyword, friendlyName, category) in _db)
            {
                if (combined.Contains(keyword))
                {
                    return new RecognitionResult
                    {
                        SuggestedName     = friendlyName,
                        SuggestedCategory = category,
                        Recognized        = true
                    };
                }
            }

            // Non reconnu — on retourne juste un nom propre basé sur le fichier
            return new RecognitionResult
            {
                SuggestedName     = ToTitleCase(fileName),
                SuggestedCategory = CAT_AUTRE,
                Recognized        = false
            };
        }

        private static string ToTitleCase(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            // Remplace _ et - par espace, puis met chaque mot en majuscule
            var words = s.Replace('_', ' ').Replace('-', ' ').Split(' ');
            var result = new System.Text.StringBuilder();
            foreach (var w in words)
            {
                if (w.Length == 0) continue;
                if (result.Length > 0) result.Append(' ');
                result.Append(char.ToUpper(w[0]));
                if (w.Length > 1) result.Append(w.Substring(1));
            }
            return result.ToString();
        }
    }
}
