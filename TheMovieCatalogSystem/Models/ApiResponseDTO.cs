using System.Text.Json.Serialization;

namespace TheMovieCatalogSystem.Models
{
    public class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("movie")]
        public MovieDTO? Movie { get; set; }
    }
}