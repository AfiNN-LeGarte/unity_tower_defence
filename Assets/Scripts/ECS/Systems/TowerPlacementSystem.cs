using UnityEngine;
using System.Linq;

public class TowerPlacementSystem : BaseSystem
{
    public GameSettings Settings;

    public override void Execute()
    {
        if (Settings == null) return;

        var gameState = World.Query<GameStateComponent>().FirstOrDefault();
        if (gameState?.Get<GameStateComponent>().IsGameOver == true) return;

        var player = World.Query<PlayerComponent>().FirstOrDefault();
        if (player == null) return;
        var playerComp = player.Get<PlayerComponent>();

        var ui = World.Query<UIComponent>().FirstOrDefault();
        if (ui == null) return;

        foreach (var pos in ui.Get<UIComponent>().TowerPositions)
        {
            if (pos == null || pos.Button == null || !pos.IsEnabled) continue;

            var tower = World.Query<TowerComponent>()
                .FirstOrDefault(e => e.Get<TowerComponent>().SpotIndex == pos.Index);

            bool isOccupied = tower != null;
            int cost = isOccupied ? GetUpgradeCost(tower.Get<TowerComponent>().Level) : Settings.BaseTowerCost;
            bool canAfford = playerComp.Gold >= cost;

            pos.Button.interactable = canAfford;

            if (pos.CostText != null)
            {
                if (isOccupied)
                {
                    var level = tower.Get<TowerComponent>().Level;
                    pos.CostText.text = level >= 3 ? "MAX" : $"UP {cost}";
                    pos.CostText.color = canAfford && level < 3 ? Color.green : Color.red;
                }
                else
                {
                    pos.CostText.text = $"Buy {cost}";
                    pos.CostText.color = canAfford ? Color.white : Color.red;
                }
            }
        }
    }

    public void OnTowerClicked(int spotIndex)
    {
        if (Settings == null) return;

        var player = World.Query<PlayerComponent>().FirstOrDefault();
        if (player == null) return;
        var playerComp = player.Get<PlayerComponent>();

        var ui = World.Query<UIComponent>().FirstOrDefault();
        if (ui == null) return;

        var pos = ui.Get<UIComponent>().TowerPositions.FirstOrDefault(p => p.Index == spotIndex);
        if (pos == null) return;

        var tower = World.Query<TowerComponent>()
            .FirstOrDefault(e => e.Get<TowerComponent>().SpotIndex == spotIndex);

        if (tower != null)
            UpgradeTower(tower, pos, playerComp);
        else
            PlaceTower(pos, playerComp);
    }

    void PlaceTower(TowerPosition pos, PlayerComponent player)
    {
        int cost = Settings.BaseTowerCost;
        if (player.Gold < cost) return;

        var config = GetTowerConfig(1);
        var tower = World.CreateEntity();

        tower.Add(new PositionComponent { Value = pos.Position });
        tower.Add(new RotationComponent { Value = Quaternion.identity });
        tower.Add(new TowerComponent
        {
            Level = 1,
            SpotIndex = pos.Index,
            Damage = config.Damage,
            Range = config.Range,
            Cooldown = config.Cooldown,
            CooldownTimer = 0f,
            TargetId = -1
        });

        if (config.Prefab != null)
        {
            var obj = Object.Instantiate(config.Prefab, pos.Position, Quaternion.identity);
            tower.Add(new UnityObjectComponent { Obj = obj });
        }

        player.Gold -= cost;
    }

    void UpgradeTower(Entity tower, TowerPosition pos, PlayerComponent player)
    {
        var tComp = tower.Get<TowerComponent>();
        if (tComp.Level >= 3) return;

        int cost = GetUpgradeCost(tComp.Level);
        if (player.Gold < cost) return;

        int newLevel = tComp.Level + 1;
        var config = GetTowerConfig(newLevel);

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
    }

    int GetUpgradeCost(int level)
    {
        return Mathf.RoundToInt(Settings.BaseTowerCost * Settings.UpgradeCostMultiplier *
            Mathf.Pow(Settings.UpgradeCostIncrease, level - 1));
    }

    TowerLevelConfig GetTowerConfig(int level)
    {
        foreach (var c in Settings.TowerConfigs)
            if (c.Level == level) return c;
        return new TowerLevelConfig { Level = level, Damage = 20f, Range = 10f, Cooldown = 2f };
    }
}
