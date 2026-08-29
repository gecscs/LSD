namespace LayeredSelectionDisplay.Systems
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using Colossal.Logging;
    using Colossal.Mathematics;

    using Game;
    using Game.Common;
    using Game.Input;
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
        private ToolSystem m_ToolSystem;

        private ToolRaycastSystem m_ToolRaycastSystem;

        private Game.Objects.SearchSystem m_ObjectSearchSystem;

        private LayeredSelectionDisplayUISystem m_UISystem;

        private ILog m_Log;

        private Camera m_Camera;

        private bool m_Active;

        private bool m_Dragging;

        private LSDMarquee m_Marquee;

        private bool m_PendingStart;

        private bool m_HasLastValidWorldPos;

        private float3 m_LastValidWorldPos;

        public bool IsDragging =>
            m_Dragging;

        protected override void OnCreate()
        {
            base.OnCreate();

            m_Log =
                LayeredSelectionDisplayMod.Instance?.Logger;

            m_ToolSystem =
                World.GetOrCreateSystemManaged<
                    ToolSystem>();

            m_ToolRaycastSystem =
                World.GetOrCreateSystemManaged<
                    ToolRaycastSystem>();

            m_ObjectSearchSystem =
                World.GetOrCreateSystemManaged<
                    Game.Objects.SearchSystem>();

            m_UISystem =
                World.GetOrCreateSystemManaged<
                    LayeredSelectionDisplayUISystem>();

            m_Camera =
                Camera.main;
        }

        public void StartSelection()
        {
            m_Log?.Debug(
                $"{nameof(LSDMarqueeSelectionSystem)}.{nameof(StartSelection)}");

            m_Active = true;

            m_Dragging = false;

            m_Marquee = null;

            m_PendingStart = false;

            m_HasLastValidWorldPos = false;

            RefreshCamera();
        }

        public void CancelSelection()
        {
            m_Log?.Debug(
                $"{nameof(LSDMarqueeSelectionSystem)}.{nameof(CancelSelection)}");

            m_Active = false;

            m_Dragging = false;

            m_Marquee = null;

            m_PendingStart = false;

            m_HasLastValidWorldPos = false;
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

            /*
             * Keep the marquee slightly above the ground to prevent
             * z-fighting with terrain.
             */
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

            if (m_ToolSystem == null ||
                !m_ToolSystem.actionMode.IsGame())
            {
                CancelSelection();
                return;
            }

            Mouse mouse =
                Mouse.current;

            if (mouse == null)
            {
                return;
            }

            bool isPressed =
                mouse.leftButton.isPressed;

            /*
             * ----------------------------------------------------------
             * Waiting for mouse-down / raycast
             * ----------------------------------------------------------
             */
            if (!m_Dragging)
            {
                if (m_PendingStart)
                {
                    if (!isPressed)
                    {
                        m_PendingStart = false;
                        return;
                    }

                    if (TryGetRaycastHitPosition(
                            out float3 resolvedPosition))
                    {
                        StartMarquee(
                            resolvedPosition);
                    }

                    return;
                }

                if (!mouse.leftButton.wasPressedThisFrame)
                {
                    return;
                }

                if (TryGetRaycastHitPosition(
                        out float3 hitPosition))
                {
                    StartMarquee(
                        hitPosition);
                }
                else
                {
                    /*
                     * ToolRaycastSystem has not yet produced the hit
                     * for this frame. Wait for the next update.
                     */
                    m_PendingStart = true;
                }

                return;
            }

            /*
             * ----------------------------------------------------------
             * Active drag
             * ----------------------------------------------------------
             */
            if (isPressed)
            {
                if (TryGetRaycastHitPosition(
                        out float3 hitPosition))
                {
                    m_LastValidWorldPos =
                        hitPosition;

                    m_HasLastValidWorldPos =
                        true;
                }

                if (!m_HasLastValidWorldPos)
                {
                    return;
                }

                UpdateMarquee(
                    m_LastValidWorldPos);

                return;
            }

            /*
             * ----------------------------------------------------------
             * Mouse released
             * ----------------------------------------------------------
             */
            if (m_HasLastValidWorldPos)
            {
                UpdateMarquee(
                    m_LastValidWorldPos);
            }

            FinishDrag();
        }

        private void StartMarquee(
            float3 position)
        {
            RefreshCamera();

            m_Marquee =
                new LSDMarquee(
                    position);

            m_Dragging = true;

            m_PendingStart = false;

            m_HasLastValidWorldPos = true;

            m_LastValidWorldPos =
                position;
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

            /*
             * Only read the camera yaw.
             *
             * The expensive work of actually writing the overlay is
             * no longer performed here.
             */
            float cameraYaw =
                m_Camera.transform.eulerAngles.y *
                Mathf.Deg2Rad;

            m_Marquee.Update(
                position,
                cameraYaw);
        }

        private void RefreshCamera()
        {
            /*
             * Camera.main is only used when we don't currently have
             * a valid camera reference.
             */
            if (m_Camera != null)
            {
                return;
            }

            m_Camera =
                Camera.main ??
                Camera.current;
        }

        private void FinishDrag()
        {
            m_Log?.Debug(
                $"{nameof(LSDMarqueeSelectionSystem)}.{nameof(FinishDrag)}");

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

                float cameraHeight = 0f;

                if (m_Camera != null)
                {
                    cameraHeight =
                        m_Camera.transform.position.y;
                }

                /*
                 * Slightly expand the quadtree query bounds so narrow
                 * objects near the edge aren't lost due to floating
                 * point precision.
                 */
                float expandMeters =
                    math.max(
                        0.5f,
                        cameraHeight * 0.01f);

                float2 expandedMin =
                    bounds.min -
                    new float2(
                        expandMeters);

                float2 expandedMax =
                    bounds.max +
                    new float2(
                        expandMeters);

                Bounds2 expandedBounds =
                    new Bounds2(
                        expandedMin,
                        expandedMax);

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

                    if (!EntityManager.MatchesLSDFilter(
                            entity,
                            m_UISystem.SelectedVanillaFilters))
                    {
                        continue;
                    }

                    entities.Add(
                        entity);
                }

                m_UISystem.SetMarqueeEntities(
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
                m_ObjectSearchSystem.GetStaticSearchTree(
                    true,
                    out dependencies);

            /*
             * This happens only when the drag finishes, not while
             * the marquee is moving.
             */
            dependencies.Complete();

            LSDMarqueeIterator iterator =
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

        private bool TryGetRaycastHitPosition(
            out float3 worldPosition)
        {
            worldPosition = default;

            try
            {
                if (m_ToolRaycastSystem == null)
                {
                    return false;
                }

                if (!m_ToolRaycastSystem.GetRaycastResult(
                        out var rayResult))
                {
                    return false;
                }

                return TryConvertToFloat3(
                    rayResult.m_Hit.m_Position,
                    out worldPosition);
            }
            catch (Exception ex)
            {
                m_Log?.Warn(
                    ex,
                    "TryGetRaycastHitPosition failed.");

                return false;
            }
        }

        private static bool TryConvertToFloat3(
            object value,
            out float3 result)
        {
            result = default;

            if (value == null)
            {
                return false;
            }

            if (value is Vector3 vector)
            {
                result =
                    new float3(
                        vector.x,
                        vector.y,
                        vector.z);

                return true;
            }

            if (value is float3 mathVector)
            {
                result =
                    mathVector;

                return true;
            }

            /*
             * Reflection is only a fallback for unknown vector-like
             * types. It should not be reached for the normal CS2
             * raycast position type.
             */
            Type type =
                value.GetType();

            FieldInfo fx =
                type.GetField(
                    "x",
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

            FieldInfo fy =
                type.GetField(
                    "y",
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

            FieldInfo fz =
                type.GetField(
                    "z",
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

            if (fx == null ||
                fy == null ||
                fz == null)
            {
                return false;
            }

            try
            {
                object ox =
                    fx.GetValue(value);

                object oy =
                    fy.GetValue(value);

                object oz =
                    fz.GetValue(value);

                if (ox is float x &&
                    oy is float y &&
                    oz is float z)
                {
                    result =
                        new float3(
                            x,
                            y,
                            z);

                    return true;
                }

                if (ox is double dx &&
                    oy is double dy &&
                    oz is double dz)
                {
                    result =
                        new float3(
                            (float)dx,
                            (float)dy,
                            (float)dz);

                    return true;
                }
            }
            catch
            {
                // Ignore malformed vector-like values.
            }

            return false;
        }
    }
}