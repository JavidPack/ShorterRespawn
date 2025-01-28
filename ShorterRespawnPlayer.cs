using Terraria.ModLoader;
using Terraria;
using System;
using Terraria.DataStructures;
using Terraria.ID;

namespace ShorterRespawn
{
	// This class is the actual mod code that reduces the respawn timer when the player dies.
	public class ShorterRespawnPlayer : ModPlayer
	{
		private ShorterRespawnConfig config = ModContent.GetInstance<ShorterRespawnConfig>();

		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
			// If we are cheating
			if (ShorterRespawn.Instance.instantRespawn) {
				Player.respawnTimer = 0;
				return;
			}
			// otherwise, if we just want the time reduced to a more typical level
			//if (Main.expertMode)
			//{
			//	player.respawnTimer = (int)(player.respawnTimer * .75);
			//}

			// Reimplement vanilla respawnTimer logic
			Player.respawnTimer = ShorterRespawnConfig.RegularRespawnTimer;

			//check if any boss is alive
			bool bossAlive = AnyBossAlive(pvp);

			if (!config.UseSeconds) {
				if (bossAlive) {
					Player.respawnTimer = (int)(Player.respawnTimer * config.BossPenaltyScale);
				}
				if (Main.expertMode) {
					Player.respawnTimer = (int)(Player.respawnTimer * config.ExpertPenaltyScale);
				}
				Player.respawnTimer = (int)(Player.respawnTimer * config.GlobalRespawnScale);
			}
			else {
				if (bossAlive) {
					Player.respawnTimer = (Main.expertMode ? config.ExpertBossRespawnTime : config.NormalBossRespawnTime) * 60;
				}
				else {
					Player.respawnTimer = (Main.expertMode ? config.ExpertRespawnTime : config.NormalRespawnTime) * 60;
				}
			}
		}

		// used to shorten the respawn timer when the boss despawn
		public override void UpdateDead() {
			if (!config.AdjustRespawnTimeWhenBossDespawns)
				return;

			// check if any boss is alive
			bool bossAlive = AnyBossAlive(careAboutRange: false);
			if (!bossAlive) {
				// check if the number system is being used
				if (config.UseSeconds) {
					// set the respawn timer to the non boss one if it's still above it based on expert
					if (Main.expertMode && Player.respawnTimer > config.ExpertRespawnTime * 60)
						Player.respawnTimer = config.ExpertRespawnTime * 60;
					else if (!Main.expertMode && Player.respawnTimer > config.NormalRespawnTime * 60)
						Player.respawnTimer = config.NormalRespawnTime * 60;
				}
				else {
					// set the respawn timer to the non boss one if it's still above it
					if (Main.expertMode && Player.respawnTimer > config.ExpertRespawn * 60)
						Player.respawnTimer = (int)config.ExpertRespawn * 60;
					else if (!Main.expertMode && Player.respawnTimer > config.NormalRespawn * 60)
						Player.respawnTimer = (int)config.NormalRespawn * 60;
				}
			}
		}

		private bool AnyBossAlive(bool pvp = false, bool careAboutRange = true) {
			if (pvp) {
				return false;
			}
			for (int k = 0; k < Main.npc.Length - 1; k++) {
				// check if there is a boss alive
				if (Main.npc[k].active && (Main.npc[k].boss || Main.npc[k].type == NPCID.EaterofWorldsHead || Main.npc[k].type == NPCID.EaterofWorldsBody || Main.npc[k].type == NPCID.EaterofWorldsTail)) {
					// check if the range matters
					if (careAboutRange) {
						// check if the player is within the range of the boss
						if (Math.Abs(Player.Center.X - Main.npc[k].Center.X) + Math.Abs(Player.Center.Y - Main.npc[k].Center.Y) < 4000f)
							return true;
						else
							return false;
					}
					else {
						return true;
					}
				}
			}

			return false;
		}
	}
}
