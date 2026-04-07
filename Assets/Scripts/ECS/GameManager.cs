using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("📋 Settings")]
    public GameSettings Settings; // ✅ Перетаскиваем ассет из проекта

    [Header("🛣️ Path")]
    public List<GameObject> PathPoints;

    [Header("🗼 Towers")]
    public List<TowerPosition> TowerPositions = new();

    [Header("🎯 Projectiles")]
    public GameObject ProjectilePrefab;

    [Header("🖥️ UI")]
    public Text GoldText;
    public Text LivesText;
    public Text WaveText;
    
    [Header("💀 Game Over")]
    public GameObject GameOverCanvas;
    public Button RestartButton;

    World _world;

    void Start()
    {

        if (Settings == null)
        {
            Debug.LogError("❌ GameSettings не назначен в Inspector!");
            return;
        }


        if (GameOverCanvas != null)
            GameOverCanvas.SetActive(false);

        _world = new World();


        InitializePrefabDatabase();


        var gameStateEntity = _world.CreateEntity();
        gameStateEntity.Add(new GameStateComponent
        {
            IsGameOver = false,
            IsPaused = false,
            GameOverCanvas = GameOverCanvas
        });


        var waveEntity = _world.CreateEntity();
        waveEntity.Add(new WaveComponent
        {
            CurrentWave = 1,
            TotalWaves = Settings.TotalWaves,
            WaveTimer = Settings.FirstWaveDelay,
            WaveInterval = Settings.WaveInterval,
            WaveActive = false,
            EnemiesToSpawn = GetEnemiesForWave(1),
            SpawnedCount = 0
        });


        var player = _world.CreateEntity();
        player.Add(new PlayerComponent 
        { 
            Gold = Settings.StartGold, 
            Lives = Settings.StartLives 
        });


        var ui = _world.CreateEntity();
        ui.Add(new UIComponent
        {
            GoldText = GoldText,
            LivesText = LivesText,
            WaveText = WaveText,
            TowerPositions = TowerPositions
        });


        _world.AddSystem(new SpawnSystem 
        { 
            PathPoints = PathPoints,
            EnemyPrefabs = PrefabDatabase.EnemyPrefabs,
            Settings = Settings // ✅ Передаём настройки
        });
        _world.AddSystem(new MovementSystem
        {
            Settings = Settings // ✅ Передаём настройки
        });
        _world.AddSystem(new TowerSystem 
        { 
            ProjectilePrefab = PrefabDatabase.ProjectilePrefab,
            Settings = Settings
        });
        _world.AddSystem(new ProjectileSystem
        {
            Settings = Settings
        });
        _world.AddSystem(new CleanupSystem());
        _world.AddSystem(new ViewSystem());
        _world.AddSystem(new UISystem
        {
            Settings = Settings
        });
        _world.AddSystem(new TowerPlacementSystem
        {
            Settings = Settings
        });


        if (RestartButton != null)
        {
            RestartButton.onClick.AddListener(OnRestartClicked);
        }

        Debug.Log("✅ Игра запущена с настройками: " + Settings.name);
    }

    void Update()
    {
        _world.Update();
    }

    void OnDestroy()
    {
        _world?.Cleanup();
    }


    int GetEnemiesForWave(int wave)
    {
        return wave switch
        {
            1 => Settings.Wave1Enemies,
            2 => Settings.Wave2Enemies,
            3 => Settings.Wave3Enemies,
            _ => 5
        };
    }

    void InitializePrefabDatabase()
    {
        var enemyPrefabs = new Dictionary<int, List<GameObject>>();
        var towerPrefabs = new Dictionary<int, GameObject>();


        foreach (var config in Settings.EnemyConfigs)
        {
            if (config.Prefab != null)
            {
                enemyPrefabs[config.WaveNumber] = new List<GameObject> { config.Prefab };
            }
        }


        foreach (var config in Settings.TowerConfigs)
        {
            if (config.Prefab != null)
            {
                towerPrefabs[config.Level] = config.Prefab;
            }
        }

        PrefabDatabase.Initialize(enemyPrefabs, towerPrefabs, ProjectilePrefab);
    }


    void OnRestartClicked()
    {
        Debug.Log("🔄 Рестарт игры...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}