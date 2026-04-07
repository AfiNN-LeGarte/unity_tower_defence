using UnityEngine;
using System.Linq;

public class UISystem : BaseSystem
{
    public GameSettings Settings; // ✅ Настройки
    
    public override void Execute()
    {
        var player = World.Query<PlayerComponent>().FirstOrDefault();
        var ui = World.Query<UIComponent>().FirstOrDefault();
        var wave = World.Query<WaveComponent>().FirstOrDefault();
        var gameState = World.Query<GameStateComponent>().FirstOrDefault();

        if (player == null || ui == null) return;

        var playerComp = player.Get<PlayerComponent>();
        var uiComp = ui.Get<UIComponent>();


        if (gameState != null)
        {
            var state = gameState.Get<GameStateComponent>();
            
            if (playerComp.Lives <= 0 && !state.IsGameOver)
            {
                state.IsGameOver = true;
                
                if (state.GameOverCanvas != null)
                {
                    state.GameOverCanvas.SetActive(true);
                }
                
                Debug.Log("GAME OVER!");
            }
            
            if (state.IsGameOver)
            {
                return;
            }
        }


        if (uiComp.GoldText != null)
            uiComp.GoldText.text = $"Gold {playerComp.Gold}";

        if (uiComp.LivesText != null)
            uiComp.LivesText.text = $"Lives: {playerComp.Lives}";

        if (uiComp.WaveText != null && wave != null)
            uiComp.WaveText.text = $"Волна: {wave.Get<WaveComponent>().CurrentWave}/{wave.Get<WaveComponent>().TotalWaves}";


        if (uiComp.TowerPositions != null)
        {
            foreach (var towerPos in uiComp.TowerPositions)
            {
                if (towerPos == null || towerPos.CostText == null) continue;
                

                UpdateTowerButtonText(towerPos, playerComp.Gold, Settings);
            }
        }
    }


    void UpdateTowerButtonText(TowerPosition pos, int playerGold, GameSettings settings)
    {
        if (!pos.IsEnabled)
        {
            pos.CostText.text = "Block";
            pos.CostText.color = Color.red;
        }
        else if (pos.IsOccupied)
        {

            int upgradeCost = Mathf.RoundToInt(settings.BaseTowerCost * settings.UpgradeCostMultiplier);
            
            if (pos.TowerLevel < 3 && playerGold >= upgradeCost)
            {
                pos.CostText.text = $"UP {GetUpgradeCost(pos.TowerLevel)}";
                pos.CostText.color = Color.green;
            }
            else if (pos.TowerLevel >= 3)
            {
                pos.CostText.text = "MAX";
                pos.CostText.color = Color.gray;
            }
            else
            {
                pos.CostText.text = $"UP {upgradeCost}";
                pos.CostText.color = Color.red;
            }
        }
        else
        {

            int towerCost = settings.BaseTowerCost;
            
            if (playerGold >= towerCost)
            {
                pos.CostText.text = $"Buy: {towerCost}";
                pos.CostText.color = Color.black;
            }
            else
            {
                pos.CostText.text = $"Buy: {towerCost}";
                pos.CostText.color = Color.red;
            }
        }
    }

    int GetUpgradeCost(int currentLevel)
    {
        float cost = Settings.BaseTowerCost * Settings.UpgradeCostMultiplier;
        cost *= Mathf.Pow(Settings.UpgradeCostIncrease, currentLevel - 1);
        return Mathf.RoundToInt(cost);
    }
}