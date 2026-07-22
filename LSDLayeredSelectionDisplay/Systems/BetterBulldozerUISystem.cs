// <copyright file="BetterBulldozerUISystem.cs" company="0belix's Mods. MIT License">
// Copyright (c) 0belix's Mods. MIT License. All rights reserved.
// </copyright>

// #define VERBOSE
namespace LSD_Layered_Selection_Display.Systems
{
    using System;
    using System.Collections.Generic;
    using Colossal.Logging;
    using Colossal.Serialization.Entities;
    using Colossal.UI.Binding;
    using Game;
    using Game.Areas;
    using Game.Common;
    using Game.Input;
    using Game.Prefabs;
    using Game.Rendering;
    using Game.SceneFlow;
    using Game.Tools;
    using Game.UI.InGame;
    using LSD_Layered_Selection_Display.Extensions;
    using LSD_Layered_Selection_Display.Utils;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Entities;
    using UnityEngine.InputSystem;
    using static Colossal.AssetPipeline.Diagnostic.Report;

    /// <summary>
    /// UI system for Better Bulldozer extensions to the bulldoze tool.
    /// </summary>
    public partial class LSDLayeredSelectionDisplayUISystem : ExtendedUISystemBase
    {
        private const string ModId = "LSDLayeredSelectionDisplay";

        private ToolSystem m_ToolSystem;
        private ILog m_Log;
        private RenderingSystem m_RenderingSystem;
        private PrefabSystem m_PrefabSystem;
        private DefaultToolSystem m_DefaultToolSystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private ValueBinding<int> m_RaycastTarget;
        private ValueBindingHelper<bool> m_IsGame;
        private ValueBindingHelper<VanillaFilters> m_SelectedVanillaFilters;
        private ToolBaseSystem m_ActiveDefaultToolSystem;
        private ToolUISystem m_ToolUISystem;
        private cohtml.Net.View m_UiView;

        /// <summary>
        /// An enum to handle different raycast target options.
        /// </summary>
        public enum RaycastTarget
        {
            /// <summary>
            /// Do not change the raycast targets.
            /// </summary>
            Vanilla,
        }

        /// <summary>
        /// An enum used to communicate filters for vanilla bulldozer.
        /// </summary>
        public enum VanillaFilters
        {
            /// <summary>
            /// Nothing selected.
            /// </summary>
            None = 0,

            /// <summary>
            /// Roads, tracks, etc.
            /// </summary>
            Networks = 1,

            /// <summary>
            /// Things with building data.
            /// </summary>
            Buildings = 2,

            /// <summary>
            /// Trees and wild bushes.
            /// </summary>
            Trees = 4,

            /// <summary>
            /// Cultivated plants and potted plants.
            /// </summary>
            Plants = 8,

            /// <summary>
            /// Decals.
            /// </summary>
            Decals = 16,

            /// <summary>
            /// Static objects that are not anything else.
            /// </summary>
            Props = 32,

            /// <summary>
            /// Surfaces.
            /// </summary>
            Surfaces = 64,

            /// <summary>
            /// Vanilla bulldozer, no filters.
            /// </summary>
            All = 128,
        }

        /// <summary>
        /// Gets a value indicating what to raycast.
        /// </summary>
        public RaycastTarget SelectedRaycastTarget { get => (RaycastTarget)m_RaycastTarget.value; }

        /// <summary>
        /// Gets a value indicating the selected vanilla bulldoze tool filters.
        /// </summary>
        public VanillaFilters SelectedVanillaFilters { get => m_SelectedVanillaFilters.Value; }

