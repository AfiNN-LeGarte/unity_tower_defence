using UnityEngine;
using UnityEngine.EventSystems;
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

    public GameObject ActionPanel;
    public UnityEngine.UI.Button ActionButton;
    public UnityEngine.UI.Text ActionText;
    public UnityEngine.UI.Button BuyTowerButton;

    public bool IsPlacementMode = false;

    World world;
    TowerPlacementSystem placementSystem;
    bool isGameStarted = false;
    Dictionary<GameObject, int> spotIndexMap = new();

    void Start()
    {
        if (Settings == null) { Debug.LogError("GameSettings не назначен!"); return; }

        InitializePrefabDatabase();

        spotIndexMap.Clear();
        for (int i = 0; i < TowerPositions.Count; i++)
        {
            var pos = TowerPositions[i];
            if (pos == null) continue;

            pos.Index = i;
            if (pos.Spot != null)
            {
                spotIndexMap[pos.Spot] = i;
            }
        }

        if (ActionPanel != null) ActionPanel.SetActive(true);
        if (ActionText != null) ActionText.text = "ИГРАТЬ";
        if (ActionButton != null)
        {
            ActionButton.onClick.RemoveAllListeners();
            ActionButton.onClick.AddListener(OnActionClicked);
        }
        if (BuyTowerButton != null)
        {
            BuyTowerButton.onClick.RemoveAllListeners();
            BuyTowerButton.onClick.AddListener(OnBuyTowerClicked);
        }
        UpdateSpotVisuals();
    }

    void Update()
    {
        if (!isGameStarted || world == null) return;
        world.Update();

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (spotIndexMap.TryGetValue(hit.collider.gameObject, out int spotIndex))
                {
                    OnTowerClicked(spotIndex);
                }
            }
        }
    }

    int GetSpotIndexFromHit(GameObject obj)
    {
        GameObject current = obj;
        while (current != null)
        {
            if (spotIndexMap.TryGetValue(current, out int index))
                return index;

            current = current.transform.parent?.gameObject;
        }
        return -1;
    }

    void OnBuyTowerClicked()
    {
        if (IsPlacementMode)
        {
            IsPlacementMode = false;
            UpdateSpotVisuals();
            return;
        }

        var player = world?.Query<PlayerComponent>().FirstOrDefault();
        if (player != null && player.Get<PlayerComponent>().Gold >= Settings.BaseTowerCost)
        {
            IsPlacementMode = true;
            UpdateSpotVisuals();
        }
        else
        {
            Debug.Log("Недостаточно золота!");
        }
    }

    void UpdateSpotVisuals()
    {
        string txt = IsPlacementMode ? "ВЫБЕРИТЕ МЕСТО" : $"КУПИТЬ ({Settings.BaseTowerCost})";
        var btnTxt = BuyTowerButton?.GetComponent<UnityEngine.UI.Text>();
        if (btnTxt != null) btnTxt.text = txt;

        foreach (var pos in TowerPositions)
        {
            if (pos.Spot == null) continue;

            bool isOccupied = placementSystem?.IsSpotOccupied(pos.Index) ?? false;
            bool isAvailable = IsPlacementMode && !isOccupied;

            if (pos.SphereRenderer != null)
            {
                Color c = isAvailable ? Color.green : (isOccupied ? Color.gray : Color.white);
                var block = new MaterialPropertyBlock();
                block.SetColor("_Color", c);
                pos.SphereRenderer.SetPropertyBlock(block);
            }
            pos.Spot.transform.localScale = Vector3.one * (isAvailable ? 1.25f : 1.0f);
        }
    }

    public void OnTowerClicked(int index)
    {
        if (IsPlacementMode)
        {
            if (placementSystem?.IsSpotOccupied(index) == true)
            {
                Debug.Log("Место занято!");
                return;
            }

            if (placementSystem?.TryPlaceNewTower(index) == true)
            {
                IsPlacementMode = false;
                UpdateSpotVisuals();
            }
        }
        else
        {
            if (placementSystem?.IsSpotOccupied(index) == true)
            {
                placementSystem?.OnTowerClicked(index);
            }
            else
            {
                Debug.Log("Здесь нет башни. Нажмите 'Купить' для размещения.");
            }
        }
    }

    public void StartGame()
    {
        if (isGameStarted) return;
        isGameStarted = true;
        if (ActionPanel != null) ActionPanel.SetActive(false);

        world = new World();
        world.CreateEntity().Add(new GameStateComponent { IsGameOver = false, IsPaused = false, GameOverCanvas = null });

        var wave = world.CreateEntity();
        wave.Add(new WaveComponent {
            CurrentWave = 1, TotalWaves = Settings.TotalWaves,
            WaveTimer = Settings.FirstWaveDelay, WaveInterval = Settings.WaveInterval,
            WaveActive = false, EnemiesToSpawn = GetEnemiesForWave(1), SpawnedCount = 0
        });

        var player = world.CreateEntity();
        player.Add(new PlayerComponent { Gold = Settings.StartGold, Lives = Settings.StartLives });

        world.CreateEntity().Add(new UIComponent {
            GoldText = GoldTextObj?.GetComponent<UnityEngine.UI.Text>(),
                                 LivesText = LivesTextObj?.GetComponent<UnityEngine.UI.Text>(),
                                 WaveText = WaveTextObj?.GetComponent<UnityEngine.UI.Text>(),
                                 ActionPanel = ActionPanel, ActionButton = ActionButton, ActionText = ActionText
        });

        placementSystem = new TowerPlacementSystem { Settings = Settings, TowerPositions = TowerPositions };

        world.AddSystem(new SpawnSystem { PathPoints = PathPoints, Settings = Settings });
        world.AddSystem(new MovementSystem { Settings = Settings });
        world.AddSystem(new TowerSystem { Settings = Settings });
        world.AddSystem(new ProjectileSystem { Settings = Settings });
        world.AddSystem(new CleanupSystem());
        world.AddSystem(new ViewSystem());
        world.AddSystem(new UISystem { Settings = Settings });
        world.AddSystem(placementSystem);
    }

    public void OnActionClicked() { if (isGameStarted) ResetGameState(); StartGame(); }
    public void ResetGameState()
    {
        if (world != null) {
            var list = world.GetAll().ToList();
            foreach(var e in list) {
                if(e.Has<UnityObjectComponent>()) Object.Destroy(e.Get<UnityObjectComponent>().Obj);
                world.DestroyEntity(e);
            }
        }
        world?.Cleanup();
        world = null;
        isGameStarted = false;
        IsPlacementMode = false;
    }
    void OnDestroy() { world?.Cleanup(); }
    int GetEnemiesForWave(int w) => w switch { 1=>Settings.Wave1Enemies, 2=>Settings.Wave2Enemies, 3=>Settings.Wave3Enemies, _=>5 };
    void InitializePrefabDatabase()
    {
        var e = new Dictionary<int, List<GameObject>>();
        var t = new Dictionary<int, GameObject>();
        foreach (var c in Settings.EnemyConfigs) if(c.Prefab!=null) e[c.WaveNumber] = new List<GameObject>{c.Prefab};
        foreach (var c in Settings.TowerConfigs) if(c.Prefab!=null) t[c.Level] = c.Prefab;
        PrefabDatabase.Initialize(e, t, ProjectilePrefab);
    }
}
