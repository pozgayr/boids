using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Boids;

public class Avoid : IPoint
{
    Random rng = new Random();
    public Vector2 Pos { get; private set; }
    Vector2 _velocity;
    float _friction = 0.99f;
    Texture2D _img;
    float size = 0.5f;
    float halfH;
    float halfW;
    Vector2 origin;
    float _pushForce = 0.5f;


    public Avoid(Vector2 pos, Texture2D img)
    {
        Pos = pos;
        _img = img;
        halfW = _img.Width * size * 0.5f;
        halfH = _img.Height * size * 0.5f;
        origin = new Vector2(_img.Width * 0.5f, _img.Height * 0.5f);

        _velocity = new Vector2(
                    (float)(rng.NextDouble() - 0.5),
                    (float)(rng.NextDouble() - 0.5)
                    );
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_img,
                          Pos,
                          null,
                          Color.White,
                          0.0f,
                          origin,
                          size,
                          SpriteEffects.None,
                          0f);
    }
    public void Update(List<Boid> boids, List<Avoid> avoids, List<Chaser> chasers)
    {
        Vector2 boidPush = AvoidBoids(boids, radius: 60f);
        Vector2 chaserPush = AvoidBoids(chasers, radius: 80f);
        Vector2 avoidPush = AvoidBoids(avoids, radius: 70f);

        _velocity += boidPush + chaserPush + avoidPush;
        Move();
    }
    private void Move()
    {
        Vector2 p = Pos;
        p += _velocity;
        _velocity *= _friction;

        if (p.X > Game1.screenWidth + halfW) p.X = -halfW;
        if (p.X < -halfW) p.X = Game1.screenWidth + halfW;

        if (p.Y > Game1.screenHeight + halfH) p.Y = -halfH;
        if (p.Y < -halfH) p.Y = Game1.screenHeight + halfH;

        Pos = p;
    }


    private Vector2 Push(Vector2 otherPos, float radius, float strength)
    {
        Vector2 diff = Pos - otherPos;
        float dist = diff.Length();

        if (dist < radius && dist > 0.0001f)
        {
            // smooth falloff: strong when close, weaker when far
            float factor = 1f - (dist / radius);
            factor *= factor; // optional smoothing

            diff.Normalize();

            return diff * (strength * factor);
        }

        return Vector2.Zero;
    }

    private Vector2 AvoidBoids(IEnumerable<IPoint> boids, float radius)
    {
        Vector2 total = Vector2.Zero;
        foreach (var b in boids)
        {
            Vector2 push = Push(b.Pos, radius, _pushForce);
            total += push;
        }
        return total;
    }
}
