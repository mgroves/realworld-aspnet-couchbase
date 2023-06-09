using System.Text.Json.Serialization;

namespace Conduit.Web.ViewModels
{
    public record UserViewModel
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string Username { get; set; }
        public string Bio { get; set; }
        public string Image { get; set; }
    }
}
