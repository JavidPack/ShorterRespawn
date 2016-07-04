using Terraria.ModLoader;
using Terraria;
using System;

namespace ShorterRespawn
{
	public class ShorterRespawn : Mod
	{
		// A true/false value we'll use for the "cheat" functionality of this mod.
		internal bool instantRespawn = false;

		public ShorterRespawn()
		{
			Properties = new ModProperties()
			{
				Autoload = true, // We need Autoload to be true so our ModPlayer class below will be loaded.
			};
		}

		// We integrate with other mods in PostSetupContent.
		public override void PostSetupContent()
		{
			try
			{
				Mod cheatSheet = ModLoader.GetMod("CheatSheet");
				Mod herosMod = ModLoader.GetMod("HEROsMod");
				// Prefer Heros Mod
				if (herosMod != null)
				{
					//ErrorLogger.Log("Integrating with HEROs Mod");
					SetupHEROsModIntegration(herosMod);
				}
				// If Heros isn't available, try ChearSheet
				else if (cheatSheet != null)
				{
					//ErrorLogger.Log("Integrating with Cheat Sheet");
					SetupCheatSheetIntegration(cheatSheet);
				}
				else
				{
					//ErrorLogger.Log("No Integration");
				}
			}
			catch (Exception e)
			{
				ErrorLogger.Log("ShorterRespawn PostSetupContent Error: " + e.StackTrace + e.Message);
			}
			instantRespawn = false;
		}

		// This is the old, not-so-convenient way of doing things, using the Mod.Call method.
		private void SetupCheatSheetIntegration(Mod cheatSheet)
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
		}

		// The New way in 0.8.2.2
		/*
		private void SetupCheatSheetIntegration(Mod cheatSheet)
		{
			((CheatSheet.CheatSheet)cheatSheet).RegisterButton(GetTexture("InstantRespawnButton"), InstantRespawnButtonPressed, InstantRespawnTooltip);
		}
		*/

		private void SetupHEROsModIntegration(Mod herosMod)
		{
			// Add Permissions always. 
			herosMod.Call(
				// Special string
				"AddPermission",
				// Permission Name
				"ModifyPersonalRespawnTime",
				// Permission Display Name
				"Modify Personal Respawn Time"
			);
			// Add Buttons only to non-servers (otherwise the server will crash, since textures aren't loaded on servers)
			if (!Main.dedServ)
			{
				herosMod.Call(
					// Special string
					"AddSimpleButton",
					// Name of Permission governing the availability of the button/tool
					"ModifyPersonalRespawnTime",
					// Texture of the button. 38x38 is recommended for HERO's Mod. Also, a white outline on the icon similar to the other icons will look good.
					GetTexture("InstantRespawnButton"),
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
			return instantRespawn ? "Disable Instant Respawn" : "Enable Instant Respawn";
		}

		// This method is called when the button is pressed using Heros mod or Cheat Sheet
		public void InstantRespawnButtonPressed()
		{
			instantRespawn = !instantRespawn;
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

	// This class is the actual mod code that reduces the respawn timer when the player dies.
	public class ShorterRespawnPlayer : ModPlayer
	{
		public override void Kill(double damage, int hitDirection, bool pvp, string deathText)
		{
			// If we are cheating
			if ((mod as ShorterRespawn).instantRespawn)
			{
				player.respawnTimer = 0;
				return;
			}
			// otherwise, if we just want the time reduced to a more typical level
			if (Main.expertMode)
			{
				player.respawnTimer = (int)(player.respawnTimer * .75);
			}
		}
	}
}
