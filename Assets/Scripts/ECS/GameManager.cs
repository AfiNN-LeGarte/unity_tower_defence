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

    public GameObject GoldTextObj;
    public GameObject LivesTextObj;
    public GameObject WaveTextObj;

    public GameObject GameOverCanvas;
    public GameObject RestartButtonObj;
    public GameObject StartPanel;
    
    public GameObject VictoryTextObj; // Текст победы (назначить в инспекторе)
    public GameObject DefeatTextObj;  // Текст поражения (назначить в инспекторе)

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

        if (GameOverCanvas != null)
            GameOverCanvas.SetActive(false);

        if (StartPanel != null)
            StartPanel.SetActive(true);

        InitializePrefabDatabase();
    }

    public void StartGame()
    {
        if (isGameStarted) return;
        isGameStarted = true;

        if (StartPanel != null)
            StartPanel.SetActive(false);

        world = new World();

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
            GoldTextObj = GoldTextObj,
            LivesTextObj = LivesTextObj,
            WaveTextObj = WaveTextObj,
            RestartButtonObj = RestartButtonObj,
            TowerPositions = TowerPositions,
            StartPanel = StartPanel,
            VictoryTextObj = VictoryTextObj,
            DefeatTextObj = DefeatTextObj
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

        SubscribeToButtons();

        SubscribeToRestartButton();

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


    void SubscribeToRestartButton()
    {
        if (RestartButtonObj != null)
        {
            var button = RestartButtonObj.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
                button.onClick.AddListener(OnRestartClicked);
        }
    }

    public void OnTowerClicked(int spotIndex)
    {
        placementSystem?.OnTowerClicked(spotIndex);
    }

    public void OnStartButtonClicked()
    {
        if (!isGameStarted)
        {
            StartGame();
        }
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
        
        // Сбрасываем состояние игры (уничтожаем башни, очищаем мир)
        ResetGameState(showStartPanel: false);
        
        // Сразу запускаем игру заново
        StartGame();
        
        Debug.Log("Игра перезапущена");
    }

    public void ResetGameState(bool showStartPanel = true)
    {
        // Сначала уничтожаем все объекты башен
        if (world != null)
        {
            var towers = world.Query<TowerComponent>().ToList();
            foreach (var tower in towers)
            {
                if (tower.Has<UnityObjectComponent>())
                {
                    var view = tower.Get<UnityObjectComponent>();
                    if (view.Obj != null)
                        Object.Destroy(view.Obj);
                }
            }
            
            // Также уничтожаем все остальные объекты (враги, снаряды и т.д.)
            var allObjects = world.Query<UnityObjectComponent>().ToList();
            foreach (var obj in allObjects)
            {
                if (obj.Obj != null)
                    Object.Destroy(obj.Obj);
            }
            
            // Очищаем все сущности мира
            var allEntities = world.GetAll().ToList();
            foreach (var entity in allEntities)
            {
                world.DestroyEntity(entity);
            }
        }
        
        world?.Cleanup();
        world = null;
        isGameStarted = false;
        
        if (GameOverCanvas != null)
            GameOverCanvas.SetActive(false);
        
        if (RestartButtonObj != null)
            RestartButtonObj.SetActive(false);
        
        // Скрываем тексты победы/поражения
        if (VictoryTextObj != null)
            VictoryTextObj.SetActive(false);
        if (DefeatTextObj != null)
            DefeatTextObj.SetActive(false);
        
        if (StartPanel != null && showStartPanel)
            StartPanel.SetActive(true);
    }
}
