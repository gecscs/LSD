// <copyright file="BulldozeToolSystemInitializeRaycastPatch.cs" company="0belix's Mods. MIT License">
// Copyright (c) 0belix's Mods. MIT License. All rights reserved.
// </copyright>

namespace LSD_Layered_Selection_Display.Patches
{
    using Game;
    using Game.Areas;
    using Game.Common;
    using Game.Net;
    using Game.Rendering;
    using Game.Tools;
    using HarmonyLib;
    using LSD_Layered_Selection_Display.Systems;
    using Unity.Entities;

    /// <summary>
    /// Patches Bulldoze Tool System Inititialize Raycast to add Markers as something to raycast.
    /// </summary>
    [HarmonyPatch(typeof(BulldozeToolSystem), "InitializeRaycast")]
    public class BulldozeToolSystemInitializeRaycastPatch
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
            LSDLayeredSelectionDisplayUISystem betterBulldozerUISystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<LSDLayeredSelectionDisplayUISystem>();
            RenderingSystem renderingSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<RenderingSystem>();
            if (renderingSystem.markersVisible && betterBulldozerUISystem.SelectedRaycastTarget == LSDLayeredSelectionDisplayUISystem.RaycastTarget.Markers)
            {
                toolRaycastSystem.typeMask = betterBulldozerUISystem.MarkersFilter;
                if ((betterBulldozerUISystem.MarkersFilter & TypeMask.Net) == TypeMask.Net)
                {
                    toolRaycastSystem.netLayerMask = Layer.MarkerPathway | Layer.MarkerTaxiway | Layer.PowerlineLow | Layer.PowerlineHigh | Layer.WaterPipe | Layer.SewagePipe;
                    toolRaycastSystem.raycastFlags = RaycastFlags.Markers;
                    toolRaycastSystem.utilityTypeMask = UtilityTypes.LowVoltageLine | UtilityTypes.HighVoltageLine | UtilityTypes.SewagePipe | UtilityTypes.SewagePipe;
                    toolRaycastSystem.collisionMask = CollisionMask.OnGround | CollisionMask.Underground | CollisionMask.Overground;
                }
                else
                {
                    toolRaycastSystem.raycastFlags = RaycastFlags.Markers | RaycastFlags.Decals;
                }
            }
            else if (betterBulldozerUISystem.SelectedRaycastTarget == LSDLayeredSelectionDisplayUISystem.RaycastTarget.Areas)
            {
                toolRaycastSystem.typeMask = TypeMask.Areas;
                toolRaycastSystem.areaTypeMask = betterBulldozerUISystem.AreasFilter;
                toolRaycastSystem.raycastFlags |= RaycastFlags.SubElements;
            }
            else if (betterBulldozerUISystem.SelectedRaycastTarget == LSDLayeredSelectionDisplayUISystem.RaycastTarget.Lanes)
            {
                toolRaycastSystem.typeMask = TypeMask.Net;
                toolRaycastSystem.netLayerMask = Layer.Fence | Layer.LaneEditor;
                toolRaycastSystem.raycastFlags |= RaycastFlags.Markers | RaycastFlags.EditorContainers;
            }
            else if (betterBulldozerUISystem.SelectedRaycastTarget == LSDLayeredSelectionDisplayUISystem.RaycastTarget.VehiclesCimsAndAnimals)
            {
                toolRaycastSystem.typeMask = TypeMask.MovingObjects;
            }
            else if (betterBulldozerUISystem.SelectedRaycastTarget == LSDLayeredSelectionDisplayUISystem.RaycastTarget.Vanilla)
            {
                if ((betterBulldozerUISystem.SelectedVanillaFilters & LSDLayeredSelectionDisplayUISystem.VanillaFilters.Networks) != LSDLayeredSelectionDisplayUISystem.VanillaFilters.Networks)
                {
                    toolRaycastSystem.typeMask &= ~TypeMask.Net;
                }

                if ((betterBulldozerUISystem.SelectedVanillaFilters & LSDLayeredSelectionDisplayUISystem.VanillaFilters.Surfaces) != LSDLayeredSelectionDisplayUISystem.VanillaFilters.Surfaces)
                {
                    toolRaycastSystem.areaTypeMask &= ~AreaTypeMask.Surfaces;
                }

                if ((betterBulldozerUISystem.SelectedVanillaFilters & LSDLayeredSelectionDisplayUISystem.VanillaFilters.Decals) != LSDLayeredSelectionDisplayUISystem.VanillaFilters.Decals)
                {
                    toolRaycastSystem.raycastFlags &= ~RaycastFlags.Decals;
                }

                if ((betterBulldozerUISystem.SelectedVanillaFilters & LSDLayeredSelectionDisplayUISystem.VanillaFilters.Buildings) != LSDLayeredSelectionDisplayUISystem.VanillaFilters.Buildings
                    && (betterBulldozerUISystem.SelectedVanillaFilters & LSDLayeredSelectionDisplayUISystem.VanillaFilters.Trees) != LSDLayeredSelectionDisplayUISystem.VanillaFilters.Trees
                    && (betterBulldozerUISystem.SelectedVanillaFilters & LSDLayeredSelectionDisplayUISystem.VanillaFilters.Plants) != LSDLayeredSelectionDisplayUISystem.VanillaFilters.Plants
                    && (betterBulldozerUISystem.SelectedVanillaFilters & LSDLayeredSelectionDisplayUISystem.VanillaFilters.Props) != LSDLayeredSelectionDisplayUISystem.VanillaFilters.Props
                    && (betterBulldozerUISystem.SelectedVanillaFilters & LSDLayeredSelectionDisplayUISystem.VanillaFilters.Decals) != LSDLayeredSelectionDisplayUISystem.VanillaFilters.Decals)
                {
                    toolRaycastSystem.typeMask &= ~TypeMask.StaticObjects;
                }
            }
        }
    }
}
