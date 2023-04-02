﻿using CalamityHunt.Common;
using CalamityHunt.Common.Systems.Camera;
using CalamityHunt.Common.Systems.Particles;
using CalamityHunt.Content.Bosses.Goozma.Projectiles;
using CalamityHunt.Content.Bosses.Goozma.Slimes;
using CalamityHunt.Content.Items.Materials;
using CalamityHunt.Content.Particles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityHunt.Content.Bosses.Goozma
{
    [AutoloadBossHead]
    public partial class Goozma : ModNPC
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Goozma");
            NPCID.Sets.TrailCacheLength[Type] = 10;
            NPCID.Sets.TrailingMode[Type] = -1;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
            {
                SpecificallyImmuneTo = new int[] {
                    BuffID.OnFire,
                    BuffID.Ichor,
                    BuffID.CursedInferno,
                    BuffID.Poisoned,
                    BuffID.Confused
                }
            };
            NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);
            NPCID.Sets.ShouldBeCountedAsBoss[Type] = true;

            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                Rotation = 0.01f,
                Velocity = 2f
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(),
                new FlavorTextBestiaryInfoElement("Mods.CalamityHunt.Bestiary.Goozma"),
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.SlimeRain,
            });
        }

        public override void SetDefaults()
        {
            NPC.width = 90;
            NPC.height = 100;
            NPC.damage = 0;
            NPC.defense = 100;
            NPC.lifeMax = 3500000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 25);
            NPC.SpawnWithHigherTime(30);
            NPC.direction = 1;
            NPC.boss = true;
            NPC.npcSlots = 10f;
            NPC.aiStyle = -1;
            NPC.dontTakeDamage = true;
            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot($"{nameof(CalamityHunt)}/Assets/Music/GlutinousArbitration");
                Music2 = MusicLoader.GetMusicSlot($"{nameof(CalamityHunt)}/Assets/Music/PANDEMONIUM");
            }

        }

        public int Music2;

        private static int relicType;

        public override void Load()
        {
            relicType = BossDropAutoloader.AddBossRelic("Goozma", $"{nameof(CalamityHunt)}/Assets/Textures/Relics/");
            On_Main.UpdateAudio += FadeMusicOut;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            //Bag
            //npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<EntropyMatter>()));

            //Trophy

            //Relic
            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(relicType));

            //Master Drop

            LeadingConditionRule classic = new LeadingConditionRule(new Conditions.NotExpert());

            classic.OnSuccess(ItemDropRule.Common(ModContent.ItemType<EntropyMatter>(), 1, 20, 30));
            
            
            //Weapon
            //classic.OnSuccess(ItemDropRule.Common(ModContent.ItemType<EntropyMatter>()));
        }

        public ref float Time => ref NPC.ai[0];
        public ref float Attack => ref NPC.ai[1];
        public ref float Phase => ref NPC.ai[2];
        public ref NPC ActiveSlime => ref Main.npc[(int)NPC.ai[3]];

        private int currentSlime;
        private int[] nextAttack;

        public NPCAimedTarget Target => NPC.ai[3] < 0 ? NPC.GetTargetData() : (ActiveSlime.GetTargetData().Invalid ? NPC.GetTargetData() : ActiveSlime.GetTargetData());

        private enum AttackList
        {
            SpawnSelf,
            SpawnSlime,
            FlyAround,
            FlyInto,
            Vballs,
        }

        public override void OnSpawn(IEntitySource source)
        {
            nextAttack = new int[4] { -1, -1, -1, -1 };
            NPC.ai[3] = -1;
            currentSlime = -1;
            Phase = -1;
            NPC.scale = 0.5f;
            oldVel = new Vector2[NPCID.Sets.TrailCacheLength[Type]];
            Main.newMusic = Music;
            for (int i = 0; i < Main.musicFade.Length; i++)
                Main.musicFade[i] = 0.1f;
            Main.musicFade[Main.newMusic] = 1f;

            if (!Main.dedServ)
            {
                Particle crack = Particle.NewParticle(Particle.ParticleType<SpaceCrack>(), NPC.Center, Vector2.Zero, Color.Black, 50f);
                crack.data = "GoozmaColor";

                SoundStyle roar = new SoundStyle($"{nameof(CalamityHunt)}/Assets/Sounds/Goozma/GoozmaAwaken");
                SoundEngine.PlaySound(roar, NPC.Center);
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            //target.AddBuff(BuffID.VortexDebuff, 480);
        }

        private void ChangeWeather()
        {
            if (Main.slimeRain)
                Main.StopSlimeRain(false);

            Main.StopRain();

            Main.LocalPlayer.ManageSpecialBiomeVisuals("HuntOfTheOldGods:SlimeMonsoon", true, Main.LocalPlayer.Center);
        }

        private Vector2 saveTarget;
        private Vector2 eyePower;
        private Vector2[] oldVel;

        public override bool CheckDead()
        {
            if (NPC.life < 1)
            {
                switch (Phase)
                {
                    case 0:

                        Time = 0;
                        Phase++;
                        NPC.life = 1;
                        NPC.lifeMax = (int)(NPC.lifeMax * 0.15f);
                        NPC.dontTakeDamage = true;

                        break;

                    case 2:

                        Time = 0;
                        Phase++;
                        NPC.life = 1;

                        break;
                }
            }
            return Phase >= 3;
        }

        public override void AI()
        {
            ChangeWeather();
            bool noSlime = NPC.ai[3] < 0 || NPC.ai[3] >= Main.maxNPCs || ActiveSlime.ai[1] > 3 || !ActiveSlime.active;
            if (Phase == 0 && noSlime)
                Attack = (int)AttackList.SpawnSlime;

            if (Target.Invalid)
                NPC.TargetClosestUpgraded();
            if (NPC.GetTargetData().Invalid && Phase != -5)
            {
                Phase = -5;
                Time = 0;
                NPC.dontTakeDamage = true;
            }

            if (Math.Abs(NPC.Center.X - Target.Center.X) > 10)
                NPC.direction = NPC.Center.X > Target.Center.X ? -1 : 1;

            drawOffset = new Vector2(-14f * NPC.direction, 20f) + new Vector2((float)Math.Sin(NPC.localAI[0] * 0.05f % MathHelper.TwoPi) * 2, (float)Math.Cos(NPC.localAI[0] * 0.025f % MathHelper.TwoPi) * 4);
            float offsetWobble = (float)Math.Cos(NPC.localAI[0] * 0.05f % MathHelper.TwoPi) * 0.07f;
            NPC.rotation = MathHelper.Lerp(NPC.rotation, Math.Clamp(NPC.velocity.X * 0.02f, -1f, 1f) - (offsetWobble - 0.1f) * NPC.direction, 0.2f);
            extraTilt = MathHelper.Lerp(extraTilt, Math.Clamp(-NPC.velocity.X * 0.01f, -1f, 1f) - 0.09f * NPC.direction, 0.15f);
            NPC.scale = MathHelper.Lerp(NPC.scale, 1f, 0.05f);

            if (!NPC.dontTakeDamage)
            {
                if (Time % 2 == 0)
                    NPC.damage = GetDamage(0);
                else
                    NPC.damage = 0;

                if (!noSlime)
                    if (NPC.Distance(ActiveSlime.Center) < 400)
                        NPC.damage = 0;
            }

            switch (Phase)
            {
                case -1:

                    Attack = (int)AttackList.SpawnSelf;
                    NPC.direction = -1;
                    NPC.velocity.Y = -Utils.GetLerpValue(20, 40, Time, true);
                    if (Time > 60)
                    {
                        NPC.dontTakeDamage = false;
                        Phase = 0;
                        Time = 0;
                        NPC.scale = 1f;
                    }

                    Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(10, 10), DustID.TintableDust, Main.rand.NextVector2CircularEdge(10, 10), 200, Color.Black, Main.rand.NextFloat(2, 4)).noGravity = true;

                    break;

                case 0:

                    if (Attack == (int)AttackList.SpawnSlime)
                    {
                        NPC.TargetClosestUpgraded();
                        NPC.direction = NPC.DirectionTo(NPC.GetTargetData().Center).X > 0 ? 1 : -1;
                        NPC.defense = 1000;

                        if (Time > 5 && Time < 45)
                        {
                            NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(NPC.GetTargetData().Center) * Math.Max(NPC.Distance(NPC.GetTargetData().Center) - 150, 0) * 0.12f, 0.1f);
                            NPC.position += Main.rand.NextVector2Circular(6, 6);

                            for (int i = 0; i < 3; i++)
                            {
                                Vector2 inward = NPC.Center + Main.rand.NextVector2Circular(70, 70) + Main.rand.NextVector2CircularEdge(100 - Time, 100 - Time);
                                Color glowColor = SlimeUtils.GoozColor(Main.rand.Next(6));
                                glowColor.A /= 2;
                                Dust.NewDustPerfect(inward, DustID.FireworksRGB, inward.DirectionTo(NPC.Center) * Main.rand.NextFloat(3f), 0, glowColor, 1f).noGravity = true;
                            }
                        }

                        if (Time == 50)
                        {
                            NPC.velocity.Y = 7;
                            for (int i = 0; i < 25; i++)
                            {
                                Vector2 outward = NPC.Center + Main.rand.NextVector2Circular(10, 10);
                                Color glowColor = SlimeUtils.GoozColor(Main.rand.Next(6));
                                glowColor.A /= 2;
                                Dust.NewDustPerfect(outward, DustID.FireworksRGB, outward.DirectionFrom(NPC.Center) * Main.rand.NextFloat(5f, 15f), 0, glowColor, 2f).noGravity = true;
                            }

                            //for (int i = 0; i < Main.rand.Next(3, 8); i++)
                            //{
                            //    NPC slime = NPC.NewNPCDirect(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y - 50, Main.rand.Next(SlimeUtils.SlimeIDs));
                            //    slime.velocity = new Vector2(Main.rand.Next(-4, 4), Main.rand.Next(-7, -5));
                            //    if (Main.rand.NextBool(10))
                            //    {
                            //        NPC jellyfish = NPC.NewNPCDirect(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y - 50, Main.rand.Next(SlimeUtils.JellyfishIDs));
                            //        jellyfish.velocity = new Vector2(Main.rand.Next(-4, 4), Main.rand.Next(-7, -5));
                            //    }
                            //}

                            currentSlime = (currentSlime + 1) % 4;
                            nextAttack[currentSlime]++;

                            for (int i = 0; i < nextAttack.Length; i++)
                                nextAttack[i] = nextAttack[i] % 3;

                            int[] slimeTypes = new int[]
                            {
                                ModContent.NPCType<EbonianBehemuck>(),
                                ModContent.NPCType<DivineGargooptuar>(),
                                ModContent.NPCType<CrimulanGlopstrosity>(),
                                ModContent.NPCType<StellarGeliath>()
                            };

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                if (NPC.ai[3] < 0 || NPC.ai[3] >= Main.maxNPCs || !ActiveSlime.active)
                                {
                                    NPC.ai[3] = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y - 50, slimeTypes[currentSlime], ai0: -50, ai1: nextAttack[currentSlime], ai2: NPC.whoAmI);
                                    ActiveSlime.velocity.Y -= 16;
                                }
                                else
                                {
                                    Vector2 pos = ActiveSlime.Bottom;
                                    ActiveSlime.active = false;
                                    NPC.ai[3] = NPC.NewNPC(NPC.GetSource_FromAI(), (int)pos.X, (int)pos.Y, slimeTypes[currentSlime], ai0: -50, ai1: nextAttack[currentSlime], ai2: NPC.whoAmI);
                                }

                                if (!Main.dedServ)
                                {
                                    SoundStyle spawn = new SoundStyle($"{nameof(CalamityHunt)}/Assets/Sounds/Goozma/GoozmaSpawnSlime");
                                    SoundEngine.PlaySound(spawn, NPC.Center);
                                }
                            }
                            else if (Main.netMode == NetmodeID.MultiplayerClient)
                                NPC.netUpdate = true;
                        }

                        NPC.velocity *= 0.9f;

                        if (Time > 70)
                        {
                            Time = 0;
                            Attack++;
                        }
                    }
                    else
                    {
                        NPC.defense = 100;

                        //attack depends on slime's attack, which depends on slime
                        switch (currentSlime)
                        {
                            //ebon
                            case 0:
                                switch (ActiveSlime.ai[1])
                                {
                                    case 0:

                                        AresLockTo(Target.Center - new Vector2(0, 400) + Target.Velocity * new Vector2(5f, 2f));

                                        if (Time > 50)
                                        {
                                            BasicProjectileAttack(Target.Center - Target.Velocity * 10, SlimeballTypes.EbonianBubbles);
                                            NPC.velocity.X *= 0.8f;
                                        }

                                        break;
                                    case 1:

                                        FlyTo(Target.Center - Vector2.UnitY * 300 + Target.Velocity * new Vector2(10f, 5f));
                                        if (Time > 80)
                                        {
                                            if (Time % 70 == 0)
                                                Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(Target.Center + Target.Velocity * 10).SafeNormalize(Vector2.Zero) * 10, ModContent.ProjectileType<PureSlimeball>(), GetDamage(2), 0);
                                           
                                            BasicProjectileAttack(Target.Center + Target.Velocity * 20 + Main.rand.NextVector2Circular(10, 10), SlimeballTypes.EbonianTrifecta);
                                        }

                                        break;

                                    case 2:

                                        AresLockTo(Target.Center - Vector2.UnitY * 270);

                                        break;
                                }
                                break;

                            //holy
                            case 1:
                                switch (ActiveSlime.ai[1])
                                {
                                    case 0:

                                        Vector2 outerRing = Target.Center.DirectionTo(ActiveSlime.Center) * 800;
                                        FlyTo(ActiveSlime.Center - outerRing);
                                        NPC.velocity *= 0.6f;

                                        break;

                                    case 1:

                                        AresLockTo(Target.Center + new Vector2(0, 360) + Target.Velocity * new Vector2(4f, 3f));

                                        if (Time > 50)
                                        {
                                            BasicProjectileAttack(Target.Center - Target.Velocity * 10, SlimeballTypes.CrystalStorm);
                                            NPC.velocity.X *= 0.8f;
                                        }

                                        break;

                                    case 2:

                                        Orbit(500, new Vector2(600, 100));
                                        BasicProjectileAttack(Target.Center, SlimeballTypes.PixieBallDisruption);

                                        break;
                                }
                                break;

                            //crim
                            case 2:
                                switch (ActiveSlime.ai[1])
                                {
                                    case 0:

                                        if (Time > 70 && Time < 230)
                                        {
                                            FlyTo(Target.Center + new Vector2(700 * (ActiveSlime.Center.X > Target.Center.X ? -1 : 1) * Utils.GetLerpValue(0, 100, Time, true), (float)Math.Sin(Time % 110 * MathHelper.TwoPi / 110f) * 130));

                                            if ((Main.rand.NextBool(15) || Time % 30 == 0) && Time > 90)
                                                Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, Main.rand.NextVector2Circular(15, 15), ModContent.ProjectileType<PureSlimeball>(), GetDamage(2), 0);

                                        }
                                        else
                                            Fly();
                                        break;
                                    case 1:

                                        Fly();
                                        NPC.velocity *= 0.9f;

                                        break;
                                    case 2:

                                        Orbit(200, new Vector2(720, 0));

                                        if (Time > 40 && Time < 380)
                                        {
                                            BasicProjectileAttack(Target.Center, SlimeballTypes.CrimulanHop);
                                            if (NPC.Center.Y < Target.Center.Y + 50 && NPC.Center.Y > Target.Center.Y - 50)
                                                if (Time % 2 == 0)
                                                    Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(Target.Center).RotatedByRandom(0.3f) * 10f, ModContent.ProjectileType<PureSlimeball>(), GetDamage(2), 0);

                                        }

                                        NPC.velocity *= 0.9f;

                                        break;
                                }
                                break;

                            //astral
                            case 3:
                                switch (ActiveSlime.ai[1])
                                {
                                    case 0:

                                        bool inLeft = ActiveSlime.AngleTo(Target.Center) < 0.5f && ActiveSlime.AngleTo(Target.Center) > -0.5f;
                                        bool inRight = ActiveSlime.AngleTo(Target.Center) - MathHelper.Pi < 0.5f && ActiveSlime.AngleTo(Target.Center) - MathHelper.Pi > -0.5f;
                                        if (inLeft || inRight)
                                            Dash(50);
                                        else
                                        {
                                            Fly();
                                            NPC.velocity *= 0.97f;
                                        }

                                        break;
                                    case 1:

                                        Fly();

                                        break;
                                    case 2:

                                        AresLockTo(Target.Center - Vector2.UnitY * 200);
                                        NPC.velocity *= 0.8f;

                                        break;
                                }
                                break;
                        }
                    }

                    eyePower = Vector2.One * 1.5f;

                    break;

                case 1:

                    NPC.velocity = Vector2.Zero;
                    if (NPC.ai[3] > -1 && NPC.ai[3] <= Main.maxNPCs)
                        if (ActiveSlime.active)
                            ActiveSlime.active = false;

                    drawOffset += Main.rand.NextVector2Circular(10, 10) * Utils.GetLerpValue(0, 300, Time, true) * Utils.GetLerpValue(302, 300, Time, true);
                    NPC.dontTakeDamage = true;
                    NPC.life = 1 + (int)((float)Math.Pow(Utils.GetLerpValue(300, 530, Time, true), 3) * (NPC.lifeMax - 1));

                    eyePower = Vector2.SmoothStep(Vector2.One * 1.5f, new Vector2(5f, 3.6f), Utils.GetLerpValue(300, 500, Time, true));
                    if (!Main.dedServ)
                    {
                        if (Time == 1)
                        {
                            Main.instance.CameraModifiers.Add(new SlowPan(NPC.Bottom, 300, 200, 30, "GoozmaEnrage"));
                            //SoundStyle fakeDeathSound = new SoundStyle($"{nameof(CalamityHunt)}/Assets/Sounds/GoozmaFakeDeath");
                            //SoundEngine.PlaySound(fakeDeathSound, NPC.Center);
                        }
                        if (Time == 300)
                        {
                            //SoundStyle roar = new SoundStyle($"{nameof(CalamityHunt)}/Assets/Sounds/GoozmaEnrage");
                            //SoundEngine.PlaySound(roar, NPC.Center);
                        }
                    }
                    if (Time > 530)
                    {
                        Music = Music2;
                        Main.newMusic = Music;
                        Main.musicFade[Main.curMusic] = 0f;
                        Main.musicFade[Main.newMusic] = 1f;

                        Time = 0;
                        Phase++;
                        NPC.dontTakeDamage = false;
                        NPC.defense += 20;
                    }

                    break;

                case 2:

                    NPC.takenDamageMultiplier = 2f;
                    Orbit((int)(400 + ((float)NPC.life / NPC.lifeMax * 300)), new Vector2(400, 100));
                    BasicProjectileAttack(Target.Center + Target.Velocity * 2, SlimeballTypes.LastStand);

                    break;

                case -5:

                    NPC.velocity *= 0.8f;

                    if (Time > 30 && Time % 3 == 0)
                    {
                        if (!Main.dedServ)
                        {
                            Particle leave = Particle.NewParticle(Particle.ParticleType<SpaceCrack>(), NPC.Center, Vector2.Zero, Color.Black, 40f);
                            leave.data = "GoozmaBlack";
                        }
                    }

                    if (Time == 50)
                    {
                        if (!Main.dedServ)
                        {
                            Particle leave = Particle.NewParticle(Particle.ParticleType<SpaceCrack>(), NPC.Center, Vector2.Zero, Color.Black, 50f);
                            leave.data = "GoozmaColor";
                        }

                        NPC.active = false;
                    }

                    break;

                default:
                    Phase = -5;
                    break;
            };           


            Time++;

            if (!Main.dedServ)
            {
                NPC.localAI[0]++;
                NPC.localAI[1] -= NPC.velocity.X * 0.001f + NPC.direction * 0.02f;

                if (NPC.localAI[0] % 4 == 1)
                {
                    for (int i = NPCID.Sets.TrailCacheLength[Type] - 1; i > 0; i--)
                    {
                        NPC.oldPos[i] = NPC.oldPos[i - 1];
                        NPC.oldRot[i] = NPC.oldRot[i - 1];
                        oldVel[i] = oldVel[i - 1];
                    }
                    NPC.oldPos[0] = NPC.position;
                    NPC.oldRot[0] = NPC.rotation;
                    oldVel[0] = NPC.velocity;
                }

                if (Main.rand.NextBool(8))
                {
                    Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height + 20, DustID.TintableDust, 0, 0, 180, Color.Black, 3f);
                    dust.noGravity = true;
                }
            }
        }

        public override void OnKill()
        {
            if (!Main.dedServ)
            {
                Main.newMusic = Music;
                for (int i = 0; i < Main.musicFade.Length; i++)
                    Main.musicFade[i] = 0.1f;

                SoundStyle deathSound = new SoundStyle($"{nameof(CalamityHunt)}/Assets/Sounds/Goozma/GoozmaPop");
                SoundEngine.PlaySound(deathSound, NPC.Center);
            }
        }

        //public override bool CheckDead()
        //{
        //    if (Phase == 0)
        //    {
        //        Time = 0;
        //        Phase++; 
        //        NPC.dontTakeDamage = true;
        //        NPC.lifeMax = (int)(NPC.lifeMax * 0.1f);
        //    }

        //    return false;
        //}

        private void Fly()
        {
            Attack = (int)AttackList.FlyAround;
            NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(Target.Center).SafeNormalize(Vector2.Zero) * Math.Max(0, NPC.Distance(Target.Center) - 300) * 0.03f, 0.02f);
        }        
        
        private void FlyTo(Vector2 target)
        {
            Attack = (int)AttackList.FlyInto;
            NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(target).SafeNormalize(Vector2.Zero) * Math.Max(1, NPC.Distance(target) * 0.2f), 0.1f);
        }        
        
        private void AresLockTo(Vector2 target)
        {
            Attack = (int)AttackList.FlyAround;
            NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(target).SafeNormalize(Vector2.Zero) * Math.Max(1, NPC.Distance(target) * 0.4f), 0.27f);
        }

        private void Dash(int dashCD)
        {
            Attack = (int)AttackList.FlyInto;
            NPC.damage = GetDamage(0);

            if (NPC.Center.Y < Target.Center.Y + 90 && NPC.Center.Y > Target.Center.Y - 90)
            {
                Time = 0;
                NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, (NPC.Center.Y - Target.Center.Y) * 0.2f, 0.1f);
                Fly();
            }

                float power = 0.05f;
            if (Time > dashCD - 6)
                power = 0.2f;
            else if (Time > dashCD - 15)
                NPC.velocity *= 0.9f;

            NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(Target.Center).SafeNormalize(Vector2.Zero) * Math.Clamp(NPC.Distance(Target.Center) * power * 1.5f, 20, 100), power > 0.05f ? 0.1f : 0.03f);

            if (Time > dashCD)
                Time = 0;
        }
        
        private void DashTo(Vector2 targetPos, int time)
        {
            Attack = (int)AttackList.FlyInto;
            NPC.damage = GetDamage(0);

            float power = 0.05f;
            if (Time > time - 6)
                power = 0.2f;
            else if (Time > time - 15)
                NPC.velocity *= 0.9f;

            NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(targetPos).SafeNormalize(Vector2.Zero) * Math.Clamp(NPC.Distance(targetPos) * power * 1.5f, 20, 100), power > 0.05f ? 0.1f : 0.03f);

            if (Time > time)
                Time = 0;
        }

        private int randDirection;

        private void Orbit(int frequency, Vector2 startingVector)
        {
            Vector2 target = Target.Center + startingVector.RotatedBy(Time % frequency / (float)frequency * MathHelper.TwoPi);

            NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(target).SafeNormalize(Vector2.Zero) * NPC.Distance(target) * 0.1f, 0.1f);
        }

        private enum SlimeballTypes
        {
            Default,
            EbonianBubbles,
            EbonianTrifecta,
            CrystalStorm,
            PixieBallDisruption,
            CrimulanSlam,
            CrimulanHop,
            LastStand
        }

        private void BasicProjectileAttack(Vector2 targetPos, SlimeballTypes type)
        {
            SoundStyle shotSound = new SoundStyle($"{nameof(CalamityHunt)}/Assets/Sounds/Goozma/GoozmaShot", 1, 2);
            shotSound.PitchVariance = 0.15f;            
            SoundStyle bloatSound = new SoundStyle($"{nameof(CalamityHunt)}/Assets/Sounds/Goozma/GoozmaShot", 1, 2);
            bloatSound.PitchVariance = 0.15f;            
            SoundStyle fireballSound = new SoundStyle($"{nameof(CalamityHunt)}/Assets/Sounds/Goozma/GoozmaShot", 1, 2);
            fireballSound.PitchVariance = 0.15f;
            switch (type)
            {
                case SlimeballTypes.Default:

                    if (NPC.Distance(Target.Center) > 50)
                    {
                        if (Time % 60 == 0)
                            Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(targetPos).SafeNormalize(Vector2.Zero).RotatedByRandom(0.3f) * 3f, ModContent.ProjectileType<SlimeShot>(), 5, 0);
                    }

                    break;

                case SlimeballTypes.EbonianBubbles:

                    if (NPC.Distance(Target.Center) > 300)
                    {
                        if (Time % 10 == 0 && !Main.dedServ)
                            SoundEngine.PlaySound(shotSound, NPC.Center);

                        float angle = MathHelper.SmoothStep(1.4f, 0.7f, Time / 350f);
                        if (Time % 12 == 0)
                        {
                            Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitY.RotatedBy(angle) * Main.rand.Next(4, 7), ModContent.ProjectileType<SlimeShot>(), GetDamage(1), 0);
                            Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitY.RotatedBy(-angle) * Main.rand.Next(4, 7), ModContent.ProjectileType<SlimeShot>(), GetDamage(1), 0);
                        }
                        if ((Time + 6) % 12 == 0)
                        {
                            Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitY.RotatedBy(angle + 0.4f) * Main.rand.Next(2, 4), ModContent.ProjectileType<SlimeShot>(), GetDamage(1), 0);
                            Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitY.RotatedBy(-angle - 0.4f) * Main.rand.Next(2, 4), ModContent.ProjectileType<SlimeShot>(), GetDamage(1), 0);
                        }
                        if (Target.Center.Y < NPC.Top.Y - 300)
                            Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(Target.Center).SafeNormalize(Vector2.Zero) * Main.rand.Next(2, 8), ModContent.ProjectileType<SlimeShot>(), GetDamage(1), 0);
                    }

                    break;

                case SlimeballTypes.EbonianTrifecta:

                    if (Time >= 180)
                    {
                        if (Time - 180 % 60 == 0)
                        {
                            if (!Main.dedServ)
                                SoundEngine.PlaySound(shotSound, NPC.Center);

                            Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(targetPos).SafeNormalize(Vector2.Zero).RotatedByRandom(0.3f) * 3f, ModContent.ProjectileType<SlimeShot>(), GetDamage(1), 0);
                        }
                    }


                    break;

                case SlimeballTypes.CrystalStorm:

                    if (NPC.Distance(Target.Center) > 300)
                    {
                        if (Time % 15 == 0 && !Main.dedServ)
                            SoundEngine.PlaySound(shotSound, NPC.Center);

                        float angle = MathHelper.SmoothStep(-1.5f, -1f, Time / 350f);
                        if (Time % 15 == 0)
                        {
                            Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, -Vector2.UnitY.RotatedBy(angle) * Main.rand.Next(4, 7), ModContent.ProjectileType<SlimeShot>(), GetDamage(1), 0);
                            Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, -Vector2.UnitY.RotatedBy(-angle) * Main.rand.Next(4, 7), ModContent.ProjectileType<SlimeShot>(), GetDamage(1), 0);
                        }
                        if ((Time + 8) % 8 == 0)
                        {
                            Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, -Vector2.UnitY.RotatedBy(angle - 0.4f) * Main.rand.Next(2, 4), ModContent.ProjectileType<SlimeShot>(), GetDamage(1), 0);
                            Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, -Vector2.UnitY.RotatedBy(-angle + 0.4f) * Main.rand.Next(2, 4), ModContent.ProjectileType<SlimeShot>(), GetDamage(1), 0);
                        }
                        if (Target.Center.Y > NPC.Top.Y + 300)
                            Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(Target.Center).SafeNormalize(Vector2.Zero) * Main.rand.Next(2, 8), ModContent.ProjectileType<SlimeShot>(), GetDamage(1), 0);
                    }

                    break;

                case SlimeballTypes.PixieBallDisruption:

                    if (Time % 80 == 0)
                    {
                        int count = Main.rand.Next(10, 18);
                        for (int i = 0; i < count; i++)
                        {
                            Projectile dart = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, new Vector2(Main.rand.Next(10, 20), 0).RotatedBy(MathHelper.TwoPi / count * i), ModContent.ProjectileType<BloatedBlast>(), GetDamage(1), 0);
                            dart.ai[1] = 1;
                            dart.localAI[0] = Time * 2f;
                        }
                        if (!Main.dedServ)
                            SoundEngine.PlaySound(shotSound, NPC.Center);
                    }

                    break;
                
                case SlimeballTypes.CrimulanSlam:

                    if (NPC.Distance(Target.Center) > 300)
                    {
                        if (Time % 15 == 0 && !Main.dedServ)
                            SoundEngine.PlaySound(shotSound, NPC.Center);

                        if ((Time + Main.rand.Next(0, 2)) % 8 == 0)
                            Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(targetPos).SafeNormalize(Vector2.Zero).RotatedByRandom(0.15f).RotatedBy(0.4f) * 20f, ModContent.ProjectileType<SlimeShot>(), GetDamage(1), 0);
                        if ((Time + Main.rand.Next(0, 2)) % 8 == 0)
                            Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(targetPos).SafeNormalize(Vector2.Zero).RotatedByRandom(0.15f).RotatedBy(-0.4f) * 20f, ModContent.ProjectileType<SlimeShot>(), GetDamage(1), 0);

                    }

                    break;

                case SlimeballTypes.CrimulanHop:

                    if (NPC.Distance(Target.Center) > 100)
                    {
                        if (Time % 10 == 0 && !Main.dedServ)
                            SoundEngine.PlaySound(shotSound, NPC.Center);

                        if (Time % 25 == 0)
                            Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(targetPos).SafeNormalize(Vector2.Zero).RotatedByRandom(0.3f) * 3f, ModContent.ProjectileType<SlimeShot>(), GetDamage(1), 0);

                    }

                    break;

                case SlimeballTypes.LastStand:

                    if (Time % 170 > 130)
                    {
                        int freq = 10;
                        if (Main.expertMode)
                            freq = 8;
                        if ((Time % 170) % freq == 0)
                        {
                            if (!Main.dedServ)
                                SoundEngine.PlaySound(fireballSound, NPC.Center);

                            Projectile fireball = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(Target.Center).SafeNormalize(Vector2.Zero).RotatedByRandom(0.6f) * 16f, ModContent.ProjectileType<RainbowBall>(), GetDamage(1), 0);
                            fireball.localAI[0] = Time * 2f;
                        }
                    }
                    else if (Time % 20 == 0)
                    {
                        if (!Main.dedServ)
                            SoundEngine.PlaySound(shotSound, NPC.Center);

                        Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(targetPos).SafeNormalize(Vector2.Zero).RotatedByRandom(1f), ModContent.ProjectileType<SlimeShot>(), GetDamage(1), 0);
                    }

                    if (Time % 270 == 0)
                    {
                        Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(targetPos).SafeNormalize(Vector2.Zero) * 10, ModContent.ProjectileType<BloatedBlast>(), GetDamage(5), 0);
                        if (!Main.dedServ)
                            SoundEngine.PlaySound(bloatSound, NPC.Center);
                    }

                    if (Time % 140 == 0)
                    {
                        Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(targetPos).SafeNormalize(Vector2.Zero), ModContent.ProjectileType<GooLightning>(), GetDamage(3), 0, -1, -80, 1500, 0);
                    }

                    if (Time % 55 == 0 && !Main.rand.NextBool(20))
                    {
                        Projectile statick = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, Main.rand.NextVector2CircularEdge(1, 1), ModContent.ProjectileType<GooLightning>(), GetDamage(4), 0, -1, -50, 1500, 1);
                        statick.ai[2] = 1;
                    }

                    break;
            }
        }

        private int GetDamage(int attack, float modifier = 1f)
        {
            int damage = attack switch
            {
                0 => 5,//contact
                1 => 50,//slime balls
                2 => 80,//pure gel
                3 => 120, //lightning
                4 => 20, //static
                5 => 50, //bloat
                _ => 0
            };

            return (int)(damage * modifier);
        }

        private float extraTilt;
        private Vector2 drawOffset;

        private void FadeMusicOut(On_Main.orig_UpdateAudio orig, Main self)
        {
            orig(self);

            if (Main.npc.Any(n => n.active && n.type == Type))
            {
                NPC goozma = Main.npc.FirstOrDefault(n => n.active && n.type == Type);
                if (goozma.ai[2] == 1)
                {
                    for (int i = 0; i < Main.musicFade.Length; i++)
                    {
                        float volume = Main.musicFade[i] * Main.musicVolume * Utils.GetLerpValue(320, 50, goozma.ai[0], true);
                        float tempFade = Main.musicFade[i];
                        Main.audioSystem.UpdateCommonTrackTowardStopping(i, volume, ref tempFade, Main.musicFade[i] > 0.15f && goozma.ai[0] < 320);
                        Main.musicFade[i] = tempFade;
                    }
                }
            }
        }
    }
}
