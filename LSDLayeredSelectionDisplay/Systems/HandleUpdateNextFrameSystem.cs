// <copyright file="HandleUpdateNextFrameSystem.cs" company="0belix's Mods. MIT License">
// Copyright (c) 0belix's Mods. MIT License. All rights reserved.
// </copyright>

namespace LSD_Layered_Selection_Display.Systems
{
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using LSD_Layered_Selection_Display;
    using LSD_Layered_Selection_Display.Components;
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// Adds updated to applicable entities that need a delayed update.
    /// </summary>
    public partial class HandleUpdateNextFrameSystem : GameSystemBase
    {
        private ILog m_Log;
        private EntityQuery m_UpdateNextFrameQuery;
        private ModificationBarrier5 m_Barrier;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandleUpdateNextFrameSystem"/> class.
        /// </summary>
        public HandleUpdateNextFrameSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = LSDLayeredSelectionDisplayMod.Instance.Logger;
            m_Log.Info($"{nameof(HandleUpdateNextFrameSystem)} Created.");
            m_UpdateNextFrameQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
               {
                    ComponentType.ReadOnly<UpdateNextFrame>(),
               },
                None = new ComponentType[]
               {
                    ComponentType.ReadOnly<Deleted>(),
                    ComponentType.ReadOnly<Updated>(),
                    ComponentType.ReadOnly<DeleteInXFrames>(),
               },
            });
            m_Barrier = World.GetOrCreateSystemManaged<ModificationBarrier5>();
            RequireAnyForUpdate(m_UpdateNextFrameQuery);
            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            EntityCommandBuffer buffer = m_Barrier.CreateCommandBuffer();
            NativeArray<Entity> updateNextFrameEntities = m_UpdateNextFrameQuery.ToEntityArray(Allocator.Temp);
            buffer.AddComponent<Updated>(updateNextFrameEntities);
            buffer.RemoveComponent<UpdateNextFrame>(updateNextFrameEntities);
        }
    }
}
