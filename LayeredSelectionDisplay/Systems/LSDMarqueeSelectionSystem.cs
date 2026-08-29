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

        /*
         * Dedicated terrain-only raycast, independent of ToolRaycastSystem
         * and LSD's own DefaultToolSystemInitializeRaycastPatch, which
         * reconfigures ToolRaycastSystem.typeMask for hover-filtering
         * purposes and does not guarantee TypeMask.Terrain stays set.
         * See LSDTerrainRaycast for the full rationale.
         */
        private LSDTerrainRaycast m_TerrainRaycast;

        private LSDMarquee m_Marquee;

        private bool m_Active;

        private bool m_Dragging;

        /*
         * Last valid terrain/world position.
         *
         * Once a drag starts, this is deliberately retained when a
         * raycast temporarily fails.
         */
        private float3 m_LastValidWorldPosition;

        private bool m_HasLastValidWorldPosition;

        /*
         * Raycast result sampled exactly once per OnUpdate.
         */
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

            m_Dragging = false;

            m_Marquee = null;

            m_HasLastValidWorldPosition = false;

            m_HasFrameWorldPosition = false;

            RefreshCamera();
        }

        public void CancelSelection()
        {
            m_Active = false;

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

            /*
             * ----------------------------------------------------------
             * SAMPLE THE WORLD POSITION ONCE
             * ----------------------------------------------------------
             *
             * This is important.
             *
             * We register this frame's terrain raycast input first, then
             * read back the previously-completed result. There is exactly
             * one raycast registration and one result read per frame, and
             * it is entirely our own dedicated terrain-only raycast - not
             * shared with, or affected by, ToolRaycastSystem's active-tool
             * configuration or LSD's own filter-driven raycast patches.
             */
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

            /*
             * ----------------------------------------------------------
             * WAITING FOR MOUSE DOWN
             * ----------------------------------------------------------
             */
            if (!m_Dragging)
            {
                if (!mouse.leftButton.wasPressedThisFrame)
                {
                    return;
                }

                /*
                 * We cannot start a world-space marquee without a
                 * resolved world position.
                 *
                 * Crucially, we simply wait for the next frame.
                 * We do NOT create a fake position and we do NOT cancel.
                 */
                if (!m_HasFrameWorldPosition)
                {
                    return;
                }

                StartMarquee(
                    m_FrameWorldPosition);

                return;
            }

            /*
             * ----------------------------------------------------------
             * DRAGGING
             * ----------------------------------------------------------
             */

            /*
             * Mouse release ends the marquee.
             *
             * We deliberately do this BEFORE any other state changes.
             */
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

            /*
             * If the button is still down, continue dragging.
             */
            if (!mouse.leftButton.isPressed)
            {
                return;
            }

            /*
             * A failed raycast is NOT a drag cancellation.
             *
             * We simply keep the last valid position.
             */
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