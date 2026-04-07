using UnityEngine;
using System.Collections.Generic;

public class PositionComponent : Component
{
    public Vector3 Value;
}

public class RotationComponent : Component
{
    public Quaternion Value;
}

public class HealthComponent : Component
{
    public float Current;
    public float Max;
}

public class MovementComponent : Component
{
    public float Speed;
    public float RotationSpeed;
    public Vector3 Direction;
    public bool IsMoving;
}

public class EnemyComponent : Component
{
    public int Reward;
    public int Wave;
}

public class PathComponent : Component
{
    public Queue<Vector3> Waypoints;
    public int CurrentIndex;
}

public class TowerComponent : Component
{
    public int Level;
    public int SpotIndex;
    public float Damage;
    public float Range;
    public float Cooldown;
    public float CooldownTimer;
    public int TargetId;
}

public class PlayerComponent : Component
{
    public int Gold;
    public int Lives;
}

public class ProjectileComponent : Component
{
    public int TargetId;
    public float Damage;
}

public class UIComponent : Component
{
    public UnityEngine.UI.Text GoldText;
    public UnityEngine.UI.Text LivesText;
    public UnityEngine.UI.Text WaveText;
    public List<TowerPosition> TowerPositions;
}

public class UnityObjectComponent : Component
{
    public GameObject Obj;
}

public class WaveComponent : Component
{
    public int CurrentWave;
    public int TotalWaves;
    public float WaveTimer;
    public float WaveInterval;
    public bool WaveActive;
    public int EnemiesToSpawn;
    public int SpawnedCount;
}

public class AvoidanceComponent : Component
{
    public float AvoidRadius;
    public float AvoidStrength;
    public Vector3 AvoidDirection;
}

public class GameStateComponent : Component
{
    public bool IsGameOver;
    public bool IsPaused;
    public GameObject GameOverCanvas;
}