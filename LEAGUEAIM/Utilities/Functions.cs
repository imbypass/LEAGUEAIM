using Microsoft.Win32;
using Script_Engine.Cloud;
using Script_Engine.Utilities;
using System.Security.Principal;
using System.Text;

namespace LEAGUEAIM.Utilities
{
	internal class Functions
	{
		private static readonly Random random = new();
		public static bool IsAdministrator => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
		public static string RandomString(int length, bool useUppercase = true, bool useLowercase = true, bool useNumbers = true)
		{
			if (!useLowercase && !useUppercase && !useNumbers)
				throw new ArgumentException("Must use at least one of the following: lowercase, uppercase, or numbers");
			string chars = "";
			if (useLowercase) chars += "abcdefghijklmnopqrstuvwxyz";
			if (useUppercase) chars += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			if (useNumbers) chars += "0123456789";
			return new string(Enumerable.Range(1, length).Select(_ => chars[random.Next(chars.Length)]).ToArray());
		}
		public static void ImportPattern(string pattern)
		{
			// filetype|id
			string[] data = Encoding.UTF8.GetString(Convert.FromBase64String(pattern)).Split('|');

			string type = data[0];
			int id = int.Parse(data[1]);

			string importType = type.TrimEnd('s');

			CloudEntry entry = CloudMethods.RetrieveFile(type, id);

			DialogResult dialogResult = MessageBox.Show($"Would you like to import {importType} \"{entry.Name}\" from the cloud?", "LEAGUEAIM", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

			if (dialogResult == DialogResult.No) Environment.Exit(0);

			CloudMethods.SaveFile(entry);

			MessageBox.Show("Sucessfully imported file!", "LEAGUEAIM", MessageBoxButtons.OK, MessageBoxIcon.Information);

			Environment.Exit(0);
		}
		public static void CreateUrlScheme()
		{
			Logger.DebugLine("Creating URL scheme..");
			RegistryKey key = Registry.ClassesRoot.CreateSubKey("LEAGUEAIM");
			key.SetValue("", "URL:LEAGUEAIM Protocol");
			key.SetValue("URL Protocol", "");
			RegistryKey command = key.CreateSubKey(@"shell\open\command");
			command.SetValue("", "\"" + Environment.ProcessPath + "\" \"%1\"");
			key.Close();
			command.Close();

			return;
		}
		public static void CheckForImport(string[] args)
		{
			if (args.Length == 0) return;

			string importData = args[0];

			if (importData.StartsWith("leagueaim:"))
			{
				string pattern = importData.Replace("leagueaim://", "");
				pattern = pattern.TrimEnd('/');

				ImportPattern(pattern);
			}
		}
	}
}
