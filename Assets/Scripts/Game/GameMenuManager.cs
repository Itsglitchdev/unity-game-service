using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Authentication;
using System;

public class GameMenuManager : MonoBehaviour
{
    [Header("NavigationButtons")]
    [SerializeField] private Button signOutButton;
    [SerializeField] private Button leaderboardButton;

    [Header("GameButtons")]
    [SerializeField] private Button helixDriftButton;

    [Header("UI Panels")]
    [SerializeField] private GameObject gameListPanel;
    [SerializeField] private GameObject leaderboardPanel;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI playerNameText;

    private const string LogoutKey = "UserLoggedOut";

    private void Start()
    {
        EventListners();
        UpdatePlayerInfo();
        gameListPanel.SetActive(true);
        leaderboardPanel.SetActive(false);
    }

    private void EventListners()
    {
        signOutButton.onClick.AddListener(OnSignOutButtonClicked);
        helixDriftButton.onClick.AddListener(OnHelixDriftButtonClicked);
        leaderboardButton.onClick.AddListener(OnLeaderboardButtonClicked);
    }

    private void OnEnable()
    {
        UpdatePlayerInfo();
    }

    private void UpdatePlayerInfo()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            var playerInfo = AuthenticationService.Instance.PlayerInfo;

            if (string.IsNullOrEmpty(playerInfo?.Username))
            {
                // For anonymous users, use the PlayerName if set, otherwise generate consistent guest name
                string playerName = AuthenticationService.Instance.PlayerName;
                if (!string.IsNullOrEmpty(playerName))
                {
                    playerNameText.text = playerName;
                }
                else
                {
                    string playerId = AuthenticationService.Instance.PlayerId;
                    playerNameText.text = GetConsistentGuestName(playerId);
                }
            }
            else
            {
                playerNameText.text = playerInfo.Username;
            }
        }
        else
        {
            playerNameText.text = "Not signed in";
            SceneLoader.LoadScene(SceneName.MainMenu);
        }
    }

    private string GetConsistentGuestName(string playerId)
    {
        return $"Player_{playerId.Substring(0, 2)}";
    }

    private void OnSignOutButtonClicked()
    {
        try
        {
            AuthenticationService.Instance.SignOut(true);

            PlayerPrefs.SetInt(LogoutKey, 1); // Mark as logged out
            PlayerPrefs.Save();

            playerNameText.text = "Signing out...";
            SceneLoader.LoadScene(SceneName.MainMenu);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Sign out failed: {ex.Message}");
        }
    }

    private void OnHelixDriftButtonClicked() => SceneLoader.LoadScene(SceneName.HelixDrift);

    private void OnLeaderboardButtonClicked()
    {
        gameListPanel.SetActive(!gameListPanel.activeSelf);
        leaderboardPanel.SetActive(!leaderboardPanel.activeSelf);
    }
}