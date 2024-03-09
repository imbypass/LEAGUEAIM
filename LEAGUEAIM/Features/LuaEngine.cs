using NLua;
using ImGuiNET;
using LEAGUEAIM.Utilities;
using System.Numerics;
using System.Diagnostics;
using Script_Engine.Utilities;
using Script_Engine.Cloud;

namespace LEAGUEAIM.Features
{
	internal class LuaEngine
	{
		public static Lua Engine = new();
		public static readonly LogitechWrapper Wrapper = new();
		public static string Base = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LEAGUEAIM");
		public static string Location = Path.Combine(Base, "scripts");
		public static string CurrentScript = string.Empty;
		public static string[] ScriptList;


		public static string[] GetScriptList()
		{
			if (!Directory.Exists(Location)) Directory.CreateDirectory(Location);

			string[] profiles = [];
			string[] files = Directory.GetFiles(Location);
			foreach (string file in files)
			{
				if (file.EndsWith(".lua"))
				{
					Array.Resize(ref profiles, profiles.Length + 1);
					profiles[^1] = file.Replace(Location, "")[1..];

				}
			}
			if (profiles.Length > 0 && Settings.Menu.CurrentScript == -1)
				Settings.Menu.CurrentScript = 0;

			return profiles;
		}
		public static void Load()
		{
			string scriptName = Settings.Lua.ScriptName;
			if (ScriptContainsUnsupportedMethods(scriptName))
			{
				Wrapper.OutputLogMessage("Script contains unsupported methods. Please check the documentation for more information.\n");
				return;
			}

			Logger.LogitechLine("Loading script..");
			Engine["Logitech"] = Wrapper;
			Engine.DoString(LogitechWrapper.Sandbox());
			LEAGUEAIM.Engine.Logs.Clear();
			try
			{
				string scriptPath = Path.Combine(Location, Settings.Lua.ScriptName);
				CurrentScript = File.ReadAllText(scriptPath);
				Engine.DoString(CurrentScript);

				Wrapper.OutputLogMessage("LOADED!\n");
				if (Engine["OnEvent"] is LuaFunction OnEvent)
					OnEvent.Call("PROFILE_ACTIVATED", true.ToString());
			}
			catch (NLua.Exceptions.LuaScriptException ex)
			{
				string outputMsg = String.Format("[ERROR]: {0}\n", ex.Message.Replace("chunk", Settings.Lua.ScriptName));
				Wrapper.OutputLogMessage(outputMsg);
			}
		}
		public static void Unload()
		{
			Logger.LogitechLine("Unloading script..");
			try
			{
				if (Engine["OnEvent"] is LuaFunction OnEvent)
					OnEvent.Call("PROFILE_DEACTIVATED", true.ToString());
			}
			catch (NLua.Exceptions.LuaScriptException ex)
			{
				string outputMsg = String.Format("[ERROR]: {0}\n", ex.Message.Replace("chunk", Settings.Lua.ScriptName));
				Wrapper.OutputLogMessage(outputMsg);
			}
			LEAGUEAIM.Engine.Logs.Clear();
			CurrentScript = string.Empty;
		}
		public static bool ScriptContainsUnsupportedMethods(string scriptName)
		{
			string scriptPath = Path.Combine(Location, scriptName);
			string scriptContents = File.ReadAllText(scriptPath);
			foreach (string method in LogitechWrapper.GetUnsupportedMethods())
			{
				if (scriptContents.Contains(method))
					return true;
			}
			return false;
		}
		public static void Run()
		{
			try
			{
				Program.StartTime = DateTime.Now;
				EventHandler.Run();
			}
			catch (ThreadInterruptedException) {
			
			}
		}
		public static void Render()
		{
			ImGui.Text("Logitech Scripts");
			if (CurrentScript != string.Empty)
			{
				ImGui.SameLine();
				ImGui.Text("-");
				ImGui.SameLine();
				ImGui.TextColored(Settings.Colors.AccentColor, $"{Settings.Lua.ScriptName}");
			}
			if (ImGui.ListBox("###SCRIPTS", ref Settings.Lua.CurrentScript, ScriptList, ScriptList.Length, 4))
			{
				Settings.Lua.ScriptName = ScriptList[Settings.Lua.CurrentScript];
			}
			string activeState = (Settings.Lua.Enabled ? "Deactivate" : "Activate");
			if (ImGui.Button($"{activeState} Script", new Vector2(Settings.ButtonSizes.Full, 30)))
			{
				if (Settings.Lua.CurrentScript > -1)
				{
					Settings.Lua.Enabled = !Settings.Lua.Enabled;

					if (Settings.Lua.Enabled)
					{
						Load();
						LEAGUEAIM.Engine.LuaThread = new Thread(Run) { IsBackground = true };
						LEAGUEAIM.Engine.LuaThread.Start();
					}
					else
					{
						Unload();
						LEAGUEAIM.Engine.LuaThread.Interrupt();
						LEAGUEAIM.Engine.LuaThread = null;
					}
				}
			}
			if (Drawing.IconButton("Open", IconFonts.FontAwesome6.FolderOpen, new(Settings.ButtonSizes.Half, 28), true, ImGui.GetStyle().FrameRounding, 0))
			{
				string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LEAGUEAIM", "scripts");
				if (Directory.Exists(folderPath))
				{
					ProcessStartInfo startInfo = new()
					{
						Arguments = folderPath,
						FileName = "explorer.exe"
					};
					Process.Start(startInfo);
				}
			}
			ImGui.SameLine();
			if (Drawing.IconButton("Delete", IconFonts.FontAwesome6.TrashCan, new(Settings.ButtonSizes.Half, 28), true, ImGui.GetStyle().FrameRounding, 0))
			{
				if (Settings.Lua.CurrentScript > -1)
					ImGui.OpenPopup("Delete Script");
			}
			if (Drawing.IconButton("Send to Cloud", IconFonts.FontAwesome6.Upload, new(Settings.ButtonSizes.Full, 28), true, ImGui.GetStyle().FrameRounding, 0))
			{
				if (Settings.Lua.CurrentScript > -1)
					ImGui.OpenPopup("Upload Script");
			}
			ImGui.Separator();
			ImGui.Text("Console");
			string console = LEAGUEAIM.Engine.Logs.Output.SpliceText(50);
			ImGui.InputTextMultiline("##OUTPUT", ref console, 10000, new(ImGui.GetContentRegionAvail().X, 100), ImGuiInputTextFlags.ReadOnly);


			Vector2 cMenuPos = ImGui.GetWindowPos();
			Vector2 cMenuSize = ImGui.GetWindowSize();
			bool open = true;
			ImGui.SetNextWindowPos(new(cMenuPos.X + (cMenuSize.X / 2) - 175, cMenuPos.Y + (cMenuSize.Y / 2) - 55));
			ImGui.SetNextWindowSize(new(350, 110));
			if (ImGui.BeginPopupModal("Delete Script", ref open, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoTitleBar))
			{
				string scriptName = LuaEngine.ScriptList[Settings.Lua.CurrentScript];
				ImGui.Spacing();
				Drawing.TextCentered($"Are you sure you want to delete script: {scriptName.Replace(".lua", "").TruncateWord(12)}.lua?");
				ImGui.Spacing();
				ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.333f, ImGui.GetCursorPosY()));
				if (ImGui.Button("Yes", new(68, 28)))
				{
					File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LEAGUEAIM", "scripts", scriptName));
					ImGui.CloseCurrentPopup();
				}
				ImGui.SameLine();
				ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.666f, ImGui.GetCursorPosY()));
				if (ImGui.Button("No", new(68, 28)))
				{
					ImGui.CloseCurrentPopup();
				}

				ImGui.EndPopup();
			}

			cMenuPos = ImGui.GetWindowPos();
			cMenuSize = ImGui.GetWindowSize();
			bool upload_script = true;
			ImGui.SetNextWindowPos(new(cMenuPos.X + (cMenuSize.X / 2) - 175, cMenuPos.Y + (cMenuSize.Y / 2) - 55));
			ImGui.SetNextWindowSize(new(350, 110));
			ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
			ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
			if (ImGui.BeginPopupModal("Upload Script", ref upload_script, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoTitleBar))
			{
				string scriptName = LuaEngine.ScriptList[Settings.Lua.CurrentScript];
				ImGui.Spacing();
				Drawing.TextCentered($"Do you want to upload \"{scriptName}\" to the cloud?");
				ImGui.Spacing();
				ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.333f, ImGui.GetCursorPosY()));
				if (ImGui.Button("Yes", new(68, 28)))
				{
					CloudMethods.UploadFile("scripts", scriptName);
					ImGui.CloseCurrentPopup();
				}
				ImGui.SameLine();
				ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.666f, ImGui.GetCursorPosY()));
				if (ImGui.Button("No", new(68, 28)))
				{
					ImGui.CloseCurrentPopup();
				}

				ImGui.EndPopup();
			}
			ImGui.PopStyleColor();
			ImGui.PopStyleVar();
		}

		public static class EventHandler
		{
			public static void Run()
			{

				try
				{
					while (true)
					{
						#region " ONUPDATE "
						// ONUPDATE
						try
						{
							if (Engine["OnUpdate"] is LuaFunction OnUpdate)
							{
								OnUpdate.Call();
							}
						}
						catch (NLua.Exceptions.LuaScriptException ex)
						{
							string outputMsg = String.Format("[ERROR]: {0}\n", ex.Message.Replace("chunk", Settings.Lua.ScriptName));
							
							Wrapper.OutputLogMessage(outputMsg);
						}
						#endregion

						#region " MOUSE EVENTS "

						// Mouse Usage
						Keys[] MouseNames = { Keys.LButton, Keys.MButton, Keys.RButton, Keys.XButton1, Keys.XButton2 };
						bool[] MouseActive = new bool[MouseNames.Length];
						bool[] MousePressed = new bool[MouseNames.Length];

						// MOUSE_BUTTON_PRESSED
						try
						{
							for (int i = 0; i < MouseNames.Length; i++)
							{
								MouseActive[i] = VirtualInput.GetAsyncKeyState(MouseNames[i]);

								if (MouseActive[i] && !MousePressed[i])
								{
									try
									{
										if (Engine["OnEvent"] is LuaFunction OnEvent)
										{
											if ((MouseNames[i] == Keys.LButton && Settings.Lua.PrimaryMouseButtonEvents) || MouseNames[i] != Keys.LButton)
												OnEvent.Call("MOUSE_BUTTON_PRESSED", i + 1);
										}
										if (Engine["OnMouseDown"] is LuaFunction OnMouseDown)
										{
											if ((MouseNames[i] == Keys.LButton && Settings.Lua.PrimaryMouseButtonEvents) || MouseNames[i] != Keys.LButton)
												OnMouseDown.Call(i + 1);
										}
										if ((MouseNames[i] == Keys.LButton && Settings.Lua.PrimaryMouseButtonEvents) || (MouseNames[i] == Keys.RButton && Settings.Lua.SecondaryMouseButtonEvents) || MouseNames[i] != Keys.LButton || MouseNames[i] != Keys.RButton)
											MousePressed[i] = true;
									}
									catch (NLua.Exceptions.LuaScriptException ex)
									{
										string outputMsg = String.Format("[ERROR]: {0}\n", ex.Message.Replace("chunk", Settings.Lua.ScriptName));
										
										Wrapper.OutputLogMessage(outputMsg);
									}
								}
								if (!MouseActive[i] && MousePressed[i])
								{
									try
									{
										if (Engine["OnEvent"] is LuaFunction OnEvent)
										{
											OnEvent.Call("MOUSE_BUTTON_RELEASED", i + 1);
										}
										if (Engine["OnMouseUp"] is LuaFunction OnMouseUp)
										{
											OnMouseUp.Call(i + 1);
										}
										if ((MouseNames[i] == Keys.LButton && Settings.Lua.PrimaryMouseButtonEvents) || (MouseNames[i] == Keys.RButton && Settings.Lua.SecondaryMouseButtonEvents) || MouseNames[i] != Keys.LButton || MouseNames[i] != Keys.RButton)
											MousePressed[i] = false;
									}
									catch (NLua.Exceptions.LuaScriptException ex)
									{
										string outputMsg = String.Format("[ERROR]: {0}\n", ex.Message.Replace("chunk", Settings.Lua.ScriptName));
										Wrapper.OutputLogMessage(outputMsg);
									}
								}
							}
						}
						catch (Exception ex)
						{
							string outputMsg = String.Format("[ERROR]: {0}\n", ex.Message.Replace("chunk", Settings.Lua.ScriptName));
							
							Wrapper.OutputLogMessage(outputMsg);
						}

						// MOUSE_MOVE
						try
						{
							Point oldPos = VirtualInput.Mouse.GetPosition();
							Point newPos = VirtualInput.Mouse.GetPosition();
							if (newPos != oldPos)
							{
								if (Engine["OnEvent"] is LuaFunction OnEvent)
								{
									OnEvent.Call("MOUSE_MOVE", newPos.X, newPos.Y);
								}
								if (Engine["OnMouseMove"] is LuaFunction OnMouseMove)
								{
									OnMouseMove.Call(newPos.X, newPos.Y);
								}
							}

						}
						catch (NLua.Exceptions.LuaScriptException ex)
						{
							string outputMsg = String.Format("[ERROR]: {0}\n", ex.Message.Replace("chunk", Settings.Lua.ScriptName));
							
							Wrapper.OutputLogMessage(outputMsg);
						}
						#endregion

						#region " KEYBOARD EVENTS "
						// Toggle Key Usage
						Keys[] KeyLockNames = { Keys.CapsLock, Keys.NumLock, Keys.Scroll };
						bool[] KeyLockActive = new bool[KeyLockNames.Length];

						// Full Keyboard Usage
						Keys[] KeyboardNames = (Keys[])Enum.GetValues(typeof(Keys));
						bool[] KeyboardActive = new bool[KeyboardNames.Length];
						bool[] KeyboardPressed = new bool[KeyboardNames.Length];

						// KEY_PRESSED
						try
						{
							for (int i = 0; i < KeyboardNames.Length; i++)
							{
								Keys key = KeyboardNames[i];
								KeyboardActive[i] = VirtualInput.GetAsyncKeyState(key) && !MouseNames.Contains(key);
								if (KeyboardActive[i] && !KeyboardPressed[i])
								{
									try
									{
										string res = key.ToString().CleanInput();
										if (Engine["OnEvent"] is LuaFunction OnEvent)
										{
											OnEvent.Call("KEY_PRESSED", res);
										}
										if (Engine["OnKeyDown"] is LuaFunction OnKeyDown)
										{
											OnKeyDown.Call(res);
										}
										KeyboardPressed[i] = true;
									}
									catch (NLua.Exceptions.LuaScriptException ex)
									{
										string outputMsg = String.Format("[ERROR]: {0}\n", ex.Message.Replace("chunk", Settings.Lua.ScriptName));
										
										Wrapper.OutputLogMessage(outputMsg);
									}
								}
								if (!KeyboardActive[i] && KeyboardPressed[i])
								{
									try
									{
										string res = key.ToString().CleanInput();
										if (Engine["OnEvent"] is LuaFunction OnEvent)
										{
											OnEvent.Call("KEY_RELEASED", res);
										}
										if (Engine["OnKeyUp"] is LuaFunction OnKeyUp)
										{
											OnKeyUp.Call(res);
										}
										KeyboardPressed[i] = false;
									}
									catch (NLua.Exceptions.LuaScriptException ex)
									{
										string outputMsg = String.Format("[ERROR]: {0}\n", ex.Message.Replace("chunk", Settings.Lua.ScriptName));
										
										Wrapper.OutputLogMessage(outputMsg);
									}
								}
							}
						}
						catch (Exception ex)
						{
							string outputMsg = String.Format("[ERROR]: {0}\n", ex.Message.Replace("chunk", Settings.Lua.ScriptName));
							
							Wrapper.OutputLogMessage(outputMsg);
						}

						// KEYLOCK_TOGGLED
						try
						{
							for (int i = 0; i < KeyLockNames.Length; i++)
							{
								int trackedKey = -1;
								string trackedName = String.Empty;
								switch (i)
								{
									case 0:
										trackedKey = 0x14;
										trackedName = "CAPSLOCK";
										break;
									case 1:
										trackedKey = 0x90;
										trackedName = "NUMLOCK";
										break;
									case 2:
										trackedKey = 0x91;
										trackedName = "SCROLLLOCK";
										break;
								}
								bool trackedState = (((ushort)VirtualInput.GetKeyState(trackedKey)) & 0xffff) != 0;

								if (KeyLockActive[i] != trackedState)
								{
									try
									{
										if (Engine["OnEvent"] is LuaFunction OnEvent)
										{
											KeyLockActive[i] = trackedState;
											OnEvent.Call("KEYLOCK_TOGGLED", trackedName, trackedState);
										}
									}
									catch (NLua.Exceptions.LuaScriptException ex)
									{
										string outputMsg = String.Format("[ERROR]: {0}\n", ex.Message.Replace("chunk", Settings.Lua.ScriptName));
										
										Wrapper.OutputLogMessage(outputMsg);
									}
								}
							}
						}
						catch (Exception ex)
						{
							string outputMsg = String.Format("[ERROR]: {0}\n", ex.Message.Replace("chunk", Settings.Lua.ScriptName));
							
							Wrapper.OutputLogMessage(outputMsg);
						}
						#endregion

						#region " CONTROLLER EVENTS "
						// Controller Buttons
						ControllerInput.ControllerKeys[] ControllerKeys =
						{
					ControllerInput.ControllerKeys.A, ControllerInput.ControllerKeys.B, ControllerInput.ControllerKeys.X,
					ControllerInput.ControllerKeys.Y, ControllerInput.ControllerKeys.LeftShoulder, ControllerInput.ControllerKeys.RightShoulder,
					ControllerInput.ControllerKeys.DPadUp, ControllerInput.ControllerKeys.DPadDown,
					ControllerInput.ControllerKeys.DPadLeft, ControllerInput.ControllerKeys.DPadRight, ControllerInput.ControllerKeys.Start,
					ControllerInput.ControllerKeys.Back
				};
						bool[] ControllerActive = new bool[ControllerKeys.Length];
						bool[] ControllerPressed = new bool[ControllerKeys.Length];

						// Controller Triggers
						ControllerInput.ControllerTrigger[] ControllerTriggers =
						{
					ControllerInput.ControllerTrigger.Left, ControllerInput.ControllerTrigger.Right
				};
						bool[] ControllerTriggerActive = new bool[ControllerTriggers.Length];
						bool[] ControllerTriggerPressed = new bool[ControllerTriggers.Length];

						// Controller Stick (Left)
						ControllerInput.ControllerStick[] ControllerSticksLeft =
						{
					ControllerInput.ControllerStick.LeftX,
					ControllerInput.ControllerStick.LeftY,
				};
						int[] ControllerStickLeftDeltas = new int[ControllerSticksLeft.Length];
						bool[] ControllerStickLeftActive = new bool[ControllerSticksLeft.Length];

						// Controller Stick (Right)
						ControllerInput.ControllerStick[] ControllerSticksRight =
						{
					ControllerInput.ControllerStick.RightX,
					ControllerInput.ControllerStick.RightY,
				};
						int[] ControllerStickRightDeltas = new int[ControllerSticksRight.Length];
						bool[] ControllerStickRightActive = new bool[ControllerSticksRight.Length];

						// CONTROLLER_STICK_LEFT
						try
						{
							for (int i = 0; i < ControllerSticksLeft.Length; i++)
							{
								ControllerInput.ControllerStick stick = ControllerSticksLeft[i];
								bool IsXAxis = i % 2 == 0;
								ControllerStickLeftDeltas[i] = IsXAxis ? ControllerInput.GetHorizontalDelta(stick) : ControllerInput.GetVerticalDelta(stick);
								if (ControllerStickLeftDeltas[i] != 0 && !ControllerStickLeftActive[i])
								{
									try
									{
										string res = stick.ToString().CleanInput();
										if (Engine["OnEvent"] is LuaFunction OnEvent)
										{
											OnEvent.Call("CONTROLLER_STICK_LEFT_" + (IsXAxis ? "X" : "Y") + "_MOVED", ControllerStickLeftDeltas[i].ToString());
											OnEvent.Call("CONTROLLER_STICK_LEFT_MOVED", ControllerStickLeftDeltas[i].ToString());
										}
										if (Engine["OnMouseMove"] is LuaFunction OnMouseMove)
										{
											OnMouseMove.Call(res, ControllerStickLeftDeltas[i].ToString());
										}
										ControllerStickLeftActive[i] = true;
									}
									catch (NLua.Exceptions.LuaScriptException ex)
									{
										string outputMsg = String.Format("[ERROR]: {0}\n", ex.Message.Replace("chunk", Settings.Lua.ScriptName));
										
										Wrapper.OutputLogMessage(outputMsg);
									}
								}
								if (ControllerStickLeftDeltas[i] == 0 && ControllerStickLeftActive[i])
								{
									try
									{
										string res = stick.ToString().CleanInput();
										if (Engine["OnEvent"] is LuaFunction OnEvent)
										{
											OnEvent.Call("CONTROLLER_STICK_LEFT_" + (IsXAxis ? "X" : "Y") + "_MOVED", ControllerStickLeftDeltas[i].ToString());
											OnEvent.Call("CONTROLLER_STICK_LEFT_MOVED", ControllerStickLeftDeltas[i].ToString());
										}
										if (Engine["OnMouseMove"] is LuaFunction OnMouseMove)
										{
											OnMouseMove.Call(res, ControllerStickLeftDeltas[i].ToString());
										}
										ControllerStickLeftActive[i] = false;
									}
									catch (NLua.Exceptions.LuaScriptException ex)
									{
										string outputMsg = String.Format("[ERROR]: {0}\n", ex.Message.Replace("chunk", Settings.Lua.ScriptName));
										
										Wrapper.OutputLogMessage(outputMsg);
									}
								}
							}
						}
						catch (Exception ex)
						{
							string outputMsg = String.Format("[ERROR]: {0}\n", ex.Message.Replace("chunk", Settings.Lua.ScriptName));
							
							Wrapper.OutputLogMessage(outputMsg);
						}

						// CONTROLLER_STICK_RIGHT
						try
						{
							for (int i = 0; i < ControllerSticksRight.Length; i++)
							{
								ControllerInput.ControllerStick stick = ControllerSticksRight[i];
								bool IsXAxis = i % 2 == 0;
								ControllerStickRightDeltas[i] = IsXAxis ? ControllerInput.GetHorizontalDelta(stick) : ControllerInput.GetVerticalDelta(stick);
								if (ControllerStickRightDeltas[i] != 0 && !ControllerStickRightActive[i])
								{
									try
									{
										string res = stick.ToString().CleanInput();
										if (Engine["OnEvent"] is LuaFunction OnEvent)
										{
											OnEvent.Call("CONTROLLER_STICK_RIGHT_" + (IsXAxis ? "X" : "Y") + "_MOVED", ControllerStickRightDeltas[i].ToString());
											OnEvent.Call("CONTROLLER_STICK_RIGHT_MOVED", ControllerStickRightDeltas[i].ToString());
										}
										if (Engine["OnMouseMove"] is LuaFunction OnMouseMove)
										{
											OnMouseMove.Call(res, ControllerStickRightDeltas[i].ToString());
										}
										ControllerStickRightActive[i] = true;
									}
									catch (NLua.Exceptions.LuaScriptException ex)
									{
										string outputMsg = String.Format("[ERROR]: {0}\n", ex.Message.Replace("chunk", Settings.Lua.ScriptName));
										
										Wrapper.OutputLogMessage(outputMsg);
									}
								}
								if (ControllerStickRightDeltas[i] == 0 && ControllerStickRightActive[i])
								{
									try
									{
										string res = stick.ToString().CleanInput();
										if (Engine["OnEvent"] is LuaFunction OnEvent)
										{
											OnEvent.Call("CONTROLLER_STICK_RIGHT_" + (IsXAxis ? "X" : "Y") + "_MOVED", ControllerStickRightDeltas[i].ToString());
											OnEvent.Call("CONTROLLER_STICK_RIGHT_MOVED", ControllerStickRightDeltas[i].ToString());
										}
										if (Engine["OnMouseMove"] is LuaFunction OnMouseMove)
										{
											OnMouseMove.Call(res, ControllerStickRightDeltas[i].ToString());
										}
										ControllerStickRightActive[i] = false;
									}
									catch (NLua.Exceptions.LuaScriptException ex)
									{
										string outputMsg = String.Format("[ERROR]: {0}\n", ex.Message.Replace("chunk", Settings.Lua.ScriptName));
										
										Wrapper.OutputLogMessage(outputMsg);
									}
								}
							}
						}
						catch (Exception ex)
						{
							string outputMsg = String.Format("[ERROR]: {0}\n", ex.Message.Replace("chunk", Settings.Lua.ScriptName));
							
							Wrapper.OutputLogMessage(outputMsg);
						}

						// CONTROLLER_BUTTON_PRESSED
						try
						{
							for (int i = 0; i < ControllerKeys.Length; i++)
							{
								ControllerInput.ControllerKeys key = ControllerKeys[i];
								ControllerActive[i] = ControllerInput.IsKeyPressed(key);
								if (ControllerActive[i] && !ControllerPressed[i])
								{
									try
									{
										string res = key.ToString().CleanInput();
										if (Engine["OnEvent"] is LuaFunction OnEvent)
										{
											OnEvent.Call("CONTROLLER_BUTTON_PRESSED", res);
										}
										if (Engine["OnKeyDown"] is LuaFunction OnKeyDown)
										{
											OnKeyDown.Call(res);
										}
										ControllerPressed[i] = true;
									}
									catch (NLua.Exceptions.LuaScriptException ex)
									{
										string outputMsg = String.Format("[ERROR]: {0}\n", ex.Message.Replace("chunk", Settings.Lua.ScriptName));
										
										Wrapper.OutputLogMessage(outputMsg);
									}
								}
								if (!ControllerActive[i] && ControllerPressed[i])
								{
									try
									{
										string res = key.ToString().CleanInput();
										if (Engine["OnEvent"] is LuaFunction OnEvent)
										{
											OnEvent.Call("CONTROLLER_BUTTON_RELEASED", res);
										}
										if (Engine["OnKeyUp"] is LuaFunction OnKeyUp)
										{
											OnKeyUp.Call(res);
										}
										ControllerPressed[i] = false;
									}
									catch (NLua.Exceptions.LuaScriptException ex)
									{
										string outputMsg = String.Format("[ERROR]: {0}\n", ex.Message.Replace("chunk", Settings.Lua.ScriptName));
										
										Wrapper.OutputLogMessage(outputMsg);
									}
								}
							}
						}
						catch (Exception ex)
						{
							Wrapper.OutputLogMessage(String.Format("[ERROR]: {0}\n", ex.Message.Replace("chunk", Settings.Lua.ScriptName)));
						}

						// CONTROLLER_TRIGGER_PRESSED
						try
						{
							for (int i = 0; i < ControllerTriggers.Length; i++)
							{
								ControllerInput.ControllerTrigger key = ControllerTriggers[i];
								ControllerTriggerActive[i] = ControllerInput.IsTriggerHeld(key);
								if (ControllerTriggerActive[i] && !ControllerTriggerPressed[i])
								{
									try
									{
										string res = key.ToString().CleanInput();
										if (Engine["OnEvent"] is LuaFunction OnEvent)
										{
											OnEvent.Call("CONTROLLER_TRIGGER_PRESSED", res);
										}
										if (Engine["OnKeyDown"] is LuaFunction OnKeyDown)
										{
											OnKeyDown.Call(res);
										}
										ControllerTriggerPressed[i] = true;
									}
									catch (NLua.Exceptions.LuaScriptException ex)
									{
										string outputMsg = String.Format("[ERROR]: {0}\n", ex.Message.Replace("chunk", Settings.Lua.ScriptName));
										
										Wrapper.OutputLogMessage(outputMsg);
									}
								}
								if (!ControllerTriggerActive[i] && ControllerTriggerPressed[i])
								{
									try
									{
										string res = key.ToString().CleanInput();
										if (Engine["OnEvent"] is LuaFunction OnEvent)
										{
											OnEvent.Call("CONTROLLER_TRIGGER_RELEASED", res);
										}
										if (Engine["OnKeyUp"] is LuaFunction OnKeyUp)
										{
											OnKeyUp.Call(res);
										}
										ControllerTriggerPressed[i] = false;
									}
									catch (NLua.Exceptions.LuaScriptException ex)
									{
										string outputMsg = String.Format("[ERROR]: {0}\n", ex.Message.Replace("chunk", Settings.Lua.ScriptName));
										
										Wrapper.OutputLogMessage(outputMsg);
									}
								}
							}
						}
						catch (Exception ex)
						{
							string outputMsg = String.Format("[ERROR]: {0}\n", ex.Message.Replace("chunk", Settings.Lua.ScriptName));
							
							Wrapper.OutputLogMessage(outputMsg);
						}
						#endregion

						Thread.Sleep(1);
					}
				}
				catch
				{
					//Wrapper.OutputLogMessage(String.Format("[ERROR]: {0}\n", ex.Message.Replace("chunk", Settings.Lua.ScriptName)));
				}
			}
		}

	}
	internal class LogitechWrapper
	{
		public LogitechWrapper()
		{
			Logger.LogitechLine("Initializing Logitech compatibility layer..");
		}
		public static string Sandbox()
		{
			string sandbox = string.Format(string.Format("{2}\n{0}\n{1}",
				string.Join("\n", GetSupportedMethods()),
				string.Join("\n", GetUnsupportedMethods()),
				string.Join("\n", GetSandboxedMethods())), "Logitech");
			return sandbox;
		}
		public void Unsupported(string func)
		{
			OutputLogMessage(string.Format("ERROR: Function \"{0}\" is not supported in LEAGUEAIM.\n", func));
		}
		public bool IsKeyLockOn(string key)
		{
			switch (key.ToLower())
			{
				case "numlock":
					return (((ushort)VirtualInput.GetKeyState(0x90)) & 0xffff) != 0;
				case "capslock":
					return (((ushort)VirtualInput.GetKeyState(0x14)) & 0xffff) != 0;
				case "scrolllock":
					return (((ushort)VirtualInput.GetKeyState(0x91)) & 0xffff) != 0;
				default:
					return false;
			}
		}
		public void Sleep(int time)
		{
			Thread.Sleep(time);
		}
		public bool IsMouseButtonPressed(int button)
		{
			switch (button)
			{
				case 1:
					return VirtualInput.GetAsyncKeyState(Keys.LButton);
				case 2:
					return VirtualInput.GetAsyncKeyState(Keys.MButton);
				case 3:
					return VirtualInput.GetAsyncKeyState(Keys.RButton);
				case 4:
					return VirtualInput.GetAsyncKeyState(Keys.XButton1);
				case 5:
					return VirtualInput.GetAsyncKeyState(Keys.XButton2);
				default:
					return false;
			}
		}
		public bool IsModifierPressed(string key)
		{
			switch (key.ToLower())
			{
				case "alt": return VirtualInput.GetAsyncKeyState(Keys.LMenu) || VirtualInput.GetAsyncKeyState(System.Windows.Forms.Keys.RMenu);
				case "lalt": return VirtualInput.GetAsyncKeyState(System.Windows.Forms.Keys.LMenu);
				case "ralt": return VirtualInput.GetAsyncKeyState(System.Windows.Forms.Keys.RMenu);
				case "ctrl": return VirtualInput.GetAsyncKeyState(System.Windows.Forms.Keys.LControlKey) || VirtualInput.GetAsyncKeyState(System.Windows.Forms.Keys.RControlKey);
				case "lctrl": return VirtualInput.GetAsyncKeyState(System.Windows.Forms.Keys.LControlKey);
				case "rctrl": return VirtualInput.GetAsyncKeyState(System.Windows.Forms.Keys.RControlKey);
				case "shift": return VirtualInput.GetAsyncKeyState(System.Windows.Forms.Keys.LShiftKey) || VirtualInput.GetAsyncKeyState(System.Windows.Forms.Keys.RShiftKey);
				case "lshift": return VirtualInput.GetAsyncKeyState(System.Windows.Forms.Keys.LShiftKey);
				case "rshift": return VirtualInput.GetAsyncKeyState(System.Windows.Forms.Keys.RShiftKey);
				default: return false;
			}
		}
		public bool IsKeyPressed(string key)
		{
			string res = key.ToString();
			switch (key.ToString().ToLower())
			{
				case "0": res = "D0"; break;
				case "1": res = "D1"; break;
				case "2": res = "D2"; break;
				case "3": res = "D3"; break;
				case "4": res = "D4"; break;
				case "5": res = "D5"; break;
				case "6": res = "D6"; break;
				case "7": res = "D7"; break;
				case "8": res = "D8"; break;
				case "9": res = "D9"; break;
			}
			Keys code = (Keys)Enum.Parse(typeof(Keys), res, true);
			return VirtualInput.GetAsyncKeyState(code);
		}

		public bool IsControllerButtonPressed(string key)
		{
			ControllerInput.ControllerKeys strName = ControllerInput.GetKeyFromString(key);
			return ControllerInput.IsKeyPressed(strName);
		}
		public bool IsControllerTriggerHeld(string trig)
		{
			ControllerInput.ControllerTrigger strName = ControllerInput.GetTriggerFromString(trig);
			return ControllerInput.IsTriggerHeld(strName);
		}
		public int GetStickDelta(string stick, string axis)
		{
			stick = stick.ToLower();
			axis = axis.ToLower();

			if (stick == "left")
			{
				if (axis == "x")
				{
					return ControllerInput.GetHorizontalDelta(ControllerInput.ControllerStick.LeftX);
				}
				else if (axis == "y")
				{
					return ControllerInput.GetVerticalDelta(ControllerInput.ControllerStick.LeftY);
				}
			}
			else if (stick == "right")
			{
				if (axis == "x")
				{
					return ControllerInput.GetHorizontalDelta(ControllerInput.ControllerStick.RightX);
				}
				else if (axis == "y")
				{
					return ControllerInput.GetVerticalDelta(ControllerInput.ControllerStick.RightY);
				}
			}
			return 0;
		}

		public string ToReadable(string key)
		{
			var isNumber = int.TryParse(key, out int n);
			if (!isNumber) return key.ToString().ToReadableKey();
			return key.ToString();
		}
		public void MoveMouseRelative(int x, int y)
		{
			VirtualInput.Mouse.Move(x, y);
		}
		public void MoveMouseTo(int x, int y)
		{
			VirtualInput.Mouse.MoveTo(x, y);
		}
		public void ScrollMouseRelative(int val)
		{
			VirtualInput.Mouse.MoveWheel(val);
		}
		public void OutputLogMessage(object message, params string[] args)
		{
			try
			{
				string msg = message.ToString();
				Engine.Logs.Write(String.Format(msg.Replace("\n", Environment.NewLine), args));
				
				Logger.LogitechLine(msg.Trim());
			}
			catch (Exception ex)
			{
				OutputLogMessage("ERROR: " + ex.Message);
			}
		}
		public void ClearLog()
		{
			Engine.Logs.Clear();
		}
		public void SendString(string key)
		{
			key = key.Replace("\n", String.Empty);
			key = key.Replace("\t", String.Empty);
			for (int i = 0; i < key.Length; i++)
			{
				char c = key[i];

				if (c == ' ') { PressKey("space"); continue; }

				if (VirtualInput.CharIsPunctuation(c))
				{
					PressKey("shift");
					VirtualInput.DirectInputKey puncK = VirtualInput.StrToDik(VirtualInput.GetUnshiftedChar(c).ToLower());
					VirtualInput.Keyboard.Press(puncK);
					ReleaseKey("shift");
					continue;
				}

				VirtualInput.DirectInputKey dik = VirtualInput.StrToDik(c.ToString().ToLower());

				if (VirtualInput.CharIsCaptial(c))
				{
					PressKey("shift");
					VirtualInput.Keyboard.Press(dik);
					ReleaseKey("shift");
					continue;
				}
				else
					VirtualInput.Keyboard.Press(dik);
			}
		}
		public void PressKey(string key)
		{
			VirtualInput.DirectInputKey dik = VirtualInput.StrToDik(key.ToLower());
			VirtualInput.Keyboard.Down(dik);
		}
		public void ReleaseKey(string key)
		{
			VirtualInput.DirectInputKey dik = VirtualInput.StrToDik(key.ToLower());
			VirtualInput.Keyboard.Up(dik);
		}
		public void PressAndReleaseKey(string key)
		{
			VirtualInput.DirectInputKey dik = VirtualInput.StrToDik(key.ToLower());
			VirtualInput.Keyboard.Press(dik);
		}
		public void PressMouseButton(int button)
		{
			Keys key;
			switch (button)
			{
				case 1: key = Keys.LButton; break;
				case 2: key = Keys.MButton; break;
				case 3: key = Keys.RButton; break;
				case 4: key = Keys.XButton1; break;
				case 5: key = Keys.XButton2; break;
				default: key = Keys.None; break;
			}
			VirtualInput.Mouse.Down(key);
		}
		public void ReleaseMouseButton(int button)
		{
			Keys key;
			switch (button)
			{
				case 1: key = Keys.LButton; break;
				case 2: key = Keys.MButton; break;
				case 3: key = Keys.RButton; break;
				case 4: key = Keys.XButton1; break;
				case 5: key = Keys.XButton2; break;
				default: key = Keys.None; break;
			}
			VirtualInput.Mouse.Up(key);
		}
		public void PressAndReleaseMouseButton(int button)
		{
			Keys key;
			switch (button)
			{
				case 1: key = Keys.LButton; break;
				case 2: key = Keys.MButton; break;
				case 3: key = Keys.RButton; break;
				case 4: key = Keys.XButton1; break;
				case 5: key = Keys.XButton2; break;
				default: key = Keys.None; break;
			}
			VirtualInput.Mouse.Click(key);
		}
		public LuaTable GetMousePosition()
		{
			Point mousePos = VirtualInput.Mouse.GetPosition();
			return (LuaTable)LuaEngine.Engine.DoString(String.Format("return {{ {0}, {1} }}", mousePos.X, mousePos.Y))[0];
		}
		public void EnablePrimaryMouseButtonEvents(bool enabled)
		{
			Settings.Lua.PrimaryMouseButtonEvents = enabled;
		}
		public void EnableSecondaryMouseButtonEvents(bool enabled)
		{
			Settings.Lua.SecondaryMouseButtonEvents = enabled;
		}

		public void EnableCoreFunctionality(bool enabled)
		{
			Settings.Lua.CoreFunctionality = enabled;
		}
		public int GetRunningTime()
		{
			Double elapsedMillisecs = ((TimeSpan)(DateTime.Now - Program.StartTime)).TotalMilliseconds;
			return (int)elapsedMillisecs;
		}
		public void Popup(string message, string title = null)
		{
			if (title != null)
				MessageBox.Show(message, string.Format("LEAGUEAIM - {0}", title), MessageBoxButtons.OK);
			else
				MessageBox.Show(message, "LEAGUEAIM", MessageBoxButtons.OK);
		}
		public void Beep()
		{
			Console.Beep(380, 200);
		}
		public void Exit()
		{

		}

		public void ExitApplication()
		{
			Environment.Exit(0);
		}


		private string UserFilePath()
		{
			string f = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LEAGUEAIM", "scripts", "files");
			if (!Directory.Exists(f)) { Directory.CreateDirectory(f); }
			return f;
		}
		public string ReadFile(string file)
		{
			file = file.Replace("//", "_");
			file = Path.Combine(UserFilePath(), file);
			if (!File.Exists(file)) { File.Create(file).Close(); }
			return File.ReadAllText(file);
		}
		public void WriteFile(string file, string content)
		{
			file = file.Replace("//", "_");
			file = Path.Combine(UserFilePath(), file);
			if (!File.Exists(file)) { File.Create(file).Close(); }
			File.WriteAllText(file, content);
		}
		public string GetCurrentProfile()
		{
			return Profiles.CurrentProfile;
		}


		public LuaTable GetPixelColor(int x, int y)
		{
			Color color = VirtualInput.Screen.GetPixelColor(new Point(x, y));
			return (LuaTable)LuaEngine.Engine.DoString(String.Format("return {{ {0}, {1}, {2} }}", color.R, color.G, color.B))[0];
		}
		public int GetScreenHeight()
		{
			return Screen.PrimaryScreen.Bounds.Height;
		}
		public int GetScreenWidth()
		{
			return Screen.PrimaryScreen.Bounds.Width;
		}

		public string toupper(string str)
		{
			return str.ToUpper();
		}
		public string tolower(string str)
		{
			return str.ToLower();
		}
		private static string[] GetSupportedMethods()
		{
			List<string> methodList = new List<string>();

			Type lt = typeof(LogitechWrapper);
			foreach (var method in lt.GetMethods())
			{
				var args = string.Join
					(", ", method.GetParameters()
								 .Select(x => x.Name)
								 .ToArray());
				if (method.IsPublic && !method.Name.Contains("Logitech:") && method.Name != "GetType" && method.Name != "GetHashCode" && method.Name != "Equals" && method.Name != "ToString" && method.Name != "Unsupported" && method.Name != "Sandbox")
					methodList.Add(String.Format("{0} = function({1}) return {{0}}:{0}({1}) end", method.Name, args));
			}

			return methodList.ToArray();
		}
		internal static string[] GetUnsupportedMethods()
		{
			List<string> methodList = [];
			string[] methodNames = new string[]
			{
				"PlayMacro",
				"PressMacro",
				"ReleaseMacro",
				"AbortMacro",
				"SetBacklightColor",
				"SetMouseDPITable",
				"SetMouseDPITableIndex",
				"GetMKeyState",
				"SetMKeyState",
				"PressHidKey",
				"ReleaseHidKey",
				"PressAndReleaseHidKey",
			};
			foreach (var method in methodNames)
			{
				methodList.Add(String.Format("{0} = function(key) return {{0}}:Unsupported('{0}') end", method));
			}
			return methodList.ToArray();
		}
		private static string[] GetSandboxedMethods()
		{
			List<string> methodList = new List<string>();
			string[] methodNames = new string[]
			{
				"load",
				"loadfile",
				"dofile",
				"debug",
				"string.dump",
				"setfenv",
				"getfenv",
				"os",
				"io"
			};
			foreach (var method in methodNames)
			{
				methodList.Add(String.Format("{0} = function() end", method));
			}
			return methodList.ToArray();
		}
	}
	internal class LuaLog
	{
		public LuaLog() {
			Output = string.Empty;
		}
		public void Dispose()
		{
			Output = string.Empty;
			this.Dispose();
		}
		public void Write(string message)
		{
			Output += message;
		}
		public void WriteLine(string message)
		{
			Output += message + "\n";
		}
		public void Clear()
		{
			Output = string.Empty;
		}
		public string Output { get; set; }
	}
}