namespace LayeredSelectionDisplay.Extensions
{
    using Colossal.Entities;
    using Game.Common;
    using Game.Objects;
    using Game.Prefabs;
    using LayeredSelectionDisplay.Systems;
    using Unity.Entities;

    internal static class EntityFilterExtensions
    {
        public static bool MatchesLSDFilter(
            this EntityManager entityManager,
            Entity entity,
            LayeredSelectionDisplayUISystem.VanillaFilters filters)
        {
            if (filters == LayeredSelectionDisplayUISystem.VanillaFilters.None)
            {
                return false;
            }

            if (filters == LayeredSelectionDisplayUISystem.VanillaFilters.All)
            {
                return true;
            }

            if ((filters &
                 LayeredSelectionDisplayUISystem.VanillaFilters.Buildings) != 0 &&
                entityManager.HasComponent<Game.Buildings.Building>(entity))
            {
                return true;
            }

            if ((filters &
                 LayeredSelectionDisplayUISystem.VanillaFilters.Trees) != 0 &&
                entityManager.HasComponent<Tree>(entity))
            {
                return true;
            }

            if ((filters &
                 LayeredSelectionDisplayUISystem.VanillaFilters.Plants) != 0 &&
                entityManager.HasComponent<Plant>(entity) &&
                !entityManager.HasComponent<Tree>(entity))
            {
                return true;
            }

            if ((filters &
                 LayeredSelectionDisplayUISystem.VanillaFilters.Props) != 0 &&
                entityManager.HasComponent<Object>(entity) &&
                entityManager.HasComponent<Static>(entity))
            {
                return true;
            }

            if ((filters &
                 LayeredSelectionDisplayUISystem.VanillaFilters.Decals) != 0 &&
                entityManager.IsDecal(entity))
            {
                return true;
            }

            return false;
        }

        private static bool IsDecal(
            this EntityManager entityManager,
            Entity entity)
        {
            if (!entityManager.TryGetComponent(
                    entity,
                    out PrefabRef prefabRef))
            {
                return false;
            }

            if (!entityManager.TryGetBuffer(
                    prefabRef,
                    true,
                    out DynamicBuffer<SubMesh> submeshes))
            {
                return false;
            }

            if (submeshes.Length == 0)
            {
                return false;
            }

            if (!entityManager.TryGetComponent(
                    submeshes[0].m_SubMesh,
                    out MeshData meshData))
            {
                return false;
            }

            return (meshData.m_State &
                    MeshFlags.Decal) ==
                   MeshFlags.Decal;
        }
    }
}