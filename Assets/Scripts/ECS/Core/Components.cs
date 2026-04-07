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
    public int Reward;      // Монеты за убийство
    public int Wave;        // Номер волны
}

public class PathComponent : Component
{
    public Queue<Vector3> Waypoints;
    public int CurrentIndex;
}

public class TowerComponent : Component
{
    public int Level;           // Уровень башни (1-3)
    public float Damage;        // Урон (зависит от уровня)
    public float Range;         // Радиус (зависит от уровня)
    public float Cooldown;      // Перезарядка (зависит от уровня)
    public float CooldownTimer; // Таймер перезарядки
    public int TargetId;        // ID цели
    public int SpotIndex;       // Индекс позиции
}

public class TowerSlotComponent : Component
{
    public int SlotIndex;
    public bool IsOccupied;
    public int TowerEntityId;
    public Vector3 Position;
    public int TowerLevel;      // Для улучшения
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