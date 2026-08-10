// <copyright file="DefaultToolSystemInitializeRaycastPatch.cs" company="0belix's Mods. MIT License">
// Copyright (c) 0belix's Mods. MIT License. All rights reserved.
// </copyright>

namespace LayeredSelectionDisplay.Patches
{
    using Game;
    using Game.Areas;
    using Game.Common;
    using Game.Net;
    using Game.Rendering;
    using Game.Tools;
    using HarmonyLib;
    using LayeredSelectionDisplay.Systems;
    using Unity.Entities;

    /// <summary>
    /// Patches Bulldoze Tool System Inititialize Raycast to add Markers as something to raycast.
    /// </summary>
    [HarmonyPatch(typeof(DefaultToolSystem), "InitializeRaycast")]
    public class DefaultToolSystemInitializeRaycastPatch
    {
        /// <summary>
        /// Patches Bulldoze Tool System Inititialize Raycast to add Markers as something to raycast.
        /// </summary>
        public static void Postfix()
        {
            ToolSystem toolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>();
            if (!toolSystem.actionMode.IsGame())
            {
                return;
            }

            ToolRaycastSystem toolRaycastSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolRaycastSystem>();
            LayeredSelectionDisplayUISystem betterBulldozerUISystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<LayeredSelectionDisplayUISystem>();
            RenderingSystem renderingSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<RenderingSystem>();
            if (betterBulldozerUISystem.SelectedRaycastTarget == LayeredSelectionDisplayUISystem.RaycastTarget.Vanilla)
            {
                if ((betterBulldozerUISystem.SelectedVanillaFilters & LayeredSelectionDisplayUISystem.VanillaFilters.Networks) != LayeredSelectionDisplayUISystem.VanillaFilters.Networks)
                {
                    toolRaycastSystem.typeMask &= ~TypeMask.Net;
                }

                if ((betterBulldozerUISystem.SelectedVanillaFilters & LayeredSelectionDisplayUISystem.VanillaFilters.Surfaces) != LayeredSelectionDisplayUISystem.VanillaFilters.Surfaces)
                {
                    toolRaycastSystem.areaTypeMask &= ~AreaTypeMask.Surfaces;
                }

                if ((betterBulldozerUISystem.SelectedVanillaFilters & LayeredSelectionDisplayUISystem.VanillaFilters.Decals) != LayeredSelectionDisplayUISystem.VanillaFilters.Decals)
                {
                    toolRaycastSystem.raycastFlags &= ~RaycastFlags.Decals;
                }

                if ((betterBulldozerUISystem.SelectedVanillaFilters & LayeredSelectionDisplayUISystem.VanillaFilters.Buildings) != LayeredSelectionDisplayUISystem.VanillaFilters.Buildings
                    && (betterBulldozerUISystem.SelectedVanillaFilters & LayeredSelectionDisplayUISystem.VanillaFilters.Trees) != LayeredSelectionDisplayUISystem.VanillaFilters.Trees
                    && (betterBulldozerUISystem.SelectedVanillaFilters & LayeredSelectionDisplayUISystem.VanillaFilters.Plants) != LayeredSelectionDisplayUISystem.VanillaFilters.Plants
                    && (betterBulldozerUISystem.SelectedVanillaFilters & LayeredSelectionDisplayUISystem.VanillaFilters.Props) != LayeredSelectionDisplayUISystem.VanillaFilters.Props
                    && (betterBulldozerUISystem.SelectedVanillaFilters & LayeredSelectionDisplayUISystem.VanillaFilters.Decals) != LayeredSelectionDisplayUISystem.VanillaFilters.Decals)
                {
                    toolRaycastSystem.typeMask &= ~TypeMask.StaticObjects;
                }
            }
        }
    }
}
