namespace LayeredSelectionDisplay.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using Colossal.Logging;
    using Game.Tools;
    using LayeredSelectionDisplay.Domain;
    using LayeredSelectionDisplay.Systems;
    using Unity.Entities;

    /// <summary>
    /// Runtime bridge to Extra Detailing Tools.
    /// </summary>
    public static class EDTBridge
    {
        private const string kTransformGizmoToolType =
            "ExtraDetailingTools.Systems.Tools.TransformGizmoTool";

        private static ILog m_Log;

        private static Type s_TransformGizmoToolType;
        private static MethodInfo s_SelectedEntitiesMethod;

        private static LayeredSelectionDisplayUISystem m_UISystem;

        private static bool s_Initialized;

        /// <summary>
        /// Initializes the EDTBridge by finding the TransformGizmoTool type and its SelectEntities method through reflection.
        /// </summary>
        private static void Initialize()
        {
            if (s_Initialized)
            {
                return;
            }

            s_Initialized = true;

            m_Log = LayeredSelectionDisplayMod.Instance.Logger;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type toolType = assembly.GetType(
                        kTransformGizmoToolType,
                        throwOnError: false);

                    if (toolType == null)
                    {
                        continue;
                    }

                    s_TransformGizmoToolType = toolType;

                    s_SelectedEntitiesMethod = s_TransformGizmoToolType.GetMethod(
                        "SelectEntities",
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic,
                        binder: null,
                        types: new[] { typeof(IEnumerable<Entity>), typeof(Entity), typeof(bool) },
                        modifiers: null);

                    break;
                }
                catch
                {
                    // Ignore assemblies that cannot be inspected.
                }
            }
        }

        /// <summary>
        /// Returns true when EDT's TransformGizmoTool can be found.
        /// </summary>
        public static bool IsAvailable
        {
            get
            {
                Initialize();
                return s_TransformGizmoToolType != null;
            }
        }

        /// <summary>
        /// Activates EDT's TransformGizmoTool with the specified entity.
        /// The mode is intentionally NOT set here.
        /// EDT initializes the tool after activation and can overwrite the mode.
        /// The caller should set the mode on a later frame using SetMode().
        /// </summary>
        public static bool OpenTransformGizmo(
            World world,
            bool singleEntity)
        {
            Initialize();

            if (s_TransformGizmoToolType == null)
            {
                return false;
            }

            if (world == null)
            {
                return false;
            }

            ToolSystem toolSystem =
                world.GetOrCreateSystemManaged<ToolSystem>();

            if (toolSystem == null)
            {
                return false;
            }

            object transformTool =
                world.GetOrCreateSystemManaged(
                    s_TransformGizmoToolType);

            if (transformTool is not ToolBaseSystem toolBaseSystem)
            {
                return false;
            }

            toolSystem.activeTool = toolBaseSystem;

            return true;
        }

        /// <summary>
        /// Sets EDT's TransformGizmoTool mode through reflection.
        /// This should normally be called after EDT has had one frame
        /// to initialize its tool.
        /// </summary>
        /// <param name="world">The world instance.</param>
        /// <param name="curatedEntities">The list of curated entities.</param>
        /// <param name="primaryEntity">The primary entity to be used as pivot by EDT.</param>
        /// <returns>True if the entities were selected successfully; otherwise, false.</returns>
        public static bool SelectEntities(
            World world,
            List<SelectedEntity> curatedEntities,
            SelectedEntity primaryEntity)
        {
            Initialize();

            m_Log?.Debug($"{nameof(EDTBridge)}: curatedEntities.Count = {curatedEntities.Count}");
            m_Log?.Debug($"{nameof(EDTBridge)}: primaryEntity = {primaryEntity.ToString()}");

            if (world == null ||
                s_TransformGizmoToolType == null ||
                s_SelectedEntitiesMethod == null)
            {
                m_Log?.Debug($"{nameof(EDTBridge)}: World or TransformGizmoTool or SelectedEntitiesMethod not available.");
                return false;
            }

            object transformTool =
                world.GetOrCreateSystemManaged(
                    s_TransformGizmoToolType);

            if (transformTool == null)
            {
                m_Log?.Debug($"{nameof(EDTBridge)}: TransformGizmoTool not available.");
                return false;
            }

            try
            {
                List<Entity> curatedList = new List<Entity>();

                Entity primary = new Entity() { Index = primaryEntity.Index, Version = primaryEntity.Version };

                foreach (SelectedEntity selectedEntity in curatedEntities)
                {
                    Entity entity = new Entity
                    {
                        Index = selectedEntity.Index,
                        Version = selectedEntity.Version,
                    };

                    if (!world.EntityManager.Exists(entity))
                    {
                        continue;
                    }

                    curatedList.Add(entity);
                }

                s_SelectedEntitiesMethod.Invoke(
                    transformTool,
                    new object[] { curatedList, primary, true });

                return true;
            }
            catch (Exception ex)
            {
                m_Log?.Error($"{nameof(EDTBridge)}.SelectEntities: Error occurred while Attempting to suppy EDT with multiple selected entities. Exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Returns the actual EDT TransformGizmoTool instance.
        /// </summary>
        public static ToolBaseSystem GetTransformGizmoTool(World world)
        {
            Initialize();

            if (world == null ||
                s_TransformGizmoToolType == null)
            {
                return null;
            }

            object transformTool =
                world.GetOrCreateSystemManaged(
                    s_TransformGizmoToolType);

            return transformTool as ToolBaseSystem;
        }
    }
}