// <copyright file="LSDLayeredSelectionDisplayUISystem.cs" company="0belix's Mods. MIT License">
// Copyright (c) 0belix's Mods. MIT License. All rights reserved.
// </copyright>

// #define VERBOSE
namespace LSD_Layered_Selection_Display.Systems
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
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
    using Game.UI;
    using Game.UI.InGame;
    using LSD_Layered_Selection_Display.Domain;
    using LSD_Layered_Selection_Display.Extensions;
    using LSD_Layered_Selection_Display.Utils;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Entities;
    using Unity.Jobs;
    using UnityEngine.InputSystem;
    using static Colossal.AssetPipeline.Diagnostic.Report;

    /// <summary>
    /// UI system for LSD extensions to the default tool.
    /// </summary>
    public partial class LSDLayeredSelectionDisplayUISystem : ExtendedUISystemBase
    {
        private const string ModId = "LSDLayeredSelectionDisplay";

        private const string MoveItToolID = "MoveItTool";
        private ToolSystem m_ToolSystem;
        private ToolBaseSystem m_MoveItTool;
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

        private ValueBinding<bool> m_IsMarqueeToolSelected;

        private NativeHashSet<Entity> m_MoveItSelectedEntities = new(0, Allocator.Persistent);
        private PropertyInfo m_MoveItSelectedEntitiesPropertyInfo;
        private GetterValueBinding<HashSet<Entity>> m_MoveItSelectedEntitiesBinding;

        private ValueBinding<SelectedEntities> m_SelectedEntitiesBinding;

        // private HashSet<Entity> m_MoveItSelectedEntitiesHashSet = new HashSet<Entity>();

        // private GetterValueBinding<List<Entity>> m_MoveItSelectedEntities;
        private JobHandle m_writeDeps;
        private JobHandle m_readDeps;
        private ModificationBarrier1 m_ModificationBarrier1;

        /// <summary>
        /// Get data, can be used inside or outside of system
        /// </summary>
        /// <param name="readOnly">true.</param>
        /// <param name="deps">dependency.</param>
        /// <returns>MoveItSelectedEntities.</returns>
        public NativeHashSet<Entity> GetEntities(bool readOnly, out JobHandle deps)
        {
            deps = readOnly ? m_writeDeps : JobHandle.CombineDependencies(m_readDeps, m_writeDeps);
            return m_MoveItSelectedEntities;
        }

        /// <summary>
        /// Register jobhandle as read dependency.
        /// </summary>
        /// <param name="jobHandle">jobhandle to add.</param>
        public void AddEntitiesReader(JobHandle jobHandle)
        {
            m_readDeps = JobHandle.CombineDependencies(m_readDeps, jobHandle);
        }

        /// <summary>
        /// Registers jobhandle as write dependency.
        /// </summary>
        /// <param name="jobHandle">jobhandle to add.</param>
        public void AddEntitiesWriter(JobHandle jobHandle)
        {
            m_writeDeps = JobHandle.CombineDependencies(m_writeDeps, jobHandle);
        }

        /// <summary>
        /// An enum to handle different raycast target options.
        /// </summary>
        public enum RaycastTarget
        {
            /// <summary>
            /// Do not change the raycast targets.
            /// </summary>
            Vanilla,

            /// <summary>
            /// Exclusively target standalone lanes such as fences, hedges, street markings, or vehicle lanes.
            /// </summary>
            Lanes,
        }

        /// <summary>
        /// An enum used to communicate filters for default tool.
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
        /// Gets a value indicating the selected default tool filters.
        /// </summary>
        public VanillaFilters SelectedVanillaFilters { get => m_SelectedVanillaFilters.Value; }

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
            m_DefaultToolSystem = World.GetOrCreateSystemManaged<DefaultToolSystem>();
            m_ActiveDefaultToolSystem = m_DefaultToolSystem;
            m_ModificationBarrier1 = World.GetOrCreateSystemManaged<ModificationBarrier1>();

            // These establish binding with UI.
            AddBinding(m_RaycastTarget = new ValueBinding<int>(ModId, "RaycastTarget", (int)RaycastTarget.Vanilla));
            m_IsGame = CreateBinding("IsGame", false);
            m_SelectedVanillaFilters = CreateBinding("SelectedVanillaFilters", VanillaFilters.Networks | VanillaFilters.Buildings | VanillaFilters.Trees | VanillaFilters.Plants | VanillaFilters.Decals | VanillaFilters.Props);

            // These handle events activating actions triggered by clicking buttons in the UI.
            CreateTrigger("ChangeVanillaFilter", (int value) => ChangeVanillaFilters((VanillaFilters)value));

            // Initialize the marquee tool selection state to false.
            m_IsMarqueeToolSelected = new ValueBinding<bool>(ModId, "IsMarqueeToolSelected", false);

            AddBinding(m_IsMarqueeToolSelected);

            // This handles the event when the marquee tool is selected in the UI.
            AddBinding(new TriggerBinding(ModId, "OnChangeMarqueeToolSelected", OnChangeMarqueeToolSelected));

            var moveItTool = World.GetOrCreateSystemManaged<ToolSystem>().tools.Find(x => x.toolID.Equals(MoveItToolID));

            AddBinding(m_SelectedEntitiesBinding = new ValueBinding<SelectedEntities>(ModId, "SelectedEntities", new SelectedEntities() { Entities = new List<SelectedEntity>() }));

            // AddBinding(m_MoveItSelectedEntitiesBinding = new GetterValueBinding<HashSet<Entity>>(
            //    ModId,
            //    "SelectedEntities",
            //    () => (HashSet<Entity>)m_MoveItSelectedEntitiesPropertyInfo.GetValue(moveItTool),
            //    new CollectionWriter<Entity>()));
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);

            if (World.GetOrCreateSystemManaged<ToolSystem>().tools.Find(x => x.toolID.Equals(MoveItToolID)) is ToolBaseSystem moveItTool)
            {
                // Found it
                m_Log.Info($"{nameof(LSDLayeredSelectionDisplayUISystem)}.{nameof(OnGameLoadingComplete)} found Move It.");
                PropertyInfo moveItSelectedEntities = moveItTool.GetType().GetProperty("SelectedEntities");
                if (moveItSelectedEntities is not null)
                {
                    m_MoveItTool = moveItTool;
                    m_MoveItSelectedEntitiesPropertyInfo = moveItSelectedEntities;
                    m_Log.Info($"{nameof(LSDLayeredSelectionDisplayUISystem)}.{nameof(OnGameLoadingComplete)} saved moveItTool");
                }
            }
            else
            {
                m_Log.Info($"{nameof(LSDLayeredSelectionDisplayUISystem)}.{nameof(OnGameLoadingComplete)} move it tool not found");
            }

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

            m_IsMarqueeToolSelected.Update(false);
        }

        /// <summary>
        /// Handles the update logic for the Better Bulldozer UI system.
        /// </summary>
        protected override void OnUpdate()
        {
            base.OnUpdate();
        }

        /// <summary>
        /// Handles the event when the marquee tool is selected in the UI.
        /// </summary>
        /// <param name="isSelected">
        /// A boolean indicating whether the marquee tool is selected.
        /// </param>
        private void OnChangeMarqueeToolSelected()
        {
            // m_Log.Debug($"{nameof(LSDLayeredSelectionDisplayMod)}.{nameof(OnChangeMarqueeToolSelected)} OnChangeMarqueeToolSelected button was clicked (before updating). m_IsMarqueeToolSelected: {m_IsMarqueeToolSelected.value}");

            // m_Log.Debug($"{nameof(LSDLayeredSelectionDisplayMod)}.{nameof(OnChangeMarqueeToolSelected)} OnChangeMarqueeToolSelected button was clicked. isSelected: {isSelected}");
            m_IsMarqueeToolSelected.Update(!m_IsMarqueeToolSelected.value);

            if (m_IsMarqueeToolSelected.value)
            {
                if (World.GetOrCreateSystemManaged<ToolSystem>().tools.Find(x => x.toolID.Equals(MoveItToolID)) is ToolBaseSystem moveItTool)
                {
                    PropertyInfo moveItSelectedEntities = moveItTool.GetType().GetProperty("SelectedEntities");
                    if (moveItSelectedEntities is not null)
                    {
                        m_MoveItTool = moveItTool;

                        // m_MoveItSelectedEntitiesPropertyInfo = moveItSelectedEntities;
                        m_Log.Info($"{nameof(LSDLayeredSelectionDisplayUISystem)}.{nameof(OnGameLoadingComplete)} saved moveItTool");

                        HashSet<Entity> selectedEntities = (HashSet<Entity>)m_MoveItSelectedEntitiesPropertyInfo.GetValue(moveItTool);

                        SelectedEntities moveItSelectedEntitiesBinding = new SelectedEntities() { Entities = new List<SelectedEntity>() };

                        foreach (var item in selectedEntities)
                        {
                            moveItSelectedEntitiesBinding.AddSelectedEntity(item);
                            m_Log.Debug($"{nameof(LSDLayeredSelectionDisplayUISystem)}.{nameof(OnChangeMarqueeToolSelected)} OnChangeMarqueeToolSelected button was clicked (after updating). m_IsMarqueeToolSelected: {m_IsMarqueeToolSelected.value}. Selected Entity: {item}");
                        }

                        m_SelectedEntitiesBinding.Update(moveItSelectedEntitiesBinding);
                    }
                }
            }

            // m_IsMarqueeToolSelected.Update(isSelected);
            // m_Log.Debug($"{nameof(LSDLayeredSelectionDisplayMod)}.{nameof(OnChangeMarqueeToolSelected)} OnChangeMarqueeToolSelected button was clicked (after updating). m_IsMarqueeToolSelected: {m_IsMarqueeToolSelected.value}");
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
    }
}