        /// <summary>
        /// Hacks UI to ensure button for bulldozer in main toolbar is appropriately selected or not.
        /// </summary>
        public void EnsureToolbarBulldozerClassList()
        {
            if (m_UiView == null)
            {
                m_UiView = GameManager.instance.userInterface.view.View;
            }

            // This script creates the LSDLayeredSelectionDisplay object if it doesn't exist.
            m_UiView.ExecuteScript("if (yyLSDLayeredSelectionDisplay == null) var yyLSDLayeredSelectionDisplay = {};");

            if (m_ToolSystem.activeTool == m_DefaultToolSystem)
            {
                // This script searches through all img and adds removes selected if the src of that image contains the bulldozer.svg.
                m_UiView.ExecuteScript($"yyLSDLayeredSelectionDisplay.tagElements = document.getElementsByTagName(\"img\"); for (yyLSDLayeredSelectionDisplay.i = 0; yyLSDLayeredSelectionDisplay.i < yyLSDLayeredSelectionDisplay.tagElements.length; yyLSDLayeredSelectionDisplay.i++) {{ if (yyLSDLayeredSelectionDisplay.tagElements[yyLSDLayeredSelectionDisplay.i].src.includes(\"Bulldozer.svg\")) {{ yyLSDLayeredSelectionDisplay.tagElements[yyLSDLayeredSelectionDisplay.i].parentNode.classList.add(\"selected\");  }} }} ");
            }
            else
            {
                // This script searches through all img and adds removes selected if the src of that image contains the bulldozer.svg.
                m_UiView.ExecuteScript($"yyLSDLayeredSelectionDisplay.tagElements = document.getElementsByTagName(\"img\"); for (yyLSDLayeredSelectionDisplay.i = 0; yyLSDLayeredSelectionDisplay.i < yyLSDLayeredSelectionDisplay.tagElements.length; yyLSDLayeredSelectionDisplay.i++) {{ if (yyLSDLayeredSelectionDisplay.tagElements[yyLSDLayeredSelectionDisplay.i].src.includes(\"Bulldozer.svg\")) {{ yyLSDLayeredSelectionDisplay.tagElements[yyLSDLayeredSelectionDisplay.i].parentNode.classList.remove(\"selected\");  }} }} ");
            }
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = LSDLayeredSelectionDisplayMod.Instance.Logger;
            m_Log.Info($"{nameof(LSDLayeredSelectionDisplayUISystem)}.{nameof(OnCreate)}");
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_DefaultToolSystem = World.GetOrCreateSystemManaged<DefaultToolSystem>();
            m_RenderingSystem = World.GetOrCreateSystemManaged<RenderingSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_ObjectToolSystem = World.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_ToolUISystem = World.GetOrCreateSystemManaged<ToolUISystem>();
            m_ToolSystem.EventToolChanged += OnToolChanged;
            m_DefaultToolSystem = World.GetOrCreateSystemManaged<DefaultToolSystem>();
            m_ToolSystem.EventPrefabChanged += OnPrefabChanged;
            m_ActiveDefaultToolSystem = m_DefaultToolSystem;
            m_UiView = GameManager.instance.userInterface.view.View;

            // These establish binding with UI.
            AddBinding(m_RaycastTarget = new ValueBinding<int>(ModId, "RaycastTarget", (int)RaycastTarget.Vanilla));
            m_IsGame = CreateBinding("IsGame", false);
            m_SelectedVanillaFilters = CreateBinding("SelectedVanillaFilters", VanillaFilters.Networks | VanillaFilters.Buildings | VanillaFilters.Trees | VanillaFilters.Plants | VanillaFilters.Decals | VanillaFilters.Props);

            // These handle events activating actions triggered by clicking buttons in the UI.
            CreateTrigger("ChangeVanillaFilter", (int value) => ChangeVanillaFilters((VanillaFilters)value));
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);
            m_Log.Debug($"{nameof(LSDLayeredSelectionDisplayUISystem)}.{nameof(OnGameLoadingComplete)} Old Tool Order:");
            foreach (ToolBaseSystem toolBaseSystem in m_ToolSystem.tools)
            {
                m_Log.Debug($"{nameof(LSDLayeredSelectionDisplayUISystem)}.{nameof(OnGameLoadingComplete)} {toolBaseSystem.toolID}");
            }

            m_Log.Debug($"{nameof(LSDLayeredSelectionDisplayUISystem)}.{nameof(OnGameLoadingComplete)} New Order:");

            foreach (ToolBaseSystem toolBaseSystem in m_ToolSystem.tools)
            {
                m_Log.Debug($"{nameof(LSDLayeredSelectionDisplayUISystem)}.{nameof(OnGameLoadingComplete)} {toolBaseSystem.toolID}");
            }

