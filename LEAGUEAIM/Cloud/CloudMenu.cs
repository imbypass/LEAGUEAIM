using ImGuiNET;
using LEAGUEAIM;
using LEAGUEAIM.Utilities;
using System.Numerics;
using System.Text;

namespace Script_Engine.Cloud
{
	internal static class CloudMenu
	{
		enum SortingMethods : byte
		{
			NameAsc,
			NameDesc,
			AuthorAsc,
			AuthorDesc,
			TypeAsc,
			TypeDesc,
			RatingAsc,
			RatingDesc
		}
		private static SortingMethods SortingMethod = SortingMethods.RatingAsc;
		public static bool Enabled = false;
		private static List<CloudEntry> configs = [];
		private static List<CloudEntry> scripts = [];
		private static List<CloudEntry> patterns = [];
		private static List<CloudEntry> styles = [];
		private static List<CloudEntry> community = [];
		private static Vector2 menuPos = new(0, 0);

		private static readonly string[] FilterTypes = ["All", "Profiles", "Patterns", "Scripts", "Styles"];
		private static int FilterType = 0;
		private static string SearchQueryCommunity = string.Empty;
		private static string SearchQueryProfiles = string.Empty;
		private static string SearchQueryPatterns = string.Empty;
		private static string SearchQueryScripts = string.Empty;

		private static bool HasRoomOnRight()
		{
			return Menu.Position.X + Menu.Size.X + ImGui.GetWindowSize().X + 1 < Program.ScreenSize.Width;
		}
		private static bool HasRoomOnLeft()
		{
			return Menu.Position.X - ImGui.GetWindowSize().X - 1 > 0;
		}

		public static void Render()
		{
			if (!Enabled || !Settings.Engine.IsVisible)
				return;

			ImGui.SetNextWindowSizeConstraints(new Vector2(600, 400), new Vector2(600, 400));
			if (ImGui.Begin("Cloud", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoNavInputs | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoDocking))
			{
				Vector2 rightSide = Menu.Position + new Vector2(Menu.Size.X, 0) + new Vector2(1, 0);
				Vector2 leftSide = Menu.Position - new Vector2(ImGui.GetWindowSize().X, 0) - new Vector2(1, 0);

				// check if the CloudMenu has room on the right side of the Menu and wont get cut off on screen
				if (HasRoomOnRight())
					menuPos = rightSide;
				else if (HasRoomOnLeft())
					menuPos = leftSide;
				else
					menuPos = rightSide;

				ImGui.SetWindowPos(menuPos, ImGuiCond.Always);

				Drawing.TextHeader("Cloud Storage", 2f, 5);

				ImGui.Spacing();

				// create tabs
				ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(15, 15));
				if (ImGui.BeginTabBar("##TABS", ImGuiTabBarFlags.None))
				{
					int textOffset = 5;

					int columnWidthName = 230;
					int columnWidthAuthor = 180;
					int columnWidthActions = 160;

					// CREATE CONFIGS TAB
					if (ImGui.BeginTabItem("Profiles"))
					{
						RenderStorageList(configs, ref SearchQueryProfiles);

						ImGui.EndTabItem();
					}

					// CREATE PATTERNS TAB
					if (ImGui.BeginTabItem("Patterns"))
					{
						RenderStorageList(patterns, ref SearchQueryPatterns);

						ImGui.EndTabItem();
					}

					// CREATE SCRIPTS TAB
					if (ImGui.BeginTabItem("Scripts"))
					{
						RenderStorageList(scripts, ref SearchQueryScripts);

						ImGui.EndTabItem();
					}

					// CREATE SCRIPTS TAB
					if (ImGui.BeginTabItem("Styles"))
					{
						RenderStorageList(styles, ref SearchQueryScripts);

						ImGui.EndTabItem();
					}

					// CREATE COMMUNITY TAB
					if (ImGui.BeginTabItem("Community"))
					{
						ImGui.SameLine();

						// SEARCH LABEL
						ImGui.PushStyleColor(ImGuiCol.Text, Color.White.ToVector4());
						ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 65);
						ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 275);
						ImGui.Text("Search:");

						ImGui.SameLine();

						// SEARCH BOX
						ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 65);
						ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 215);
						ImGui.SetNextItemWidth(200);
						ImGui.InputTextWithHint("##Search", "Search community", ref SearchQueryCommunity, 32);
						ImGui.PopStyleColor();

