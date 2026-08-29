namespace LayeredSelectionDisplay.Selection
{
    using Game.Common;
    using Game.Tools;

    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;

    using UnityEngine;

    /// <summary>
    /// Dedicated terrain-only raycast for the LSD marquee.
    ///
    /// Unlike ToolRaycastSystem.GetRaycastResult(), which reflects whatever
    /// TypeMask/flags the currently active tool has configured for the
    /// frame (and which LSD itself actively mutates via
    /// DefaultToolSystemInitializeRaycastPatch to drive hover filtering),
    /// this raycaster registers its own RaycastInput requesting ONLY
    /// TypeMask.Terrain, every frame, completely independent of whatever
    /// the active tool or LSD's own filter patches are doing.
    ///
    /// This mirrors Move It's RaycastBase/RaycastTerrain pattern, with one
    /// deliberate difference: Move It registers its input once (from the
    /// constructor) and relies on the tool's own InitializeRaycast()
    /// override, invoked every frame by ToolRaycastSystem while Move It is
    /// the active tool, to keep it alive. LSD's marquee is NOT the active
    /// tool (it runs layered on top of whatever tool is active), so there
    /// is no InitializeRaycast() hook driving it - Update() must be called
    /// explicitly once per frame to keep the registration current.
    /// </summary>
    internal sealed class LSDTerrainRaycast
    {
        private readonly RaycastSystem m_RaycastSystem;

        private readonly object m_Owner;

        public LSDTerrainRaycast(World world)
        {
            m_RaycastSystem =
                world.GetOrCreateSystemManaged<RaycastSystem>();

            m_Owner = this;
        }

        /// <summary>
        /// Registers this frame's terrain raycast input. Must be called
        /// once per frame (every frame the marquee system is active),
        /// before reading TryGetHitPosition.
        /// </summary>
        public void Update()
        {
            Camera camera = Camera.main;

            if (camera == null)
            {
                return;
            }

            RaycastInput input = new RaycastInput
            {
                m_Line = ToolRaycastSystem.CalculateRaycastLine(camera),
                m_Offset = default,
                m_TypeMask = TypeMask.Terrain,
            };

            m_RaycastSystem.AddInput(m_Owner, input);
        }

        /// <summary>
        /// Reads back the terrain hit position from this raycaster's most
        /// recently completed result. Because RaycastSystem processes
        /// inputs with a one-frame pipeline (see RaycastSystem.OnUpdate),
        /// this reflects the result of the PREVIOUS Update() call, not
        /// necessarily the one just made this frame - call Update() first,
        /// then TryGetHitPosition(), every frame, for a steady stream of
        /// one-frame-latent but consistent terrain hits.
        /// </summary>
        public bool TryGetHitPosition(out float3 position)
        {
            position = default;

            NativeArray<RaycastResult> results =
                m_RaycastSystem.GetResult(m_Owner);

            if (!results.IsCreated || results.Length == 0)
            {
                return false;
            }

            float3 hit = results[0].m_Hit.m_HitPosition;

            if (!math.all(math.isfinite(hit)) ||
                hit.x == float.MaxValue ||
                hit.y == float.MaxValue ||
                hit.z == float.MaxValue)
            {
                return false;
            }

            position = hit;

            return true;
        }
    }
}