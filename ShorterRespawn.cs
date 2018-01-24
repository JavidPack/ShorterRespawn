using Terraria.ModLoader;
using Terraria;
using System;
using Terraria.DataStructures;
using System.ComponentModel;
using Newtonsoft.Json;

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
				// If Heros isn't available, try CheatSheet
				else if (cheatSheet != null)
				{
					//ErrorLogger.Log("Integrating with Cheat Sheet");
					SetupCheatSheetIntegration(cheatSheet);
				}
				else
				{
					// No cheat integration
				}
			}
			catch (Exception e)
			{
				ErrorLogger.Log("ShorterRespawn PostSetupContent Error: " + e.StackTrace + e.Message);
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

		// The New way in 0.8.3.1
		private void SetupCheatSheetIntegration(Mod cheatSheet)
		{
			// Don't GetTexture in Server code.
			if (!Main.dedServ)
			{
				CheatSheet.CheatSheetInterface.RegisterButton(cheatSheet, GetTexture("InstantRespawnButton"), InstantRespawnButtonPressed, InstantRespawnTooltip);
			}
		}

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
		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
		{
			// If we are cheating
			if ((mod as ShorterRespawn).instantRespawn)
			{
				player.respawnTimer = 0;
				return;
			}
			// otherwise, if we just want the time reduced to a more typical level
			//if (Main.expertMode)
			//{
			//	player.respawnTimer = (int)(player.respawnTimer * .75);
			//}

			ShorterRespawnConfig config = mod.GetConfig<ShorterRespawnConfig>();

			// Reimplement vanilla respawnTimer logic
			player.respawnTimer = 600;
			bool bossAlive = false;
			if (Main.netMode != 0 && !pvp)
			{
				for (int k = 0; k < 200; k++)
				{
					if (Main.npc[k].active && (Main.npc[k].boss || Main.npc[k].type == 13 || Main.npc[k].type == 14 || Main.npc[k].type == 15) && Math.Abs(player.Center.X - Main.npc[k].Center.X) + Math.Abs(player.Center.Y - Main.npc[k].Center.Y) < 4000f)
					{
						bossAlive = true;
						break;
					}
				}
			}
			if (bossAlive)
			{
				player.respawnTimer = (int)(player.respawnTimer * config.BossPenaltyScale);
			}
			if (Main.expertMode)
			{
				player.respawnTimer = (int)(player.respawnTimer * config.ExpertPenaltyScale);
			}
			player.respawnTimer = (int)(player.respawnTimer * config.GlobalRespawnScale);
		}
	}

	public class ShorterRespawnConfig : ModConfig
	{
		public override MultiplayerSyncMode Mode
		{
			get
			{
				return MultiplayerSyncMode.ServerDictates;
			}
		}

		[DefaultValue(1f)]
		[Range(0f, 3f)]
		[Label("Global Respawn Scale")]
		public float GlobalRespawnScale;

		[Range(1f, 3f)]
		[DefaultValue(1.5f)]
		public float ExpertPenaltyScale;

		[Range(1f, 3f)]
		[DefaultValue(2f)]
		public float BossPenaltyScale;

		// Vanilla RespawnTime
		public const int RegularRespawnTimer = 600;

		[JsonIgnore]
		[Range(0, 2000)]
		[Label("Normal Respawn Time in Ticks")]
		public int NormalRespawn { get { return (int)(GlobalRespawnScale * RegularRespawnTimer); } }

		[Range(0, 2000)]
		[JsonIgnore]
		[Label("Normal Boss Respawn Time in Ticks")]
		public int NormalBossRespawn { get { return (int)(GlobalRespawnScale * RegularRespawnTimer * BossPenaltyScale); } }

		[JsonIgnore]
		[Range(0, 2000)]
		[Label("Expert Respawn Time in Ticks")]
		internal int ExpertRespawn { get { return (int)(GlobalRespawnScale * RegularRespawnTimer * ExpertPenaltyScale); } }

		[Range(0, 2000)]
		[JsonIgnore]
		[Label("Expert Boss Respawn Time in Ticks")]
		public int ExpertBossRespawn { get { return (int)(GlobalRespawnScale * RegularRespawnTimer * BossPenaltyScale * ExpertPenaltyScale); } }
	}
}
