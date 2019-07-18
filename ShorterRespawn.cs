using Terraria.ModLoader;
using Terraria;
using System;
using Terraria.DataStructures;
using System.ComponentModel;
using Newtonsoft.Json;
using Terraria.ModLoader.Config;
using System.Runtime.Serialization;

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
		internal const string ModifyPersonalRespawnTime_Display = "Modify Personal Respawn Time";
		internal const string ModifyGlobalRespawnTime_Permission = "ModifyGlobalRespawnTime";
		internal const string ModifyGlobalRespawnTime_Display = "Modify Global Respawn Time";

		public override void Load()
		{
			Instance = this;
			cheatSheet = ModLoader.GetMod("CheatSheet");
			herosMod = ModLoader.GetMod("HEROsMod");
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
			if (ShorterRespawn.Instance.instantRespawn)
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
			player.respawnTimer = ShorterRespawnConfig.RegularRespawnTimer;
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
		public override ConfigScope Mode => ConfigScope.ServerSide;

		// Vanilla RespawnTime
		public const int RegularRespawnTimer = 600;

		[Header("Presets")]
		[JsonIgnore]
		[Label("[i:889] Shorter Respawn Preset")]
		[Tooltip("This preset reduces only Expert Penalty and is the default.")]
		public bool ShorterRespawnPreset {
			get => GlobalRespawnScale == 1f && ExpertPenaltyScale == 1.125f && BossPenaltyScale == 2f;
			set {
				if (value) {
					GlobalRespawnScale = 1f;
					ExpertPenaltyScale = 1.125f;
					BossPenaltyScale = 2f;
				}
			}
		}

		[JsonIgnore]
		[Label("[i:3099] Terraria Defaults Preset")]
		[Tooltip("This preset restores the default Terraria behavior.")]
		public bool TerrariaDefaultsPreset {
			get => GlobalRespawnScale == 1f && ExpertPenaltyScale == 1.5f && BossPenaltyScale == 2f;
			set {
				if (value) {
					GlobalRespawnScale = 1f;
					ExpertPenaltyScale = 1.5f;
					BossPenaltyScale = 2f;
				}
			}
		}

		[Header("Configurable Respawn Scales")]
		[DefaultValue(1f)]
		[Range(0f, 3f)]
		[Label("[i:1175] Global Respawn Scale")]
		public float GlobalRespawnScale { get; set; }

		[DefaultValue(1.125f)]
		[Range(1f, 3f)]
		[Label("[i:3233] Expert Mode Penalty Scale")]
		[Tooltip("Extra respawn time penalty while in Expert Mode. Default Terraria value is 1.5.")]
		public float ExpertPenaltyScale { get; set; }

		[DefaultValue(2f)]
		[Range(1f, 3f)]
		[Label("[i:43] Boss Penalty Scale")]
		[Tooltip("Extra respawn time penalty for deaths during boss fights. Default Terraria value is 2.")]
		public float BossPenaltyScale { get; set; }

		// The 3 fields above are used for the 4 properties below to convey the effect of the players choices.

		[Header("Resulting Respawn Times")]
		[JsonIgnore]
		[Range(0f, 60f)]
		[Label("Normal Respawn Time in Seconds")]
		[Tooltip("Default Terraria time is 10 seconds")]
		public float NormalRespawn => GlobalRespawnScale * RegularRespawnTimer / 60;

		[JsonIgnore]
		[Range(0f, 60f)]
		[Label("Normal Boss Respawn Time in Seconds")]
		[Tooltip("Default Terraria time is 20 seconds")]
		public float NormalBossRespawn => GlobalRespawnScale * RegularRespawnTimer * BossPenaltyScale / 60;

		[JsonIgnore]
		[Range(0f, 60f)]
		[Label("Expert Respawn Time in Seconds")]
		[Tooltip("Default Terraria time is 15 seconds")]
		public float ExpertRespawn => GlobalRespawnScale * RegularRespawnTimer * ExpertPenaltyScale / 60;

		[JsonIgnore]
		[Range(0f, 60f)]
		[Label("Expert Boss Respawn Time in Seconds")]
		[Tooltip("Default Terraria time is 30 seconds")]
		public float ExpertBossRespawn => GlobalRespawnScale * RegularRespawnTimer * BossPenaltyScale * ExpertPenaltyScale / 60;

		public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message)
		{
			if(ShorterRespawn.Instance.herosMod != null && ShorterRespawn.Instance.herosMod.Version >= new Version(0, 2, 2))
			{
				if (ShorterRespawn.Instance.herosMod.Call("HasPermission", whoAmI, ShorterRespawn.ModifyGlobalRespawnTime_Permission) is bool result && result)
					return true;
				message = $"You lack the \"{ShorterRespawn.ModifyGlobalRespawnTime_Display}\" permission.";
				return false;
			}
			return base.AcceptClientChanges(pendingConfig, whoAmI, ref message);
		}
	}
}
