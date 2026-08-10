namespace LayeredSelectionDisplay.Domain
{
    using Unity.Entities;

    public sealed class HoverState
    {
        public Entity HoveredEntity = Entity.Null;

        public bool Dirty;
    }
}
