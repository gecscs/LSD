namespace LayeredSelectionDisplay.Systems
{
    using System.Collections.Generic;
    using Colossal.Logging;
    using Colossal.Mathematics;
    using Game;
    using Game.Common;
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

        public void StartSelection()
        {
            m_Log.Debug($"{nameof(LSDMarqueeSelectionSystem)}.{nameof(StartSelection)} called");
            m_Active = true;
            m_Dragging = false;
            m_Marquee = null;
        }

        public void CancelSelection()
        {
            m_Log.Debug($"{nameof(LSDMarqueeSelectionSystem)}.{nameof(CancelSelection)} called");
            m_Active = false;
            m_Dragging = false;
            m_Marquee = null;
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
            // add in OnUpdate near the actionMode check
            m_Log?.Verbose($"{nameof(OnUpdate)} active={m_Active} dragging={m_Dragging} actionModeIsGame={m_ToolSystem?.actionMode.IsGame()} mouseExists={(Mouse.current != null)} leftPressed={Mouse.current?.leftButton?.isPressed}");
            if (!m_Active)
            {
                return;
            }

            if (!m_ToolSystem.actionMode.IsGame())
            {
                CancelSelection();
                return;
            }

            Mouse mouse = Mouse.current;

            if (mouse == null)
            {
                return;
            }

            if (mouse.leftButton.wasPressedThisFrame)
            {
                StartDrag();

                return;
            }

            if (m_Dragging && mouse.leftButton.isPressed)
            {
                UpdateDrag();

                return;
            }

            if (m_Dragging && mouse.leftButton.wasReleasedThisFrame)
            {
                FinishDrag();
            }
        }

        private void StartDrag()
        {
            m_Log?.Debug($"{nameof(StartDrag)} called. Mouse: {Mouse.current != null}");
            if (!TryGetMouseWorldPosition(out float3 position))
            {
                m_Log?.Warn($"{nameof(StartDrag)} TryGetMouseWorldPosition returned false - cannot start marquee.");
                return;
            }

            m_Marquee = new LSDMarquee(position);
            m_Dragging = true;
            m_Log?.Debug($"{nameof(StartDrag)} marquee started at {position}");
        }

        private void UpdateDrag()
        {
            if (!TryGetMouseWorldPosition(
                    out float3 position))
            {
                return;
            }

            float cameraYaw =
                UnityEngine.Camera.main
                    .transform
                    .eulerAngles
                    .y *
                Mathf.Deg2Rad;

            m_Marquee.Update(
                position,
                cameraYaw);
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

            m_Log.Debug($"{nameof(LSDMarqueeSelectionSystem)}.{nameof(FinishDrag)} candidates: {candidates}");

            try
            {
                SearchObjects(
                    candidates,
                    m_Marquee.Bounds,
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

                    m_Log.Debug($"{nameof(LSDMarqueeSelectionSystem)}.{nameof(FinishDrag)} entity found: {entity.ToString()}");

                    if (!EntityManager.MatchesLSDFilter(
                            entity,
                            m_UISystem.SelectedVanillaFilters))
                    {
                        continue;
                    }

                    entities.Add(entity);
                }

                m_Log.Debug($"{nameof(LSDMarqueeSelectionSystem)}.{nameof(FinishDrag)} entities found: {entities}");

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

            var staticTree = m_ObjectSearchSystem.GetStaticSearchTree(true, out dependencies);

            var iterator = new LSDMarqueeIterator
                {
                    Entities = entities,
                    OuterBounds = bounds,
                    SelectionQuad = quad,
                };

            staticTree.Iterate(ref iterator);

            dependencies.Complete();
        }

        private bool TryGetMouseWorldPosition(out Unity.Mathematics.float3 worldPosition)
        {
            worldPosition = default;

            try
            {
                var mouse = UnityEngine.InputSystem.Mouse.current;
                if (mouse is null)
                {
                    m_Log?.Debug("TryGetMouseWorldPosition: Mouse.current is null");
                    return false;
                }

                var mp = mouse.position.ReadValue();
                var cam = UnityEngine.Camera.main;
                if (cam is null)
                {
                    m_Log?.Warn("TryGetMouseWorldPosition: Camera.main is null");
                    return false;
                }

                var ray = cam.ScreenPointToRay(new UnityEngine.Vector3(mp.x, mp.y, 0f));
                var plane = new UnityEngine.Plane(UnityEngine.Vector3.up, UnityEngine.Vector3.zero);

                if (plane.Raycast(ray, out float enter))
                {
                    var pt = ray.GetPoint(enter);
                    worldPosition = new Unity.Mathematics.float3(pt.x, pt.y, pt.z);
                    return true;
                }

                m_Log?.Debug("TryGetMouseWorldPosition: ray did not hit plane");
                return false;
            }
            catch (System.Exception ex)
            {
                m_Log?.Error(ex, "TryGetMouseWorldPosition: exception");
                return false;
            }
        }
    }
}