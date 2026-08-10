// <copyright file="LayeredSelectionDisplayModSettings.cs" company="0belix's Mods. MIT License">
// Copyright (c) 0belix's Mods. MIT License. All rights reserved.
// </copyright>

namespace LayeredSelectionDisplay.Settings
{
    using System.Collections.Generic;
    using Colossal;
    using Colossal.IO.AssetDatabase;
    using Game;
    using Game.Input;
    using Game.Modding;
    using Game.Settings;
    using Game.Tools;
    using Game.UI;
    using Game.UI.Widgets;
    using LayeredSelectionDisplay.Systems;
    using Newtonsoft.Json.Linq;
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

        /// <inheritdoc/>
        public override void SetDefaults()
        {
            throw new System.NotImplementedException();
        }
    }
}
