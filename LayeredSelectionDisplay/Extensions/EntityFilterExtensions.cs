namespace LayeredSelectionDisplay.Extensions
{
    using Colossal.Entities;
    using Colossal.Logging;
    using Game.Citizens;
    using Game.Common;
    using Game.Creatures;
    using Game.Objects;
    using Game.Prefabs;
    using Game.Vehicles;
    using LayeredSelectionDisplay.Systems;
    using Unity.Entities;

    internal static class EntityFilterExtensions
    {
        public static bool MatchesLSDFilter(this EntityManager entityManager, Entity entity, LayeredSelectionDisplayUISystem.VanillaFilters filters, bool subElementSelectionActive)
        {
            ILog m_Log = LayeredSelectionDisplayMod.Instance?.Logger;
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} filters: {filters.ToString()}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} entity: {entity.ToString()}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} plant? {entityManager.HasComponent<Plant>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} tree? {entityManager.HasComponent<Tree>(entity)}");

            bool allowSubObjectSelection = LayeredSelectionDisplayMod.Instance.Settings.AllowSubObjectSelection;

            if (filters == LayeredSelectionDisplayUISystem.VanillaFilters.None)
            {
                return false;
            }

            if (filters == LayeredSelectionDisplayUISystem.VanillaFilters.All)
            {
                return true;
            }

            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} entity: {entity.ToString()}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} AllowSubObjectSelection? {allowSubObjectSelection}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} Has owner? {entityManager.HasComponent<Owner>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} SubLane? {entityManager.HasComponent<SubLane>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} SubNet? {entityManager.HasComponent<SubNet>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} Marker? {entityManager.HasComponent<Marker>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} SpawnLocation? {entityManager.HasComponent<Game.Objects.SpawnLocation>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} UtilityObject? {entityManager.HasComponent<Game.Objects.UtilityObject>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} Citizen? {entityManager.HasComponent<Citizen>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} Vehicle? {entityManager.HasComponent<Vehicle>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} CreatureData? {entityManager.HasComponent<CreatureData>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} Moving? {entityManager.HasComponent<Moving>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} CargoTransport? {entityManager.HasComponent<Game.Vehicles.CargoTransport>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} PublicTransport? {entityManager.HasComponent<Game.Vehicles.PublicTransport>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} Car? {entityManager.HasComponent<Game.Vehicles.Car>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} Airplane? {entityManager.HasComponent<Game.Vehicles.Airplane>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} Helicopter? {entityManager.HasComponent<Game.Vehicles.Helicopter>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} Train? {entityManager.HasComponent<Game.Vehicles.Train>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} PassengerTransport? {entityManager.HasComponent<PassengerTransport>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} Bicycle? {entityManager.HasComponent<Bicycle>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} Aircraft? {entityManager.HasComponent<Aircraft>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} Ambulance? {entityManager.HasComponent<Game.Vehicles.Ambulance>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} CarTrailer? {entityManager.HasComponent<Game.Vehicles.CarTrailer>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} DeliveryTruck? {entityManager.HasComponent<Game.Vehicles.DeliveryTruck>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} FireEngine? {entityManager.HasComponent<Game.Vehicles.FireEngine>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} GarbageTruck? {entityManager.HasComponent<Game.Vehicles.GarbageTruck>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} GuestVehicle? {entityManager.HasComponent<Game.Vehicles.GuestVehicle>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} Hearse? {entityManager.HasComponent<Game.Vehicles.Hearse>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} MaintenanceVehicle? {entityManager.HasComponent<Game.Vehicles.MaintenanceVehicle>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} OwnedVehicle? {entityManager.HasComponent<Game.Vehicles.OwnedVehicle>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} ParkedCar? {entityManager.HasComponent<Game.Vehicles.ParkedCar>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} ParkedTrain? {entityManager.HasComponent<Game.Vehicles.ParkedTrain>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} ParkMaintenanceVehicle? {entityManager.HasComponent<Game.Vehicles.ParkMaintenanceVehicle>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} PoliceCar? {entityManager.HasComponent<Game.Vehicles.PoliceCar>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} PersonalCar? {entityManager.HasComponent<Game.Vehicles.PersonalCar>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} PostVan? {entityManager.HasComponent<Game.Vehicles.PostVan>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} PrisonerTransport? {entityManager.HasComponent<Game.Vehicles.PrisonerTransport>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} RoadMaintenanceVehicle? {entityManager.HasComponent<Game.Vehicles.RoadMaintenanceVehicle>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} Rocket? {entityManager.HasComponent<Game.Vehicles.Rocket>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} Taxi? {entityManager.HasComponent<Game.Vehicles.Taxi>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} Watercraft? {entityManager.HasComponent<Game.Vehicles.Watercraft>(entity)}");
            //m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} WorkVehicle? {entityManager.HasComponent<Game.Vehicles.WorkVehicle>(entity)}");

            // Building SubObjects
            if (allowSubObjectSelection)
            {
                // NetSubObjects
                if ((filters & LayeredSelectionDisplayUISystem.VanillaFilters.NetSubObjects) != 0 && entityManager.HasComponent<Owner>(entity) && (entityManager.HasComponent<Game.Prefabs.SubLane>(entity) || entityManager.HasComponent<Game.Prefabs.SubNet>(entity) || entityManager.HasComponent<Game.Objects.UtilityObject>(entity)))
                {
                    m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} entity is NetSubObject");
                    return true;
                }

                // Marquers
                if ((filters & LayeredSelectionDisplayUISystem.VanillaFilters.Marquers) != 0 && entityManager.HasComponent<Owner>(entity) && (entityManager.HasComponent<Marker>(entity) || entityManager.HasComponent<Game.Objects.SpawnLocation>(entity)))
                {
                    m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} entity is Marquer");
                    return true;
                }
            }
            else
            {
                // NetSubObjects
                if ((filters & LayeredSelectionDisplayUISystem.VanillaFilters.NetSubObjects) != 0 && entityManager.HasComponent<Owner>(entity) && (entityManager.HasComponent<Game.Prefabs.SubLane>(entity) || entityManager.HasComponent<Game.Prefabs.SubNet>(entity)))
                {
                    m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} entity is NetSubObject and AllowSubObjectSelection is false");
                    return false;
                }

                // Marquers
                if ((filters & LayeredSelectionDisplayUISystem.VanillaFilters.Marquers) != 0 && entityManager.HasComponent<Owner>(entity) && (entityManager.HasComponent<Marker>(entity) || entityManager.HasComponent<Game.Objects.SpawnLocation>(entity)))
                {
                    m_Log.Debug($"{nameof(EntityFilterExtensions)}.{nameof(MatchesLSDFilter)} entity is Marquer and AllowSubObjectSelection is false");
                    return false;
                }
            }

            // Buildings
            if ((filters & LayeredSelectionDisplayUISystem.VanillaFilters.Buildings) != 0 && entityManager.HasComponent<Game.Buildings.Building>(entity) &&
                (allowSubObjectSelection ? true : !entityManager.HasComponent<Owner>(entity)))
            {
                return true;
            }

            // Trees
            if ((filters & LayeredSelectionDisplayUISystem.VanillaFilters.Trees) != 0 &&
                entityManager.HasComponent<Tree>(entity) &&
                (!entityManager.HasComponent<Owner>(entity) ||
                 (allowSubObjectSelection && subElementSelectionActive)))
            {
                return true;
            }

            // Plants
            if ((filters & LayeredSelectionDisplayUISystem.VanillaFilters.Plants) != 0 &&
                entityManager.HasComponent<Plant>(entity) &&
                !entityManager.HasComponent<Tree>(entity) &&
                (!entityManager.HasComponent<Owner>(entity) ||
                 (allowSubObjectSelection && subElementSelectionActive)))
            {
                return true;
            }

            // Moving Objects
            if ((filters & LayeredSelectionDisplayUISystem.VanillaFilters.MovingObjects) != 0 &&
                (entityManager.HasComponent<Creature>(entity) ||
                entityManager.HasComponent<Vehicle>(entity) ||
                entityManager.HasComponent<Human>(entity) ||
                entityManager.HasComponent<Animal>(entity) ||
                entityManager.HasComponent<Stopped>(entity) ||
                entityManager.HasComponent<Car>(entity) ||
                entityManager.HasComponent<Airplane>(entity) ||
                entityManager.HasComponent<Helicopter>(entity) ||
                entityManager.HasComponent<PassengerTransport>(entity) ||
                entityManager.HasComponent<Train>(entity) ||
                entityManager.HasComponent<Game.Vehicles.PublicTransport>(entity) ||
                entityManager.HasComponent<Game.Vehicles.CargoTransport>(entity) ||
                entityManager.HasComponent<Game.Vehicles.Bicycle>(entity) ||
                entityManager.HasComponent<Game.Vehicles.Aircraft>(entity) ||
                entityManager.HasComponent<Game.Vehicles.Ambulance>(entity) ||
                entityManager.HasComponent<Game.Vehicles.Hearse>(entity) ||
                // entityManager.HasComponent<Game.Vehicles.GuestVehicle>(entity) ||
                entityManager.HasComponent<Game.Vehicles.GarbageTruck>(entity) ||
                entityManager.HasComponent<Game.Vehicles.FireEngine>(entity) ||
                entityManager.HasComponent<Game.Vehicles.CarTrailer>(entity) ||
                entityManager.HasComponent<Game.Vehicles.DeliveryTruck>(entity) ||
                entityManager.HasComponent<Game.Vehicles.PersonalCar>(entity) ||
                entityManager.HasComponent<Game.Vehicles.CarTrailer>(entity) ||
                entityManager.HasComponent<Game.Vehicles.ParkedTrain>(entity) ||
                entityManager.HasComponent<Game.Vehicles.ParkedCar>(entity) ||
                // entityManager.HasComponent<Game.Vehicles.OwnedVehicle>(entity) ||
                entityManager.HasComponent<Game.Vehicles.ParkMaintenanceVehicle>(entity) ||
                entityManager.HasComponent<Game.Vehicles.RoadMaintenanceVehicle>(entity) ||
                entityManager.HasComponent<Game.Vehicles.PoliceCar>(entity) ||
                entityManager.HasComponent<Game.Vehicles.PrisonerTransport>(entity) ||
                entityManager.HasComponent<Game.Vehicles.Taxi>(entity) ||
                entityManager.HasComponent<Game.Vehicles.MaintenanceVehicle>(entity) ||
                entityManager.HasComponent<Game.Vehicles.Watercraft>(entity) ||
                entityManager.HasComponent<Game.Vehicles.Rocket>(entity) ||
                entityManager.HasComponent<Game.Vehicles.WorkVehicle>(entity) ||
                entityManager.HasComponent<Moving>(entity)))
            {
                return true;
            }

            //entityManager.HasComponent<Owner>(entity);

            //entityManager.HasComponent<Marker>(entity);

            //entityManager.HasComponent<Vehicle>(entity); // Needs to be checked before Owner, Vehicles have Owner component. Also need to be checked before Citizen, Vehicles might have Citizen component.

            //entityManager.HasComponent<Citizen>(entity);

            //entityManager.HasComponent<SubLane>(entity);

            //entityManager.HasComponent<SubNet>(entity);

            //entityManager.HasComponent<Game.Objects.SubObject>(entity);

            //entityManager.HasComponent<Game.Objects.SpawnLocation>(entity);

            //entityManager.HasComponent<CreatureData>(entity);

            // Props
            if ((filters & LayeredSelectionDisplayUISystem.VanillaFilters.Props) != 0 &&
                entityManager.HasComponent<Object>(entity) &&
                entityManager.HasComponent<Static>(entity) &&
                !entityManager.HasComponent<Tree>(entity) &&
                !entityManager.HasComponent<Plant>(entity) &&
                !entityManager.IsDecal(entity) &&
                !entityManager.HasComponent<Marker>(entity) &&
                !entityManager.HasComponent<SubLane>(entity) &&
                !entityManager.HasComponent<SubNet>(entity) &&
                !entityManager.HasComponent<Game.Objects.SpawnLocation>(entity) &&
                !entityManager.HasComponent<Game.Objects.UtilityObject>(entity) &&
                (!entityManager.HasComponent<Owner>(entity) ||
                 (allowSubObjectSelection && subElementSelectionActive)))
            {
                return true;
            }

            // Decals
            if ((filters & LayeredSelectionDisplayUISystem.VanillaFilters.Decals) != 0 &&
                entityManager.IsDecal(entity) &&
                (!entityManager.HasComponent<Owner>(entity) ||
                 (allowSubObjectSelection && subElementSelectionActive)))
            {
                return true;
            }

            return false;
        }

        private static bool IsDecal(this EntityManager entityManager, Entity entity)
        {
            if (!entityManager.TryGetComponent(entity, out PrefabRef prefabRef))
            {
                return false;
            }

            if (!entityManager.TryGetBuffer(prefabRef, true, out DynamicBuffer<SubMesh> submeshes))
            {
                return false;
            }

            if (submeshes.Length == 0)
            {
                return false;
            }

            if (!entityManager.TryGetComponent(submeshes[0].m_SubMesh, out MeshData meshData))
            {
                return false;
            }

            return (meshData.m_State & MeshFlags.Decal) == MeshFlags.Decal;
        }
    }
}