						ImGui.SameLine();

						// FILTER LABEL
						ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 30);
						ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 265);
						ImGui.Text("Filter:");

						ImGui.SameLine();

						// FILTER COMBO
						ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 30);
						ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 215);
						ImGui.SetNextItemWidth(200);
						ImGui.Combo("##Filter", ref FilterType, FilterTypes, FilterTypes.Length);


						ImGui.BeginChild("##COMMUNITY", new Vector2(0, 0), ImGuiChildFlags.None, ImGuiWindowFlags.NoDocking);
						ImGui.Spacing();
						ImGui.Columns(5, "##COMMUNITY", false);

						ImGui.SetColumnWidth(0, columnWidthName - 80);
						ImGui.SetColumnWidth(1, columnWidthAuthor - 80);
						ImGui.SetColumnWidth(2, 80);
						ImGui.SetColumnWidth(3, 80);
						ImGui.SetColumnWidth(4, columnWidthActions);


						ImGui.PushFont(Fonts.MenuLg);
						ImGui.TextColored(
							(SortingMethod == SortingMethods.NameAsc || SortingMethod == SortingMethods.NameDesc) ?
								Settings.Colors.AccentColor :
								Settings.Colors.TextColor,
							(SortingMethod == SortingMethods.NameAsc || SortingMethod == SortingMethods.NameDesc) ?
								"[Name]" :
								"Name"
						);
						if (ImGui.IsItemClicked())
						{
							SortingMethod = SortingMethod == SortingMethods.NameAsc ? SortingMethods.NameDesc : SortingMethods.NameAsc;
							UpdateSortingMethod();
						}
						if (ImGui.IsItemHovered())
							ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
						ImGui.NextColumn();
						ImGui.TextColored(
							(SortingMethod == SortingMethods.AuthorAsc || SortingMethod == SortingMethods.AuthorDesc) ?
								Settings.Colors.AccentColor :
								Settings.Colors.TextColor,
							(SortingMethod == SortingMethods.AuthorAsc || SortingMethod == SortingMethods.AuthorDesc) ?
								"[Author]" :
								"Author"
						);
						if (ImGui.IsItemClicked())
						{
							SortingMethod = SortingMethod == SortingMethods.AuthorAsc ? SortingMethods.AuthorDesc : SortingMethods.AuthorAsc;
							UpdateSortingMethod();
						}
						if (ImGui.IsItemHovered())
							ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
						ImGui.NextColumn();
						ImGui.TextColored(
							(SortingMethod == SortingMethods.TypeAsc || SortingMethod == SortingMethods.TypeDesc) ?
								Settings.Colors.AccentColor :
								Settings.Colors.TextColor,
							(SortingMethod == SortingMethods.TypeAsc || SortingMethod == SortingMethods.TypeDesc) ?
								"[Type]" :
								"Type"
						);
						if (ImGui.IsItemClicked())
						{
							SortingMethod = SortingMethod == SortingMethods.TypeAsc ? SortingMethods.TypeDesc : SortingMethods.TypeAsc;
							UpdateSortingMethod();
						}
						if (ImGui.IsItemHovered())
							ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
						ImGui.NextColumn();
						ImGui.TextColored(
							(SortingMethod == SortingMethods.RatingAsc || SortingMethod == SortingMethods.RatingDesc) ?
								Settings.Colors.AccentColor :
								Settings.Colors.TextColor,
							(SortingMethod == SortingMethods.RatingAsc || SortingMethod == SortingMethods.RatingDesc) ?
								"[Rating]" :
								"Rating"
						);
						if (ImGui.IsItemClicked())
						{
							SortingMethod = SortingMethod == SortingMethods.RatingAsc ? SortingMethods.RatingDesc : SortingMethods.RatingAsc;
							UpdateSortingMethod();
						}
						if (ImGui.IsItemHovered())
							ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
						ImGui.NextColumn();
						ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5);
						ImGui.Text("Actions");
						ImGui.NextColumn();
						ImGui.PopFont();

						foreach (CloudEntry entry in FilterEntries(SearchEntries(community, SearchQueryCommunity), FilterType))
						{
							ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffset);
							ImGui.Text(entry.Name.TruncateWord(21));
							ImGui.NextColumn();
							ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffset);
							ImGui.Text(entry.Author);
							ImGui.NextColumn();
							ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffset);
							ImGui.Text((entry.Type[..1].ToUpper() + entry.Type[1..^1]).Replace("Config", "Profile"));
							ImGui.NextColumn();
							ImGui.PushFont(Fonts.Icons);
							ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffset);
							ImGui.TextColored(Settings.Colors.AccentColor, IconFonts.FontAwesome6.Star);
							ImGui.PopFont();
							ImGui.SameLine();
							ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 2);
							ImGui.Text(entry.Rating.ToString());
							ImGui.NextColumn();
							ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffset / 2);
							ImGui.PushFont(Fonts.IconsSm);
							if (ImGui.Button(IconFonts.FontAwesome6.Download + "##" + entry.Type + entry.Id, new(24, 24)))
							{
								CloudEntry item = CloudMethods.RetrieveFile(entry.Type, entry.Id);
								CloudMethods.SaveFile(item);
								ImGui.OpenPopup("File Saved");
							}
							if (ImGui.IsItemHovered())
							{
								ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
								ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
								if (ImGui.BeginTooltip())
								{
									ImGui.PushFont(Fonts.Menu);
									ImGui.Text("Download");
									ImGui.PopFont();
									ImGui.EndTooltip();
								}
								ImGui.PopStyleVar();
								ImGui.PopStyleColor();
							}
							ImGui.SameLine();
							if (ImGui.Button(IconFonts.FontAwesome6.Star + "##" + entry.Type + entry.Id, new(24, 24)))
							{
								CloudMethods.AddRating(entry.Type, entry.Id);
								UpdateEntries();
							}
							if (ImGui.IsItemHovered())
							{
								ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
								ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
								if (ImGui.BeginTooltip())
								{
									ImGui.PushFont(Fonts.Menu);
									ImGui.Text("Rate submission");
									ImGui.PopFont();
									ImGui.EndTooltip();
								}
								ImGui.PopStyleVar();
								ImGui.PopStyleColor();
							}
							ImGui.SameLine();
							if (ImGui.Button(IconFonts.FontAwesome6.Globe + "##" + entry.Type + entry.Id, new(24, 24)))
							{
								string data = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{entry.Type}|{entry.Id}"));
								string fullUri = $"https://leagueaim.gg/import?data={data}";
								ImGui.SetClipboardText(fullUri);
								ImGui.OpenPopup("Link Shared");
							}
							if (ImGui.IsItemHovered())
							{
								ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
								ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
								if (ImGui.BeginTooltip())
								{
									ImGui.PushFont(Fonts.Menu);
									ImGui.Text("Copy import link");
									ImGui.PopFont();
									ImGui.EndTooltip();
								}
								ImGui.PopStyleVar();
								ImGui.PopStyleColor();
							}
							ImGui.PopFont();

							Vector2 cMenuPos = ImGui.GetWindowPos();
							Vector2 cMenuSize = ImGui.GetWindowSize();
							bool fileSaved = true;
							ImGui.SetNextWindowPos(new(cMenuPos.X + (cMenuSize.X / 2) - 150, cMenuPos.Y + (cMenuSize.Y / 2) - 55));
							ImGui.SetNextWindowSize(new(300, 110));
							ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
							ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
							if (ImGui.BeginPopupModal("File Saved", ref fileSaved, ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoTitleBar))
							{
								ImGui.Spacing();
								Drawing.TextCentered("Successfully downloaded from the cloud!");
								ImGui.Spacing();
								ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.5f, ImGui.GetCursorPosY()));
								if (ImGui.Button("Dismiss", new(68, 28)))
								{
									ImGui.CloseCurrentPopup();
								}

								ImGui.EndPopup();
							}
							ImGui.PopStyleVar();
							ImGui.PopStyleColor();

							cMenuPos = ImGui.GetWindowPos();
							cMenuSize = ImGui.GetWindowSize();
							bool linkShared = true;
							ImGui.SetNextWindowPos(new(cMenuPos.X + (cMenuSize.X / 2) - 150, cMenuPos.Y + (cMenuSize.Y / 2) - 55));
							ImGui.SetNextWindowSize(new(300, 110));
							ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
							ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
							if (ImGui.BeginPopupModal("Link Shared", ref linkShared, ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoTitleBar))
							{
								ImGui.Spacing();
								Drawing.TextCentered("Copied link to clipboard!");
								ImGui.Spacing();
								ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.5f, ImGui.GetCursorPosY()));
								if (ImGui.Button("Dismiss", new(68, 28)))
								{
									ImGui.CloseCurrentPopup();
								}

								ImGui.EndPopup();
							}
							ImGui.PopStyleVar();
							ImGui.PopStyleColor();

							ImGui.NextColumn();
						}
						ImGui.Spacing();
						ImGui.EndChild();

						ImGui.EndTabItem();
					}

					ImGui.EndTabBar();
				}

				Drawing.AccentBar();

				ImGui.End();
			}
		}

		private static void RenderStorageList(List<CloudEntry> entries, ref string SearchQueryVar)
		{

			ImGui.SameLine();

			// SEARCH LABEL
			ImGui.PushStyleColor(ImGuiCol.Text, Color.White.ToVector4());
			ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 65);
			ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 275);
			ImGui.Text("Search:");

			ImGui.SameLine();

			// SEARCH BOX
			ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 65);
			ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 215);
			ImGui.SetNextItemWidth(200);
			ImGui.InputTextWithHint("##Search", $"Search {entries[0].Type.Replace("config", "profile")}", ref SearchQueryVar, 32);
			ImGui.PopStyleColor();

			int columnWidthName = 230;
			int columnWidthAuthor = 180;
			int columnWidthActions = 160;

			ImGui.BeginChild("##" + entries[0].Type.ToUpper(), new Vector2(0, 0), ImGuiChildFlags.None, ImGuiWindowFlags.NoDocking);
			ImGui.Spacing();
			ImGui.Columns(3, "##" + entries[0].Type.ToUpper(), false);


			ImGui.SetColumnWidth(0, columnWidthName);
			ImGui.SetColumnWidth(1, columnWidthAuthor);
			ImGui.SetColumnWidth(2, columnWidthActions);

			ImGui.PushFont(Fonts.MenuLg);
			ImGui.Text("Name");
			ImGui.NextColumn();
			ImGui.Text("Author");
			ImGui.NextColumn();
			ImGui.Text("Actions");
			ImGui.NextColumn();
			ImGui.PopFont();

			foreach (CloudEntry entry in SearchEntries(entries, SearchQueryVar))
			{
				RenderStorageEntry(entry);
			}
			ImGui.Spacing();
			ImGui.EndChild();
		}
		private static void RenderStorageEntry(CloudEntry entry)
		{
			Vector2 cMenuPos;
			Vector2 cMenuSize;

			int textOffset = 5;

			ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffset);
			ImGui.Text(entry.Name);
			ImGui.NextColumn();
			ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffset);
			ImGui.Text(entry.Author);
			ImGui.NextColumn();
			ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffset / 2);

			if (entry.Name != "No files found")
			{

				if (entry.Author != Program._XFUser.Username)
					ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetColumnWidth() - 24) / 4 + 2);

				ImGui.PushFont(Fonts.IconsSm);

				if (ImGui.Button(IconFonts.FontAwesome6.Download + "##" + entry.Type + entry.Id, new(24, 24)))
				{
					CloudEntry item = CloudMethods.RetrieveFile(entry.Type, entry.Id);
					CloudMethods.SaveFile(item);
					ImGui.OpenPopup("Entry Saved " + entry.Id);
				}
				if (ImGui.IsItemHovered())
				{
					ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
					ImGui.PushStyleColor(ImGuiCol.PopupBg, Settings.Colors.BgColor);
					ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
					if (ImGui.BeginTooltip())
					{
						ImGui.PushFont(Fonts.Menu);
						ImGui.Text("Download");
						ImGui.PopFont();
						ImGui.EndTooltip();
					}
					ImGui.PopStyleVar();
					ImGui.PopStyleColor();
					ImGui.PopStyleColor();
				}

				ImGui.SameLine();

				if (ImGui.Button(IconFonts.FontAwesome6.TrashCan + "##" + entry.Type + entry.Id, new(24, 24)))
				{
					ImGui.OpenPopup("Delete Entry " + entry.Id);
				}
				if (ImGui.IsItemHovered())
				{
					ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
					ImGui.PushStyleColor(ImGuiCol.PopupBg, Settings.Colors.BgColor);
					ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
					if (ImGui.BeginTooltip())
					{
						ImGui.PushFont(Fonts.Menu);
						ImGui.Text("Delete");
						ImGui.PopFont();
						ImGui.EndTooltip();
					}
					ImGui.PopStyleVar();
					ImGui.PopStyleColor();
					ImGui.PopStyleColor();
				}

				ImGui.SameLine();

				if (ImGui.Button(IconFonts.FontAwesome6.Globe + "##" + entry.Type + entry.Id, new(24, 24)))
				{
					string data = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{entry.Type}|{entry.Id}"));
					string fullUri = $"https://leagueaim.gg/import?data={data}";
					ImGui.SetClipboardText(fullUri);
					ImGui.OpenPopup("Entry Shared");
				}
				if (ImGui.IsItemHovered())
				{
					ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
					ImGui.PushStyleColor(ImGuiCol.PopupBg, Settings.Colors.BgColor);
					ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
					if (ImGui.BeginTooltip())
					{
						ImGui.PushFont(Fonts.Menu);
						ImGui.Text("Copy import link");
						ImGui.PopFont();
						ImGui.EndTooltip();
					}
					ImGui.PopStyleVar();
					ImGui.PopStyleColor();
					ImGui.PopStyleColor();
				}

				ImGui.SameLine();

				if (ImGui.Button(IconFonts.FontAwesome6.ShareFromSquare + "##" + entry.Type + entry.Id, new(24, 24)))
				{
					ImGui.OpenPopup("Confirm Entry " + entry.Id);
				}
				if (ImGui.IsItemHovered())
				{
					ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
					ImGui.PushStyleColor(ImGuiCol.PopupBg, Settings.Colors.BgColor);
					ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
					if (ImGui.BeginTooltip())
					{
						ImGui.PushFont(Fonts.Menu);
						ImGui.Text("Publish to Community");
						ImGui.PopFont();
						ImGui.EndTooltip();
					}
					ImGui.PopStyleVar();
					ImGui.PopStyleColor();
					ImGui.PopStyleColor();
				}

				ImGui.PopFont();
			}


			cMenuPos = ImGui.GetWindowPos();
			cMenuSize = ImGui.GetWindowSize();
			bool entry_saved = true;
			ImGui.SetNextWindowPos(new(cMenuPos.X + (cMenuSize.X / 2) - 150, cMenuPos.Y + (cMenuSize.Y / 2) - 55));
			ImGui.SetNextWindowSize(new(300, 110));
			ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
			ImGui.PushStyleColor(ImGuiCol.PopupBg, Settings.Colors.BgColor);
			ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
			if (ImGui.BeginPopupModal("Entry Saved " + entry.Id, ref entry_saved, ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoTitleBar))
			{
				ImGui.Spacing();
				Drawing.TextCentered($"{(entry.Type[..1].ToUpper() + entry.Type[1..].TrimEnd('s')).Replace("Config", "Profile")} \"{entry.Name}\" saved successfully!");
				ImGui.Spacing();
				ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.5f, ImGui.GetCursorPosY()));
				if (ImGui.Button("Dismiss", new(68, 28)))
				{
					ImGui.CloseCurrentPopup();
				}

				ImGui.EndPopup();
			}
			ImGui.PopStyleVar();
			ImGui.PopStyleColor();
			ImGui.PopStyleColor();

			cMenuPos = ImGui.GetWindowPos();
			cMenuSize = ImGui.GetWindowSize();
			bool entry_prompted = true;
			ImGui.SetNextWindowPos(new(cMenuPos.X + (cMenuSize.X / 2) - (500 / 2), cMenuPos.Y + (cMenuSize.Y / 2) - 55));
			ImGui.SetNextWindowSize(new(500, 110));
			ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
			ImGui.PushStyleColor(ImGuiCol.PopupBg, Settings.Colors.BgColor);
			ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
			if (ImGui.BeginPopupModal("Confirm Entry " + entry.Id, ref entry_prompted, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoTitleBar))
			{
				ImGui.Spacing();
				Drawing.TextCentered($"Are you sure you want to publish \"{entry.Name}\" to the community hub?");
				ImGui.Spacing();
				ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.4f, ImGui.GetCursorPosY()));
				if (ImGui.Button("Yes", new(68, 28)))
				{
					CloudMethods.AddToCommunity(entry.Type, entry.Id);
					UpdateEntries();
					ImGui.CloseCurrentPopup();
					Thread.Sleep(1);
					ImGui.OpenPopup("Entry Uploaded " + entry.Id);
				}
				ImGui.SameLine();
				ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.6f, ImGui.GetCursorPosY()));
				if (ImGui.Button("No", new(68, 28)))
				{
					ImGui.CloseCurrentPopup();
				}
				ImGui.Spacing();

				ImGui.EndPopup();
			}
			ImGui.PopStyleColor();
			ImGui.PopStyleColor();
			ImGui.PopStyleVar();

			cMenuPos = ImGui.GetWindowPos();
			cMenuSize = ImGui.GetWindowSize();
			bool entry_delete = true;
			ImGui.SetNextWindowPos(new(cMenuPos.X + (cMenuSize.X / 2) - 200, cMenuPos.Y + (cMenuSize.Y / 2) - 55));
			ImGui.SetNextWindowSize(new(400, 110));
			ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
			ImGui.PushStyleColor(ImGuiCol.PopupBg, Settings.Colors.BgColor);
			ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
			if (ImGui.BeginPopupModal("Delete Entry " + entry.Id, ref entry_delete, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoTitleBar))
			{
				ImGui.Spacing();
				Drawing.TextCentered($"Are you sure you want to delete \"{entry.Name}\" from the cloud?");
				ImGui.Spacing();
				ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.3f, ImGui.GetCursorPosY()));
				if (ImGui.Button("Yes", new(68, 28)))
				{
					CloudMethods.DeleteFile(entry.Type, entry.Id);
					UpdateEntries();
					ImGui.CloseCurrentPopup();
				}
				ImGui.SameLine();
				ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.6f, ImGui.GetCursorPosY()));
				if (ImGui.Button("No", new(68, 28)))
				{
					ImGui.CloseCurrentPopup();
				}
				ImGui.Spacing();

				ImGui.EndPopup();
			}
			ImGui.PopStyleColor();
			ImGui.PopStyleColor();
			ImGui.PopStyleVar();

			cMenuPos = ImGui.GetWindowPos();
			cMenuSize = ImGui.GetWindowSize();
			bool entry_shared = true;
			ImGui.SetNextWindowPos(new(cMenuPos.X + (cMenuSize.X / 2) - 150, cMenuPos.Y + (cMenuSize.Y / 2) - 55));
			ImGui.SetNextWindowSize(new(300, 110));
			ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
			ImGui.PushStyleColor(ImGuiCol.PopupBg, Settings.Colors.BgColor);
			ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
			if (ImGui.BeginPopupModal("Entry Shared", ref entry_shared, ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoTitleBar))
			{
				ImGui.Spacing();
				Drawing.TextCentered("Copied link to clipboard!");
				ImGui.Spacing();
				ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.5f, ImGui.GetCursorPosY()));
				if (ImGui.Button("Dismiss", new(68, 28)))
				{
					ImGui.CloseCurrentPopup();
				}

				ImGui.EndPopup();
			}
			ImGui.PopStyleVar();
			ImGui.PopStyleColor();
			ImGui.PopStyleColor();

			cMenuPos = ImGui.GetWindowPos();
			cMenuSize = ImGui.GetWindowSize();
			bool entry_uploaded = true;
			ImGui.SetNextWindowPos(new(cMenuPos.X + (cMenuSize.X / 2) - 150, cMenuPos.Y + (cMenuSize.Y / 2) - 55));
			ImGui.SetNextWindowSize(new(300, 110));
			ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
			ImGui.PushStyleColor(ImGuiCol.PopupBg, Settings.Colors.BgColor);
			ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
			if (ImGui.BeginPopupModal("Entry Uploaded " + entry.Id, ref entry_uploaded, ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoTitleBar))
			{
				ImGui.Spacing();
				Drawing.TextCentered($"{entry.Type[..1].ToUpper() + entry.Type[1..].TrimEnd('s')} \"{entry.Name}\" uploaded to community hub!");
				ImGui.Spacing();
				ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.5f, ImGui.GetCursorPosY()));
				if (ImGui.Button("Dismiss", new(68, 28)))
				{
					ImGui.CloseCurrentPopup();
				}

				ImGui.EndPopup();
			}
			ImGui.PopStyleVar();
			ImGui.PopStyleColor();
			ImGui.PopStyleColor();

			ImGui.NextColumn();
		}

		public static void UpdateEntries()
		{
			configs = CloudMethods.RetrieveFiles("configs");
			scripts = CloudMethods.RetrieveFiles("scripts");
			patterns = CloudMethods.RetrieveFiles("patterns");
			styles = CloudMethods.RetrieveFiles("styles");
			community = CloudMethods.RetrieveCommunity();

			UpdateSortingMethod();
		}
		private static void UpdateSortingMethod()
		{
			switch (SortingMethod)
			{
				case SortingMethods.NameAsc:
					community = [.. community.OrderBy(x => x.Name)];
					break;
				case SortingMethods.NameDesc:
					community = [.. community.OrderByDescending(x => x.Name)];
					break;
				case SortingMethods.AuthorAsc:
					community = [.. community.OrderBy(x => x.Author)];
					break;
				case SortingMethods.AuthorDesc:
					community = [.. community.OrderByDescending(x => x.Author)];
					break;
				case SortingMethods.TypeAsc:
					community = [.. community.OrderBy(x => x.Type)];
					break;
				case SortingMethods.TypeDesc:
					community = [.. community.OrderByDescending(x => x.Type)];
					break;
				case SortingMethods.RatingAsc:
					community = [.. community.OrderByDescending(x => x.Rating)];
					break;
				case SortingMethods.RatingDesc:
					community = [.. community.OrderBy(x => x.Rating)];
					break;
			}
		}
		private static List<CloudEntry> FilterEntries(List<CloudEntry> items, int type)
		{
			if (type == 0)
				return items;

			return items.Where(x => x.Type.ToString().Replace("config", "profile").Equals(FilterTypes[type].ToLower(), StringComparison.CurrentCultureIgnoreCase)).ToList();

		}
		private static List<CloudEntry> SearchEntries(List<CloudEntry> items, string text)
		{
			if (string.IsNullOrEmpty(text))
				return items;

			return items.Where(
				x => x.Name.Contains(text, StringComparison.CurrentCultureIgnoreCase) ||
				x.Author.Contains(text, StringComparison.CurrentCultureIgnoreCase)
			).ToList();
		}

		public static void UpdateLoop()
		{
			while (true)
			{
				UpdateEntries();
				Thread.Sleep(1000 * 30);
			}
		}
	}
}
