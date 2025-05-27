using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class HD_GameManager : MonoBehaviour
{
    public static HD_GameManager Instance;

    [Header("Game State")]
    public bool isGamePlaying = false;
    public int currentScore = 0;
    public int highScore = 0;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text bestScoreText;
    [SerializeField] private TMP_Text tapToPlayText;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Coin Spawning")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private Transform coinParent;
    [SerializeField] private float coinSpawnRate = 2f;
    private int activeCoins = 0;
    private float coinTimer = 0f;

    [Header("Game Settings")]
    [SerializeField] private float scoreTimeInterval = 1f;

    [Header("References")]
    [SerializeField] private HD_Player playerScript;
    [SerializeField] private HD_Obstacle[] obstacles;

    private float scoreTimer;
    private const string HIGHSCOREKEY = "HighScore";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (playerScript == null)
            playerScript = GetComponent<HD_Player>();
            
        ShowTapToPlay();
    }

    private void Update()
    {
        if (!isGamePlaying && Input.GetMouseButtonDown(0))
        {
            StartGame();
        }

        if (isGamePlaying)
        {
            scoreTimer += Time.deltaTime;
            if (scoreTimer >= scoreTimeInterval)
            {
                AddScore(1);
                scoreTimer = 0f;
            }

            coinTimer += Time.deltaTime;
            if (coinTimer >= coinSpawnRate && activeCoins < 2)
            {
                SpawnCoin();
                coinTimer = 0f;
            }
        }
    }

    private void Init()
    {
        currentScore = 0;
        scoreTimer = 0f;
        coinTimer = 0f;
        activeCoins = 0;
        isGamePlaying = false;

        highScore = PlayerPrefs.GetInt(HIGHSCOREKEY, 0);
        UpdateScoreUI();
    }

    private void ShowTapToPlay()
    {
        isGamePlaying = false;
        
        if (tapToPlayText != null)
        {
            tapToPlayText.gameObject.SetActive(true);
            Debug.Log("Tap to Play shown");
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateBestScoreUI();
    }

    public void StartGame()
    {
        Debug.Log("Starting Game");
        
        isGamePlaying = true;
        currentScore = 0;
        scoreTimer = 0f;
        coinTimer = 0f;
        activeCoins = 0;
        
        if (tapToPlayText != null)
            tapToPlayText.gameObject.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (playerScript != null)
            playerScript.ResetPlayer();

        ResetAllObstacles();

        UpdateScoreUI();
        ClearCoins();
    }

    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = currentScore.ToString();
    }

    private void UpdateBestScoreUI()
    {
        if (bestScoreText != null)
            bestScoreText.text = "Best: " + highScore.ToString();
    }

    public void GameOver()
    {
        Debug.Log("Game Over");
        
        isGamePlaying = false;
        StopAllCoroutines();

        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt(HIGHSCOREKEY, highScore);
            PlayerPrefs.Save();
        }

        ShowGameOverPanel();
    }

    private void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    private void SpawnCoin()
    {
        if (coinPrefab == null || coinParent == null) return;

        GameObject coin = Instantiate(coinPrefab, coinParent);
        activeCoins++;
    }

    public void OnCoinCollected()
    {
        activeCoins = Mathf.Max(0, activeCoins - 1);
    }

    private void ClearCoins()
    {
        if (coinParent != null)
        {
            foreach (Transform child in coinParent)
            {
                Destroy(child.gameObject);
            }
        }
        activeCoins = 0;
    }

    private void ResetAllObstacles()
    {
        foreach (HD_Obstacle obstacle in obstacles)
        {
            obstacle.ResetObstacle();
        }
    }

    public void RestartGame()
    {
        Debug.Log("Restarting Game");
        
        StopAllCoroutines();
        ClearCoins();

        if (playerScript != null)
            playerScript.ResetPlayer();

        ResetAllObstacles();
        Init();
        ShowTapToPlay();
    }
}