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

    private const string LogoutKey = "UserLoggedOut";

    private void Start()
    {
        signOutButton.onClick.AddListener(OnSignOutButtonClicked);
        UpdatePlayerInfo();
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
                string playerId = AuthenticationService.Instance.PlayerId;
                playerNameText.text = $"Guest_{playerId.Substring(0, 6)}";
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
}
