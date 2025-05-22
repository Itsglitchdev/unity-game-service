using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class MainMenuManager : MonoBehaviour
{

    [Header("Main Menu Buttons")]
    [SerializeField] private Button signInAsGuestButton;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI statusText;

    private async void Start()
    {
        // Initialize Unity Services
        await InitializeUnityServices();

        signInAsGuestButton.onClick.AddListener(OnSignInAsGuestButtonClicked);
        UpdateUI();
    }

    private async void OnSignInAsGuestButtonClicked()
    {
        try
        {
            signInAsGuestButton.interactable = false;

            if (statusText != null)
                statusText.text = "Signing in as guest...";

            // Sign in anonymously (as guest)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            if (statusText != null)
                statusText.text = "Sign in successful!";

            // Load the game scene
            SceneLoader.Load(SceneName.Loading, SceneName.GameMenu);
            
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"Sign in failed: {ex.Message}");

            if (statusText != null)
                statusText.text = "Sign in failed. Please try again.";

            signInAsGuestButton.interactable = true;
        }
    }

    private void UpdateUI()
    {
        // Check if already signed in
        if (AuthenticationService.Instance.IsSignedIn)
        {
            signInAsGuestButton.interactable = false;
            
            if (statusText != null)
                statusText.text = "Already signed in!";
        }
        else
        {
            signInAsGuestButton.interactable = true;
            
            if (statusText != null)
                statusText.text = "Ready to sign in";
        }
    }

    #region Unity Services Methods
    private async Task InitializeUnityServices()
    {
        await UnityServices.InitializeAsync();
    }
    #endregion
}
