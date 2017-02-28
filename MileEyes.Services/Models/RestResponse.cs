namespace MileEyes.Services.Models
{
    public class RestResponse
    {
        public string Message { get; set; }
        public bool Error { get; set; }
        public bool NotAuthorised { get; set; }
    }
}