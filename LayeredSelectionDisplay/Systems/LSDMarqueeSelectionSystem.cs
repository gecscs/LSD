namespace LayeredSelectionDisplay.Systems
{
    using System.Collections.Generic;
    using Colossal.Json;
    using Colossal.Logging;
    using Colossal.Mathematics;
    using Game;
    using Game.Citizens;
    using Game.Common;
    using Game.Creatures;
    using Game.Objects;
    using Game.Prefabs;
    using Game.Routes;
    using Game.Tools;
    using Game.Vehicles;
    using LayeredSelectionDisplay.Extensions;
    using LayeredSelectionDisplay.Selection;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using UnityEngine;
    using UnityEngine.InputSystem;

    [UpdateAfter(typeof(ToolRaycastSystem))]
    public partial class LSDMarqueeSelectionSystem :
        GameSystemBase
    {
        private Game.Objects.SearchSystem
            m_ObjectSearchSystem;

        private LayeredSelectionDisplayUISystem
            m_UISystem;

        private ILog m_Log;

        private Camera m_Camera;

        private LSDTerrainRaycast m_TerrainRaycast;

        private LSDMarquee m_Marquee;

        private bool m_Active;

        private bool m_Dragging;

        private float3 m_LastValidWorldPosition;

        private bool m_HasLastValidWorldPosition;

        private float3 m_FrameWorldPosition;

        private bool m_HasFrameWorldPosition;

        private EntityQuery m_MovingEntityQuery;

        private void SearchMovingEntities(
            NativeList<Entity> entities,
            Bounds2 bounds,
            Quad2 quad)
        {
            NativeArray<Entity> movingEntities =
                m_MovingEntityQuery.ToEntityArray(Allocator.Temp);

            try
            {
                for (int i = 0; i < movingEntities.Length; i++)
                {
                    Entity entity =
                        movingEntities[i];

                    Game.Objects.Transform transform =
                        EntityManager.GetComponentData<Game.Objects.Transform>(
                            entity);

                    float3 position =
                        transform.m_Position;

                    float2 point =
                        new float2(
                            position.x,
                            position.z);

                    Bounds2 entityBounds =
                        new Bounds2(
                            point,
                            point);

                    bool intersects =
                        MathUtils.Intersect(
                            bounds,
                            entityBounds) &&
                        MathUtils.Intersect(
                            entityBounds,
                            quad);

                    if (!intersects)
                    {
                        continue;
                    }

                    entities.Add(entity);
                }
            }
            finally
            {
                movingEntities.Dispose();
            }
        }

        public bool IsDragging => m_Dragging;

        protected override void OnCreate()
        {
            base.OnCreate();

            m_Log = LayeredSelectionDisplayMod.Instance?.Logger;

            m_ObjectSearchSystem = World.GetOrCreateSystemManaged<Game.Objects.SearchSystem>();

            m_UISystem = World.GetOrCreateSystemManaged<LayeredSelectionDisplayUISystem>();

            m_TerrainRaycast = new LSDTerrainRaycast(World);

            m_Camera = Camera.main;

            m_MovingEntityQuery =
                GetEntityQuery(
                    new EntityQueryDesc
                    {
                        All = new ComponentType[]
                        {
                            ComponentType.ReadOnly<Game.Objects.Transform>()
                        },
                        Any = new ComponentType[]
                        {
                            ComponentType.ReadOnly<Citizen>(),
                            ComponentType.ReadOnly<Vehicle>(),
                            ComponentType.ReadOnly<Car>(),
                            ComponentType.ReadOnly<Airplane>(),
                            ComponentType.ReadOnly<Helicopter>(),
                            //ComponentType.ReadOnly<PassengerTransport>(),
                            ComponentType.ReadOnly<Train>(),
                            //ComponentType.ReadOnly<Game.Vehicles.PublicTransport>(),
                            //ComponentType.ReadOnly<Game.Vehicles.CargoTransport>(),
                            ComponentType.ReadOnly<Creature>(),
                            //ComponentType.ReadOnly<CreatureData>(),
                            ComponentType.ReadOnly<Stopped>(),
                            ComponentType.ReadOnly<Human>(),
                            ComponentType.ReadOnly<Animal>(),
                            ComponentType.ReadOnly<Bicycle>(),
                            ComponentType.ReadOnly<Aircraft>(),
                            //ComponentType.ReadOnly<Game.Vehicles.Ambulance>(),
                            //ComponentType.ReadOnly<Game.Vehicles.Hearse>(),
                            //// ComponentType.ReadOnly<Game.Vehicles.GuestVehicle>(),
                            //ComponentType.ReadOnly<Game.Vehicles.GarbageTruck>(),
                            //ComponentType.ReadOnly<Game.Vehicles.FireEngine>(),
                            //ComponentType.ReadOnly<Game.Vehicles.CarTrailer>(),
                            //ComponentType.ReadOnly<Game.Vehicles.DeliveryTruck>(),
                            //ComponentType.ReadOnly<Game.Vehicles.PersonalCar>(),
                            //ComponentType.ReadOnly<Game.Vehicles.CarTrailer>(),
                            //ComponentType.ReadOnly<Game.Vehicles.ParkedTrain>(),
                            //ComponentType.ReadOnly<Game.Vehicles.ParkedCar>(),
                            //// ComponentType.ReadOnly<Game.Vehicles.OwnedVehicle>(),
                            //ComponentType.ReadOnly<Game.Vehicles.ParkMaintenanceVehicle>(),
                            //ComponentType.ReadOnly<Game.Vehicles.RoadMaintenanceVehicle>(),
                            //ComponentType.ReadOnly<Game.Vehicles.PrisonerTransport>(),
                            //ComponentType.ReadOnly<Game.Vehicles.Taxi>(),
                            //ComponentType.ReadOnly<Game.Vehicles.MaintenanceVehicle>(),
                            //ComponentType.ReadOnly<Game.Vehicles.Watercraft>(),
                            //ComponentType.ReadOnly<Game.Vehicles.Rocket>(),
                            //ComponentType.ReadOnly<Game.Vehicles.WorkVehicle>(),
                            ComponentType.ReadOnly<Game.Objects.Moving>(),
                        },
                        None = new ComponentType[]
                        {
                            ComponentType.ReadOnly<Game.Common.Deleted>(),
                        },
                    });
        }

        public void StartSelection()
        {
            m_Active = true;

            m_UISystem.SetMarqueeToolState(true);

            m_Dragging = false;

            m_Marquee = null;

            m_HasLastValidWorldPosition = false;

            m_HasFrameWorldPosition = false;

            RefreshCamera();
        }

        public void CancelSelection()
        {
            m_Active = false;

            m_UISystem.SetMarqueeToolState(false);

            m_Dragging = false;

            m_Marquee = null;

            m_HasLastValidWorldPosition = false;

            m_HasFrameWorldPosition = false;
        }

        public bool TryGetCurrentQuad(out Quad2 quad)
        {
            if (m_Marquee == null)
            {
                quad = default;

                return false;
            }

            quad = m_Marquee.Quad;

            return true;
        }

        public bool TryGetMarquee(
            out Quad2 quad,
            out float height)
        {
            if (!m_Dragging ||
                m_Marquee == null)
            {
                quad = default;

                height = 0f;

                return false;
            }

            quad =
                m_Marquee.Quad;

            height =
                m_Marquee.StartPosition.y +
                0.5f;

            return true;
        }

        public bool TryGetMarqueeStartY(out float y)
        {
            if (m_Marquee == null)
            {
                y = 0f;

                return false;
            }

            y = m_Marquee.StartPosition.y;

            return true;
        }

        protected override void OnUpdate()
        {
            if (!m_Active)
            {
                return;
            }

            Mouse mouse = Mouse.current;

            if (mouse == null)
            {
                return;
            }

            m_TerrainRaycast.Update();

            m_HasFrameWorldPosition = m_TerrainRaycast.TryGetHitPosition(out m_FrameWorldPosition);

            if (m_HasFrameWorldPosition)
            {
                m_LastValidWorldPosition = m_FrameWorldPosition;

                m_HasLastValidWorldPosition = true;
            }

            if (!m_Dragging)
            {
                if (!mouse.leftButton.wasPressedThisFrame)
                {
                    return;
                }

                if (!m_HasFrameWorldPosition)
                {
                    return;
                }

                StartMarquee(m_FrameWorldPosition);

                return;
            }

            if (mouse.leftButton.wasReleasedThisFrame)
            {
                if (m_HasLastValidWorldPosition)
                {
                    UpdateMarquee(m_LastValidWorldPosition);
                }

                FinishDrag();

                return;
            }

            if (!mouse.leftButton.isPressed)
            {
                return;
            }

            if (m_HasFrameWorldPosition)
            {
                UpdateMarquee(m_FrameWorldPosition);
            }
        }

        private void StartMarquee(float3 position)
        {
            RefreshCamera();

            m_Marquee =
                new LSDMarquee(
                    position);

            m_Dragging =
                true;

            m_LastValidWorldPosition =
                position;

            m_HasLastValidWorldPosition =
                true;
        }

        private void UpdateMarquee(float3 position)
        {
            if (m_Marquee == null)
            {
                return;
            }

            RefreshCamera();

            if (m_Camera == null)
            {
                return;
            }

            float cameraYaw = m_Camera.transform.eulerAngles.y * Mathf.Deg2Rad;

            m_Marquee.Update(position, cameraYaw);
        }

        private void RefreshCamera()
        {
            if (m_Camera == null)
            {
                m_Camera =
                    Camera.main ??
                    Camera.current;
            }
        }

        private void FinishDrag()
        {
            if (m_Marquee == null)
            {
                CancelSelection();

                return;
            }

            NativeList<Entity> candidates = new NativeList<Entity>(Allocator.Temp);

            try
            {
                Quad2 quad = m_Marquee.Quad;

                Bounds2 bounds = m_Marquee.Bounds;

                RefreshCamera();

                float cameraHeight = m_Camera != null ? m_Camera.transform.position.y : 0f;

                float expandMeters = math.max(0.5f, cameraHeight * 0.01f);

                Bounds2 expandedBounds = new Bounds2(bounds.min - new float2(expandMeters), bounds.max + new float2(expandMeters));

                SearchObjects(candidates, expandedBounds, quad);

                List<Entity> entities = new List<Entity>(candidates.Length);

                for (int i = 0; i < candidates.Length; i++)
                {
                    Entity entity = candidates[i];

                    if (!EntityManager.Exists(entity))
                    {
                        continue;
                    }

                    if (!EntityManager.MatchesLSDFilter(entity, m_UISystem.SelectedVanillaFilters, m_UISystem.SubElementSelectionActive))
                    {
                        continue;
                    }

                    entities.Add(entity);
                }

                m_UISystem.SetMarqueeEntities(entities);
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
            m_Log.Debug($"entities (start): {entities.ToJSONString()}");

            SearchMovingEntities(entities, bounds, quad);

            //m_Log.Debug($"Moving-tree candidates: {entities.Length}");

            //for (int i = 0; i < entities.Length; i++)
            //{
            //    Entity entity = entities[i];

            //    m_Log.Debug(
            //        $"Moving candidate {entity}: " +
            //        $"Citizen={EntityManager.HasComponent<Citizen>(entity)}, " +
            //        $"Vehicle={EntityManager.HasComponent<Vehicle>(entity)}, " +
            //        $"Stopped={EntityManager.HasComponent<Stopped>(entity)}, " +
            //        $"Creature={EntityManager.HasComponent<Creature>(entity)}, " +
            //        $"Animal={EntityManager.HasComponent<Animal>(entity)}, " +
            //        $"Human={EntityManager.HasComponent<Human>(entity)}, " +
            //        $"Moving={EntityManager.HasComponent<Moving>(entity)}, " +
            //        $"Car={EntityManager.HasComponent<Car>(entity)}, " +
            //        $"Airplane={EntityManager.HasComponent<Airplane>(entity)}, " +
            //        $"Moving={EntityManager.HasComponent<Helicopter>(entity)}, " +
            //        $"Train={EntityManager.HasComponent<Train>(entity)}, " +
            //        $"PublicTransport={EntityManager.HasComponent<Game.Vehicles.PublicTransport>(entity)}, " +
            //        $"CargoTransport={EntityManager.HasComponent<Game.Vehicles.CargoTransport>(entity)}, " +
            //        $"PassengerTransport={EntityManager.HasComponent<Game.Vehicles.PassengerTransport>(entity)}, " +
            //        $"Bicycle={EntityManager.HasComponent<Game.Vehicles.Bicycle>(entity)}, " +
            //        $"Aircraft={EntityManager.HasComponent<Game.Vehicles.Aircraft>(entity)}, " +
            //        $"Ambulance={EntityManager.HasComponent<Game.Vehicles.Ambulance>(entity)}, " +
            //        $"Hearse={EntityManager.HasComponent<Game.Vehicles.Hearse>(entity)}, " +
            //        $"GuestVehicle={EntityManager.HasComponent<Game.Vehicles.GuestVehicle>(entity)}, " +
            //        $"GarbageTruck={EntityManager.HasComponent<Game.Vehicles.GarbageTruck>(entity)}, " +
            //        $"FireEngine={EntityManager.HasComponent<Game.Vehicles.FireEngine>(entity)}, " +
            //        $"CarTrailer={EntityManager.HasComponent<Game.Vehicles.CarTrailer>(entity)}, " +
            //        $"DeliveryTruck={EntityManager.HasComponent<Game.Vehicles.DeliveryTruck>(entity)}, " +
            //        $"PersonalCar={EntityManager.HasComponent<Game.Vehicles.PersonalCar>(entity)}, " +
            //        $"CarTrailer={EntityManager.HasComponent<Game.Vehicles.CarTrailer>(entity)}, " +
            //        $"ParkedTrain={EntityManager.HasComponent<Game.Vehicles.ParkedTrain>(entity)}, " +
            //        $"ParkedCar={EntityManager.HasComponent<Game.Vehicles.ParkedCar>(entity)}, " +
            //        $"OwnedVehicle={EntityManager.HasComponent<Game.Vehicles.OwnedVehicle>(entity)}, " +
            //        $"ParkMaintenanceVehicle={EntityManager.HasComponent<Game.Vehicles.ParkMaintenanceVehicle>(entity)}, " +
            //        $"RoadMaintenanceVehicle={EntityManager.HasComponent<Game.Vehicles.RoadMaintenanceVehicle>(entity)}, " +
            //        $"PoliceCar={EntityManager.HasComponent<Game.Vehicles.PoliceCar>(entity)}, " +
            //        $"PrisonerTransport={EntityManager.HasComponent<Game.Vehicles.PrisonerTransport>(entity)}, " +
            //        $"Taxi={EntityManager.HasComponent<Game.Vehicles.Taxi>(entity)}, " +
            //        $"MaintenanceVehicle={EntityManager.HasComponent<Game.Vehicles.MaintenanceVehicle>(entity)}, " +
            //        $"Watercraft={EntityManager.HasComponent<Game.Vehicles.Watercraft>(entity)}, " +
            //        $"Rocket={EntityManager.HasComponent<Game.Vehicles.Rocket>(entity)}, " +
            //        $"WorkVehicle={EntityManager.HasComponent<Game.Vehicles.WorkVehicle>(entity)}, " +
            //        $"CreatureData={EntityManager.HasComponent<CreatureData>(entity)}");
            //}

            m_Log.Debug($"entities (2): {entities.ToJSONString()}");

            JobHandle movingDependencies;

            var movingTree =
                m_ObjectSearchSystem
                    .GetMovingSearchTree(
                        false,
                        out movingDependencies);

            //m_Log.Debug($"movingDependencies (2): {movingDependencies.ToJSONString()}");

            m_ObjectSearchSystem.AddMovingSearchTreeReader(movingDependencies);

            movingDependencies.Complete();

            //m_Log.Debug($"movingDependencies (2): {movingDependencies.ToJSONString()}");

            var iterator =
                new LSDMarqueeIterator
                {
                    Entities =
                        entities,

                    OuterBounds =
                        bounds,

                    SelectionQuad =
                        quad
                };

            movingTree.Iterate(
                ref iterator);

            //m_Log.Debug($"Default Moving-tree candidates: {entities.Length}");

            //for (int i = 0; i < entities.Length; i++)
            //{
            //    Entity entity = entities[i];

            //    m_Log.Debug(
            //        $"Default Moving candidate {entity}: " +
            //        $"Citizen={EntityManager.HasComponent<Citizen>(entity)}, " +
            //        $"Vehicle={EntityManager.HasComponent<Vehicle>(entity)}, " +
            //        $"Stopped={EntityManager.HasComponent<Stopped>(entity)}, " +
            //        $"Creature={EntityManager.HasComponent<Creature>(entity)}, " +
            //        $"Animal={EntityManager.HasComponent<Animal>(entity)}, " +
            //        $"Human={EntityManager.HasComponent<Human>(entity)}, " +
            //        $"Moving={EntityManager.HasComponent<Moving>(entity)}, " +
            //        $"Car={EntityManager.HasComponent<Car>(entity)}, " +
            //        $"Airplane={EntityManager.HasComponent<Airplane>(entity)}, " +
            //        $"Moving={EntityManager.HasComponent<Helicopter>(entity)}, " +
            //        $"Train={EntityManager.HasComponent<Train>(entity)}, " +
            //        $"PublicTransport={EntityManager.HasComponent<Game.Vehicles.PublicTransport>(entity)}, " +
            //        $"CargoTransport={EntityManager.HasComponent<Game.Vehicles.CargoTransport>(entity)}, " +
            //        $"PassengerTransport={EntityManager.HasComponent<Game.Vehicles.PassengerTransport>(entity)}, " +
            //        $"Bicycle={EntityManager.HasComponent<Game.Vehicles.Bicycle>(entity)}, " +
            //        $"Aircraft={EntityManager.HasComponent<Game.Vehicles.Aircraft>(entity)}, " +
            //        $"Ambulance={EntityManager.HasComponent<Game.Vehicles.Ambulance>(entity)}, " +
            //        $"Hearse={EntityManager.HasComponent<Game.Vehicles.Hearse>(entity)}, " +
            //        $"GuestVehicle={EntityManager.HasComponent<Game.Vehicles.GuestVehicle>(entity)}, " +
            //        $"GarbageTruck={EntityManager.HasComponent<Game.Vehicles.GarbageTruck>(entity)}, " +
            //        $"FireEngine={EntityManager.HasComponent<Game.Vehicles.FireEngine>(entity)}, " +
            //        $"CarTrailer={EntityManager.HasComponent<Game.Vehicles.CarTrailer>(entity)}, " +
            //        $"DeliveryTruck={EntityManager.HasComponent<Game.Vehicles.DeliveryTruck>(entity)}, " +
            //        $"PersonalCar={EntityManager.HasComponent<Game.Vehicles.PersonalCar>(entity)}, " +
            //        $"CarTrailer={EntityManager.HasComponent<Game.Vehicles.CarTrailer>(entity)}, " +
            //        $"ParkedTrain={EntityManager.HasComponent<Game.Vehicles.ParkedTrain>(entity)}, " +
            //        $"ParkedCar={EntityManager.HasComponent<Game.Vehicles.ParkedCar>(entity)}, " +
            //        $"OwnedVehicle={EntityManager.HasComponent<Game.Vehicles.OwnedVehicle>(entity)}, " +
            //        $"ParkMaintenanceVehicle={EntityManager.HasComponent<Game.Vehicles.ParkMaintenanceVehicle>(entity)}, " +
            //        $"RoadMaintenanceVehicle={EntityManager.HasComponent<Game.Vehicles.RoadMaintenanceVehicle>(entity)}, " +
            //        $"PoliceCar={EntityManager.HasComponent<Game.Vehicles.PoliceCar>(entity)}, " +
            //        $"PrisonerTransport={EntityManager.HasComponent<Game.Vehicles.PrisonerTransport>(entity)}, " +
            //        $"Taxi={EntityManager.HasComponent<Game.Vehicles.Taxi>(entity)}, " +
            //        $"MaintenanceVehicle={EntityManager.HasComponent<Game.Vehicles.MaintenanceVehicle>(entity)}, " +
            //        $"Watercraft={EntityManager.HasComponent<Game.Vehicles.Watercraft>(entity)}, " +
            //        $"Rocket={EntityManager.HasComponent<Game.Vehicles.Rocket>(entity)}, " +
            //        $"WorkVehicle={EntityManager.HasComponent<Game.Vehicles.WorkVehicle>(entity)}, " +
            //        $"CreatureData={EntityManager.HasComponent<CreatureData>(entity)}");
            //}

            JobHandle staticDependencies;

            var staticTree =
                m_ObjectSearchSystem
                    .GetStaticSearchTree(
                        false,
                        out staticDependencies);

            //m_Log.Debug($"staticDependencies (2): {staticDependencies.ToJSONString()}");

            m_ObjectSearchSystem.AddStaticSearchTreeReader(staticDependencies);

            staticDependencies.Complete();

            //m_Log.Debug($"staticDependencies (2): {staticDependencies.ToJSONString()}");

            var iterator2 =
                new LSDMarqueeIterator
                {
                    Entities =
                        entities,

                    OuterBounds =
                        bounds,

                    SelectionQuad =
                        quad
                };

            staticTree.Iterate(
                ref iterator2);

            m_Log.Debug($"entities (3): {entities.ToJSONString()}");
        }
    }
}