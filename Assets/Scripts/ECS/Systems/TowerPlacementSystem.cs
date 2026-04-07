using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class TowerPlacementSystem : BaseSystem
{
    public GameSettings Settings; // ✅ Настройки
    
    private bool _initialized = false;
    private readonly Dictionary<TowerPosition, Button> _buttons = new();
    private PlayerComponent _playerCache = null;

    public override void Execute()
    {

        var gameState = World.Query<GameStateComponent>().FirstOrDefault();
        if (gameState != null && gameState.Get<GameStateComponent>().IsGameOver)
        {
            foreach (var btn in _buttons.Values)
            {
                if (btn != null && btn.gameObject != null)
                    btn.interactable = false;
            }
            return;
        }

        if (_playerCache == null)
        {
            var player = World.Query<PlayerComponent>().FirstOrDefault();
            if (player != null)
                _playerCache = player.Get<PlayerComponent>();
        }

        if (_playerCache == null) return;

        if (!_initialized)
        {
            InitializeButtons();
            _initialized = true;
        }

        UpdateButtons(_playerCache.Gold);
    }

    void InitializeButtons()
    {
        var ui = World.Query<UIComponent>().FirstOrDefault();
        if (ui == null) return;

        var uiComp = ui.Get<UIComponent>();
        if (uiComp.TowerPositions == null) return;

        foreach (var pos in uiComp.TowerPositions)
        {
            if (pos == null || pos.Button == null) continue;

            _buttons[pos] = pos.Button;
            var position = pos;

            pos.Button.onClick.AddListener(() => OnTowerClicked(position));
        }
    }

    void UpdateButtons(int playerGold)
    {
        foreach (var kvp in _buttons)
        {
            var pos = kvp.Key;
            var btn = kvp.Value;

            if (btn == null || btn.gameObject == null) continue;

            btn.interactable = CanInteract(pos, playerGold);
        }
    }

    bool CanInteract(TowerPosition pos, int playerGold)
    {
        if (!pos.IsEnabled) return false;

        if (pos.IsOccupied)
        {
            return pos.TowerLevel < 3 && playerGold >= GetUpgradeCost(pos.TowerLevel);
        }
        else
        {
            return playerGold >= GetTowerCost(pos.TowerLevel);
        }
    }

    void OnTowerClicked(TowerPosition pos)
    {
        if (_playerCache == null) return;

        if (pos.IsOccupied)
        {
            UpgradeTower(pos);
        }
        else
        {
            PlaceTower(pos);
        }
    }

    void PlaceTower(TowerPosition pos)
    {
        var towerCost = GetTowerCost(pos.TowerLevel);
        if (_playerCache.Gold < towerCost) return;

        var tower = World.CreateEntity();
        

        var config = GetTowerConfig(pos.TowerLevel);
        
        tower.Add(new PositionComponent { Value = pos.Position });
        tower.Add(new RotationComponent { Value = Quaternion.identity });
        tower.Add(new TowerComponent
        {
            Level = pos.TowerLevel,
            Damage = config.Damage,
            Range = config.Range,
            Cooldown = config.Cooldown,
            CooldownTimer = 0f,
            TargetId = -1,
            SpotIndex = pos.GetHashCode()
        });
        tower.Add(new TowerSlotComponent
        {
            SlotIndex = pos.GetHashCode(),
            IsOccupied = true,
            TowerEntityId = tower.Id,
            Position = pos.Position,
            TowerLevel = pos.TowerLevel
        });

        if (config.Prefab != null)
        {
            var obj = Object.Instantiate(config.Prefab, pos.Position, Quaternion.identity);
            tower.Add(new UnityObjectComponent { Obj = obj });
        }

        pos.IsOccupied = true;
        pos.TowerEntityId = tower.Id;
        _playerCache.Gold -= towerCost;

        Debug.Log($"🗼 Башня уровня {pos.TowerLevel} размещена! Урон={config.Damage}, Радиус={config.Range}");
    }

    void UpgradeTower(TowerPosition pos)
    {
        if (pos.TowerLevel >= 3) return;

        var upgradeCost = GetUpgradeCost(pos.TowerLevel);
        if (_playerCache.Gold < upgradeCost) return;

        var tower = World.GetAll().FirstOrDefault(e => 
            e.IsActive && 
            e.Has<TowerComponent>() && 
            e.Get<TowerComponent>().SpotIndex == pos.GetHashCode());

        if (tower != null)
        {
            var tComp = tower.Get<TowerComponent>();
            pos.TowerLevel++;
            tComp.Level = pos.TowerLevel;


            var config = GetTowerConfig(pos.TowerLevel);
            tComp.Damage = config.Damage;
            tComp.Range = config.Range;
            tComp.Cooldown = config.Cooldown;

            if (tower.Has<UnityObjectComponent>())
            {
                var view = tower.Get<UnityObjectComponent>();
                if (view.Obj != null)
                {
                    Object.Destroy(view.Obj);
                    if (config.Prefab != null)
                    {
                        var newObj = Object.Instantiate(config.Prefab, pos.Position, Quaternion.identity);
                        view.Obj = newObj;
                    }
                }
            }

            _playerCache.Gold -= upgradeCost;
            Debug.Log($"⬆️ Башня улучшена до уровня {pos.TowerLevel}! Урон={config.Damage}, Радиус={config.Range}");
        }
    }


    int GetTowerCost(int level)
    {
        return Settings.BaseTowerCost;
    }


    int GetUpgradeCost(int currentLevel)
    {


        float cost = Settings.BaseTowerCost * Settings.UpgradeCostMultiplier;
        cost *= Mathf.Pow(Settings.UpgradeCostIncrease, currentLevel - 1);
        
        return Mathf.RoundToInt(cost);
    }


    TowerLevelConfig GetTowerConfig(int level)
    {
        foreach (var config in Settings.TowerConfigs)
        {
            if (config.Level == level)
                return config;
        }
        

        Debug.LogWarning($"⚠️ TowerConfig для уровня {level} не найден! Используем значения по умолчанию.");
        return new TowerLevelConfig 
        { 
            Level = level, 
            Damage = 20f, 
            Range = 10f, 
            Cooldown = 2f 
        };
    }

    public override void Cleanup()
    {
        foreach (var btn in _buttons.Values)
        {
            if (btn != null)
                btn.onClick.RemoveAllListeners();
        }
        _buttons.Clear();
    }
}