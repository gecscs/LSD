// <copyright file="LayeredSelectionDisplayModSettings.cs" company="0belix's Mods. MIT License">
// Copyright (c) 0belix's Mods. MIT License. All rights reserved.
// </copyright>

namespace LayeredSelectionDisplay.Settings
{
    using Colossal.IO.AssetDatabase;
    using Game.Modding;
    using Game.Settings;
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
        /// Sets if the List Panel is expanded or not.
        /// </summary>
        [SettingsUIHidden]
        public bool ExpandedListPanel { get; set; } = false;

        /// <inheritdoc/>
        public override void SetDefaults()
        {
            throw new System.NotImplementedException();
        }
    }
}
