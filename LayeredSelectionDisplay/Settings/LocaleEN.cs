// <copyright file="LocaleEN.cs" company="0belix's Mods. MIT License">
// Copyright (c) 0belix's Mods. MIT License. All rights reserved.
// </copyright>

namespace LayeredSelectionDisplay.Settings
{
    using System.Collections.Generic;
    using Colossal;

    /// <summary>
    /// Localization for <see cref="LayeredSelectionDisplayMod"/> mod in English.
    /// </summary>
    public class LocaleEN : IDictionarySource
    {
        private readonly LayeredSelectionDisplayModSettings m_Setting;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocaleEN"/> class.
        /// </summary>
        /// <param name="setting">Settings class.</param>
        public LocaleEN(LayeredSelectionDisplayModSettings setting)
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
            return $"LayeredSelectionDisplay.WARNING_TOOLTIP[{key}]";
        }

        /// <summary>
        /// Returns the locale key for a tooltip title key.
        /// </summary>
        /// <param name="key">The bracketed portion of locale key.</param>
        /// <returns>Localization key for translations.</returns>
        public static string TooltipTitleKey(string key)
        {
            return $"LayeredSelectionDisplay.TOOLTIP_TITLE[{key}]";
        }

        /// <inheritdoc/>
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "Layered Selection Display" },
                { m_Setting.GetOptionLabelLocaleID(nameof(LayeredSelectionDisplayModSettings.Version)), "Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(LayeredSelectionDisplayModSettings.Version)), $"Version number of the Layered Selection Display mod installed." },
                { "LAYERED_SELECTION_DISPLAY.Filter", "Filter" },
                { "LAYERED_SELECTION_DISPLAY_DESCRIPTION.SurfacesFilterButton", "For selecting surfaces inside or outside of buildings in one click." },
                { TooltipTitleKey("AllFilters"), "Toggle all Filters on/off" },
                { TooltipDescriptionKey("AllFilters"), "Either selects all or none of the Filters depending on your current selection. Having none selected will prevent the pointer from selecting any assets." },
                { TooltipTitleKey("BuildingFilter"), "Building Filter" },
                { TooltipDescriptionKey("BuildingFilter"), "Toggling this off will prevent the tool from selecting Building assets." },
                { TooltipTitleKey("VanillaNetworksFilter"), "Network Filter" },
                { TooltipDescriptionKey("VanillaNetworksFilter"), "Toggling this off will prevent the tool from selecting Network assets such as roads, tracks, and powerlines." },
                { TooltipTitleKey("TreeFilter"), "Tree Filter" },
                { TooltipDescriptionKey("TreeFilter"), "Toggling this off will prevent the tool from selecting trees and wild bushes." },
                { TooltipTitleKey("PlantFilter"), "Plant Filter" },
                { TooltipDescriptionKey("PlantFilter"), "Toggling this off will prevent the tool from selecting plants that are not trees such as cultivated bushes and potted plants." },
                { TooltipTitleKey("DecalFilter"), "Decal Filter" },
                { TooltipDescriptionKey("DecalFilter"), "Toggling this off will prevent the tool from selecting decals." },
                { TooltipTitleKey("PropFilter"), "Any other Prop Filter" },
                { TooltipDescriptionKey("PropFilter"), "Toggling this off will prevent the tool from selecting any props that are not trees, plants, or decals." },
                { TooltipDescriptionKey("VanillaSurfaceFilter"), "Toggling this off will prevent the tool from selecting surfaces." },
                { "LAYERED_SELECTION_DISPLAY_LISTPANEL.Title", "LSD - Selected Assets" },
                { "LAYERED_SELECTION_DISPLAY_LISTPANEL.Intro", "Click any asset from the list to open its info panel." },
                { "LAYERED_SELECTION_DISPLAY_LISTPANEL.RefreshButtonToolTip", "Refresh list with current selection from Move It" },
                { "LAYERED_SELECTION_DISPLAY_LISTPANEL.NoItemsSelected", "No items selected. Use Move It marquee selection tool to choose assets and then click the refresh button at the top of this panel. You will need to close Move It in order to see the highlighted item when hovering it on the list." },
                { "LAYERED_SELECTION_DISPLAY_LISTPANEL.NoItemsSelectedTip", "Use MoveIt marquee selection tool to choose assets and then click the refresh button in this panel. You will need to close MoveIt in order to see the highlighted item when hovering it on the list." },
                { "LAYERED_SELECTION_DISPLAY_LISTPANEL.MarqueeSelectionToolTip", "Marquee Tool (not yet implemented)" },
                { "LAYERED_SELECTION_DISPLAY_LISTPANEL.RemoveButtonToolTip", "Remove asset from list" },
                { "LAYERED_SELECTION_DISPLAY_MAINPANEL.Tools", "Tools" },
                { "LAYERED_SELECTION_DISPLAY_MAINPANEL.MarqueeToolToolTip", "Click to open the List Panel and import selected assets from Move It." },
            };
        }

        /// <inheritdoc/>
        public void Unload()
        {
        }

        /// <summary>
        /// Gets the ToolTip Description Key.
        /// </summary>
        /// <param name="key">The Tooltip Key.</param>
        /// <returns>Returns the key.</returns>
        private string TooltipDescriptionKey(string key)
        {
            return $"LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[{key}]";
        }
    }
}