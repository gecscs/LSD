// <copyright file="LSDLayeredSelectionDisplayModSettings.cs" company="0belix's Mods. MIT License">
// Copyright (c) 0belix's Mods. MIT License. All rights reserved.
// </copyright>

namespace LSD_Layered_Selection_Display.Settings
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
    using LSD_Layered_Selection_Display.Systems;
    using Newtonsoft.Json.Linq;
    using Unity.Entities;
    using Unity.Mathematics;

    /// <summary>
    /// The mod settings for the LSD Layered Selection Display mod.
    /// </summary>
    [FileLocation("ModsSettings/LSD_Layered_Selection_Display/LSD_Layered_Selection_Display")]
    public class LSDLayeredSelectionDisplayModSettings : ModSetting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LSDLayeredSelectionDisplayModSettings"/> class.
        /// </summary>
        /// <param name="mod">LSD Layered Selection Display mod.</param>
        public LSDLayeredSelectionDisplayModSettings(IMod mod)
            : base(mod)
        {
            // SetDefaults();
        }

        /// <summary>
        /// Gets a value indicating the version.
        /// </summary>
        public string Version => LSDLayeredSelectionDisplayMod.Instance.Version;

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
