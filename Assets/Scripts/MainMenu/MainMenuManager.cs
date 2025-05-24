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
    [SerializeField] private Button signIn;
    [SerializeField] private Button signUp;

    [Header("Input Field")]
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private TMP_InputField passwordInputField;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI statusText;

    private const string LogoutKey = "UserLoggedOut";

    private async void Start()
    {
        await InitializeUnityServices();

        bool userLoggedOut = PlayerPrefs.GetInt(LogoutKey, 0) == 1;

        try
        {
            if (!userLoggedOut)
            {
                // This will trigger Unity to try to restore session if token is valid
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                if (AuthenticationService.Instance.IsSignedIn)
                {
                    signInAsGuestButton.interactable = false;
                    signIn.interactable = false;
                    signUp.interactable = false;
                    if (statusText != null)
                        statusText.text = "Restoring session...";

                    await Task.Delay(1000);
                    SceneLoader.Load(SceneName.Loading, SceneName.GameMenu);
                    return;
                }
            }
        }
        catch (AuthenticationException ex)
        {
            signInAsGuestButton.interactable = true;
            signIn.interactable = true;
            signUp.interactable = true;
            Debug.Log("Session restore failed: " + ex.Message);
            PlayerPrefs.SetInt(LogoutKey, 1);
        }

        passwordInputField.contentType = TMP_InputField.ContentType.Password;
        passwordInputField.ForceLabelUpdate();
        usernameInputField.contentType = TMP_InputField.ContentType.Standard;
        usernameInputField.ForceLabelUpdate();

        EventListeners();
        UpdateUI();
    }

    private void EventListeners()
    {
        signInAsGuestButton.onClick.AddListener(OnSignInAsGuestButtonClicked);
        signIn.onClick.AddListener(OnSignInButtonClicked);
        signUp.onClick.AddListener(OnSignUpButtonClicked);
    }


    # region Event Listeners
    private async void OnSignInAsGuestButtonClicked()
    {
        try
        {
            signInAsGuestButton.interactable = false;
            signIn.interactable = false;
            signUp.interactable = false;
            statusText.text = "Signing in as guest...";

            AuthenticationService.Instance.SignOut(true); // Clear any session
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            PlayerPrefs.SetInt(LogoutKey, 0); // Mark user as signed in
            PlayerPrefs.Save();

            SceneLoader.Load(SceneName.Loading, SceneName.GameMenu);
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"Guest sign in failed: {ex.Message}");
            statusText.text = "Guest sign in failed.";
            signInAsGuestButton.interactable = true;
            signIn.interactable = true;
            signUp.interactable = true;
        }
    }

    private async void OnSignInButtonClicked()
    {
        try
        {
            signIn.interactable = false;
            signUp.interactable = false;
            signInAsGuestButton.interactable = false;
            statusText.text = "Signing in...";

            AuthenticationService.Instance.SignOut(true); ; // Clear guest session
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(
                usernameInputField.text, passwordInputField.text);

            PlayerPrefs.SetInt(LogoutKey, 0);
            PlayerPrefs.Save();

            SceneLoader.Load(SceneName.Loading, SceneName.GameMenu);
        }
        catch (AuthenticationException ex)
        {
            if (ex.ErrorCode == AuthenticationErrorCodes.InvalidProvider)
            {
                statusText.text = "Invalid username or password.";
            }
            else if (ex.ErrorCode == AuthenticationErrorCodes.InvalidParameters)
            {
                statusText.text = "No account found with those credentials.";
            }
            else
            {
                statusText.text = "Sign in failed. Please try again.";
            }

            Debug.LogError($"Sign in failed: {ex.Message}");
            signIn.interactable = true;
            signUp.interactable = true;
            signInAsGuestButton.interactable = true;
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
            signIn.interactable = false;
            signUp.interactable = false;
            signInAsGuestButton.interactable = false;
            statusText.text = "Creating account...";

            AuthenticationService.Instance.SignOut(true); // Clear any session
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);

            PlayerPrefs.SetInt(LogoutKey, 0);
            PlayerPrefs.Save();

            SceneLoader.Load(SceneName.Loading, SceneName.GameMenu);
        }
        catch (AuthenticationException ex)
        {
            if (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
            {
                statusText.text = "Account already exists. Please sign in instead or use a different username.";
            }
            else
            {
                Debug.LogError($"Sign up failed: {ex.Message}");
                statusText.text = "Sign up failed. Try a different username.";
            }

            signIn.interactable = true;
            signUp.interactable = true;
            signInAsGuestButton.interactable = true;
        }
    }
    #endregion

    private void UpdateUI()
    {
        bool signedIn = AuthenticationService.Instance.IsSignedIn;
        signInAsGuestButton.interactable = !signedIn;
        signIn.interactable = !signedIn;
        signUp.interactable = !signedIn;

        statusText.text = signedIn ? "Already signed in!" : "Ready to sign in";
    }

    # region Validation
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
        await UnityServices.InitializeAsync();
    }
}
