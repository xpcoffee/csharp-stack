namespace Api.Model;

// Models/GoogleAuthRequest.cs
public class GoogleAuthRequest
{
    public string IdToken { get; set; }
}

// Models/AuthResponse.cs
public class AuthResponse
{
    public string Token { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
}

// Models/GoogleUserInfo.cs
public class GoogleUserInfo
{
    public string Email { get; set; }
    public string Name { get; set; }
    public string Picture { get; set; }
    public string Sub { get; set; } // Google user ID
}
