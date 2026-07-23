// <copyright file="LSDLayeredSelectionDisplayModSettings.cs" company="0belix's Mods. MIT License">
// Copyright (c) 0belix's Mods. MIT License. All rights reserved.
// </copyright>

namespace LSD_Layered_Selection_Display.Settings
{
    using Colossal.IO.AssetDatabase;
    using Game;
    using Game.Input;
    using Game.Modding;
    using Game.Settings;
    using Game.Tools;
    using LSD_Layered_Selection_Display.Systems;
    using Unity.Entities;

    /// <summary>
    /// The mod settings for the LSD Layered Selection Display mod.
    /// </summary>
    [FileLocation("Mods_LSD_Layered_Selection_Display")]
    public class LSDLayeredSelectionDisplayModSettings : ModSetting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LSDLayeredSelectionDisplayModSettings"/> class.
        /// </summary>
        /// <param name="mod">LSD Layered Selection Display mod.</param>
        public LSDLayeredSelectionDisplayModSettings(IMod mod)
            : base(mod)
        {
            SetDefaults();
        }

        /// <summary>
        /// Gets a value indicating the version.
        /// </summary>
        public string Version => LSDLayeredSelectionDisplayMod.Instance.Version;

        /// <inheritdoc/>
        public override void SetDefaults()
        {
        }
    }
}
