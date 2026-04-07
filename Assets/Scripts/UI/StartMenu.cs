using UnityEngine;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    [SerializeField] private GameObject startMenuPanel;
    [SerializeField] private Button startButton;

    private void Awake()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }

        if (startMenuPanel != null)
        {
            startMenuPanel.SetActive(true);
        }
    }

    private void StartGame()
    {
        if (startMenuPanel != null)
        {
            startMenuPanel.SetActive(false);
        }

        GameManager.Instance.StartGame();
    }
}
