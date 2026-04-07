using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Settings")]
    public GameSettings Settings;

    [Header("Path")]
    public List<GameObject> PathPoints;

    [Header("Towers")]
    public List<TowerPosition> TowerPositions = new();

    [Header("Projectiles")]
    public GameObject ProjectilePrefab;

    [Header("UI")]
    public Text GoldText;
    public Text LivesText;
    public Text WaveText;

    [Header("Game Over")]
    public GameObject GameOverCanvas;
    public Button RestartButton;

    World world;
    TowerPlacementSystem placementSystem;

    void Start()
    {
        if (Settings == null)
        {
            Debug.LogError("GameSettings не назначен в Inspector!");
            return;
        }

        if (GameOverCanvas != null)
            GameOverCanvas.SetActive(false);

        world = new World();
        InitializePrefabDatabase();

        var gameState = world.CreateEntity();
        gameState.Add(new GameStateComponent
        {
            IsGameOver = false,
            IsPaused = false,
            GameOverCanvas = GameOverCanvas
        });

        var wave = world.CreateEntity();
        wave.Add(new WaveComponent
        {
            CurrentWave = 1,
            TotalWaves = Settings.TotalWaves,
            WaveTimer = Settings.FirstWaveDelay,
            WaveInterval = Settings.WaveInterval,
            WaveActive = false,
            EnemiesToSpawn = GetEnemiesForWave(1),
            SpawnedCount = 0
        });

        var player = world.CreateEntity();
        player.Add(new PlayerComponent
        {
            Gold = Settings.StartGold,
            Lives = Settings.StartLives
        });

        var ui = world.CreateEntity();
        ui.Add(new UIComponent
        {
            GoldText = GoldText,
            LivesText = LivesText,
            WaveText = WaveText,
            TowerPositions = TowerPositions
        });

        placementSystem = new TowerPlacementSystem { Settings = Settings };

        world.AddSystem(new SpawnSystem { PathPoints = PathPoints, Settings = Settings });
        world.AddSystem(new MovementSystem { Settings = Settings });
        world.AddSystem(new TowerSystem { ProjectilePrefab = ProjectilePrefab, Settings = Settings });
        world.AddSystem(new ProjectileSystem { Settings = Settings });
        world.AddSystem(new CleanupSystem());
        world.AddSystem(new ViewSystem());
        world.AddSystem(new UISystem { Settings = Settings });
        world.AddSystem(placementSystem);

        SubscribeToButtons();

        if (RestartButton != null)
            RestartButton.onClick.AddListener(OnRestartClicked);

        Debug.Log("Игра запущена: " + Settings.name);
    }

    void Update()
    {
        world.Update();
    }

    void OnDestroy()
    {
        world?.Cleanup();
    }

    void SubscribeToButtons()
    {
        foreach (var pos in TowerPositions)
        {
            if (pos.Button != null)
            {
                int index = pos.Index;
                pos.Button.onClick.AddListener(() => OnTowerClicked(index));
            }
        }
    }

    public void OnTowerClicked(int spotIndex)
    {
        placementSystem.OnTowerClicked(spotIndex);
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
                enemyPrefabs[config.WaveNumber] = new List<GameObject> { config.Prefab };
        }

        foreach (var config in Settings.TowerConfigs)
        {
            if (config.Prefab != null)
                towerPrefabs[config.Level] = config.Prefab;
        }

        PrefabDatabase.Initialize(enemyPrefabs, towerPrefabs, ProjectilePrefab);
    }

    void OnRestartClicked()
    {
        Debug.Log("Рестарт игры...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}