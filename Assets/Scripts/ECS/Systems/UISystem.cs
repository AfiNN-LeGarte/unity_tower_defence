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

        // Проверяем, игра уже закончена? Если да - просто выходим
        if (state != null && state.IsGameOver)
            return;

        // Проверка на поражение (жизни <= 0)
        if (pComp.Lives <= 0)
        {
            state.IsGameOver = true;
            state.IsVictory = false; // Поражение
            
            if (state.GameOverCanvas != null) 
            {
                state.GameOverCanvas.SetActive(true);
                
                // Показываем кнопку рестарт через ссылку из UIComponent
                if (uiComp.RestartButtonObj != null)
                    uiComp.RestartButtonObj.SetActive(true);
            }

            // Скрываем StartPanel если есть
            if (uiComp.StartPanel != null)
                uiComp.StartPanel.SetActive(false);
            
            // Показываем текст поражения, скрываем текст победы
            if (uiComp.DefeatTextObj != null)
                uiComp.DefeatTextObj.SetActive(true);
            if (uiComp.VictoryTextObj != null)
                uiComp.VictoryTextObj.SetActive(false);
            
            // Обновляем текст жизней перед выходом
            UnityEngine.UI.Text livesTextLocal = null;
            if (uiComp.LivesTextObj != null) livesTextLocal = uiComp.LivesTextObj.GetComponent<UnityEngine.UI.Text>();
            if (livesTextLocal != null) livesTextLocal.text = $"Lives: {pComp.Lives}";
            
            return; // Выходим сразу, чтобы не обновлять остальной UI
        }

        // Обычное обновление UI во время игры
        UnityEngine.UI.Text goldText = null;
        UnityEngine.UI.Text livesText = null;
        UnityEngine.UI.Text waveText = null;

        if (uiComp.GoldTextObj != null) goldText = uiComp.GoldTextObj.GetComponent<UnityEngine.UI.Text>();
        if (uiComp.LivesTextObj != null) livesText = uiComp.LivesTextObj.GetComponent<UnityEngine.UI.Text>();
        if (uiComp.WaveTextObj != null) waveText = uiComp.WaveTextObj.GetComponent<UnityEngine.UI.Text>();

        if (goldText != null) goldText.text = $"Gold: {pComp.Gold}";
        if (livesText != null) livesText.text = $"Lives: {pComp.Lives}";
        if (waveText != null && wave != null)
            waveText.text = $"Wave: {wave.Get<WaveComponent>().CurrentWave}/{wave.Get<WaveComponent>().TotalWaves}";
    }
}
