using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;


public abstract class Component { }


public abstract class BaseSystem
{
    public World World { get; set; }
    
    public abstract void Execute();
    

    public virtual void Cleanup() { }
}


public class Entity
{
    public int Id { get; }
    public bool IsActive = true;
    
    private readonly Dictionary<Type, Component> _components = new();

    public Entity(int id) => Id = id;

    public void Add(Component component)
    {
        _components[component.GetType()] = component;
    }

    public T Get<T>() where T : Component
    {
        return (T)_components[typeof(T)];
    }

    public bool TryGet<T>(out T component) where T : Component
    {
        if (_components.TryGetValue(typeof(T), out var c))
        {
            component = (T)c;
            return true;
        }
        component = null;
        return false;
    }

    public bool Has<T>() where T : Component
    {
        return _components.ContainsKey(typeof(T));
    }

    public void Remove<T>() where T : Component
    {
        _components.Remove(typeof(T));
    }

    public void Destroy()
    {
        IsActive = false;
        _components.Clear();
    }
}






public class World
{
    private readonly List<Entity> _entities = new();
    private readonly List<BaseSystem> _systems = new();
    private int _nextId = 1;
    private readonly Queue<Entity> _entityPool = new();


    private readonly List<Entity> _entitiesToAdd = new();
    private readonly List<Entity> _entitiesToRemove = new();
    private bool _isUpdating = false;

    public Entity CreateEntity()
    {
        Entity entity;
        
        if (_entityPool.TryDequeue(out entity))
        {
            entity.IsActive = true;
        }
        else
        {
            entity = new Entity(_nextId++);
        }

        if (_isUpdating)
        {
            _entitiesToAdd.Add(entity);
        }
        else
        {
            _entities.Add(entity);
        }
        
        return entity;
    }

    public void DestroyEntity(Entity entity)
    {
        if (_isUpdating)
        {
            _entitiesToRemove.Add(entity);
        }
        else
        {
            entity.Destroy();
            _entityPool.Enqueue(entity);
            _entities.Remove(entity);
        }
    }


    public IEnumerable<Entity> Query<T1>() where T1 : Component
    {
        foreach (var e in _entities)
            if (e.IsActive && e.Has<T1>())
                yield return e;
    }


    public IEnumerable<Entity> Query<T1, T2>() where T1 : Component where T2 : Component
    {
        foreach (var e in _entities)
            if (e.IsActive && e.Has<T1>() && e.Has<T2>())
                yield return e;
    }


    public IEnumerable<Entity> Query<T1, T2, T3>() 
        where T1 : Component where T2 : Component where T3 : Component
    {
        foreach (var e in _entities)
            if (e.IsActive && e.Has<T1>() && e.Has<T2>() && e.Has<T3>())
                yield return e;
    }


    public IEnumerable<Entity> Query<T1, T2, T3, T4>() 
        where T1 : Component where T2 : Component where T3 : Component where T4 : Component
    {
        foreach (var e in _entities)
            if (e.IsActive && e.Has<T1>() && e.Has<T2>() && e.Has<T3>() && e.Has<T4>())
                yield return e;
    }


    public IEnumerable<Entity> Query<T1, T2, T3, T4, T5>() 
        where T1 : Component where T2 : Component where T3 : Component 
        where T4 : Component where T5 : Component
    {
        foreach (var e in _entities)
            if (e.IsActive && e.Has<T1>() && e.Has<T2>() && e.Has<T3>() && e.Has<T4>() && e.Has<T5>())
                yield return e;
    }

    public List<Entity> GetAll() => _entities;

    public void AddSystem(BaseSystem system)
    {
        system.World = this;
        _systems.Add(system);
    }

    public void Update()
    {
        _isUpdating = true;

        foreach (var system in _systems)
            system.Execute();

        _isUpdating = false;


        foreach (var entity in _entitiesToAdd)
            _entities.Add(entity);
        
        foreach (var entity in _entitiesToRemove)
        {
            entity.Destroy();
            _entityPool.Enqueue(entity);
            _entities.Remove(entity);
        }

        _entitiesToAdd.Clear();
        _entitiesToRemove.Clear();

        _entities.RemoveAll(e => !e.IsActive);
    }

    public void Cleanup()
    {
        foreach (var system in _systems)
            system.Cleanup();
    }
}

public static class PrefabDatabase
{
    public static Dictionary<int, List<GameObject>> EnemyPrefabs { get; private set; }
    public static Dictionary<int, GameObject> TowerPrefabs { get; private set; }
    public static GameObject ProjectilePrefab { get; private set; }

    public static void Initialize(
        Dictionary<int, List<GameObject>> enemies,
        Dictionary<int, GameObject> towers,
        GameObject projectile)
    {
        EnemyPrefabs = enemies;
        TowerPrefabs = towers;
        ProjectilePrefab = projectile;
    }

    public static GameObject GetTowerPrefab(int level)
    {
        return TowerPrefabs.TryGetValue(level, out var prefab) ? prefab : null;
    }

    public static List<GameObject> GetEnemyPrefabs(int wave)
    {
        return EnemyPrefabs.TryGetValue(wave, out var prefabs) ? prefabs : null;
    }
}