using LEAGUEAIM;
using Newtonsoft.Json;
using Script_Engine.Utilities;
using System.Text;

namespace Script_Engine.Cloud
{
	internal class CloudMethods
	{
		readonly static string endpoint = "http://auth.leagueaim.gg/cloud";
		public static List<CloudEntry> RetrieveFiles(string type)
		{
			// create an HttpClient to send a post request and retrieve the data as as tring
			using HttpClient client = new();
			// create a new form url encoded content
			var content = new FormUrlEncodedContent(new[]
			{
				new KeyValuePair<string, string>("REQUEST_TYPE", "LIST"),
				new KeyValuePair<string, string>("REQUEST_USER", Program._XFUser.Username),
				new KeyValuePair<string, string>("DATA_TYPE", type),
				new KeyValuePair<string, string>("DATA_ID", 0.ToString()),
			});

			// send the post request to the endpoint
			var result = client.PostAsync($"{endpoint}/index.php", content).Result;

			// return the result as a string
			string json_data = result.Content.ReadAsStringAsync().Result;


			List<CloudEntry> entries = [.. JsonConvert.DeserializeObject<CloudEntry[]>(json_data)];

			foreach (CloudEntry entry in entries)
			{
				entry.Type = type;
			}

			Logger.WriteLine($"Retrieved {entries.Count} {type} from the cloud!");

			if (entries.Count > 0)
				return entries;

			entries.Add(new CloudEntry()
			{
				Name = "No files found",
				Type = type,
				Author = ""
			});
			return entries;
		}
		public static CloudEntry RetrieveFile(string type, int id)
		{
			// create an HttpClient to send a post request and retrieve the data as as tring
			using HttpClient client = new();
			// create a new form url encoded content
			var content = new FormUrlEncodedContent(new[]
			{
				new KeyValuePair<string, string>("REQUEST_TYPE", "GET"),
				new KeyValuePair<string, string>("REQUEST_USER", null),
				new KeyValuePair<string, string>("DATA_TYPE", type),
				new KeyValuePair<string, string>("DATA_ID", id.ToString()),
			});

			// send the post request to the endpoint
			var result = client.PostAsync($"{endpoint}/index.php", content).Result;

			// return the result as a string
			string json_data = result.Content.ReadAsStringAsync().Result;

			if (json_data != "false")
			{
				// deserialize the json data into an Entry object
				CloudEntry e = JsonConvert.DeserializeObject<CloudEntry>(json_data);

				e.Type = type;

				Logger.WriteLine($"Retrieved {e.Name} from the cloud!");

				return e;
			}
			else
			{
				return new CloudEntry();
			}
		}
		public static List<CloudEntry> RetrieveCommunity()
		{
			// create an HttpClient to send a post request and retrieve the data as as tring
			using HttpClient client = new();
			// create a new form url encoded content
			var content = new FormUrlEncodedContent(new[]
			{
				new KeyValuePair<string, string>("REQUEST_TYPE", "COMM"),
				new KeyValuePair<string, string>("REQUEST_USER", Program._XFUser.Username),
			});

			// send the post request to the endpoint
			var result = client.PostAsync($"{endpoint}/index.php", content).Result;

			// return the result as a string
			string json_data = result.Content.ReadAsStringAsync().Result;

			List<CloudEntry> entries = [.. JsonConvert.DeserializeObject<CloudEntry[]>(json_data)];

			return entries;
		}
		public static bool SaveFile(CloudEntry entry)
		{
			string Base = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LEAGUEAIM");

			CloudEntryType type = entry.GetDataType();

			string fileName = Path.Combine(Base, $"{type.Folder()}", $"{entry.SanitizedName()}.{type.Extension()}");

			string data = entry.DecodedData();

			File.WriteAllText(fileName, data);

			Logger.WriteLine($"Saved {entry.Name} to the local filesystem!");

			return true;
		}
		public static bool UploadFile(string type, string name)
		{
			string Base = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LEAGUEAIM");

			string filePath, data;
			switch (type)
			{
				case "configs":
					filePath = Path.Combine(Base, "profiles", name.Replace(".ini", "") + ".ini");
					break;
				case "scripts":
					filePath = Path.Combine(Base, "scripts", name.Replace(".lua", "") + ".lua");
					break;
				case "patterns":
					filePath = Path.Combine(Base, "patterns", name.Replace(".txt", "") + ".txt");
					break;
				default:
					return false;
			}
			data = Convert.ToBase64String(Encoding.UTF8.GetBytes(File.ReadAllText(filePath)));

			// create an HttpClient to upload the file
			using HttpClient client = new();
			// create a new form url encoded content
			var content = new FormUrlEncodedContent(new[]
			{
				new KeyValuePair<string, string>("REQUEST_TYPE", "SET"),
				new KeyValuePair<string, string>("REQUEST_USER", Program._XFUser.Username),
				new KeyValuePair<string, string>("DATA_TYPE", type),
				new KeyValuePair<string, string>("DATA_NAME", name),
				new KeyValuePair<string, string>("DATA_DATA", data),
			});

			// send the post request to the endpoint
			var result = client.PostAsync($"{endpoint}/index.php", content).Result;

			// return the result as a string
			string json_data = result.Content.ReadAsStringAsync().Result;

			Console.WriteLine(json_data);

			Logger.WriteLine($"Uploaded {name} to the cloud!");

			CloudMenu.UpdateEntries();

			// return the result as a string
			return true;
		}
		public static bool DeleteFile(string type, int id)
		{
			// create an HttpClient to send a post request and retrieve the data as as tring
			using HttpClient client = new();
			// create a new form url encoded content
			var content = new FormUrlEncodedContent(new[]
			{
				new KeyValuePair<string, string>("REQUEST_TYPE", "REM"),
				new KeyValuePair<string, string>("REQUEST_USER", Program._XFUser.Username),
				new KeyValuePair<string, string>("DATA_TYPE", type),
				new KeyValuePair<string, string>("DATA_ID", id.ToString()),
			});

			// send the post request to the endpoint
			var result = client.PostAsync($"{endpoint}/index.php", content).Result;

			// return the result as a string
			string json_data = result.Content.ReadAsStringAsync().Result;

			Logger.WriteLine($"Deleted a file from the cloud!");

			CloudMenu.UpdateEntries();

			// return the result as a string
			return true;
		}
		public static bool CheckRating(string type, int id)
		{
			using HttpClient client = new();
			// create a new form url encoded content
			var content = new FormUrlEncodedContent(new[]
			{
				new KeyValuePair<string, string>("REQUEST_TYPE", "CHECK"),
				new KeyValuePair<string, string>("REQUEST_USER", Program._XFUser.Username),
				new KeyValuePair<string, string>("DATA_TYPE", type),
				new KeyValuePair<string, string>("DATA_ID", id.ToString()),
			});

			// send the post request to the endpoint
			var result = client.PostAsync($"{endpoint}/index.php", content).Result;

			// return the result as a string
			string json_data = result.Content.ReadAsStringAsync().Result;

			return (json_data != "[]");
		}
		public static bool AddRating(string type, int id)
		{

			Logger.WriteLine($"Updating rating for entry ({id})..");

			using HttpClient client = new();
			// create a new form url encoded content
			var content = new FormUrlEncodedContent(new[]
			{
				new KeyValuePair<string, string>("REQUEST_TYPE", "VOTE"),
				new KeyValuePair<string, string>("REQUEST_USER", Program._XFUser.Username),
				new KeyValuePair<string, string>("DATA_TYPE", type),
				new KeyValuePair<string, string>("DATA_ID", id.ToString()),
			});

			// send the post request to the endpoint
			var result = client.PostAsync($"{endpoint}/index.php", content).Result;

			// return the result as a string
			string json_data = result.Content.ReadAsStringAsync().Result;

			return false;
		}
		public static bool AddToCommunity(string type, int id)
		{
			Logger.WriteLine($"Uploading entry to community hub ({id})..");

			using HttpClient client = new();
			// create a new form url encoded content
			var content = new FormUrlEncodedContent(new[]
			{
				new KeyValuePair<string, string>("REQUEST_TYPE", "ADD"),
				new KeyValuePair<string, string>("DATA_TYPE", type),
				new KeyValuePair<string, string>("DATA_ID", id.ToString()),
			});

			// send the post request to the endpoint
			var result = client.PostAsync($"{endpoint}/index.php", content).Result;

			// return the result as a string
			string json_data = result.Content.ReadAsStringAsync().Result;

			Console.WriteLine(json_data);

			return false;
		}
	}
}
