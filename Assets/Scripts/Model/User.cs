using UnityEngine;

public class User
{
    public int Id { get; private set; }
    public string Username { get; private set; }

    public User(int id, string username)
    {
        Id = id;
        Username = username;
    }

    
    public static User FromLoginResponse(APIService.LoginResponse response)
    {
        return new User(response.id, response.username);
    }

    
    public static User FromRegisterResponse(APIService.RegisterResponse response)
    {
        return new User(response.id, response.username);
    }

    
    public void SaveToPlayerPrefs()
    {
        PlayerPrefs.SetInt("UserId", Id);
        PlayerPrefs.SetString("Username", Username);
        PlayerPrefs.Save();
    }

    
    public static User LoadFromPlayerPrefs()
    {
        if (!PlayerPrefs.HasKey("UserId") || !PlayerPrefs.HasKey("Username"))
        {
            return null;
        }

        int id = PlayerPrefs.GetInt("UserId");
        string username = PlayerPrefs.GetString("Username");
        
        return new User(id, username);
    }

    
    public static bool IsLoggedIn()
    {
        return PlayerPrefs.HasKey("UserId") && PlayerPrefs.HasKey("Username");
    }

    
    public static void ClearUserData()
    {
        PlayerPrefs.DeleteKey("UserId");
        PlayerPrefs.DeleteKey("Username");
        PlayerPrefs.Save();
    }
} 