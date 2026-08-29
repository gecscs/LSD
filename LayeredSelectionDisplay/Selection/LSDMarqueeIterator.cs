namespace LayeredSelectionDisplay.Selection
{
    using System;

    using Colossal.Collections;
    using Colossal.Mathematics;

    using Game.Areas;
    using Game.Common;

    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// Collects entities whose QuadTree bounds intersect
    /// the marquee selection quad.
    /// </summary>
    internal struct LSDMarqueeIterator :
        IDisposable,
        INativeQuadTreeIterator<Entity, QuadTreeBoundsXZ>,
        IUnsafeQuadTreeIterator<Entity, QuadTreeBoundsXZ>,
        INativeQuadTreeIterator<AreaSearchItem, QuadTreeBoundsXZ>,
        IUnsafeQuadTreeIterator<AreaSearchItem, QuadTreeBoundsXZ>
    {
        public NativeList<Entity> Entities;

        public Bounds2 OuterBounds;

        public Quad2 SelectionQuad;

        public bool Intersect(QuadTreeBoundsXZ bounds)
        {
            Bounds2 bounds2 = bounds.m_Bounds.xz;

            if (!MathUtils.Intersect(OuterBounds, bounds2))
            {
                return false;
            }

            return MathUtils.Intersect(bounds2, SelectionQuad);
        }

        public void Iterate(
            QuadTreeBoundsXZ bounds,
            Entity entity)
        {
            Bounds2 bounds2 = bounds.m_Bounds.xz;

            if (!MathUtils.Intersect(OuterBounds, bounds2))
            {
                return;
            }

            if (!MathUtils.Intersect(bounds2, SelectionQuad))
            {
                return;
            }

            Entities.Add(entity);
        }

        public void Iterate(
            QuadTreeBoundsXZ bounds,
            AreaSearchItem item)
        {
            Bounds2 bounds2 = bounds.m_Bounds.xz;

            if (!MathUtils.Intersect(OuterBounds, bounds2))
            {
                return;
            }

            if (!MathUtils.Intersect(bounds2, SelectionQuad))
            {
                return;
            }

            Entities.Add(item.m_Area);
        }

        public void Dispose()
        {
            // The owner of the NativeList is responsible for disposing it.
        }
    }
}