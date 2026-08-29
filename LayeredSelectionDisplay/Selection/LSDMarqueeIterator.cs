namespace LayeredSelectionDisplay.Selection
{
    using System;

    using Colossal.Collections;
    using Colossal.Mathematics;

    using Game.Areas;
    using Game.Common;

    using Unity.Collections;
    using Unity.Entities;

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

        public bool Intersect(
            QuadTreeBoundsXZ bounds)
        {
            Bounds2 objectBounds =
                bounds.m_Bounds.xz;

            return
                MathUtils.Intersect(
                    OuterBounds,
                    objectBounds) &&
                MathUtils.Intersect(
                    objectBounds,
                    SelectionQuad);
        }

        public void Iterate(
            QuadTreeBoundsXZ bounds,
            Entity entity)
        {
            if (!Intersect(bounds))
            {
                return;
            }

            Entities.Add(entity);
        }

        public void Iterate(
            QuadTreeBoundsXZ bounds,
            AreaSearchItem item)
        {
            if (!Intersect(bounds))
            {
                return;
            }

            Entities.Add(
                item.m_Area);
        }

        public void Dispose()
        {
            /*
             * The owner of the NativeList disposes it.
             */
        }
    }
}