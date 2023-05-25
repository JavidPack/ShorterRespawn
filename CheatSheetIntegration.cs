using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ShorterRespawn
{
	[JITWhenModsEnabled("CheatSheet")]
	internal static class CheatSheetIntegration
	{
		internal static void SetupCheatSheetIntegration() {
			// Don't GetTexture in Server code.
			if (!Main.dedServ) {
				var shorterRespawn = ShorterRespawn.Instance;
				CheatSheet.CheatSheetInterface.RegisterButton(shorterRespawn.Assets.Request<Texture2D>("InstantRespawnButton"), shorterRespawn.InstantRespawnButtonPressed, shorterRespawn.InstantRespawnTooltip);
			}
		}
	}
}
