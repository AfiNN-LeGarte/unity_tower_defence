using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class TowerPlacementSystem : BaseSystem
{
    public GameSettings Settings;
    public List<TowerPosition> TowerPositions;

    public override void Execute()
    {
        if (Settings == null || TowerPositions == null) return;
        var gameState = World.Query<GameStateComponent>().FirstOrDefault();
        if (gameState?.Get<GameStateComponent>().IsGameOver == true) return;

        var player = World.Query<PlayerComponent>().FirstOrDefault();
        if (player == null) return;
        var playerComp = player.Get<PlayerComponent>();

        foreach (var pos in TowerPositions)
        {
            if (pos == null || pos.CostText == null) continue;

            if (pos.Button != null && pos.Button.interactable)
                pos.Button.interactable = false;

            var tower = World.Query<TowerComponent>()
            .FirstOrDefault(e => e.Get<TowerComponent>().SpotIndex == pos.Index);
            bool isOccupied = tower != null;
            int cost = isOccupied ? GetUpgradeCost(tower.Get<TowerComponent>().Level) : Settings.BaseTowerCost;
            bool canAfford = playerComp.Gold >= cost;

            if (isOccupied)
            {
                var level = tower.Get<TowerComponent>().Level;
                pos.CostText.text = level >= 3 ? "MAX" : $"UP {cost}";
                pos.CostText.color = canAfford && level < 3 ? Color.green : Color.red;
            }
            else
            {
                pos.CostText.text = $"Buy {cost}";
                pos.CostText.color = canAfford ? Color.white : Color.gray;
            }
        }
    }

    public bool TryPlaceNewTower(int spotIndex)
    {
        int cost = Settings.BaseTowerCost;
        var player = World.Query<PlayerComponent>().FirstOrDefault();
        if (player == null || player.Get<PlayerComponent>().Gold < cost)
        {
            Debug.Log("[ECS] Недостаточно золота для постройки!");
            return false;
        }

        var pos = TowerPositions?.FirstOrDefault(p => p.Index == spotIndex);
        if (pos == null) return false;

        if (IsSpotOccupied(spotIndex))
        {
            Debug.Log("[ECS] Место уже занято!");
            return false;
        }

        PlaceTower(pos, player.Get<PlayerComponent>());
        return true;
    }

    public void OnTowerClicked(int spotIndex)
    {
        var player = World.Query<PlayerComponent>().FirstOrDefault();
        if (player == null) return;
        var playerComp = player.Get<PlayerComponent>();

        var pos = TowerPositions?.FirstOrDefault(p => p.Index == spotIndex);
        if (pos == null) return;

        var tower = World.Query<TowerComponent>()
        .FirstOrDefault(e => e.Get<TowerComponent>().SpotIndex == spotIndex);

        if (tower != null)
            UpgradeTower(tower, pos, playerComp);
        else
            Debug.Log("[ECS] На этом месте нет башни. Используйте кнопку покупки.");
    }

    public bool IsSpotOccupied(int index) =>
    World.Query<TowerComponent>().Any(e => e.Get<TowerComponent>().SpotIndex == index);

    private void PlaceTower(TowerPosition pos, PlayerComponent player)
    {
        int cost = Settings.BaseTowerCost;
        if (player.Gold < cost) return;

        var config = GetTowerConfig(1);
        if (config == null) { Debug.LogError("[ECS] Tower Config Level 1 not found!"); return; }

        var tower = World.CreateEntity();
        tower.Add(new PositionComponent { Value = pos.Position });
        tower.Add(new RotationComponent { Value = Quaternion.identity });
        tower.Add(new TowerComponent
        {
            Level = 1, SpotIndex = pos.Index, Damage = config.Damage,
            Range = config.Range, Cooldown = config.Cooldown, CooldownTimer = 0f, TargetId = -1
        });

        if (config.Prefab != null)
        {
            var obj = Object.Instantiate(config.Prefab, pos.Position, Quaternion.identity);
            obj.transform.SetParent(pos.Spot.transform, worldPositionStays: true); // <-- Важно!
            tower.Add(new UnityObjectComponent { Obj = obj });
        }

        player.Gold -= cost;
        Debug.Log("[ECS] Башня успешно построена!");
    }

    private void UpgradeTower(Entity tower, TowerPosition pos, PlayerComponent player)
    {
        var tComp = tower.Get<TowerComponent>();
        if (tComp.Level >= 3)
        {
            Debug.Log("[ECS] Башня максимального уровня!");
            return;
        }

        int cost = GetUpgradeCost(tComp.Level);
        if (player.Gold < cost)
        {
            Debug.Log($"[ECS] Недостаточно золота для улучшения. Нужно: {cost}");
            return;
        }

        int newLevel = tComp.Level + 1;
        var config = GetTowerConfig(newLevel);
        if (config == null) return;

        tComp.Level = newLevel;
        tComp.Damage = config.Damage;
        tComp.Range = config.Range;
        tComp.Cooldown = config.Cooldown;

        if (tower.Has<UnityObjectComponent>())
        {
            var view = tower.Get<UnityObjectComponent>();
            if (view.Obj != null) Object.Destroy(view.Obj);
            if (config.Prefab != null)
                view.Obj = Object.Instantiate(config.Prefab, pos.Position, Quaternion.identity);
        }
        player.Gold -= cost;
        Debug.Log("[ECS] Башня улучшена!");
    }

    private int GetUpgradeCost(int level) =>
    Mathf.RoundToInt(Settings.BaseTowerCost * Settings.UpgradeCostMultiplier *
    Mathf.Pow(Settings.UpgradeCostIncrease, level - 1));

    private TowerLevelConfig GetTowerConfig(int level)
    {
        foreach (var c in Settings.TowerConfigs)
            if (c.Level == level) return c;
            return new TowerLevelConfig { Level = level, Damage = 20f, Range = 10f, Cooldown = 2f };
    }
}
