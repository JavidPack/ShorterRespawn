using Terraria.ModLoader;
using Terraria;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Localization;

namespace ShorterRespawn
{
	public class ShorterRespawn : Mod
	{
		// A true/false value we'll use for the "cheat" functionality of this mod.
		internal bool instantRespawn = false;

		internal static ShorterRespawn Instance;
		internal Mod cheatSheet;
		internal Mod herosMod;
		internal const string ModifyPersonalRespawnTime_Permission = "ModifyPersonalRespawnTime";
		internal const string ModifyPersonalRespawnTime_Display = "Modify Personal Respawn Time"; // TODO: Heros needs to support localization keys.
		internal const string ModifyGlobalRespawnTime_Permission = "ModifyGlobalRespawnTime";
		internal const string ModifyGlobalRespawnTime_Display = "Modify Global Respawn Time";

		public override void Load()
		{
			Instance = this;
			ModLoader.TryGetMod("CheatSheet", out cheatSheet);
			ModLoader.TryGetMod("HEROsMod", out herosMod);
		}

		public override void Unload() {
			Instance = null;
		}

		// We integrate with other mods in PostSetupContent.
		public override void PostSetupContent()
		{
			try
			{
				// Prefer Heros Mod
				if (herosMod != null)
				{
					//ErrorLogger.Log("Integrating with HEROs Mod");
					SetupHEROsModIntegration();
				}
				// If Heros isn't available, try CheatSheet
				else if (cheatSheet != null)
				{
					//ErrorLogger.Log("Integrating with Cheat Sheet");
					CheatSheetIntegration.SetupCheatSheetIntegration();
				}
				else
				{
					// No cheat integration
				}
			}
			catch (Exception e)
			{
				Logger.Warn("ShorterRespawn PostSetupContent Error: " + e.StackTrace + e.Message);
			}
			instantRespawn = false;
		}

		// This is the old, not-so-convenient way of doing things, using the Mod.Call method.
		/*private void SetupCheatSheetIntegration(Mod cheatSheet)
		{
			if (!Main.dedServ)
			{
				// "AddButton_Test" is a special string, in the future, other strings could represent different types of buttons or behaviors
				cheatSheet.Call(
					"AddButton_Test",
					GetTexture("InstantRespawnButton"),
					(Action)InstantRespawnButtonPressed,
					(Func<string>)InstantRespawnTooltip
				);
			}
			instantRespawn = false;
		}*/

		private void SetupHEROsModIntegration()
		{
			// Add Permissions always. 
			herosMod.Call(
				// Special string
				"AddPermission",
				// Permission Name
				ModifyPersonalRespawnTime_Permission,
				// Permission Display Name
				ModifyPersonalRespawnTime_Display
			);
			// This 2nd permission is for changing ModConfig values. non-cheat.
			herosMod.Call(
				"AddPermission",
				ModifyGlobalRespawnTime_Permission,
				ModifyGlobalRespawnTime_Display
			);
			// Add Buttons only to non-servers (otherwise the server will crash, since textures aren't loaded on servers)
			if (!Main.dedServ)
			{
				herosMod.Call(
					// Special string
					"AddSimpleButton",
					// Name of Permission governing the availability of the button/tool
					ModifyPersonalRespawnTime_Permission,
					// Texture of the button. 38x38 is recommended for HERO's Mod. Also, a white outline on the icon similar to the other icons will look good.
					Assets.Request<Texture2D>("InstantRespawnButton", ReLogic.Content.AssetRequestMode.ImmediateLoad),
					// A method that will be called when the button is clicked
					(Action)InstantRespawnButtonPressed,
					// A method that will be called when the player's permissions have changed
					(Action<bool>)PermissionChanged,
					// A method that will be called when the button is hovered, returning the Tooltip
					(Func<string>)InstantRespawnTooltip
				);
			}
		}

		// This method is called when the cursor is hovering over the button in Heros mod or Cheat Sheet
		public string InstantRespawnTooltip()
		{
			return Language.GetTextValue(GetLocalizationKey(instantRespawn ? "DisableInstantRespawn" : "EnableInstantRespawn"));
		}

		// This method is called when the button is pressed using Heros mod or Cheat Sheet
		public void InstantRespawnButtonPressed()
		{
			instantRespawn = !instantRespawn;
			// TODO: message
		}

		// This method is called when Permissions change while using HERO's Mod.
		// We need to make sure to disable instantRespawn when we are no longer allowed to use the tool.
		public void PermissionChanged(bool hasPermission)
		{
			if (!hasPermission)
			{
				instantRespawn = false;
			}
		}
	}
}
