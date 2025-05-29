using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Unity.Services.Authentication.PlayerAccounts;
using System.Threading;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    [SerializeField] private Button signInAsGuestButton;
    [SerializeField] private Button signIn;
    [SerializeField] private Button signUp;

    [Header("Input Field")]
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private TMP_InputField passwordInputField;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI statusText;

    private const string LogoutKey = "UserLoggedOut";
    private const string LastAuthTypeKey = "LastAuthType"; // Track auth method
    private const string SavedUsernameKey = "SavedUsername";
    private const float SESSION_RESTORE_TIMEOUT = 5f; // Reduced timeout for WebGL
    
    private CancellationTokenSource cancellationTokenSource;

    private async void Start()
    {
        await InitializeUnityServices();

        #if UNITY_WEBGL && !UNITY_EDITOR
            // ✅ FIX: For WebGL, always show login page - no session restoration
            Debug.Log("WebGL build detected - skipping session restoration");
            SetupUI();
        #else
            // Session restoration only for non-WebGL platforms (Editor, Standalone, Mobile, etc.)
            bool userLoggedOut = PlayerPrefs.GetInt(LogoutKey, 0) == 1;
            
            if (!userLoggedOut && AuthenticationService.Instance.SessionTokenExists)
            {
                await AttemptSessionRestore();
            }
            else
            {
                SetupUI();
            }
        #endif
    }

    private void OnDestroy()
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
    }

    // ✅ Note: This method is now only used by non-WebGL platforms
    // WebGL builds will always go directly to login UI
    private bool HasValidSessionIndicators()
    {
        return AuthenticationService.Instance.SessionTokenExists && 
               !string.IsNullOrEmpty(PlayerPrefs.GetString(LastAuthTypeKey, ""));
    }

    private async Task AttemptSessionRestore()
    {
        cancellationTokenSource = new CancellationTokenSource();
        
        try
        {
            SetButtonsInteractable(false);
            if (statusText != null)
                statusText.text = "Restoring session...";

            // ✅ FIX: Use timeout with Task.Delay for better control
            var timeoutTask = Task.Delay((int)(SESSION_RESTORE_TIMEOUT * 1000), cancellationTokenSource.Token);
            
            string lastAuthType = PlayerPrefs.GetString(LastAuthTypeKey, "");
            Task signInTask;
            
            // Try to restore based on last auth method
            if (lastAuthType == "anonymous")
            {
                signInTask = AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            else if (lastAuthType == "username" && !string.IsNullOrEmpty(PlayerPrefs.GetString(SavedUsernameKey, "")))
            {
                // For username auth, we can't restore without password, so fall back to UI
                throw new System.Exception("Username session expired - credentials required");
            }
            else
            {
                // Unknown auth type, try anonymous as fallback
                signInTask = AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            // Wait for either sign-in completion or timeout
            var completedTask = await Task.WhenAny(signInTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                // Timeout occurred
                throw new System.OperationCanceledException("Session restore timed out");
            }

            // Check if sign-in was successful
            if (AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("Session restored successfully");
                await Task.Delay(500); // Brief delay for UI feedback
                SceneLoader.Load(SceneName.Loading, SceneName.GameMenu);
                return;
            }
            else
            {
                throw new System.Exception("Failed to restore session - not signed in");
            }
        }
        catch (System.OperationCanceledException)
        {
            Debug.LogWarning("Session restore timed out");
            HandleSessionRestoreFailure();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Session restore failed: {ex.Message}");
            HandleSessionRestoreFailure();
        }
    }

    // ✅ NEW: Centralized session restore failure handling
    private void HandleSessionRestoreFailure()
    {
        PlayerPrefs.SetInt(LogoutKey, 1); // Mark as logged out
        PlayerPrefs.DeleteKey(LastAuthTypeKey); // Clear auth type
        PlayerPrefs.Save();
        
        // Clear any corrupted session
        try
        {
            AuthenticationService.Instance.SignOut(true);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"SignOut failed: {ex.Message}");
        }
        
        SetupUI();
    }

    private void SetupUI()
    {
        passwordInputField.contentType = TMP_InputField.ContentType.Password;
        passwordInputField.ForceLabelUpdate();
        usernameInputField.contentType = TMP_InputField.ContentType.Standard;
        usernameInputField.ForceLabelUpdate();

        // ✅ FIX: Load saved username for convenience
        string savedUsername = PlayerPrefs.GetString(SavedUsernameKey, "");
        if (!string.IsNullOrEmpty(savedUsername))
        {
            usernameInputField.text = savedUsername;
        }

        EventListeners();
        UpdateUI();
    }

    private void EventListeners()
    {
        signInAsGuestButton.onClick.AddListener(OnSignInAsGuestButtonClicked);
        signIn.onClick.AddListener(OnSignInButtonClicked);
        signUp.onClick.AddListener(OnSignUpButtonClicked);
    }

    // ✅ NEW: Helper method to set button interactability
    private void SetButtonsInteractable(bool interactable)
    {
        signInAsGuestButton.interactable = interactable;
        signIn.interactable = interactable;
        signUp.interactable = interactable;
    }

    #region Event Listeners
    private async void OnSignInAsGuestButtonClicked()
    {
        try
        {
            SetButtonsInteractable(false);
            statusText.text = "Signing in as guest...";

            AuthenticationService.Instance.SignOut(true); // Clear any session
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            // ✅ Set consistent guest name for leaderboard
            string guestName = GetConsistentGuestName();
            await AuthenticationService.Instance.UpdatePlayerNameAsync(guestName);

            // ✅ FIX: Save auth type for session restoration
            PlayerPrefs.SetInt(LogoutKey, 0);
            PlayerPrefs.SetString(LastAuthTypeKey, "anonymous");
            PlayerPrefs.Save();

            SceneLoader.Load(SceneName.Loading, SceneName.GameMenu);
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"Guest sign in failed: {ex.Message}");
            statusText.text = "Guest sign in failed. Please try again.";
            SetButtonsInteractable(true);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Unexpected error during guest sign in: {ex.Message}");
            statusText.text = "Sign in failed. Please try again.";
            SetButtonsInteractable(true);
        }
    }

    private async void OnSignInButtonClicked()
    {
        try
        {
            SetButtonsInteractable(false);
            statusText.text = "Signing in...";

            AuthenticationService.Instance.SignOut(true); // Clear guest session
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(
                usernameInputField.text, passwordInputField.text);

            // ✅ Set PlayerName to username
            await AuthenticationService.Instance.UpdatePlayerNameAsync(usernameInputField.text);

            // ✅ FIX: Save auth info for session restoration
            PlayerPrefs.SetInt(LogoutKey, 0);
            PlayerPrefs.SetString(LastAuthTypeKey, "username");
            PlayerPrefs.SetString(SavedUsernameKey, usernameInputField.text);
            PlayerPrefs.Save();

            SceneLoader.Load(SceneName.Loading, SceneName.GameMenu);
        }
        catch (AuthenticationException ex)
        {
            string errorMessage = "Sign in failed. Please try again.";
            
            if (ex.ErrorCode == AuthenticationErrorCodes.InvalidProvider)
            {
                errorMessage = "Invalid username or password.";
            }
            else if (ex.ErrorCode == AuthenticationErrorCodes.InvalidParameters)
            {
                errorMessage = "No account found with those credentials.";
            }
            else if (ex.ErrorCode == AuthenticationErrorCodes.ClientInvalidUserState)
            {
                errorMessage = "Please try signing in again.";
            }

            statusText.text = errorMessage;
            Debug.LogError($"Sign in failed: {ex.Message}");
            SetButtonsInteractable(true);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Unexpected error during sign in: {ex.Message}");
            statusText.text = "Sign in failed. Please try again.";
            SetButtonsInteractable(true);
        }
    }

    private async void OnSignUpButtonClicked()
    {
        string username = usernameInputField.text;
        string password = passwordInputField.text;

        if (!IsUsernameValid(username, out string usernameError))
        {
            statusText.text = $"Error: {usernameError}";
            return;
        }

        if (!IsPasswordValid(password, out string passwordError))
        {
            statusText.text = $"Error: {passwordError}";
            return;
        }

        try
        {
            SetButtonsInteractable(false);
            statusText.text = "Creating account...";

            AuthenticationService.Instance.SignOut(true); // Clear any session
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);

            // ✅ Set PlayerName to username
            await AuthenticationService.Instance.UpdatePlayerNameAsync(username);

            // ✅ FIX: Save auth info for session restoration
            PlayerPrefs.SetInt(LogoutKey, 0);
            PlayerPrefs.SetString(LastAuthTypeKey, "username");
            PlayerPrefs.SetString(SavedUsernameKey, username);
            PlayerPrefs.Save();

            SceneLoader.Load(SceneName.Loading, SceneName.GameMenu);
        }
        catch (AuthenticationException ex)
        {
            string errorMessage = "Sign up failed. Try a different username.";
            
            if (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
            {
                errorMessage = "Account already exists. Please sign in instead.";
            }
            else if (ex.ErrorCode == AuthenticationErrorCodes.ClientInvalidUserState)
            {
                errorMessage = "Please try again.";
            }

            statusText.text = errorMessage;
            Debug.LogError($"Sign up failed: {ex.Message}");
            SetButtonsInteractable(true);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Unexpected error during sign up: {ex.Message}");
            statusText.text = "Sign up failed. Please try again.";
            SetButtonsInteractable(true);
        }
    }
    #endregion

    private void UpdateUI()
    {
        bool signedIn = AuthenticationService.Instance.IsSignedIn;
        SetButtonsInteractable(!signedIn);

        statusText.text = signedIn ? "Already signed in!" : "Ready to sign in";
    }

    // ✅ Generate consistent guest name
    private string GetConsistentGuestName()
    {
        string playerId = AuthenticationService.Instance.PlayerId;
        return $"Player_{playerId.Substring(0, 6)}"; // Slightly longer for better uniqueness
    }

    #region Validation
    private bool IsUsernameValid(string username, out string error)
    {
        error = "";
        if (string.IsNullOrWhiteSpace(username)) { error = "Username required."; return false; }
        if (username.Length < 3 || username.Length > 20) { error = "Username 3–20 chars."; return false; }
        if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9._@-]+$")) { error = "Invalid chars."; return false; }
        return true;
    }

    private bool IsPasswordValid(string password, out string error)
    {
        error = "";
        if (string.IsNullOrEmpty(password)) { error = "Password required."; return false; }
        if (password.Length < 8 || password.Length > 30) { error = "Password 8–30 chars."; return false; }
        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-z]")) { error = "Needs lowercase."; return false; }
        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]")) { error = "Needs uppercase."; return false; }
        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"\d")) { error = "Needs number."; return false; }
        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[\W_]")) { error = "Needs special char."; return false; }
        return true;
    }
    #endregion

    private async Task InitializeUnityServices()
    {
        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log("Unity Services initialized successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to initialize Unity Services: {ex.Message}");
            if (statusText != null)
                statusText.text = "Service initialization failed. Please refresh.";
        }
    }
}