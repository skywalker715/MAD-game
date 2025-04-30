using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using System.Text;
using Proyecto26;

public class AuthManager : MonoBehaviour
{
    [Header("Login Panel")]
    public GameObject loginPanel;
    public TMP_InputField loginUsernameInput;
    public TMP_InputField loginPasswordInput;
    public Button loginButton;
    public Button goToRegisterButton;
    public TMP_Text loginStatusText;

    [Header("Register Panel")]
    public GameObject registerPanel;
    public TMP_InputField registerUsernameInput;
    public TMP_InputField registerPasswordInput;
    public TMP_InputField registerConfirmPasswordInput;
    public Button registerButton;
    public Button goToLoginButton;
    public TMP_Text registerStatusText;

    private static AuthManager _instance;
    
    public static AuthManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("AuthManager");
                _instance = obj.AddComponent<AuthManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ShowLoginPanel();
        
        loginButton.onClick.AddListener(OnLoginClick);
        registerButton.onClick.AddListener(OnRegisterClick);
        goToRegisterButton.onClick.AddListener(ShowRegisterPanel);
        goToLoginButton.onClick.AddListener(ShowLoginPanel);
    }

    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        ClearInputFields();
    }

    public void ShowRegisterPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        ClearInputFields();
    }

    private void ClearInputFields()
    {
        loginUsernameInput.text = "";
        loginPasswordInput.text = "";
        loginStatusText.text = "";
        
        registerUsernameInput.text = "";
        registerPasswordInput.text = "";
        registerConfirmPasswordInput.text = "";
        registerStatusText.text = "";
    }

    public void OnLoginClick()
    {
        string username = loginUsernameInput.text;
        string password = loginPasswordInput.text;
        
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            loginStatusText.text = "Please enter both username and password";
            return;
        }
        
        loginStatusText.text = "Logging in...";
        LoginUser(username, password);
    }

    public void OnRegisterClick()
    {
        string username = registerUsernameInput.text;
        string password = registerPasswordInput.text;
        string confirmPassword = registerConfirmPasswordInput.text;
        
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            registerStatusText.text = "Please fill all fields";
            return;
        }
        
        if (password != confirmPassword)
        {
            registerStatusText.text = "Passwords do not match";
            return;
        }
        
        registerStatusText.text = "Registering...";
        RegisterUser(username, password);
    }

    private void RegisterUser(string username, string password)
    {
        APIService.Instance.RegisterUser(
            username, 
            password, 
            response => {
                User user = User.FromRegisterResponse(response);
                user.SaveToPlayerPrefs();
                
                registerStatusText.text = "Registration successful!";
                StartCoroutine(ShowLoginPanelAfterDelay(1.5f));
            },
            error => {
                registerStatusText.text = "Registration failed: " + error.Message;
            }
        );
    }

    private IEnumerator ShowLoginPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowLoginPanel();
    }

    private void LoginUser(string username, string password)
    {
        APIService.Instance.LoginUser(
            username,
            password,
            response => {
                User user = User.FromLoginResponse(response);
                user.SaveToPlayerPrefs();
                
                loginStatusText.text = "Login successful!";
                
                StartCoroutine(LoadHomeSceneAfterDelay(1.0f));
            },
            error => {
                loginStatusText.text = "Login failed: " + 
                    (error is RequestException && ((RequestException)error).StatusCode == 401 ? 
                    "Invalid username or password" : error.Message);
            }
        );
    }

    private IEnumerator LoadHomeSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("HomeScene");
    }
}