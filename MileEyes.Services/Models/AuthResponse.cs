namespace MileEyes.Services.Models
{
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string error { get; set; }
        public string error_description { get; set; }

        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string userName { get; set; }
        public string issued { get; set; }
        public string expires { get; set; }
    }
}