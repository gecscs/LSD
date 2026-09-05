// <copyright file="LayeredSelectionDisplayUISystem.cs" company="0belix's Mods. MIT License">
// Copyright (c) 0belix's Mods. MIT License. All rights reserved.
// </copyright>

// #define VERBOSE
namespace LayeredSelectionDisplay.Systems
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Colossal.Entities;
    using Colossal.Logging;
    using Colossal.Serialization.Entities;
    using Colossal.UI.Binding;
    using Game;
    using Game.Areas;
    using Game.Common;
    using Game.Prefabs;
    using Game.Rendering;
    using Game.Routes;
    using Game.SceneFlow;
    using Game.Tools;
    using Game.UI.InGame;
    using LayeredSelectionDisplay.Domain;
    using LayeredSelectionDisplay.Extensions;
    using LayeredSelectionDisplay.Settings;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using UnityEngine.PlayerLoop;
    using static Game.Simulation.CityProductionStatisticSystem.CityResourceUsage;

    /// <summary>
    /// UI system for LSD extensions to the default tool.
    /// </summary>
    public partial class LayeredSelectionDisplayUISystem : ExtendedUISystemBase
    {
        private const string ModId = "LayeredSelectionDisplay";
        private const string MoveItToolID = "MoveItTool";
        private const string TransformGizmoToolId = "TransformGizmoTool";
        private ValueBinding<bool> m_EdtExists;
        private ValueBinding<bool> m_TransformGizmoToolExists;
        private int m_PendingTransformMode = 1;
        private Entity m_PendingTransformEntity = Entity.Null;
        private bool m_RestartTransformGizmo;
        private bool m_SetPendingTransformMode;
        private ToolSystem m_ToolSystem;
        private ToolBaseSystem m_MoveItTool;
        private ToolBaseSystem m_EDTTransformTool;
        private LSDMarqueeSelectionSystem m_MarqueeSelectionSystem;
        private ILog m_Log;
        private RenderingSystem m_RenderingSystem;
        private PrefabSystem m_PrefabSystem;
        private PrefabUISystem m_prefabUISystem;
        private DefaultToolSystem m_DefaultToolSystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private ValueBinding<int> m_RaycastTarget;
        private ValueBindingHelper<bool> m_IsGame;
        private ValueBindingHelper<bool> m_IsEditor;
        private ValueBindingHelper<VanillaFilters> m_SelectedVanillaFilters;
        private ToolBaseSystem m_ActiveDefaultToolSystem;
        private ToolUISystem m_ToolUISystem;

        private ValueBinding<bool> m_IsFiltersPanelVisible;

        private ValueBinding<bool> m_IsMoveItInstalled;
        private ValueBinding<bool> m_IsMarqueeToolSelected;
        private ValueBinding<bool> m_IsMarqueeToolActive;
        private LayeredSelectionDisplayModSettings m_settings;
        private ValueBinding<float2> m_PanelPosition;
        private ValueBinding<bool> m_ExpandedListPanel;
        private NativeHashSet<Entity> m_MoveItSelectedEntities = new(0, Allocator.Persistent);
        private PropertyInfo m_MoveItSelectedEntitiesPropertyInfo;
        private GetterValueBinding<HashSet<Entity>> m_MoveItSelectedEntitiesBinding;
        private ValueBinding<SelectedEntities> m_SelectedEntitiesBinding;
        private JobHandle m_writeDeps;
        private JobHandle m_readDeps;
        private Entity m_HoveredEntity = Entity.Null;
        private Entity m_PreviousHoveredEntity = Entity.Null;
        private HoverState m_HoverState;

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

        /// <summary>
        /// Gets a value indicating whether the marquee tool is selected.
        /// </summary>
        public HoverState HoverState => m_HoverState;

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = LayeredSelectionDisplayMod.Instance.Logger;
            m_Log.Info($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnCreate)}");
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_MarqueeSelectionSystem = World.GetOrCreateSystemManaged<LSDMarqueeSelectionSystem>();
            m_DefaultToolSystem = World.GetOrCreateSystemManaged<DefaultToolSystem>();
            m_RenderingSystem = World.GetOrCreateSystemManaged<RenderingSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_ObjectToolSystem = World.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_ToolUISystem = World.GetOrCreateSystemManaged<ToolUISystem>();
            m_DefaultToolSystem = World.GetOrCreateSystemManaged<DefaultToolSystem>();
            m_ActiveDefaultToolSystem = m_DefaultToolSystem;
            m_prefabUISystem = World.GetOrCreateSystemManaged<PrefabUISystem>();
            m_settings = LayeredSelectionDisplayMod.Instance?.Settings;
            m_HoverState = new HoverState();


            // These establish binding with UI.
            AddBinding(m_RaycastTarget = new ValueBinding<int>(ModId, "RaycastTarget", (int)RaycastTarget.Vanilla));
            m_IsGame = CreateBinding("IsGame", false);
            m_SelectedVanillaFilters = CreateBinding("SelectedVanillaFilters", VanillaFilters.Networks | VanillaFilters.Buildings | VanillaFilters.Trees | VanillaFilters.Plants | VanillaFilters.Decals | VanillaFilters.Props);

            // Flag to indicate whether the filters panel is visible in the UI.
            m_IsFiltersPanelVisible = new ValueBinding<bool>(ModId, "IsFiltersPanelVisible", false);
            AddBinding(m_IsFiltersPanelVisible);

            // These handle events activating actions triggered by clicking buttons in the UI.
            CreateTrigger("ChangeVanillaFilter", (int value) => ChangeVanillaFilters((VanillaFilters)value));

            // Initialize the Move It presence flag
            m_IsMoveItInstalled = new ValueBinding<bool>(ModId, "IsMoveItInstalled", false);
            AddBinding(m_IsMoveItInstalled);

            // Initialize the marquee tool panel  state to false.
            m_IsMarqueeToolSelected = new ValueBinding<bool>(ModId, "IsMarqueeToolSelected", false);
            AddBinding(m_IsMarqueeToolSelected);

            // Initialize the marquee tool selection state to false
            m_IsMarqueeToolActive = new ValueBinding<bool>(ModId, "IsMarqueeToolActive", false);
            AddBinding(m_IsMarqueeToolActive);

            float2 initialPanelPosition = new float2(x: 0.5f, y: 0.5f); // Default position if settings are not available

            if (m_settings?.GameListPanelPosition.x is not null && m_settings?.GameListPanelPosition.y is not null)
            {
                float2 settingsGameListPanelPosition = new float2(x: m_settings.GameListPanelPosition.x, y: m_settings.GameListPanelPosition.y); // Default position if settings are not available
                AddBinding(m_PanelPosition = new ValueBinding<float2>(ModId, "PanelPosition", settingsGameListPanelPosition));
            }
            else
            {
                AddBinding(m_PanelPosition = new ValueBinding<float2>(ModId, "PanelPosition", initialPanelPosition));
            }

            AddBinding(new TriggerBinding<float2>(ModId, "SetPanelPosition", SetPanelPosition));

            // This handles the event when the filters panel visibility is toggled in the UI.
            AddBinding(new TriggerBinding(ModId, "OnChangeFiltersPanelVisibility", OnChangeFiltersPanelVisibility));

            // This handles the event when the marquee tool is selected in the UI.
            AddBinding(new TriggerBinding(ModId, "OnChangeListPanelVisibility", OnChangeListPanelVisibility));

            AddBinding(new TriggerBinding(ModId, "SelectMarqueeTool", SelectMarqueeTool));

            AddBinding(m_ExpandedListPanel = new ValueBinding<bool>(ModId, "ExpandedListPanel", m_settings?.ExpandedListPanel ?? false));

            AddBinding(new TriggerBinding(ModId, "OnTogglePanelSize", OnTogglePanelSize));

            CreateTrigger("OnOpenTransform", (int index, int version) => OnOpenTransform(index, version));
            CreateTrigger("OnEntitySelect", (int index, int version) => OnEntitySelect(index, version));
            CreateTrigger("OnEntityHover", (int index, int version) => OnEntityHover(index, version));
            CreateTrigger("OnEntityLeave", (int index, int version) => OnEntityLeave(index, version));
            CreateTrigger("RefreshSelection", () => GetUpdatedSelectedEntitiesFromMoveIt());

            var moveItTool = World.GetOrCreateSystemManaged<ToolSystem>().tools.Find(x => x.toolID.Equals(MoveItToolID));

            m_TransformGizmoToolExists = new ValueBinding<bool>(ModId, "TransformGizmoToolExists", false);
            AddBinding(m_TransformGizmoToolExists);

            AddBinding(m_SelectedEntitiesBinding = new ValueBinding<SelectedEntities>(ModId, "SelectedEntities", new SelectedEntities() { Entities = new List<SelectedEntity>() }));

            EntityQuery m_HighlightedQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<Highlighted>() },
                None = new[]
                {
                    ComponentType.ReadOnly<Deleted>(),
                    ComponentType.ReadOnly<Temp>(),
                    ComponentType.ReadOnly<Overridden>(),
                },
            });
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);

            // m_Log.Debug($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnGameLoadingComplete)} before getting Move It tool.");

            if (World.GetOrCreateSystemManaged<ToolSystem>().tools.Find(x => x.toolID.Equals(MoveItToolID)) is ToolBaseSystem moveItTool)
            {
                // Found it
                // m_Log.Info($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnGameLoadingComplete)} found Move It.");
                PropertyInfo moveItSelectedEntities = moveItTool.GetType().GetProperty("SelectedEntities");
                if (moveItSelectedEntities is not null)
                {
                    m_MoveItTool = moveItTool;
                    m_MoveItSelectedEntitiesPropertyInfo = moveItSelectedEntities;
                    m_IsMoveItInstalled.Update(true);
                    // m_Log.Info($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnGameLoadingComplete)} saved moveItTool");
                }
            }
            else
            {
                m_Log.Info($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnGameLoadingComplete)} move it tool not found");
            }

            if (World.GetOrCreateSystemManaged<ToolSystem>().tools.Find(x => x.toolID.Equals(TransformGizmoToolId)) is ToolBaseSystem m_EdtTool)
            {
                m_EdtExists = new ValueBinding<bool>(ModId, "EdtExists", true);
               // m_Log.Debug($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnGameLoadingComplete)} EdtExists true.");
            }
            else
            {
                m_EdtExists = new ValueBinding<bool>(ModId, "EdtExists", false);
                // m_Log.Debug($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnGameLoadingComplete)} EdtExists false.");
            }

            if (World.GetOrCreateSystemManaged<ToolSystem>().tools.Find(x => x.toolID.Equals(TransformGizmoToolId)) is ToolBaseSystem gizmoTool)
            {
                // Found it
                m_TransformGizmoToolExists.Update(true);
                m_EDTTransformTool = gizmoTool;
            }
            else
            {
                m_TransformGizmoToolExists.Update(false);
                m_Log.Info($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnGameLoadingComplete)} EDT TransformGizmoTool not found");
            }

            //if (World.GetExistingSystemManaged<ToolSystem>().tools.Find(x => x.toolID.Equals(TransformGizmoToolId)) is ToolBaseSystem m_TransformGizmoTool)
            //{
            //    m_TransformGizmoToolExists = new ValueBinding<bool>(ModId, "TransformGizmoToolExists", true);
            //    // m_Log.Debug($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnGameLoadingComplete)} TransformGizmoToolExists true.");
            //    // m_Log.Debug($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnGameLoadingComplete)} m_TransformGizmoTool.toolID: " + m_TransformGizmoTool.toolID);
            //}
            //else
            //{
            //    m_TransformGizmoToolExists = new ValueBinding<bool>(ModId, "TransformGizmoToolExists", false);
            //    // m_Log.Debug($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnGameLoadingComplete)} TransformGizmoToolExists false.");
            //}

            m_Log.Debug($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnGameLoadingComplete)} after attempting to get Move It tool.");
            m_Log.Debug($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnGameLoadingComplete)} Old Tool Order:");

            foreach (ToolBaseSystem toolBaseSystem in m_ToolSystem.tools)
            {
                m_Log.Debug($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnGameLoadingComplete)} {toolBaseSystem.toolID}");
            }

            m_Log.Debug($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnGameLoadingComplete)} New Order:");

            foreach (ToolBaseSystem toolBaseSystem in m_ToolSystem.tools)
            {
                m_Log.Debug($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnGameLoadingComplete)} {toolBaseSystem.toolID}");
            }

            // ensure settings are available before any trigger using them
            m_settings = LayeredSelectionDisplayMod.Instance?.Settings;

            float2 initialPanelPosition = new float2(x: 0.5f, y: 0.5f); // Default position if settings are not available

            if (mode == GameMode.Game)
            {
                m_IsGame.Value = true;
                m_PanelPosition.Update(m_settings?.GameListPanelPosition ?? initialPanelPosition);
                return;
            }

            if (mode == GameMode.Editor)
            {
                m_IsEditor.Value = true;
                m_PanelPosition.Update(m_settings?.EditorListPanelPosition ?? initialPanelPosition);
                return;
            }

            m_IsGame.Value = false;
            m_IsMarqueeToolSelected.Update(false);
        }

        /// <summary>
        /// Handles deferred activation of the EDT TransformGizmoTool.
        ///
        /// EDT initializes its tool after ToolSystem.activeTool is changed.
        /// Therefore the desired mode must be applied on a later update,
        /// after EDT has finished its initialization.
        /// </summary>
        protected override void OnUpdate()
        {
            base.OnUpdate();

            // Phase 1:
            // Restart EDT with a new entity.
            if (m_RestartTransformGizmo)
            {
                m_RestartTransformGizmo = false;

                Entity entity = m_PendingTransformEntity;
                m_PendingTransformEntity = Entity.Null;

                if (entity == Entity.Null ||
                    !EntityManager.Exists(entity))
                {
                    return;
                }

                // EDT restores ToolSystem.selected when it shuts down.
                // Clear that value and provide the entity to consume
                m_ToolSystem.selected = Entity.Null;
                m_ToolSystem.selected = entity;

                // Activate EDT
                // Do NOT set the mode yet.
                if (EDTBridge.OpenTransformGizmo(World, entity))
                {
                    // The mode will be applied on the next update.
                    m_PendingTransformMode = 1;
                    m_SetPendingTransformMode = true;
                }

                return;
            }

            // Phase 2:
            // Apply the desired mode AFTER EDT has initialized.
            if (m_SetPendingTransformMode)
            {
                // Make sure EDT is now the active tool.
                if (m_EDTTransformTool != null &&
                    m_ToolSystem.activeTool == m_EDTTransformTool)
                {
                    if (EDTBridge.SetMode(
                            World,
                            m_PendingTransformMode))
                    {
                        m_SetPendingTransformMode = false;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the position of the panel in the UI and updates the mod settings accordingly.
        /// </summary>
        /// <param name="position"> The position to set. </param>
        private void SetPanelPosition(float2 position)
        {
            if (m_PanelPosition == null)
            {
                // m_Log?.Error($"{nameof(SetPanelPosition)}: UI binding for PanelPosition is null.");
                return;
            }

            m_PanelPosition.Update(position);
            m_ToolSystem ??= World.GetOrCreateSystemManaged<ToolSystem>();

            if (m_settings == null)
            {
                // m_Log?.Warn($"{nameof(SetPanelPosition)}: settings object is null; skipping save.");
                return;
            }

            try
            {
                if (m_ToolSystem.actionMode.IsGame())
                {
                    m_Log.Debug($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(SetPanelPosition)} Saving GameListPanelPosition to settings: {position}");
                    m_settings.GameListPanelPosition = position;
                    m_settings.ApplyAndSave();
                }
                else if (m_ToolSystem.actionMode.IsEditor())
                {
                    m_settings.EditorListPanelPosition = position;
                    m_settings.ApplyAndSave();
                }
            }
            catch (Exception ex)
            {
                m_Log?.Error(ex, $"{nameof(SetPanelPosition)}: failed to save settings");
            }
        }

        /// <summary>
        /// Toggles the visibility of the filters panel in the UI.
        /// </summary>
        private void OnChangeFiltersPanelVisibility()
        {
            m_IsFiltersPanelVisible.Update(
                !m_IsFiltersPanelVisible.value);
        }

        /// <summary>
        /// Handles the event when the marquee tool is selected in the UI.
        /// </summary>
        /// <param name="isSelected">
        /// A boolean indicating whether the marquee tool is selected.
        /// </param>
        private void OnChangeListPanelVisibility()
        {
            m_IsMarqueeToolSelected.Update(
                !m_IsMarqueeToolSelected.value);

            if (!m_IsMarqueeToolSelected.value)
            {
                m_MarqueeSelectionSystem.CancelSelection();
            }
        }

        public void SetMarqueeToolState(bool state)
        {
            m_IsMarqueeToolActive.Update(state);
        }

        public void SetMarqueeEntities(IEnumerable<Entity> entities)
        {
            SelectedEntities binding =
                new SelectedEntities
                {
                    Entities =
                        new List<SelectedEntity>(),
                };

            foreach (Entity entity in entities)
            {
                AddEntityToSelectedEntities(
                    binding,
                    entity);
            }

            m_SelectedEntitiesBinding.Update(
                binding);
        }

        /// <summary>
        /// Toggles the size of the List Panel in the UI.
        /// </summary>
        private void OnTogglePanelSize()
        {
            m_ExpandedListPanel.Update(!m_ExpandedListPanel.value);

            if (m_settings == null)
            {
                m_Log?.Warn($"{nameof(SetPanelPosition)}: settings object is null; skipping save.");
                return;
            }

            try
            {
                // m_Log.Debug($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnTogglePanelSize)} Saving ExpandedListPanel to settings: {m_ExpandedListPanel.value}");
                m_settings.ExpandedListPanel = m_ExpandedListPanel.value;
                m_settings.ApplyAndSave();
            }
            catch (Exception ex)
            {
                m_Log?.Error(ex, $"{nameof(OnTogglePanelSize)}: failed to save settings");
            }
        }

        private void AddEntityToSelectedEntities(SelectedEntities binding, Entity item)
        {
            if (EntityManager.TryGetComponent(item, out PrefabRef prefabRef))
            {
                m_prefabUISystem.GetTitleAndDescription(prefabRef.m_Prefab, out string titleId, out string description);

                string localizedName = string.Empty;

                if (GameManager.instance.localizationManager.activeDictionary.TryGetValue(titleId, out var name))
                {
                    localizedName = name;

                    if (m_PrefabSystem.TryGetPrefab(prefabRef.m_Prefab, out PrefabBase prefab) || prefab is not null)
                    {
                        if (prefab is not NetPrefab)
                        {
                            binding.AddSelectedEntity(item, localizedName ?? prefab.name.Replace("_", " "));
                        }

                        return;
                    }
                }
                else
                {
                    if (m_PrefabSystem.TryGetPrefab(
                            prefabRef.m_Prefab,
                            out PrefabBase prefab) ||
                        prefab is not null)
                    {
                        if (prefab is not NetPrefab)
                        {
                            binding.AddSelectedEntity(
                                item,
                                prefab.name.Replace(
                                    "_",
                                    " "));
                        }

                        return;
                    }
                }
            }

            binding.AddSelectedEntity(
                item,
                item.Index + " : " + item.Version);
        }

        /// <summary>
        /// Future implementation for the usage of a custom marquee tool yet to be developed
        /// </summary>
        private void SelectMarqueeTool()
        {
            // m_Log.Debug($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(SelectMarqueeTool)} Start LSD Marquee Selection");
            // m_Log.Debug($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(SelectMarqueeTool)} m_MoveItSelectedEntities (before): {m_MoveItSelectedEntities.Count}");
            m_MarqueeSelectionSystem.StartSelection();
            // m_Log.Debug($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(SelectMarqueeTool)} m_MoveItSelectedEntities (after): {m_MoveItSelectedEntities.Count}");
        }

        /// <summary>
        /// Updates the selected entities from the Move It tool and updates the binding for the UI.
        /// </summary>
        private void GetUpdatedSelectedEntitiesFromMoveIt()
        {
            if (m_MoveItTool is null || m_MoveItSelectedEntitiesPropertyInfo is null)
            {
                return;
            }

            HashSet<Entity> selectedEntities = (HashSet<Entity>)m_MoveItSelectedEntitiesPropertyInfo.GetValue(m_MoveItTool);

            SelectedEntities binding = new SelectedEntities
                {
                    Entities = new List<SelectedEntity>(),
                };

            foreach (Entity entity in selectedEntities)
            {
                AddEntityToSelectedEntities(binding, entity);
            }

            m_SelectedEntitiesBinding.Update(
                binding);
        }

        /// <summary>
        /// Handles the event when an entity is selected in the UI.
        /// </summary>
        /// <param name="index"> The index of the selected entity. </param>
        /// <param name="version"> The version of the selected entity. </param>
        private void OnEntitySelect(int index, int version)
        {
            // m_Log.Debug($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnEntitySelect)} Entity selected: {index}, {version}");

            Entity entity = new Entity
            {
                Index = index,
                Version = version,
            };

            if (EntityManager.Exists(entity))
            {
                // m_Log.Debug($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnEntitySelect)} Entity exists: {index}, {version}");

                m_ToolSystem.selected = entity;
                m_ToolSystem.activeTool = m_DefaultToolSystem;
            }
            else
            {
                m_Log.Debug($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnEntitySelect)} Entity does not exist: {index}, {version}");
            }
        }

        /// <summary>
        /// Handles the event when an entity is opened for transformation in the UI.
        /// </summary>
        /// <param name="index">The index of the selected entity.</param>
        /// <param name="version">The version of the selected entity.</param>
        private void OnOpenTransform(int index, int version)
        {
            Entity entity = new Entity
            {
                Index = index,
                Version = version,
            };

            if (!m_TransformGizmoToolExists.value ||
                !EntityManager.Exists(entity))
            {
                m_Log?.Error(
                    $"{nameof(OnOpenTransform)}: Entity does not exist or " +
                    $"TransformGizmoTool is not available. " +
                    $"index = {index}, version = {version}.");

                return;
            }

            // Set to open in Move mode
            const int desiredMode = 1;

            // Restart EDT so it gets the new entity through its normal initialization path
            if (m_ToolSystem.activeTool == m_EDTTransformTool)
            {
                m_PendingTransformEntity = entity;
                m_PendingTransformMode = desiredMode;
                m_RestartTransformGizmo = true;

                // Cancel any previous deferred mode operation
                m_SetPendingTransformMode = false;

                // Let EDT perform its normal shutdown so that it can be opened fresh
                m_ToolSystem.activeTool = m_DefaultToolSystem;

                return;
            }

            m_ToolSystem.selected = entity;

            if (EDTBridge.OpenTransformGizmo(World, entity))
            {
                // EDT will initialize itself during this frame/update.
                // Apply mode 1 on the following update.
                m_PendingTransformMode = desiredMode;
                m_SetPendingTransformMode = true;
            }
        }

        /// <summary>
        /// Handles the event when an entity is hovered over in the UI.
        /// </summary>
        /// <param name="index"> The index of the entity being hovered over. </param>
        /// <param name="version"> The version of the entity. </param>
        private void OnEntityHover(int index, int version)
        {
            // m_Log.Debug($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnEntityHover)} Entity hovered: {index}, {version}");

            Entity entity = new Entity
            {
                Index = index,
                Version = version,
            };

            if (EntityManager.Exists(entity))
            {
                // m_Log.Debug($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnEntityHover)} Entity exists: {index}, {version}");
                m_HoverState.HoveredEntity = entity;
                m_HoverState.Dirty = true;
            }
            else
            {
                m_Log.Debug($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnEntityHover)} Entity does not exist: {index}, {version}");
            }
        }

        /// <summary>
        /// Handles the event when an entity is no longer hovered over in the UI.
        /// </summary>
        /// <param name="index"> The index of the entity that is no longer being hovered over. </param>
        /// <param name="version"> The version of the entity. </param>
        private void OnEntityLeave(int index, int version)
        {
            // m_Log.Debug($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnEntityLeave)} Entity left: {index}, {version}");

            Entity entity = new Entity
            {
                Index = index,
                Version = version,
            };

            if (EntityManager.Exists(entity))
            {
                // m_Log.Debug($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnEntityLeave)} Entity exists: {index}, {version}");

                if (m_HoverState.HoveredEntity == entity)
                {
                    m_HoverState.HoveredEntity = Entity.Null;
                    m_HoverState.Dirty = true;
                }
            }
            else
            {
                m_Log.Debug($"{nameof(LayeredSelectionDisplayUISystem)}.{nameof(OnEntityLeave)} Entity does not exist: {index}, {version}");
            }
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
