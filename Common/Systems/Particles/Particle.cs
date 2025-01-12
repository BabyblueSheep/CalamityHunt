﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace CalamityHunt.Common.Systems.Particles;

[Autoload(Side = ModSide.Client)]
public abstract class Particle : ModTexturedType
{
    public int Type { get; private set; }

    public bool ShouldRemove { get; set; } = false;

    public Vector2 position;

    public Vector2 velocity;

    public float rotation;

    public float scale = 1f;

    public Color color = Color.White;

    public virtual void OnSpawn() { }

    public virtual void Update() { }

    public virtual void Draw(SpriteBatch spriteBatch) { }

    protected override void Register()
    {
        Type = ParticleLoader.ReserveID();
        if (AssetDirectory.Textures.Particle is null) {
            AssetDirectory.Textures.Particle = new Dictionary<int, Asset<Texture2D>>();
        }
        AssetDirectory.Textures.Particle.Add(Type, ModContent.Request<Texture2D>(Texture));

        if (ParticleLoader.Instance is null) {
            ParticleLoader.Instance = new Dictionary<Type, Particle>();
        }
        ParticleLoader.Instance.Add(GetType(), this);
    }

    public static T Create<T>(Action<T> initializer) where T : Particle
    {
        T newParticle = (T)ModContent.GetInstance<T>().MemberwiseClone();
        initializer(newParticle);
        newParticle.OnSpawn();
        return newParticle;
    }
}
