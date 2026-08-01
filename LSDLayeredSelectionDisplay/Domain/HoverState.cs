namespace LSD_Layered_Selection_Display.Domain
{
    using Unity.Entities;

    public sealed class HoverState
    {
        public Entity HoveredEntity = Entity.Null;

        public bool Dirty;
    }
}
