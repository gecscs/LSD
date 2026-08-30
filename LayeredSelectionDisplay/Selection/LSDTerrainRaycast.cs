namespace LayeredSelectionDisplay.Selection
{
    using Game.Common;
    using Game.Tools;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

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