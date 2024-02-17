using ImGuiNET;
using LEAGUEAIM;
using LEAGUEAIM.Utilities;
using Microsoft.Win32;
using Script_Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using static LEAGUEAIM.Settings;

namespace LEAGUEAIM
{
    internal class LoginHelper
    {
        public static bool IsSaved = false;
        public static bool IsReady = false;
        public static string Username;
        public static string Password;
        public static string Response;

        public static void Render()
        {
            if (IsReady) { return; }

            if (!IsSaved)
            {
                Logger.WriteLine("Found credentials in local database");

                if (LoadCredentials(out Username, out Password))
                {
                    CheckLogin();
                }

                IsSaved = true;
            }

            LARenderer.ApplyAccentColor();

            ImGui.SetNextWindowSize(new Vector2(270, 290));

            if (ImGui.Begin("LOGIN", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize))
            {
                Drawing.TextHeader("Login", 2f, 5);
                ImGui.Text("Username:");
                ImGui.PushItemWidth(245);
                ImGui.InputText("##Username", ref Username, 32, ImGuiInputTextFlags.None);
                ImGui.PopItemWidth();
                ImGui.Text("Password:");
                ImGui.PushItemWidth(245);
                ImGui.InputText("##Password", ref Password, 32, ImGuiInputTextFlags.Password);
                if (ImGui.IsItemClicked())
                {
                    Password = string.Empty;
                }
                ImGui.PopItemWidth();
                ImGui.Spacing();
                ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 165) * 0.5f, ImGui.GetCursorPosY()));
                if (ImGui.Button("Login", new(165, 28)))
                {
                    if (Username.Contains('@'))
                    {
                        Response = "Please login with your username, not your email address.";
                        ImGui.OpenPopup("Login Error");
                    }
                    else
                    {
                        CheckLogin();
                    }
                }
                bool err = true;
                Vector2 cMenuPos = ImGui.GetWindowPos();
                Vector2 cMenuSize = ImGui.GetWindowSize();
                float stringSize = ImGui.CalcTextSize(Response).X;
                ImGui.SetNextWindowPos(new(cMenuPos.X + cMenuSize.X / 2 - (stringSize + 100) * .5f, cMenuPos.Y + cMenuSize.Y / 2 - 50));
                ImGui.SetNextWindowSize(new(stringSize + 100, 100));
				ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
				ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
				if (ImGui.BeginPopupModal("Login Error", ref err, ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize))
                {
                    ImGui.Spacing();
                    Drawing.TextCentered(Response);
                    ImGui.Spacing();
                    ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.5f, ImGui.GetCursorPosY()));
                    if (ImGui.Button("Dismiss", new(68, 28)))
                    {
                        ImGui.CloseCurrentPopup();
                    }

                    ImGui.EndPopup();
                }
                ImGui.PopStyleColor();
                ImGui.PopStyleVar();
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 1, 0.5f));
                ImGui.Spacing();
                ImGui.Text("Forgot password?");
                if (ImGui.IsItemClicked())
                {
                    ProcessStartInfo p = new()
                    {
                        FileName = "https://leagueaim.gg/forum/index.php?lost-password/",
                        UseShellExecute = true,
                        Verb = "open"
                    };
                    Process.Start(p);
                }
                ImGui.PopStyleColor();

                Drawing.AccentBar();

                ImGui.End();
            }
        }
        public static void SaveCredentials(string username, string password)
        {
            using RegistryKey credsKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\WOW6432Node\LEAGUEAIM");
            credsKey.SetValue("Username", username);
            credsKey.SetValue("Password", password);
            credsKey.Close();
        }
        public static bool LoadCredentials(out string username, out string password)
        {
            using RegistryKey credsKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\LEAGUEAIM");
            if (credsKey != null)
            {
                username = credsKey.GetValue("Username").ToString();
                password = credsKey.GetValue("Password").ToString();
                return username.Length > 0 && password.Length > 0;
            }
            else
            {
                username = string.Empty;
                password = string.Empty;
                return false;
            }
        }
        public static void CheckLogin()
        {
            Program._XF.Authenticate(Username, Password);

            if (Program._XF.IsAuthenticated)
            {
                Program._XFUser = Program._XF.GetUser();

                string remoteHwid = Program._XFUser.Fields[0].Value;
                string localHwid = WindowsIdentity.GetCurrent().User.Value;

                bool isBanned = Program._XFUser.UserGroups.ToList().Any(group => group.UserGroupTitle == "Banned");
                bool isRegistered = Program._XFUser.UserGroups.ToList().Any(group => group.UserGroupTitle == "Registered");
                bool isSubscribed = Program._XFUser.UserGroups.ToList().Any(group => group.UserGroupTitle == Product.ProductName);

                if (Program._XFUser == null)
                {
                    Response = "Unable to login. Please check credentials and try again.";
                    Logger.ErrorLine(Response);
                    ImGui.OpenPopup("Login Error");
                    return;
                }

                if (remoteHwid != string.Empty && remoteHwid != localHwid)
                {
                    Response = "Invalid or mismatched HWID.";
                    Logger.ErrorLine(Response);
                    ImGui.OpenPopup("Login Error");
                    return;
                }

                if (isBanned)
                {
                    Response = "Your account has been banned. Contact an admin to appeal.";
                    Logger.ErrorLine(Response);
                    ImGui.OpenPopup("Login Error");
                    return;
                }

                if (!isSubscribed)
                {
                    Response = "No active subscription found.";
                    Logger.ErrorLine(Response);
                    ImGui.OpenPopup("Login Error");
                    return;
                }

                if (!isRegistered)
                {
                    Response = "Account not found.";
                    Logger.ErrorLine(Response);
                    ImGui.OpenPopup("Login Error");
                    return;
                }

                if (remoteHwid == string.Empty)
                {
                    Logger.Write("Updating HWID.. ");
                    Program._XFUser.Fields[0].Value = localHwid;
                    Program._XF.UpdateHwid();
                    Logger.WriteLine("Done.");
                }

                Logger.WriteLine("Authentication successful!");
                Logger.Events.UserLoggedIn();
                Settings.Engine.AvatarPath = DownloadAvatarToDisk();
                SaveCredentials(Username, Password);
                Engine.StartThreads();
                IsReady = true;

                Logger.Write($"Logged in as:");
                Logger.Text.FromColor(Settings.Colors.AccentColor.ToColor());
                Console.WriteLine($" {Program._XFUser.Username}");
                Logger.Text.Reset();
                Logger.WriteLine("Welcome to LEAGUEAIM.gg!");
            }
            else
            {
                Response = "Unable to login. Please check credentials and try again.";
                ImGui.OpenPopup("Login Error");
            }
        }
        public static void CheckSession(int timeoutCheck)
        {
            if (timeoutCheck <= 3)
            {
                try
                {
                    Program._XF.Authenticate(Username, Password);

                    if (Program._XF.IsAuthenticated && Program._XFUser != null)
                    {
                        Program._XFUser = Program._XF.GetUser();

                        string remoteHwid = Program._XFUser.Fields[0].Value;

                        string localHwid = WindowsIdentity.GetCurrent().User.Value;

                        bool isBanned = Program._XFUser.UserGroups.ToList().Any(group => group.UserGroupTitle == "Banned");

                        bool isRegistered = Program._XFUser.UserGroups.ToList().Any(group => group.UserGroupTitle == "Registered");

                        bool isSubscribed = Program._XFUser.UserGroups.ToList().Any(group => group.UserGroupTitle == Product.ProductName);

                        bool hwidMatches = remoteHwid == localHwid;

                        if (isRegistered && isSubscribed && hwidMatches && !isBanned)
                            return;

                        Environment.Exit(0);
                    }
                }
                catch
                {
                    Thread.Sleep(1000);

                    CheckSession(timeoutCheck + 1);
                }
            }
            else
            {
                Program._XF = new(Program._Config);

                CheckSession(0);
            }
        }
        public static string DownloadAvatarToDisk()
        {
            string localPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LEAGUEAIM", $"{Program._XFUser.Username}.png");
            string remotePath = Program._XFUser.Links.AvatarSmall.Split("?")[0];

            if (remotePath == null || remotePath == string.Empty)
                remotePath = "http://auth.leagueaim.gg/forum/default.png";

            using HttpClient hc = new(new HttpClientHandler() { Proxy = null, UseProxy = false });
            using HttpResponseMessage response = hc.GetAsync(remotePath).Result;
            using HttpContent content = response.Content;
            using Stream stream = content.ReadAsStreamAsync().Result;
            using FileStream fs = new(localPath, FileMode.Create, FileAccess.Write);
            stream.CopyTo(fs);
            stream.Dispose();
            content.Dispose();
            response.Dispose();
            hc.Dispose();

            return localPath;
        }
        public static void Loop()
        {
            while (true)
            {
                Thread.Sleep(1000 * 60 * 2);

                CheckSession(0);
            }
        }
    }
}
