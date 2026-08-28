namespace LayeredSelectionDisplay.Selection
{
    using System;
    using System.Collections.Generic;
    using Colossal.Collections;
    using Colossal.Entities;
    using Colossal.Mathematics;
    using Game.Areas;
    using Game.Common;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;

    internal struct LSDMarqueeIterator : IDisposable,
    INativeQuadTreeIterator<Entity, Game.Common.QuadTreeBoundsXZ>, IUnsafeQuadTreeIterator<Entity, Game.Common.QuadTreeBoundsXZ>,
    INativeQuadTreeIterator<AreaSearchItem, Game.Common.QuadTreeBoundsXZ>, IUnsafeQuadTreeIterator<AreaSearchItem, Game.Common.QuadTreeBoundsXZ>
    {
        public NativeList<Entity> Entities;

        public Bounds2 OuterBounds;

        public Quad2 SelectionQuad;

        public bool Intersect(QuadTreeBoundsXZ bounds)
        {
            // Return true when the query bounds intersect both our outer bounds and the selection quad.
            if (!MathUtils.Intersect(OuterBounds, bounds.m_Bounds.xz))
            {
                return false;
            }

            return MathUtils.Intersect(bounds.m_Bounds.xz, SelectionQuad);
        }

        public void Iterate(
            QuadTreeBoundsXZ bounds,
            Entity entity)
        {
            if (!MathUtils.Intersect(
                    OuterBounds,
                    bounds.m_Bounds.xz))
            {
                return;
            }

            if (!MathUtils.Intersect(bounds.m_Bounds.xz, SelectionQuad))
            {
                return;
            }

            Entities.Add(entity);
        }

        // Implement the interface member for AreaSearchItem
        public void Iterate(
            QuadTreeBoundsXZ bounds,
            AreaSearchItem item)
        {
            if (!MathUtils.Intersect(
                    OuterBounds,
                    bounds.m_Bounds.xz))
            {
                return;
            }

            if (!MathUtils.Intersect(bounds.m_Bounds.xz, SelectionQuad))
            {
                return;
            }

            // AreaSearchItem contains an Entity in m_Area; add that to the Entities list
            Entities.Add(item.m_Area);
        }

        public void Dispose()
        {
            // If this struct owns the NativeList, dispose it here:
            // Entities.Dispose();
        }
    }
}