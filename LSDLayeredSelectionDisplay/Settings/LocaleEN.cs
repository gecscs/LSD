// <copyright file="LocaleEN.cs" company="0belix's Mods. MIT License">
// Copyright (c) 0belix's Mods. MIT License. All rights reserved.
// </copyright>

namespace LSD_Layered_Selection_Display.Settings
{
    using System.Collections.Generic;
    using Colossal;
    using Colossal.PSI.Common;
    using Game.Settings;

    /// <summary>
    /// Localization for <see cref="LSDLayeredSelectionDisplayMod"/> mod in English.
    /// </summary>
    public class LocaleEN : IDictionarySource
    {
        private readonly LSDLayeredSelectionDisplayModSettings m_Setting;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocaleEN"/> class.
        /// </summary>
        /// <param name="setting">Settings class.</param>
        public LocaleEN(LSDLayeredSelectionDisplayModSettings setting)
        {
            m_Setting = setting;
        }

        /// <summary>
        /// Returns the locale key for a warning tooltip.
        /// </summary>
        /// <param name="key">The bracketed portion of locale key.</param>
        /// <returns>Localization key for translations.</returns>
        public static string WarningTooltipKey(string key)
        {
            return $"LSDLayeredSelectionDisplay.WARNING_TOOLTIP[{key}]";
        }

        /// <summary>
        /// Returns the locale key for a tooltip title key.
        /// </summary>
        /// <param name="key">The bracketed portion of locale key.</param>
        /// <returns>Localization key for translations.</returns>
        public static string TooltipTitleKey(string key)
        {
            return $"LSDLayeredSelectionDisplay.TOOLTIP_TITLE[{key}]";
        }

        /// <inheritdoc/>
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "Layered Selection Display" },
                { m_Setting.GetOptionLabelLocaleID(nameof(LSDLayeredSelectionDisplayModSettings.Version)), "Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(LSDLayeredSelectionDisplayModSettings.Version)), $"Version number for the Layered Selection Display mod installed." },
                { "YY_LSD_LAYERED_SELECTION_DISPLAY.Filter", "Filter" },
                { TooltipTitleKey("AllFilters"), "Toggle all Filters on/off" },
                { TooltipDescriptionKey("AllFilters"), "Either selects all or none of the Filters depending on your current selection. Having none selected will prevent the Bulldoze Tool from working." },
                { TooltipTitleKey("BuildingFilter"), "Building Filter" },
                { TooltipDescriptionKey("BuildingFilter"), "Toggling this off will prevent the Bulldoze Tool from removing Building assets." },
                { TooltipTitleKey("VanillaNetworksFilter"), "Network Filter" },
                { TooltipDescriptionKey("VanillaNetworksFilter"), "Toggling this off will prevent the Bulldoze Tool from removing Network assets such as roads, tracks, and powerlines." },
                { TooltipTitleKey("TreeFilter"), "Tree Filter" },
                { TooltipDescriptionKey("TreeFilter"), "Toggling this off will prevent the Bulldoze Tool from removing trees and wild bushes." },
                { TooltipTitleKey("PlantFilter"), "Plant Filter" },
                { TooltipDescriptionKey("PlantFilter"), "Toggling this off will prevent the Bulldoze Tool from removing plants that are not trees such as cultivated bushes and potted plants." },
                { TooltipTitleKey("DecalFilter"), "Decal Filter" },
                { TooltipDescriptionKey("DecalFilter"), "Toggling this off will prevent the Bulldoze Tool from removing decals." },
                { TooltipTitleKey("PropFilter"), "Any other Prop Filter" },
                { TooltipDescriptionKey("PropFilter"), "Toggling this off will prevent the Bulldoze Tool from removing any props that are not trees, plants, or decals." },
                { TooltipDescriptionKey("VanillaSurfaceFilter"), "Toggling this off will prevent the Bulldoze Tool from removing surfaces." }
            };
        }

        /// <inheritdoc/>
        public void Unload()
        {
        }

        private string TooltipDescriptionKey(string key)
        {
            return $"LSDLayeredSelectionDisplay.TOOLTIP_DESCRIPTION[{key}]";
        }
    }
}