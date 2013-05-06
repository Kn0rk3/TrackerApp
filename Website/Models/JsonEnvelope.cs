
namespace TrackerApp.Website.Models
{
    public class JsonEnvelope<T>
    {
        public bool Success { get; set; }

        public T Data { get; set; }

        public string Message { get; set; }
    }
}