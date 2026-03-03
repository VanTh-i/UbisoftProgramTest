using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("UI Ref")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI latencyLabel;
    public Slider latencySlider;
    public TextMeshProUGUI playerWiner;
    public GameObject gameOverPanel;

    [Header("Game Settings")]
    public float matchDuration = 60f;
    private float currentTime;
    private bool isGameOver = false;

    private PlayerState[] latestPlayersData;

    private void Start()
    {
        currentTime = matchDuration;
        gameOverPanel.SetActive(false);

        NetworkSimulator.Instance.OnClientReceivedMessage += HandleMessageFromServer;

        if (latencySlider != null)
        {
            latencySlider.minValue = 0f;
            latencySlider.maxValue = 1f;
            latencySlider.value = NetworkSimulator.Instance.SimulatedLatency;
            UpdateLatency(latencySlider.value);
            latencySlider.onValueChanged.AddListener(UpdateLatency);
        }
    }
    private void Update()
    {
        if (isGameOver) return;

        currentTime -= Time.deltaTime;
        if (currentTime <= 0)
        {
            currentTime = 0f;
            isGameOver = true;
            timerText.text = "Time's up!";

            DetermineWinner();
            Time.timeScale = 0f;
        }
        else
        {
            timerText.text = $"Time: " + Mathf.CeilToInt(currentTime).ToString() + "s";
        }
    }

    private void UpdateLatency(float value)
    {
        NetworkSimulator.Instance.SimulatedLatency = value;

        if (latencyLabel != null)
        {
            latencyLabel.text = $"Latency: {value:F2}s";
        }
    }

    private void DetermineWinner()
    {
        if (latestPlayersData == null || latestPlayersData.Length == 0) return;

        gameOverPanel.SetActive(true);
        int highestScore = -1;
        int winnerID = -1;

        foreach (PlayerState p in latestPlayersData)
        {
            if (p.Score > highestScore)
            {
                highestScore = p.Score;
                winnerID = p.PlayerID;
            }
        }

        playerWiner.text = (winnerID == 1) ? "YOU is the WINNER" : $"BOT ID {winnerID} is the WINNER";
    }

    private void HandleMessageFromServer(BaseMessage msg)
    {
        if (isGameOver) return;

        if (msg is GameStateUpdateMessage gameState)
        {
            latestPlayersData = gameState.Players;

            string newScoreText = "--- SCOREBOARD ---\n";

            foreach (PlayerState p in gameState.Players)
            {
                string playerName = (p.PlayerID == 1) ? "<color=green>You (Local): </color>" : $"<color=red>Bot {p.PlayerID}: </color>";
                newScoreText += $"{playerName}: have {p.Score} eggs\n";
            }

            if (scoreText != null)
            {
                scoreText.text = newScoreText;
            }
        }
    }

    //button event
    public void OnPlayAgainButtonClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
