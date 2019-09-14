using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

namespace UniVox.Core
{
    public partial class EntityWorldMap : IDisposable
    {
        private Unity.Entities.World _world;

        private readonly Dictionary<int3, Entity> _entities;

        public EntityWorldMap(string worldName = default)
        {
            _world = new Unity.Entities.World(worldName);
            _entities = new Dictionary<int3, Entity>();
        }

        //Creates an Empty Entity
        private Entity CreateEntity(int3 position)
        {
            return _entities[position] = EntityManager.CreateEntity();
        }

        public Entity GetOrCreateEntity(int3 position)
        {
            if (!_entities.TryGetValue(position, out var entity))
                entity = CreateEntity(position);
            return entity;
        }
        
        public Unity.Entities.World EntityWorld => _world;
        public EntityManager EntityManager => _world.EntityManager;

        public void Dispose()
        {
            _world?.Dispose();
        }
    }
}