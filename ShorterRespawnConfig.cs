using System;
using System.ComponentModel;
using Newtonsoft.Json;
using Terraria.ModLoader.Config;

namespace ShorterRespawn
{
	public class ShorterRespawnConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		// Vanilla RespawnTime
		public const int RegularRespawnTimer = 600;

		[Header("Presets")]
		[JsonIgnore]
		[ShowDespiteJsonIgnore]
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
		[ShowDespiteJsonIgnore]
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

		[Header("ConfigurableRespawnScales")]
		[DefaultValue(1f)]
		[Range(0f, 3f)]
		public float GlobalRespawnScale { get; set; }

		[DefaultValue(1.125f)]
		[Range(1f, 3f)]
		public float ExpertPenaltyScale { get; set; }

		[DefaultValue(2f)]
		[Range(1f, 3f)]
		public float BossPenaltyScale { get; set; }

		// The 3 fields above are used for the 4 properties below to convey the effect of the players choices.

		const string DefaultRespawnTimeTooltip = "$Mods.ShorterRespawn.Configs.ShorterRespawnConfig.DefaultRespawnTimeTooltip";

		[Header("ResultingRespawnTimes")]
		[JsonIgnore]
		[ShowDespiteJsonIgnore]
		[Range(0f, 60f)]
		[TooltipKey(DefaultRespawnTimeTooltip), TooltipArgs(10)]
		public float NormalRespawn => GlobalRespawnScale * RegularRespawnTimer / 60;

		[JsonIgnore]
		[ShowDespiteJsonIgnore]
		[Range(0f, 60f)]
		[TooltipKey(DefaultRespawnTimeTooltip), TooltipArgs(20)]
		public float NormalBossRespawn => GlobalRespawnScale * RegularRespawnTimer * BossPenaltyScale / 60;

		[JsonIgnore]
		[ShowDespiteJsonIgnore]
		[Range(0f, 60f)]
		[TooltipKey(DefaultRespawnTimeTooltip), TooltipArgs(15)]
		public float ExpertRespawn => GlobalRespawnScale * RegularRespawnTimer * ExpertPenaltyScale / 60;

		[JsonIgnore]
		[ShowDespiteJsonIgnore]
		[Range(0f, 60f)]
		[TooltipKey(DefaultRespawnTimeTooltip), TooltipArgs(30)]
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
