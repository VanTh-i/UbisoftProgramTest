using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Ref")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI latencyLabel;
    public Slider latencySlider;

    [Header("Game Settings")]
    public float matchDuration = 60f;
    private float currentTime;
    private bool isGameOver = false;

    private void Start()
    {
        currentTime = matchDuration;

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

    private void HandleMessageFromServer(BaseMessage msg)
    {
        if (isGameOver) return;

        if (msg is GameStateUpdateMessage gameState)
        {
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
}
