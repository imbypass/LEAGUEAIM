using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace LEAGUEAIM.Utilities
{
	public class IniFile
	{
		private readonly string Path;
		private readonly string EXE = Assembly.GetExecutingAssembly().GetName().Name;

		[DllImport("kernel32", CharSet = CharSet.Unicode)]
		private static extern long WritePrivateProfileString(
		  string Section,
		  string Key,
		  string Value,
		  string FilePath);

		[DllImport("kernel32", CharSet = CharSet.Unicode)]
		private static extern int GetPrivateProfileString(
		  string Section,
		  string Key,
		  string Default,
		  StringBuilder RetVal,
		  int Size,
		  string FilePath);

		public IniFile(string IniPath = null) => this.Path = new FileInfo(IniPath ?? this.EXE + ".ini").FullName.ToString();

		public string Read(string Key, string Section = null)
		{
			StringBuilder RetVal = new(byte.MaxValue);
			_ = GetPrivateProfileString(Section ?? EXE, Key, "", RetVal, byte.MaxValue, Path);
			return RetVal.ToString();
		}
		public T Read<T>(string Key, string Section = null)
		{
			string res = Read(Key, Section);
			try
			{
				return (T)Convert.ChangeType(res, typeof(T));
			}
			catch
			{
				bool val = res.ToString().Equals("true", StringComparison.CurrentCultureIgnoreCase);
				return (T)Convert.ChangeType(val, typeof(T));
			}
		}

		public void Write(string Key, string Value, string Section = null) => IniFile.WritePrivateProfileString(Section ?? this.EXE, Key, Value, this.Path);

		public void DeleteKey(string Key, string Section = null) => this.Write(Key, (string)null, Section ?? this.EXE);

		public void DeleteSection(string Section = null) => this.Write((string)null, (string)null, Section ?? this.EXE);

		public bool KeyExists(string Key, string Section = null) => this.Read(Key, Section).Length > 0;
	}
}
