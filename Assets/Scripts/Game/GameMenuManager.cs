using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Authentication;

public class GameMenuManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button signOutButton;
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI playerNameText;

    private void Start()
    {
        signOutButton.onClick.AddListener(OnSignOutButtonClicked);
        UpdatePlayerInfo();
    }

    private void UpdatePlayerInfo()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            var playerInfo = AuthenticationService.Instance.PlayerInfo;
            if (!string.IsNullOrEmpty(playerInfo.Username))
            {
                playerNameText.text = playerInfo.Username;
            }
            else
            {
                string playerId = AuthenticationService.Instance.PlayerId;
                playerNameText.text = $"Player_{playerId.Substring(0, 4)}";
            }
        }
        else
        {
            if (playerNameText != null)
                playerNameText.text = "Not signed in";

        }
    }

    private void OnSignOutButtonClicked()
    {
        try
        {
            AuthenticationService.Instance.SignOut();
            Debug.Log("Player signed out successfully");
            SceneLoader.LoadScene(SceneName.MainMenu);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Sign out failed: {ex.Message}");
        }
    }

    public string GetPlayerName()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            string playerId = AuthenticationService.Instance.PlayerId;
            return $"Player_{playerId.Substring(0, 4)}";
        }
        return "Unknown Player";
    }

    public bool IsPlayerAuthenticated()
    {
        return AuthenticationService.Instance.IsSignedIn;
    }
}