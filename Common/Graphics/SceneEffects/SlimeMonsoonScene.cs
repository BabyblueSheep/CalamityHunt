﻿using CalamityHunt.Common.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace CalamityHunt.Common.Graphics.SceneEffects
{
    public class SlimeMonsoonScene : ModSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

        public override string MapBackground => $"{nameof(CalamityHunt)}/Assets/Textures/SlimeMonsoonBG";

        public override bool IsSceneEffectActive(Player player) => GoozmaSystem.GoozmaActive || player.GetModPlayer<SceneEffectPlayer>().effectActive[(ushort)SceneEffectPlayer.EffectorType.SlimeMonsoon] > 0;

        public override void SpecialVisuals(Player player, bool isActive)
        {
            if (!SkyManager.Instance["HuntOfTheOldGods:SlimeMonsoon"].IsActive() && isActive) {
                SkyManager.Instance.Activate("HuntOfTheOldGods:SlimeMonsoon", player.Center);
            }
            else if (SkyManager.Instance["HuntOfTheOldGods:SlimeMonsoon"].IsActive() && !isActive) {
                SkyManager.Instance["HuntOfTheOldGods:SlimeMonsoon"].Deactivate();
            }

            if (!Filters.Scene["HuntOfTheOldGods:SlimeMonsoon"].IsActive() && isActive) {
                Filters.Scene.Activate("HuntOfTheOldGods:SlimeMonsoon", player.Center);
            }
        }
    }
}
