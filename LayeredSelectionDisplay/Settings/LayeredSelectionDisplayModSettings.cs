// <copyright file="LayeredSelectionDisplayModSettings.cs" company="0belix's Mods. MIT License">
// Copyright (c) 0belix's Mods. MIT License. All rights reserved.
// </copyright>

namespace LayeredSelectionDisplay.Settings
{
    using Colossal.IO.AssetDatabase;
    using Colossal.Logging;
    using Game.Modding;
    using Game.Settings;
    using LayeredSelectionDisplay.Systems;
    using System.ComponentModel;
    using Unity.Entities;
    using Unity.Mathematics;

    /// <summary>
    /// The mod settings for the LSD Layered Selection Display mod.
    /// </summary>
    [FileLocation("ModsSettings/LayeredSelectionDisplay/LayeredSelectionDisplay")]
    public class LayeredSelectionDisplayModSettings : ModSetting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LayeredSelectionDisplayModSettings"/> class.
        /// </summary>
        /// <param name="mod">LSD Layered Selection Display mod.</param>
        public LayeredSelectionDisplayModSettings(IMod mod)
            : base(mod)
        {
            // SetDefaults();
        }

        private ILog m_Log;

        /// <summary>
        /// Gets a value indicating the version.
        /// </summary>
        public string Version => LayeredSelectionDisplayMod.Instance.Version;

        /// <summary>
        /// Gets or sets the position of the game list panel.
        /// </summary>
        [SettingsUIHidden]
        public float2 GameListPanelPosition { get; set; }

        /// <summary>
        /// Gets or sets the position of the editor list panel.
        /// </summary>
        [SettingsUIHidden]
        public float2 EditorListPanelPosition { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the List Panel is expanded or not.
        /// </summary>
        [SettingsUIHidden]
        public bool ExpandedListPanel { get; set; } = false;

        private bool m_AllowSubObjectSelection = false;

        /// <summary>
        /// Gets or sets a value indicating whether sub-object selection is allowed.
        /// </summary>
        public bool AllowSubObjectSelection
        {
            get
            {
                return m_AllowSubObjectSelection;
            }

            set
            {
                m_AllowSubObjectSelection = value;
                World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<LayeredSelectionDisplayUISystem>()?.m_AllowSubObjectSelection.Update(value);
                ApplyAndSave();
            }
        }

        /// <inheritdoc/>
        public override void SetDefaults()
        {
            AllowSubObjectSelection = false;
        }
    }
}