            /*
            m_Log.Debug("Shortcuts Action Map:");
            ProxyActionMap shortcutsMap = InputManager.instance.FindActionMap(InputManager.kShortcutsMap);
            foreach (KeyValuePair<string, ProxyAction> keyValue in shortcutsMap.actions)
            {
                m_Log.Debug(keyValue.Key);
            }

            m_Log.Debug("Tool Action Map:");
            ProxyActionMap toolMap = InputManager.instance.FindActionMap(InputManager.kToolMap);
            foreach (KeyValuePair<string, ProxyAction> keyValue in toolMap.actions)
            {
                m_Log.Debug(keyValue.Key);
            }

            m_Log.Debug("kEngagementMap Action Map:");
            ProxyActionMap kEngagementMap = InputManager.instance.FindActionMap(InputManager.kEngagementMap);
            foreach (KeyValuePair<string, ProxyAction> keyValue in kEngagementMap.actions)
            {
                m_Log.Debug(keyValue.Key);
            }*/

            if (mode == GameMode.Game)
            {
                m_IsGame.Value = true;
                return;
            }

            m_IsGame.Value = false;
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            base.OnUpdate();

            /*
            if (m_ToolSystem.activeTool == m_DefaultToolSystem &&
                m_ActiveDefaultToolSystem != m_DefaultToolSystem)
            {
                if (m_ActiveDefaultToolSystem == m_RemoveVehiclesCimsAndAnimalsTool)
                {
                    m_RemoveVehiclesCimsAndAnimalsTool.MustStartRunning = true;
                }
                else if (m_ActiveDefaultToolSystem == m_SubElementDefaultToolSystem)
                {
                    m_SubElementDefaultToolSystem.MustStartRunning = true;
                }

                m_ToolSystem.activeTool = m_ActiveDefaultToolSystem;
            }*/
        }

        private void ChangeVanillaFilters(VanillaFilters toggledFilter)
        {
            if (toggledFilter != VanillaFilters.All && (m_SelectedVanillaFilters.Value & VanillaFilters.All) == VanillaFilters.All)
            {
                m_SelectedVanillaFilters.Value &= ~VanillaFilters.All;
            }
            else if (toggledFilter == VanillaFilters.All && m_SelectedVanillaFilters.Value != VanillaFilters.None)
            {
                m_SelectedVanillaFilters.Value = VanillaFilters.None;
                return;
            }
            else if (toggledFilter == VanillaFilters.All && m_SelectedVanillaFilters.Value == VanillaFilters.None)
            {
                m_SelectedVanillaFilters.Value |= VanillaFilters.Networks | VanillaFilters.Buildings | VanillaFilters.Trees | VanillaFilters.Plants | VanillaFilters.Decals | VanillaFilters.Props | VanillaFilters.Surfaces | VanillaFilters.All;
                return;
            }

            if ((m_SelectedVanillaFilters.Value & toggledFilter) == toggledFilter)
            {
                m_SelectedVanillaFilters.Value &= ~toggledFilter;
            }
            else
            {
                m_SelectedVanillaFilters.Value |= toggledFilter;
            }

            if ((int)m_SelectedVanillaFilters.Value == 127)
            {
                m_SelectedVanillaFilters.Value |= VanillaFilters.All;
            }
        }

        private void OnToolChanged(ToolBaseSystem tool)
        {
            if (tool == null)
            {
                m_Log.Debug($"{nameof(LSDLayeredSelectionDisplayUISystem)}.{nameof(OnToolChanged)} something is null.");
                return;
            }

            if (m_ToolSystem.actionMode.IsEditor())
            {
                return;
            }

            m_Log.Debug($"{nameof(LSDLayeredSelectionDisplayUISystem)}.{nameof(OnToolChanged)} tool.toolID:{tool.toolID} m_ToolSystem.activePrefab?.GetPrefabID():{m_ToolSystem.activePrefab?.GetPrefabID()} tool.GetPrefab()?.GetPrefabID():{tool.GetPrefab()?.GetPrefabID()}");

            EnsureToolbarBulldozerClassList();
        }

        /// <summary>
        /// Method implemented by event triggered by prefab changing.
        /// </summary>
        /// <param name="prefab">The new prefab.</param>
        private void OnPrefabChanged(PrefabBase prefab)
        {
            if (prefab == null)
            {
                return;
            }

            m_Log.Debug($"{nameof(LSDLayeredSelectionDisplayUISystem)}.{nameof(OnPrefabChanged)} {prefab.GetPrefabID()}");
        }
    }
}
