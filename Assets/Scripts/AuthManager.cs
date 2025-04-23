using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using System.Text;
using UnityEngine.Networking;

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

    public string serverUrl = "http://localhost:3000"; 
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
        StartCoroutine(LoginUser(username, password));
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
        StartCoroutine(RegisterUser(username, password));
    }

    private IEnumerator RegisterUser(string username, string password)
    {
        string jsonData = $"{{\"username\":\"{username}\", \"password\":\"{password}\"}}";
        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "/register", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Registration response: " + request.downloadHandler.text);
                

                JsonUtility.FromJson<RegisterResponse>(request.downloadHandler.text);
                registerStatusText.text = "Registration successful!";
                

                yield return new WaitForSeconds(1.5f);
                ShowLoginPanel();
            }
            else
            {
                Debug.LogError("Registration error: " + request.error);
                registerStatusText.text = "Registration failed: " + request.error;
            }
        }
    }

    private IEnumerator LoginUser(string username, string password)
    {
        string jsonData = $"{{\"username\":\"{username}\", \"password\":\"{password}\"}}";
        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "/login", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Login response: " + request.downloadHandler.text);
                

                LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                

                PlayerPrefs.SetInt("UserId", response.id);
                PlayerPrefs.SetString("Username", response.username);
                PlayerPrefs.Save();
                
                loginStatusText.text = "Login successful!";
                

                yield return new WaitForSeconds(1.0f);
                SceneManager.LoadScene("HomeScene");
            }
            else
            {
                Debug.LogError("Login error: " + request.error);
                loginStatusText.text = "Login failed: " + 
                    (request.responseCode == 401 ? "Invalid username or password" : request.error);
            }
        }
    }


    [Serializable]
    private class RegisterResponse
    {
        public int id;
        public string username;
    }

    [Serializable]
    private class LoginResponse
    {
        public int id;
        public string username;
        public string message;
    }
}