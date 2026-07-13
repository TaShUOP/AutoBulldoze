using Colossal.Entities;
using Game;
using Game.Areas;
using Game.Buildings;
using Game.Common;
using Game.Net;
using Game.Tools;
using Unity.Collections;
using Unity.Entities;

#if USE_BURST
using Unity.Jobs;
using Unity.Burst;
#endif

namespace AutoBulldoze
{
    /// <summary>
    /// System that automatically bulldozes abandoned buildings by applying the Deleted component.
    /// Matches the workflow: Game Update -> Scan Building Entities -> Is Abandoned? -> Issue Bulldoze (Add Deleted component).
    /// </summary>
    public partial class AutoBulldozeSystem : GameSystemBase
    {
        private EntityQuery _abandonedBuildingQuery;
#if USE_BURST
        private EndFrameBarrier _endFrameBarrier;
#endif

        protected override void OnCreate()
        {
            base.OnCreate();
            
            // 1. Scan Building Entities & Is building abandoned?
            _abandonedBuildingQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<Abandoned>(),
                    ComponentType.ReadOnly<Building>()
                },
                None = new ComponentType[]
                {
                    ComponentType.ReadOnly<Deleted>(),
                    ComponentType.ReadOnly<Temp>()
                }
            });

#if USE_BURST
            _endFrameBarrier = base.World.GetOrCreateSystemManaged<EndFrameBarrier>();
            Mod.Log.Info("AutoBulldozeSystem Initialized (Using Burst)");
#else
            Mod.Log.Info("AutoBulldozeSystem Initialized (NOT using Burst)");
#endif

            // System only updates if there are abandoned buildings to process
            RequireForUpdate(_abandonedBuildingQuery);
        }

        protected override void OnUpdate()
        {
#if USE_BURST
            // Run high performance Burst job to add Deleted component to all abandoned buildings
            AutoBulldozeJob job = default;
            job.m_entityTypeHandle = SystemAPI.GetEntityTypeHandle();
            job.m_entityCommandBuffer = _endFrameBarrier.CreateCommandBuffer();
            job.m_abandonedBuildingsChunk = _abandonedBuildingQuery.ToArchetypeChunkListAsync(World.UpdateAllocator.ToAllocator, out _);
            job.m_subLaneLookup = GetBufferLookup<SubLane>(false);
            job.m_subNetLookup = GetBufferLookup<SubNet>(false);
            job.m_subAreaLookup = GetBufferLookup<SubArea>(false);
            
            JobHandle handle = job.Schedule(Dependency);
            _endFrameBarrier.AddJobHandleForProducer(handle);
            Dependency = handle;
#endif

#if !USE_BURST
            // Fallback for when Burst isn't used
            var abandonedBuildings = _abandonedBuildingQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in abandonedBuildings)
            {
                // Issue Bulldoze Command: clean up sub-entities first
                if (EntityManager.TryGetBuffer<SubArea>(entity, false, out var subareas))
                {
                    foreach (var subArea in subareas)
                    {
                        EntityManager.AddComponent<Deleted>(subArea.m_Area);
                    }
                }

                if (EntityManager.TryGetBuffer<SubNet>(entity, false, out var subnets))
                {
                    foreach (var net in subnets)
                    {
                        EntityManager.AddComponent<Deleted>(net.m_SubNet);
                    }
                }

                if (EntityManager.TryGetBuffer<SubLane>(entity, false, out var sublanes))
                {
                    foreach (var lane in sublanes)
                    {
                        EntityManager.AddComponent<Deleted>(lane.m_SubLane);
                    }
                }

                // Issue Bulldoze Command to the main building entity
                EntityManager.AddComponent<Deleted>(entity);
            }
#endif
        }

        // Run this system relatively infrequently (not every single frame) to save performance, 
        // since buildings don't need to be demolished the exact millisecond they abandon.
        public override int GetUpdateInterval(SystemUpdatePhase phase) => phase == SystemUpdatePhase.GameSimulation ? 256 : 1;

#if USE_BURST
        [BurstCompile]
        private struct AutoBulldozeJob : IJob
        {
            public EntityCommandBuffer m_entityCommandBuffer;
            public NativeList<ArchetypeChunk> m_abandonedBuildingsChunk;
            public EntityTypeHandle m_entityTypeHandle;
            public BufferLookup<SubArea> m_subAreaLookup;
            public BufferLookup<SubNet> m_subNetLookup;
            public BufferLookup<SubLane> m_subLaneLookup;

            public void Execute()
            {
                for (int i = 0; i < m_abandonedBuildingsChunk.Length; i++)
                {
                    var chunk = m_abandonedBuildingsChunk[i];
                    var nativeArray = chunk.GetNativeArray(m_entityTypeHandle);
                    for (int j = 0; j < nativeArray.Length; j++)
                    {
                        var entity = nativeArray[j];
                        
                        // Clean up child entities
                        if (m_subAreaLookup.TryGetBuffer(entity, out var subAreas))
                        {
                            foreach (var entry in subAreas)
                            {
                                m_entityCommandBuffer.AddComponent<Deleted>(entry.m_Area);
                            }
                        }

                        if (m_subLaneLookup.TryGetBuffer(entity, out var subLanes))
                        {
                            foreach (var entry in subLanes)
                            {
                                m_entityCommandBuffer.AddComponent<Deleted>(entry.m_SubLane);
                            }
                        }

                        if(m_subNetLookup.TryGetBuffer(entity, out var subNet))
                        {
                            foreach (var entry in subNet)
                            {
                                m_entityCommandBuffer.AddComponent<Deleted>(entry.m_SubNet);
                            }
                        }

                        // Finally, delete the building entity itself
                        m_entityCommandBuffer.AddComponent<Deleted>(entity);
                    }
                }
            }
        }
#endif
    }
}
