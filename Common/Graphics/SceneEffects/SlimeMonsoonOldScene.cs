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
    public class SlimeMonsoonOldScene : ModSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

        public override string MapBackground => $"{nameof(CalamityHunt)}/Assets/Textures/SlimeMonsoonBG";

        public override bool IsSceneEffectActive(Player player) => player.GetModPlayer<SceneEffectPlayer>().effectActive[(ushort)SceneEffectPlayer.EffectorType.SlimeMonsoonOld] > 0;

        public override void SpecialVisuals(Player player, bool isActive)
        {
            if (!SkyManager.Instance["HuntOfTheOldGods:SlimeMonsoonOld"].IsActive() && isActive) {
                SkyManager.Instance.Activate("HuntOfTheOldGods:SlimeMonsoonOld", player.Center);
            }
            else if (SkyManager.Instance["HuntOfTheOldGods:SlimeMonsoonOld"].IsActive() && !isActive && !GoozmaSystem.GoozmaActive) {
                SkyManager.Instance["HuntOfTheOldGods:SlimeMonsoonOld"].Deactivate();
            }
        }
    }
}
