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

    private async void Start()
    {
        // Initialize Unity Services
        await InitializeUnityServices();

        EventListeners();
        UpdateUI();

        passwordInputField.contentType = TMP_InputField.ContentType.Password;
        passwordInputField.ForceLabelUpdate();
    }

    private void EventListeners()
    {
        signInAsGuestButton.onClick.AddListener(OnSignInAsGuestButtonClicked);
        signIn.onClick.AddListener(OnSignInButtonClicked);
        signUp.onClick.AddListener(OnSignUpButtonClicked);
    }

    #region OnClickMethods
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

    private async void OnSignInButtonClicked()
    {
        try
        {
            signIn.interactable = false;
            signUp.interactable = false;

            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(usernameInputField.text, passwordInputField.text);

            if (statusText != null)
                statusText.text = "Sign in successful!";

            // Load the game scene
            SceneLoader.Load(SceneName.Loading, SceneName.GameMenu);

        }
        catch
        {
            if (statusText != null)
                statusText.text = "Sign in failed. Please try again.";

            signIn.interactable = true;
            signUp.interactable = true;
        }
    }

    private async void OnSignUpButtonClicked()
    {

        string username = usernameInputField.text;
        string password = passwordInputField.text;

        if (!IsUsernameValid(username, out string usernameError))
        {
            statusText.text = $"Sign up failed: {usernameError}";
            return;
        }

        if (!IsPasswordValid(password, out string passwordError))
        {
            statusText.text = $"Sign up failed: {passwordError}";
            return;
        }

        
        try
        {
            signIn.interactable = false;
            signUp.interactable = false;

            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(usernameInputField.text, passwordInputField.text);

            if (statusText != null)
                statusText.text = "Sign up successful!";

            SceneLoader.Load(SceneName.Loading, SceneName.GameMenu);
        }
        catch
        {
            if (statusText != null)
                statusText.text = "Sign up failed. Please try again.";

            signIn.interactable = true;
            signUp.interactable = true;
        }
    }
    #endregion

    #region UpdateUI
    private void UpdateUI()
    {
        // Check if already signed in
        if (AuthenticationService.Instance.IsSignedIn)
        {
            signInAsGuestButton.interactable = false;
            signIn.interactable = false;
            signUp.interactable = false;

            if (statusText != null)
                statusText.text = "Already signed in!";
        }
        else
        {
            signInAsGuestButton.interactable = true;
            signIn.interactable = true;
            signUp.interactable = true;

            if (statusText != null)
                statusText.text = "Ready to sign in";
        }
    }
    #endregion

    #region Validation
    private bool IsUsernameValid(string username, out string error)
    {
        error = "";

        if (string.IsNullOrWhiteSpace(username))
        {
            error = "Username cannot be empty.";
            return false;
        }

        if (username.Length < 3 || username.Length > 20)
        {
            error = "Username must be 3–20 characters.";
            return false;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9._@-]+$"))
        {
            error = "Username contains invalid characters.";
            return false;
        }

        return true;
    }

    private bool IsPasswordValid(string password, out string error)
    {
        error = "";

        if (string.IsNullOrEmpty(password))
        {
            error = "Password cannot be empty.";
            return false;
        }

        if (password.Length < 8 || password.Length > 30)
        {
            error = "Password must be 8–30 characters.";
            return false;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-z]"))
        {
            error = "Password must include at least one lowercase letter.";
            return false;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]"))
        {
            error = "Password must include at least one uppercase letter.";
            return false;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"\d"))
        {
            error = "Password must include at least one number.";
            return false;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[\W_]"))
        {
            error = "Password must include at least one special symbol.";
            return false;
        }

        return true;
    }
    #endregion

    #region Unity Services Methods
    private async Task InitializeUnityServices()
    {
        await UnityServices.InitializeAsync();
    }
    #endregion
}
