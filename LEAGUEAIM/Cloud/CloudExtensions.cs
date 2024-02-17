using Script_Engine.Cloud.Models.Enums;

namespace Script_Engine.Cloud
{
    internal static class CloudExtensions
	{
		public static string Folder(this CloudEntryType type) => type switch
		{
			CloudEntryType.Config => "profiles",
			CloudEntryType.Script => "scripts",
			CloudEntryType.Pattern => "patterns",
			_ => "Unknown"
		};
		public static string Extension(this CloudEntryType type) => type switch
		{
			CloudEntryType.Config => "ini",
			CloudEntryType.Script => "lua",
			CloudEntryType.Pattern => "txt",
			_ => "txt"
		};
	}
}
