using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Boids;
public class Boid
{
    Vector2 _pos;
    Vector2 _velocity;
    Vector2 _acceleration;
    Texture2D _img;
    Vector2 origin;
    float rotation;
    float size = 0.4f;
    float halfH;
    float halfW;

    float _maxSpeed = 4f;
    float _maxForce = 0.05f;
    float _neighborhood = 60f;
    float _radius = 60f;

    static Random rng = new Random();


    public Boid(Vector2 pos, Texture2D img)
    {
        _pos = pos;
        _velocity = RandomDirection();
        _acceleration = RandomDirection();
        _img = img;

        halfW = _img.Width * size * 0.5f;
        halfH = _img.Height * size * 0.5f;
        origin = new Vector2(_img.Width * 0.5f, _img.Height * 0.5f);
    }

    private Vector2 RandomDirection()
    {
        float x = (float)(rng.NextDouble() * 2 - 1);   // range -1 to +1
        float y = (float)(rng.NextDouble() * 2 - 1);

        Vector2 v = new Vector2(x, y);

        if (v.LengthSquared() > 0)
            v.Normalize();

        return v;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_img,
                          _pos,
                         null,
                         Color.White,
                         rotation,
                         origin,
                         size,
                         SpriteEffects.None,
                         0f);
    }

    public void Update(List<Boid> boids, List<Avoid> avoids)
    {

        Vector2 sep = Separation(boids) * 1.5f;
        Vector2 align = Alignment(boids);
        Vector2 coh = Cohesion(boids);

        Vector2 avoid = AvoidPoints(avoids, _radius);

        _acceleration = sep + align + coh + avoid;
        _velocity += _acceleration;

        Move();
        rotation = GetBoidRotation();

    }

    private void Move()
    {
        _pos += _velocity;

        if (_pos.X > Game1.screenWidth + halfW) _pos.X = -halfW;
        if (_pos.X < -halfW) _pos.X = Game1.screenWidth + halfW;

        if (_pos.Y > Game1.screenHeight + halfH) _pos.Y = -halfH;
        if (_pos.Y < -halfH) _pos.Y = Game1.screenHeight + halfH;
    }

    private float GetBoidRotation()
    {
        return MathF.Atan2(_velocity.Y, _velocity.X) + MathF.PI / 2f;
    }

    private Vector2 Limit(Vector2 v, float max)
    {
        if (v.LengthSquared() > max * max)
            return Vector2.Normalize(v) * max;
        return v;
    }

    private Vector2 Separation(List<Boid> boids)
    {
        float desiredDist = _neighborhood / 2.0f;
        Vector2 steer = Vector2.Zero;

        int count = 0;

        foreach (var other in boids)
        {
            float d = Vector2.Distance(_pos, other._pos);

            if (other != this && d < desiredDist)
            {
                Vector2 diff = _pos - other._pos;
                diff /= d;
                steer += diff;
                count++;
            }
        }
        if (count > 0)
        {
            steer /= count;
        }
        if (steer.LengthSquared() > 0)
        {
            steer = Vector2.Normalize(steer) * _maxSpeed - _velocity;
            steer = Limit(steer, _maxForce);
        }
        return steer;
    }

    private Vector2 Alignment(List<Boid> boids)
    {
        Vector2 sum = Vector2.Zero;
        Vector2 steer = Vector2.Zero;
        int count = 0;

        foreach (var other in boids)
        {
            float d = Vector2.Distance(_pos, other._pos);
            if (other != this && d < _neighborhood)
            {
                sum += other._velocity;
                count++;
            }
        }

        if (count > 0)
        {
            sum /= count;
            sum = Vector2.Normalize(sum) * _maxSpeed;
            steer = sum - _velocity;
            steer = Limit(steer, _maxForce);
        }
        return steer;
    }

    private Vector2 Cohesion(List<Boid> boids)
    {
        Vector2 sum = Vector2.Zero;
        int count = 0;

        foreach (var other in boids)
        {
            float d = Vector2.Distance(_pos, other._pos);
            if (other != this && d < _neighborhood)
            {
                sum += other._pos;
                count++;
            }
        }
        if (count > 0)
        {
            Vector2 center = sum / count;
            return Seek(center);
        }
        return Vector2.Zero;
    }

    private Vector2 Seek(Vector2 target)
    {
        Vector2 desired = target - _pos;
        desired = Vector2.Normalize(desired) * _maxSpeed;
        Vector2 steer = desired - _velocity;
        return Limit(steer, _maxForce);
    }

    private Vector2 AvoidPoint(Vector2 target, float radius)
    {
        float d = Vector2.Distance(_pos, target);

        if (d < radius)
        {
            Vector2 desired = _pos - target;

            if (desired.LengthSquared() > 0)
                desired = Vector2.Normalize(desired) * _maxSpeed;

            Vector2 steer = desired - _velocity;
            return Limit(steer, _maxForce * 2.0f);
        }

        return Vector2.Zero;
    }

    private Vector2 AvoidPoints(List<Avoid> avoids, float radius)
    {
        Vector2 total = Vector2.Zero;

        foreach (var point in avoids)
        {
            total += AvoidPoint(point._pos, radius);
        }
        return total;
    }
}
