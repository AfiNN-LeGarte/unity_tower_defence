using UnityEngine;
using System;

[CreateAssetMenu(fileName = "GameSettings", menuName = "ECS/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Player")]
    public int StartGold = 200;
    public int StartLives = 10;

    [Header("Waves")]
    public int TotalWaves = 3;
    public float WaveInterval = 10f;
    public float FirstWaveDelay = 2f;

    public int Wave1Enemies = 5;
    public int Wave2Enemies = 10;
    public int Wave3Enemies = 15;

    public EnemyWaveConfig[] EnemyConfigs;

    [Header("Towers")]
    public TowerLevelConfig[] TowerConfigs;
    public int BaseTowerCost = 100;
    public float UpgradeCostMultiplier = 0.5f;
    public float UpgradeCostIncrease = 1.5f;

    [Header("Combat")]
    public float HitDistance = 0.5f;
    public float DestroyY = -10f;

    [Header("Movement")]
    public float PathCheckDistance = 0.5f;
    public float AvoidanceRadius = 5f;
    public float AvoidanceStrength = 2.5f;
    public float EnemySwayStrength = 0.3f;
    public float EnemySwaySpeed = 0.5f;
}

[Serializable]
public class EnemyWaveConfig
{
    public int WaveNumber;

    public float Health = 50f;
    public float Speed = 3f;
    public int Reward = 10;
    public float RotationSpeed = 10f;

    public GameObject Prefab;
}

[Serializable]
public class TowerLevelConfig
{
    public int Level;

    public float Damage = 20f;
    public float Range = 10f;
    public float Cooldown = 2f;

    public GameObject Prefab;
}
