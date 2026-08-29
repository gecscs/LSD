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
    /// <summary>
    /// Handles LSD's independent marquee selection.
    /// </summary>
    public partial class LSDMarqueeSelectionSystem : GameSystemBase
    {
        private ToolSystem m_ToolSystem;
        private ILog m_Log;

        private ToolRaycastSystem m_ToolRaycastSystem;

        private Game.Objects.SearchSystem m_ObjectSearchSystem;

        private LayeredSelectionDisplayUISystem m_UISystem;

        private bool m_Active;

        private bool m_Dragging;

        private LSDMarquee m_Marquee;

        // Pending / last valid world position handling
        private bool m_PendingStart;
        private float2 m_PendingScreenPos;
        private bool m_HasLastValidWorldPos;
        private float3 m_LastValidWorldPos;

        public void StartSelection()
        {
            m_Log.Debug($"{nameof(LSDMarqueeSelectionSystem)}.{nameof(StartSelection)} called");
            m_Active = true;
            m_Dragging = false;
            m_Marquee = null;
            m_PendingStart = false;
            m_HasLastValidWorldPos = false;
        }

        public void CancelSelection()
        {
            m_Log.Debug($"{nameof(LSDMarqueeSelectionSystem)}.{nameof(CancelSelection)} called");
            m_Active = false;
            m_Dragging = false;
            m_Marquee = null;
            m_PendingStart = false;
            m_HasLastValidWorldPos = false;
        }

        public bool TryGetCurrentQuad(out Colossal.Mathematics.Quad2 quad)
        {
            if (m_Marquee is null)
            {
                quad = default;
                return false;
            }

            quad = m_Marquee.Quad;
            return true;
        }

        public bool IsDragging => m_Dragging;

        public bool TryGetMarquee(
            out Quad2 quad,
            out float height)
        {
            if (!m_Dragging ||
                m_Marquee is null)
            {
                quad = default;
                height = 0f;
                return false;
            }

            quad = m_Marquee.Quad;
            height = m_Marquee.StartPosition.y + 0.5f;
            return true;
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            m_Log = LayeredSelectionDisplayMod.Instance?.Logger;

            m_ToolSystem =
                World.GetOrCreateSystemManaged<ToolSystem>();

            m_ToolRaycastSystem =
                World.GetOrCreateSystemManaged<ToolRaycastSystem>();

            m_ObjectSearchSystem =
                World.GetOrCreateSystemManaged<Game.Objects.SearchSystem>();

            m_UISystem =
                World.GetOrCreateSystemManaged<LayeredSelectionDisplayUISystem>();
        }

        protected override void OnUpdate()
        {
            // m_Log?.Verbose($"{nameof(OnUpdate)} active={m_Active} dragging={m_Dragging} actionModeIsGame={m_ToolSystem?.actionMode.IsGame()} mouseExists={(Mouse.current != null)} leftPressed={Mouse.current?.leftButton?.isPressed}");
            if (!m_Active)
            {
                return;
            }

            if (!m_ToolSystem.actionMode.IsGame())
            {
                CancelSelection();
                return;
            }

            // Resolve pending start only when an authoritative raycast hit becomes available
            if (m_PendingStart && !m_Dragging)
            {
                if (TryGetRaycastHitPosition(out var resolvedPos))
                {
                    m_Marquee = new LSDMarquee(resolvedPos);
                    m_Dragging = true;
                    m_PendingStart = false;
                    m_HasLastValidWorldPos = true;
                    m_LastValidWorldPos = resolvedPos;
                    // m_Log?.Debug($"{nameof(OnUpdate)} resolved pending start to {resolvedPos}");
                }
            }

            Mouse mouse = Mouse.current;

            if (mouse == null)
            {
                return;
            }

            if (mouse.leftButton.wasPressedThisFrame)
            {
                // Try to start immediately with an authoritative hit; otherwise record pending start.
                if (TryGetRaycastHitPosition(out var hitPos))
                {
                    m_Marquee = new LSDMarquee(hitPos);
                    m_Dragging = true;
                    m_HasLastValidWorldPos = true;
                    m_LastValidWorldPos = hitPos;
                    // m_Log?.Debug($"{nameof(StartDrag)} marquee started at authoritative {hitPos}");
                }
                else
                {
                    var mp = InputManager.instance.mousePosition;
                    m_PendingStart = true;
                    m_PendingScreenPos = new float2(mp.x, mp.y);
                    // m_Log?.Debug($"{nameof(StartDrag)} pending start recorded screen {m_PendingScreenPos}");
                }

                return;
            }

            if (m_Dragging && mouse.leftButton.isPressed)
            {
                if (TryGetRaycastHitPosition(out float3 hitPosition))
                {
                    m_LastValidWorldPos = hitPosition;
                    m_HasLastValidWorldPos = true;
                }

                if (!m_HasLastValidWorldPos)
                {
                    return;
                }

                float cameraYaw =
                    Camera.main.transform.eulerAngles.y *
                    Mathf.Deg2Rad;

                m_Marquee.Update(
                    m_LastValidWorldPos,
                    cameraYaw);

                return;
            }

            if (m_Dragging && mouse.leftButton.wasReleasedThisFrame)
            {
                if (TryGetRaycastHitPosition(out float3 hitPosition))
                {
                    m_LastValidWorldPos = hitPosition;
                    m_HasLastValidWorldPos = true;
                }

                if (m_HasLastValidWorldPos)
                {
                    float cameraYaw =
                        Camera.main.transform.eulerAngles.y *
                        Mathf.Deg2Rad;

                    m_Marquee.Update(
                        m_LastValidWorldPos,
                        cameraYaw);
                }

                FinishDrag();
                return;
            }
        }

        private void StartDrag()
        {
            // This method kept for compatibility; actual start logic handled in OnUpdate
            // m_Log?.Debug($"{nameof(StartDrag)} called. Mouse: {Mouse.current != null}");
        }

        private void UpdateDrag()
        {
            // handled inline in OnUpdate
        }

        private void FinishDrag()
        {
            m_Log.Debug($"{nameof(LSDMarqueeSelectionSystem)}.{nameof(FinishDrag)} called");
            if (m_Marquee == null)
            {
                m_Log.Debug($"{nameof(LSDMarqueeSelectionSystem)}.{nameof(FinishDrag)} marquee is null");
                CancelSelection();
                return;
            }

            NativeList<Entity> candidates =
                new NativeList<Entity>(
                    Allocator.Temp);

            // m_Log.Debug($"{nameof(LSDMarqueeSelectionSystem)}.{nameof(FinishDrag)} candidates: {candidates}");

            try
            {
                // Expand bounds slightly to avoid missing narrow selections due to numerical edge cases.
                float cameraHeight = 0f;
                var cam = UnityEngine.Camera.main ?? UnityEngine.Camera.current;
                if (cam != null) cameraHeight = cam.transform.position.y;
                float expandMeters = math.max(0.5f, cameraHeight * 0.01f); // 1% of camera height, min 0.5m

                // Compute bounds from Quad directly to avoid depending on Bounds2 internals
                Quad2 q = m_Marquee.Quad;
                float2 min = math.min(math.min(q.a, q.b), math.min(q.c, q.d));
                float2 max = math.max(math.max(q.a, q.b), math.max(q.c, q.d));
                float2 expandedMin = min - new float2(expandMeters);
                float2 expandedMax = max + new float2(expandMeters);
                Bounds2 expandedBounds = new Bounds2(expandedMin, expandedMax);

                SearchObjects(
                    candidates,
                    expandedBounds,
                    m_Marquee.Quad);

                var entities =
                    new List<Entity>(
                        candidates.Length);

                for (int i = 0;
                     i < candidates.Length;
                     i++)
                {
                    Entity entity =
                        candidates[i];

                    if (!EntityManager.Exists(entity))
                    {
                        continue;
                    }

                    // m_Log.Debug($"{nameof(LSDMarqueeSelectionSystem)}.{nameof(FinishDrag)} entity found: {entity.ToString()}");

                    if (!EntityManager.MatchesLSDFilter(
                            entity,
                            m_UISystem.SelectedVanillaFilters))
                    {
                        continue;
                    }

                    entities.Add(entity);
                }

                if (entities.Count == 0)
                {
                    m_Log?.Debug($"{nameof(FinishDrag)} no entities found. ExpandedBounds={expandedBounds} Quad={m_Marquee.Quad}");
                }

                // m_Log.Debug($"{nameof(LSDMarqueeSelectionSystem)}.{nameof(FinishDrag)} entities found: {entities}");

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

            // The tree must be ready before it is traversed.
            dependencies.Complete();

            var iterator =
                new LSDMarqueeIterator
                {
                    Entities = entities,
                    OuterBounds = bounds,
                    SelectionQuad = quad,
                };

            staticTree.Iterate(ref iterator);
        }

        private bool TryGetMouseWorldPosition(out Unity.Mathematics.float3 worldPosition)
        {
            worldPosition = default;

            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse is null)
            {
                m_Log?.Debug("TryGetMouseWorldPosition: Mouse.current is null");
                return false;
            }

            var mp = mouse.position.ReadValue();
            Camera cam = UnityEngine.Camera.main;

            if (cam == null)
            {
                cam = UnityEngine.Camera.current;
                if (cam == null)
                {
                    m_Log?.Warn("TryGetMouseWorldPosition: no Camera available");
                    return false;
                }
            }

            // Use game input coords as fallback
            var ray = cam.ScreenPointToRay(new UnityEngine.Vector3(mp.x, mp.y, 0f));
            var plane = new UnityEngine.Plane(UnityEngine.Vector3.up, UnityEngine.Vector3.zero);
            if (plane.Raycast(ray, out float enter))
            {
                var p = ray.GetPoint(enter);
                worldPosition = new Unity.Mathematics.float3(p.x, p.y, p.z);
                return true;
            }

            m_Log?.Debug("TryGetMouseWorldPosition: raycast/plane failed");
            return false;
        }

        // Fast direct extraction of game's authoritative raycast hit position (m_Hit.m_Position)
        private bool TryGetRaycastHitPosition(out float3 worldPosition)
        {
            worldPosition = default;
            try
            {
                if (m_ToolRaycastSystem != null && m_ToolRaycastSystem.GetRaycastResult(out var rayResult))
                {
                    var rrType = rayResult.GetType();
                    var fHit = rrType.GetField("m_Hit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (fHit != null)
                    {
                        var hitObj = fHit.GetValue(rayResult);
                        if (hitObj != null)
                        {
                            var hitType = hitObj.GetType();
                            var fPos = hitType.GetField("m_Position", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            if (fPos != null)
                            {
                                var val = fPos.GetValue(hitObj);
                                if (TryConvertToFloat3(val, out worldPosition))
                                {
                                    return true;
                                }
                            }

                            // try alternative common field names inside m_Hit
                            var alt = hitType.GetField("m_Point", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                ?? hitType.GetField("point", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                ?? hitType.GetField("position", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            if (alt != null)
                            {
                                var aval = alt.GetValue(hitObj);
                                if (TryConvertToFloat3(aval, out worldPosition))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                m_Log?.Warn(ex, "TryGetRaycastHitPosition failed");
            }

            return false;
        }

        private static bool TryConvertToFloat3(object val, out Unity.Mathematics.float3 f)
        {
            f = default;
            if (val == null) return false;
            if (val is UnityEngine.Vector3 v)
            {
                f = new Unity.Mathematics.float3(v.x, v.y, v.z);
                return true;
            }

            if (val is Unity.Mathematics.float3 ff)
            {
                f = ff;
                return true;
            }

            var t = val.GetType();
            var fx = t.GetField("x", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var fy = t.GetField("y", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var fz = t.GetField("z", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            try
            {
                if (fx != null && fy != null && fz != null)
                {
                    object ox = fx.GetValue(val);
                    object oy = fy.GetValue(val);
                    object oz = fz.GetValue(val);
                    if (ox is float x && oy is float y && oz is float z)
                    {
                        f = new Unity.Mathematics.float3(x, y, z);
                        return true;
                    }

                    if (ox is double dx && oy is double dy && oz is double dz)
                    {
                        f = new Unity.Mathematics.float3((float)dx, (float)dy, (float)dz);
                        return true;
                    }
                }
            }
            catch { }

            return false;
        }

        public bool TryGetMarqueeStartY(out float y)
        {
            if (m_Marquee is null)
            {
                y = 0f;
                return false;
            }

            y = m_Marquee.StartPosition.y;
            return true;
        }
    }
}