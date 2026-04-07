using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

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
    private readonly Dictionary<Type, Component> components = new();

    public Entity(int id) => Id = id;

    public void Add(Component c) => components[c.GetType()] = c;

    public T Get<T>() where T : Component => (T)components[typeof(T)];

    public bool Has<T>() where T : Component => components.ContainsKey(typeof(T));

    public void Destroy()
    {
        IsActive = false;
        components.Clear();
    }
}

public class World
{
    private readonly List<Entity> entities = new();
    private readonly List<BaseSystem> systems = new();
    private readonly Queue<Entity> pool = new();
    private readonly List<Entity> toRemove = new();
    private int nextId = 1;
    private bool isUpdating = false;

    public Entity CreateEntity()
    {
        Entity e = pool.Count > 0 ? pool.Dequeue() : new Entity(nextId++);
        e.IsActive = true;
        entities.Add(e);
        return e;
    }

    public void DestroyEntity(Entity e)
    {
        e.Destroy();
        if (isUpdating)
            toRemove.Add(e);
        else
        {
            pool.Enqueue(e);
            entities.Remove(e);
        }
    }

    public IEnumerable<Entity> Query<T>() where T : Component
    {
        return entities.Where(e => e.IsActive && e.Has<T>());
    }

    public List<Entity> GetAll() => entities;

    public void AddSystem(BaseSystem s)
    {
        s.World = this;
        systems.Add(s);
    }

    public void Update()
    {
        isUpdating = true;
        foreach (var s in systems)
            s.Execute();
        isUpdating = false;

        foreach (var e in toRemove)
        {
            pool.Enqueue(e);
            entities.Remove(e);
        }
        toRemove.Clear();
    }

    public void Cleanup()
    {
        foreach (var s in systems)
            s.Cleanup();
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
}