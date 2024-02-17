namespace XenForo.NET.Models
{
    using Newtonsoft.Json;

    public class Field
    {
        [JsonProperty("id")]
        public string Id
        {
            get;
            set;
        }
        [JsonProperty("title")]
        public string Title
        {
            get;
            set;
        }
        [JsonProperty("description")]
        public string Description
        {
            get;
            set;
        }
        [JsonProperty("value")]
        public string Value
        {
            get;
            set;
        }
        [JsonProperty("display_group")]
        public string DisplayGroup
        {
            get;
            set;
        }
        [JsonProperty("choices")]
        public Dictionary<string, string> Choices
        {
            get;
            set;
        }
        [JsonProperty("is_multiple_choice")]
        public bool IsMultipleChoice
        {
            get;
            set;
        }
        [JsonProperty("is_required")]
        public bool IsRequired
        {
            get;
            set;

        }
        [JsonProperty("values")]
        public Dictionary<string, string> Values
        {
            get;
            set;
        }
    }
}