using UnityEngine;
using System.Linq;

public class UISystem : BaseSystem
{
    public GameSettings Settings;

    public override void Execute()
    {
        if (Settings == null) return;
        var player = World.Query<PlayerComponent>().FirstOrDefault();
        var ui = World.Query<UIComponent>().FirstOrDefault();
        var wave = World.Query<WaveComponent>().FirstOrDefault();
        var gameState = World.Query<GameStateComponent>().FirstOrDefault();

        if (player == null || ui == null) return;

        var pComp = player.Get<PlayerComponent>();
        var uiComp = ui.Get<UIComponent>();
        var state = gameState?.Get<GameStateComponent>();

        // 1. Проверка условия поражения
        if (state != null && !state.IsGameOver && pComp.Lives <= 0)
        {
            state.IsGameOver = true;
            state.IsVictory = false;
        }

        // 2. Обновление статистики
        if (uiComp.GoldText != null) uiComp.GoldText.text = $"Gold: {pComp.Gold}";
        if (uiComp.LivesText != null) uiComp.LivesText.text = $"Lives: {pComp.Lives}";
        if (uiComp.WaveText != null && wave != null)
            uiComp.WaveText.text = $"Wave: {wave.Get<WaveComponent>().CurrentWave}/{wave.Get<WaveComponent>().TotalWaves}";

        // 3. Управление панелью с кнопкой
        if (state != null && state.IsGameOver)
        {
            if (uiComp.ActionPanel != null && !uiComp.ActionPanel.activeSelf)
                uiComp.ActionPanel.SetActive(true);

            if (uiComp.ActionText != null)
            {
                uiComp.ActionText.text = state.IsVictory 
                    ? "Победа!"
                    : "Поражение";
            }
        }
        else
        {
            // Во время активной игры панель скрыта
            if (uiComp.ActionPanel != null && uiComp.ActionPanel.activeSelf)
                uiComp.ActionPanel.SetActive(false);
        }
    }
}
