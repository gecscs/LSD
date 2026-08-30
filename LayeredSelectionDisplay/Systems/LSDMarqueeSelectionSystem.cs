namespace LayeredSelectionDisplay.Systems
{
    using System.Collections.Generic;
    using Colossal.Logging;
    using Colossal.Mathematics;
    using Game;
    using Game.Objects;
    using Game.Tools;
    using LayeredSelectionDisplay.Extensions;
    using LayeredSelectionDisplay.Selection;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using UnityEngine;
    using UnityEngine.InputSystem;

    [UpdateAfter(typeof(ToolRaycastSystem))]
    public partial class LSDMarqueeSelectionSystem :
        GameSystemBase
    {
        private Game.Objects.SearchSystem
            m_ObjectSearchSystem;

        private LayeredSelectionDisplayUISystem
            m_UISystem;

        private ILog m_Log;

        private Camera m_Camera;

        private LSDTerrainRaycast m_TerrainRaycast;

        private LSDMarquee m_Marquee;

        private bool m_Active;

        private bool m_Dragging;

        private float3 m_LastValidWorldPosition;

        private bool m_HasLastValidWorldPosition;

        private float3 m_FrameWorldPosition;

        private bool m_HasFrameWorldPosition;

        public bool IsDragging =>
            m_Dragging;

        protected override void OnCreate()
        {
            base.OnCreate();

            m_Log =
                LayeredSelectionDisplayMod
                    .Instance?
                    .Logger;

            m_ObjectSearchSystem =
                World.GetOrCreateSystemManaged<
                    Game.Objects.SearchSystem>();

            m_UISystem =
                World.GetOrCreateSystemManaged<
                    LayeredSelectionDisplayUISystem>();

            m_TerrainRaycast =
                new LSDTerrainRaycast(World);

            m_Camera =
                Camera.main;
        }

        public void StartSelection()
        {
            m_Active = true;

            m_UISystem.SetMarqueeToolState(true);

            m_Dragging = false;

            m_Marquee = null;

            m_HasLastValidWorldPosition = false;

            m_HasFrameWorldPosition = false;

            RefreshCamera();
        }

        public void CancelSelection()
        {
            m_Active = false;

            m_UISystem.SetMarqueeToolState(false);

            m_Dragging = false;

            m_Marquee = null;

            m_HasLastValidWorldPosition = false;

            m_HasFrameWorldPosition = false;
        }

        public bool TryGetCurrentQuad(
            out Quad2 quad)
        {
            if (m_Marquee == null)
            {
                quad = default;

                return false;
            }

            quad =
                m_Marquee.Quad;

            return true;
        }

        public bool TryGetMarquee(
            out Quad2 quad,
            out float height)
        {
            if (!m_Dragging ||
                m_Marquee == null)
            {
                quad = default;

                height = 0f;

                return false;
            }

            quad =
                m_Marquee.Quad;

            height =
                m_Marquee.StartPosition.y +
                0.5f;

            return true;
        }

        public bool TryGetMarqueeStartY(
            out float y)
        {
            if (m_Marquee == null)
            {
                y = 0f;

                return false;
            }

            y =
                m_Marquee.StartPosition.y;

            return true;
        }

        protected override void OnUpdate()
        {
            if (!m_Active)
            {
                return;
            }

            Mouse mouse =
                Mouse.current;

            if (mouse == null)
            {
                return;
            }

            m_TerrainRaycast.Update();

            m_HasFrameWorldPosition =
                m_TerrainRaycast.TryGetHitPosition(
                    out m_FrameWorldPosition);

            if (m_HasFrameWorldPosition)
            {
                m_LastValidWorldPosition =
                    m_FrameWorldPosition;

                m_HasLastValidWorldPosition =
                    true;
            }

            if (!m_Dragging)
            {
                if (!mouse.leftButton.wasPressedThisFrame)
                {
                    return;
                }

                if (!m_HasFrameWorldPosition)
                {
                    return;
                }

                StartMarquee(
                    m_FrameWorldPosition);

                return;
            }

            if (mouse.leftButton.wasReleasedThisFrame)
            {
                if (m_HasLastValidWorldPosition)
                {
                    UpdateMarquee(
                        m_LastValidWorldPosition);
                }

                FinishDrag();

                return;
            }

            if (!mouse.leftButton.isPressed)
            {
                return;
            }

            if (m_HasFrameWorldPosition)
            {
                UpdateMarquee(
                    m_FrameWorldPosition);
            }
        }

        private void StartMarquee(
            float3 position)
        {
            RefreshCamera();

            m_Marquee =
                new LSDMarquee(
                    position);

            m_Dragging =
                true;

            m_LastValidWorldPosition =
                position;

            m_HasLastValidWorldPosition =
                true;
        }

        private void UpdateMarquee(
            float3 position)
        {
            if (m_Marquee == null)
            {
                return;
            }

            RefreshCamera();

            if (m_Camera == null)
            {
                return;
            }

            float cameraYaw =
                m_Camera.transform
                    .eulerAngles.y *
                Mathf.Deg2Rad;

            m_Marquee.Update(
                position,
                cameraYaw);
        }

        private void RefreshCamera()
        {
            if (m_Camera == null)
            {
                m_Camera =
                    Camera.main ??
                    Camera.current;
            }
        }

        private void FinishDrag()
        {
            if (m_Marquee == null)
            {
                CancelSelection();

                return;
            }

            NativeList<Entity> candidates =
                new NativeList<Entity>(
                    Allocator.Temp);

            try
            {
                Quad2 quad =
                    m_Marquee.Quad;

                Bounds2 bounds =
                    m_Marquee.Bounds;

                RefreshCamera();

                float cameraHeight =
                    m_Camera != null
                        ? m_Camera.transform.position.y
                        : 0f;

                float expandMeters =
                    math.max(
                        0.5f,
                        cameraHeight * 0.01f);

                Bounds2 expandedBounds =
                    new Bounds2(
                        bounds.min -
                        new float2(
                            expandMeters),

                        bounds.max +
                        new float2(
                            expandMeters));

                SearchObjects(
                    candidates,
                    expandedBounds,
                    quad);

                List<Entity> entities =
                    new List<Entity>(
                        candidates.Length);

                for (int i = 0;
                     i < candidates.Length;
                     i++)
                {
                    Entity entity =
                        candidates[i];

                    if (!EntityManager.Exists(
                            entity))
                    {
                        continue;
                    }

                    if (!EntityManager
                        .MatchesLSDFilter(
                            entity,
                            m_UISystem
                                .SelectedVanillaFilters))
                    {
                        continue;
                    }

                    entities.Add(
                        entity);
                }

                m_UISystem
                    .SetMarqueeEntities(
                        entities);
            }
            finally
            {
                candidates.Dispose();

                CancelSelection();
            }
        }

        private void SearchObjects(
            NativeList<Entity> entities,
            Bounds2 bounds,
            Quad2 quad)
        {
            JobHandle dependencies;

            var staticTree =
                m_ObjectSearchSystem
                    .GetStaticSearchTree(
                        true,
                        out dependencies);

            dependencies.Complete();

            var iterator =
                new LSDMarqueeIterator
                {
                    Entities =
                        entities,

                    OuterBounds =
                        bounds,

                    SelectionQuad =
                        quad
                };

            staticTree.Iterate(
                ref iterator);
        }
    }
}