using UnityEngine;
using System;

[CreateAssetMenu(fileName = "GameSettings", menuName = "ECS/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Player Settings")]
    public int StartGold = 200;
    public int StartLives = 10;

    [Header("Wave Settings")]
    public int TotalWaves = 3;
    public float WaveInterval = 10f;
    public float FirstWaveDelay = 2f;
    
    [Header("Enemies Per Wave")]
    public int Wave1Enemies = 5;
    public int Wave2Enemies = 10;
    public int Wave3Enemies = 15;

    [Header("Enemy Stats Per Wave")]
    public EnemyWaveConfig[] EnemyConfigs;

    [Header("Tower Settings")]
    public TowerLevelConfig[] TowerConfigs;
    public int BaseTowerCost = 100;
    public float UpgradeCostMultiplier = 0.5f;
    public float UpgradeCostIncrease = 1.5f;

    [Header("Projectile Settings")]
    public float HitDistance = 0.5f;
    public float DestroyY = -10f;

    [Header("Movement Settings")]
    public float PathCheckDistance = 0.5f;
    public float AvoidanceRadius = 5f;
    public float AvoidanceStrength = 2.5f;
    public float EnemySwayStrength = 0.3f;
    public float EnemySwaySpeed = 0.5f;
}


[Serializable]
public class EnemyWaveConfig
{
    [Header("Wave Number")]
    public int WaveNumber;

    [Header("Stats")]
    public float Health = 50f;
    public float Speed = 3f;
    public int Reward = 10;
    public float RotationSpeed = 10f;

    [Header("Prefabs")]
    public GameObject Prefab;
}



[Serializable]
public class TowerLevelConfig
{
    [Header("Level")]
    public int Level;

    [Header("Stats")]
    public float Damage = 20f;
    public float Range = 10f;
    public float Cooldown = 2f;

    [Header("Prefab")]
    public GameObject Prefab;
}