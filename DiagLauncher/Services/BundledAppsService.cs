using DiagLauncher.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DiagLauncher.Services
{
    /// <summary>
    /// Enregistre automatiquement les exécutables du dossier assets\ (portable apps bundlées)
    /// dans la configuration si ils ne sont pas déjà présents.
    /// </summary>
    public static class BundledAppsService
    {
        private static string AssetsDir =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets");

        /// <summary>
        /// Scanne assets\ et ajoute les nouveaux exécutables dans la liste apps.
        /// Retourne true si au moins une app a été ajoutée (= config à sauvegarder).
        /// </summary>
        public static bool SyncBundledApps(AppSettings settings)
        {
            if (!Directory.Exists(AssetsDir))
                return false;

            var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { ".exe", ".bat", ".cmd" };

            var files = Directory.EnumerateFiles(AssetsDir, "*.*", SearchOption.AllDirectories)
                .Where(f => extensions.Contains(Path.GetExtension(f)))
                .ToList();

            bool changed = false;
            foreach (var file in files)
            {
                // Déjà enregistré ? (comparaison insensible à la casse)
                bool alreadyAdded = settings.Apps.Any(a =>
                    string.Equals(a.ExecutablePath, file, StringComparison.OrdinalIgnoreCase));

                if (alreadyAdded) continue;

                var recognition = AppRecognitionService.Recognize(file);
                var entry = new AppEntry
                {
                    Name           = recognition.SuggestedName,
                    ExecutablePath = file,
                    Arguments      = "",
                    Category       = recognition.SuggestedCategory,
                    ColorHex       = DefaultColorFor(recognition.SuggestedCategory),
                    Position       = settings.Apps.Count
                };

                settings.Apps.Add(entry);
                changed = true;
            }

            return changed;
        }

        private static string DefaultColorFor(string category)
        {
            if (category == AppRecognitionService.CAT_DIAGNOSTIC) return "#1E78C8";
            if (category == AppRecognitionService.CAT_CARTO)      return "#9C59D1";
            if (category == AppRecognitionService.CAT_REPROG)     return "#E05252";
            if (category == AppRecognitionService.CAT_OUTILS)     return "#607D8B";
            return "#F0A732";
        }
    }
}
