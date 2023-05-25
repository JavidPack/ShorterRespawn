using Terraria.ModLoader;
using Terraria;
using System;
using Terraria.DataStructures;

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
			if (Main.netMode != 0 && !pvp)
			{
				for (int k = 0; k < 200; k++)
				{
					if (Main.npc[k].active && (Main.npc[k].boss || Main.npc[k].type == 13 || Main.npc[k].type == 14 || Main.npc[k].type == 15) && Math.Abs(Player.Center.X - Main.npc[k].Center.X) + Math.Abs(Player.Center.Y - Main.npc[k].Center.Y) < 4000f)
					{
						bossAlive = true;
						break;
					}
				}
			}
			if (bossAlive)
			{
				Player.respawnTimer = (int)(Player.respawnTimer * config.BossPenaltyScale);
			}
			if (Main.expertMode)
			{
				Player.respawnTimer = (int)(Player.respawnTimer * config.ExpertPenaltyScale);
			}
			Player.respawnTimer = (int)(Player.respawnTimer * config.GlobalRespawnScale);
		}
	}
}
