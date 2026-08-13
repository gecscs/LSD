// <copyright file="LayeredSelectionDisplayHighlightSystem.cs" company="0belix's Mods. MIT License">
// Copyright (c) 0belix's Mods. MIT License. All rights reserved.
// </copyright>

// #define VERBOSE
namespace LayeredSelectionDisplay.Systems
{
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Game.Tools;
    using LayeredSelectionDisplay.Domain;
    using Unity.Entities;

    /// <summary>
    /// System that handles the highlighting of entities based on UI inputs.
    /// </summary>
    public partial class LayeredSelectionDisplayHighlightSystem : GameSystemBase
    {
        private const string ModId = "LayeredSelectionDisplay";
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
            m_Log = LayeredSelectionDisplayMod.Instance.Logger;
            m_Log.Info($"{nameof(LayeredSelectionDisplayHighlightSystem)}.{nameof(OnCreate)}");

            m_ToolOutputBarrier =
                World.GetOrCreateSystemManaged<ToolOutputBarrier>();

            m_HoverState =
                World.GetOrCreateSystemManaged<LayeredSelectionDisplayUISystem>()
                     .HoverState;
        }

        /// <summary>
        /// Updates the highlight state based on the current hovered entity.
        /// </summary>
        protected override void OnUpdate()
        {
            if (!m_HoverState.Dirty)
            {
                return;
            }

            m_HoverState.Dirty = false;

            EntityCommandBuffer ecb = m_ToolOutputBarrier.CreateCommandBuffer();

            // Remove previous highlight
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