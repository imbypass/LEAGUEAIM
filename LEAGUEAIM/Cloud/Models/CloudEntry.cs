using Newtonsoft.Json;
using Script_Engine.Cloud.Models.Enums;
using System.Text;

namespace Script_Engine.Cloud
{
    public class CloudEntry
	{
		[JsonProperty("type")]
		public string Type { get; set; }
		[JsonProperty("id")]
		public int Id { get; set; }
		[JsonProperty("author")]
		public string Author { get; set; }
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("rating")]
		public int Rating { get; set; }
		[JsonProperty("data")]
		public string Data { get; set; }
		public CloudEntry()
		{
			Type = "configs";
			Id = -1;
			Name = "";
			Data = "";
			Author = "";
			Rating = 0;
		}

		public CloudEntryType GetDataType() => Type switch
		{
			"configs" => CloudEntryType.Config,
			"scripts" => CloudEntryType.Script,
			"patterns" => CloudEntryType.Pattern,
			_ => CloudEntryType.Config
		};

		public string DecodedData() => Encoding.UTF8.GetString(Convert.FromBase64String(Data));
		public string SanitizedName()
		{
			string name = Name
				.Replace(" ", "_")
				.Replace(":", string.Empty)
				.Replace("/", string.Empty)
				.Replace("-", string.Empty)
				.Replace(".lua", string.Empty)
				.Replace(".txt", string.Empty)
				.Replace(".ini", string.Empty);

			// remove duplicate underscores
			while (name.Contains("__"))
			{
				name = name.Replace("__", "_");
			}

			return name;
		}
	}
}
