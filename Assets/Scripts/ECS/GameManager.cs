using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public GameSettings Settings;

    public List<GameObject> PathPoints;

    public List<TowerPosition> TowerPositions = new();

    public GameObject ProjectilePrefab;

    // UI поля для статистики
    public GameObject GoldTextObj;
    public GameObject LivesTextObj;
    public GameObject WaveTextObj;

    // Новая единая панель действий (старт/рестарт)
    public GameObject ActionPanel;
    public UnityEngine.UI.Button ActionButton;
    public UnityEngine.UI.Text ActionText;

    World world;
    TowerPlacementSystem placementSystem;
    bool isGameStarted = false;

    void Start()
    {
        if (Settings == null)
        {
            Debug.LogError("GameSettings не назначен в Inspector!");
            return;
        }

        InitializePrefabDatabase();

        // Начальное состояние панели до старта игры
        if (ActionPanel != null) ActionPanel.SetActive(true);
        if (ActionText != null) ActionText.text = "НАЧАТЬ ИГРУ";
        if (ActionButton != null)
        {
            ActionButton.onClick.RemoveAllListeners();
            ActionButton.onClick.AddListener(OnActionClicked);
        }
    }

    public void StartGame()
    {
        if (isGameStarted) return;
        isGameStarted = true;

        if (ActionPanel != null) ActionPanel.SetActive(false);

        world = new World();

        var gameState = world.CreateEntity();
        gameState.Add(new GameStateComponent
        {
            IsGameOver = false,
            IsPaused = false,
            GameOverCanvas = null
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
            GoldText = GoldTextObj?.GetComponent<UnityEngine.UI.Text>(),
            LivesText = LivesTextObj?.GetComponent<UnityEngine.UI.Text>(),
            WaveText = WaveTextObj?.GetComponent<UnityEngine.UI.Text>(),
            ActionPanel = ActionPanel,
            ActionButton = ActionButton,
            ActionText = ActionText
        });

        placementSystem = new TowerPlacementSystem { Settings = Settings };

        world.AddSystem(new SpawnSystem
        {
            PathPoints = PathPoints,
            Settings = Settings
        });
        world.AddSystem(new MovementSystem { Settings = Settings });
        world.AddSystem(new TowerSystem { Settings = Settings });
        world.AddSystem(new ProjectileSystem { Settings = Settings });
        world.AddSystem(new CleanupSystem());
        world.AddSystem(new ViewSystem());
        world.AddSystem(new UISystem { Settings = Settings });
        world.AddSystem(placementSystem);

        Debug.Log("Игра запущена: " + Settings.name);
    }

    void Update()
    {
        if (!isGameStarted || world == null) return;
        world.Update();
    }

    void OnDestroy()
    {
        world?.Cleanup();
    }

    public void OnTowerClicked(int spotIndex)
    {
        placementSystem?.OnTowerClicked(spotIndex);
    }

    public void OnActionClicked()
    {
        if (isGameStarted)
            ResetGameState(); // Если игра идёт или закончена - сбрасываем
        StartGame(); // Всегда запускаем после сброса или при первом старте
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

    public void ResetGameState()
    {
        if (world != null)
        {
            var allEntities = world.GetAll().ToList();
            foreach (var entity in allEntities)
            {
                if (entity.Has<UnityObjectComponent>())
                {
                    var view = entity.Get<UnityObjectComponent>();
                    if (view.Obj != null) Object.Destroy(view.Obj);
                }
                world.DestroyEntity(entity);
            }
        }
        world?.Cleanup();
        world = null;
        isGameStarted = false;
        // UI панель пока не трогаем. При следующем кадре UISystem или StartGame() выставит её состояние.
    }
}
