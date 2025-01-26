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
		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
		{
			// If we are cheating
			if (ShorterRespawn.Instance.instantRespawn)
			{
				Player.respawnTimer = 0;
				return;
			}
			// otherwise, if we just want the time reduced to a more typical level
			//if (Main.expertMode)
			//{
			//	player.respawnTimer = (int)(player.respawnTimer * .75);
			//}

			ShorterRespawnConfig config = ModContent.GetInstance<ShorterRespawnConfig>();

			// Reimplement vanilla respawnTimer logic
			Player.respawnTimer = ShorterRespawnConfig.RegularRespawnTimer;
			bool bossAlive = false;
            if (Main.netMode != NetmodeID.SinglePlayer && !pvp)
			{
				for (int k = 0; k < Main.npc.Length - 1; k++)
				{
					//check if there is a boss alive, and the player is close enough to it
					if (Main.npc[k].active && (Main.npc[k].boss || Main.npc[k].type == NPCID.EaterofWorldsHead || Main.npc[k].type == NPCID.EaterofWorldsBody || Main.npc[k].type == NPCID.EaterofWorldsTail) && Math.Abs(Player.Center.X - Main.npc[k].Center.X) + Math.Abs(Player.Center.Y - Main.npc[k].Center.Y) < 4000f)
					{
						bossAlive = true;
						break;
					}
				}
			}

			if (!config.UseNumbers)
			{
				if (bossAlive)
				{
					Player.respawnTimer = (int)(Player.respawnTimer * config.BossPenaltyScale);
				}
				if (Main.expertMode)
				{
					Player.respawnTimer = (int)(Player.respawnTimer * config.ExpertPenaltyScale);
				}
				Player.respawnTimer = (int)(Player.respawnTimer * config.GlobalRespawnScale);
			} else
			{
				if (!bossAlive)
				{
					Main.NewText("Boss is Not Alive");
					Player.respawnTimer = config.NormalRespawnTime * 60;
					if (Main.expertMode) { Player.respawnTimer = config.ExpertRespawnTime * 60; }
				} else
				{
					Main.NewText("Boss is Still Alive");
                    Player.respawnTimer = config.NormalBossRespawnTime * 60;
                    if (Main.expertMode) { Player.respawnTimer = config.ExpertBossRespawnTime * 60; }
                }
			}
		}
	}
}
