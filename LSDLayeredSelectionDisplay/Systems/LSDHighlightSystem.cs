

namespace LSD_Layered_Selection_Display.Systems
{
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Game.Prefabs;
    using Game.Rendering;
    using Game.Tools;
    using LSD_Layered_Selection_Display.Domain;
    using Unity.Entities;

    public partial class LSDHighlightSystem : GameSystemBase
    {
        private const string ModId = "LSDLayeredSelectionDisplay";
        private ILog m_Log;

        private ToolOutputBarrier m_ToolOutputBarrier;

        private HoverState m_HoverState;

        private Entity m_PreviousHovered = Entity.Null;

        /// <summary>
        /// Initializes the system and retrieves the necessary dependencies.
        /// </summary>
        protected override void OnCreate()
        {
            base.OnCreate(); 
            m_Log = LSDLayeredSelectionDisplayMod.Instance.Logger;
            m_Log.Info($"{nameof(LSDHighlightSystem)}.{nameof(OnCreate)}");

            m_ToolOutputBarrier =
                World.GetOrCreateSystemManaged<ToolOutputBarrier>();

            m_HoverState =
                World.GetOrCreateSystemManaged<LSDLayeredSelectionDisplayUISystem>()
                     .HoverState;
        }

        /// <summary>
        /// Updates the highlight state based on the current hovered entity.
        /// </summary>
        protected override void OnUpdate()
        {
            // base.OnUpdate();
            if (!m_HoverState.Dirty)
            {
                // m_Log.Debug($"{nameof(LSDHighlightSystem)}.{nameof(OnUpdate)} - No changes to highlight");
                return;
            }

            // m_Log.Debug($"{nameof(LSDHighlightSystem)}.{nameof(OnUpdate)} - Starting highlight update");

            m_HoverState.Dirty = false;

            EntityCommandBuffer ecb =
                m_ToolOutputBarrier.CreateCommandBuffer();

            // Remove previous highlight
            // m_Log.Debug($"{nameof(LSDHighlightSystem)}.{nameof(OnUpdate)} - Removing previous highlight");
            if (m_PreviousHovered != Entity.Null &&
                EntityManager.Exists(m_PreviousHovered))
            {
                if (EntityManager.HasComponent<Highlighted>(m_PreviousHovered))
                {
                    ecb.RemoveComponent<Highlighted>(m_PreviousHovered);
                }

                ecb.AddComponent<BatchesUpdated>(m_PreviousHovered);
            }

            // Add new highlight
            // m_Log.Debug($"{nameof(LSDHighlightSystem)}.{nameof(OnUpdate)} - Adding new highlight");
            Entity current = m_HoverState.HoveredEntity;

            if (current != Entity.Null &&
                EntityManager.Exists(current))
            {
                if (!EntityManager.HasComponent<Highlighted>(current))
                {
                    ecb.AddComponent<Highlighted>(current);
                }

                ecb.AddComponent<BatchesUpdated>(current);
            }

            m_PreviousHovered = current;
        }
    }
}