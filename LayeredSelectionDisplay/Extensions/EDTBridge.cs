namespace LayeredSelectionDisplay.Extensions
{
    using System;
    using System.Reflection;
    using Game.Tools;
    using Unity.Entities;

    /// <summary>
    /// Runtime bridge to Extra Detailing Tools.
    /// </summary>
    public static class EDTBridge
    {
        private const string kTransformGizmoToolType =
            "ExtraDetailingTools.Systems.Tools.TransformGizmoTool";

        private static Type s_TransformGizmoToolType;
        private static Type s_ModeType;
        private static MethodInfo s_SetModeMethod;

        private static bool s_Initialized;

        private static void Initialize()
        {
            if (s_Initialized)
            {
                return;
            }

            s_Initialized = true;

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

                    s_ModeType = s_TransformGizmoToolType.GetNestedType(
                        "Mode",
                        BindingFlags.Public |
                        BindingFlags.NonPublic);

                    if (s_ModeType != null)
                    {
                        s_SetModeMethod = s_TransformGizmoToolType.GetMethod(
                            "SetMode",
                            BindingFlags.Instance |
                            BindingFlags.Public |
                            BindingFlags.NonPublic,
                            binder: null,
                            types: new[] { s_ModeType },
                            modifiers: null);
                    }

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
            Entity entity)
        {
            Initialize();

            if (s_TransformGizmoToolType == null)
            {
                return false;
            }

            if (world == null ||
                entity == Entity.Null ||
                !world.EntityManager.Exists(entity))
            {
                return false;
            }

            ToolSystem toolSystem =
                world.GetOrCreateSystemManaged<ToolSystem>();

            if (toolSystem == null)
            {
                return false;
            }

            toolSystem.selected = entity;

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
        public static bool SetMode(
            World world,
            int mode)
        {
            Initialize();

            if (world == null ||
                s_TransformGizmoToolType == null ||
                s_ModeType == null ||
                s_SetModeMethod == null)
            {
                return false;
            }

            object transformTool =
                world.GetOrCreateSystemManaged(
                    s_TransformGizmoToolType);

            if (transformTool == null)
            {
                return false;
            }

            try
            {
                object enumValue =
                    Enum.ToObject(
                        s_ModeType,
                        mode);

                s_SetModeMethod.Invoke(
                    transformTool,
                    new[] { enumValue });

                return true;
            }
            catch
            {
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