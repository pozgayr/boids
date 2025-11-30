using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Boids;

public interface IBoid
{
    public void Update(List<Boid> boids, List<Avoid> avoids, List<Chaser> chasers);
    public void Draw(SpriteBatch spriteBatch);

}
public class Boid : IBoid, IPoint
{
    public Vector2 Pos { get; protected set; }
    protected Vector2 _velocity;
    protected Vector2 _acceleration;
    protected Texture2D _img;
    protected Vector2 origin;
    protected float rotation;
    protected float size = 0.05f;
    protected float halfH;
    protected float halfW;

    protected float _maxSpeed = 4f;
    protected float _maxForce = 0.05f;
    protected float _neighborhood = 60f;
    protected float _radius = 30f;

    static Random rng = new Random();


    public Boid(Vector2 pos, Texture2D img)
    {
        Pos = pos;
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
                          Pos,
                         null,
                         Color.White,
                         rotation,
                         origin,
                         size,
                         SpriteEffects.None,
                         0f);
    }

    public virtual void Update(List<Boid> boids, List<Avoid> avoids, List<Chaser> chasers)
    {

        Vector2 sep = Separation(boids) * 1.5f;
        Vector2 align = Alignment(boids);
        Vector2 coh = Cohesion(boids);

        Vector2 avoid = AvoidPoints(avoids, _radius);
        Vector2 avoidEnemy = AvoidPoints(chasers, _radius + 2f) * 1.5f;

        _acceleration = sep + align + coh + avoid + avoidEnemy;
        _velocity += _acceleration;

        Move();
        rotation = GetBoidRotation();

    }

    protected void Move()
    {
        Vector2 p = Pos;
        p += _velocity;

        if (p.X > Game1.screenWidth + halfW) p.X = -halfW;
        if (p.X < -halfW) p.X = Game1.screenWidth + halfW;

        if (p.Y > Game1.screenHeight + halfH) p.Y = -halfH;
        if (p.Y < -halfH) p.Y = Game1.screenHeight + halfH;

        Pos = p;
    }

    protected float GetBoidRotation()
    {
        return MathF.Atan2(_velocity.Y, _velocity.X) + MathF.PI / 2f;
    }

    protected Vector2 Limit(Vector2 v, float max)
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
            float d = Vector2.Distance(Pos, other.Pos);

            if (other != this && d < desiredDist)
            {
                Vector2 diff = Pos - other.Pos;
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
            float d = Vector2.Distance(Pos, other.Pos);
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
            float d = Vector2.Distance(Pos, other.Pos);
            if (other != this && d < _neighborhood)
            {
                sum += other.Pos;
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

    protected Vector2 Seek(Vector2 target)
    {
        Vector2 desired = target - Pos;
        desired = Vector2.Normalize(desired) * _maxSpeed;
        Vector2 steer = desired - _velocity;
        return Limit(steer, _maxForce);
    }

    protected Vector2 AvoidPoint(Vector2 target, float radius)
    {
        float d = Vector2.Distance(Pos, target);

        if (d < radius)
        {
            Vector2 desired = Pos - target;

            if (desired.LengthSquared() > 0)
                desired = Vector2.Normalize(desired);

            Vector2 steer = desired - _velocity;
            return Limit(steer, _maxForce * 2.0f);
        }

        return Vector2.Zero;
    }

    protected Vector2 AvoidPoints(IEnumerable<IPoint> avoids, float radius)
    {
        Vector2 total = Vector2.Zero;

        foreach (var point in avoids)
        {
            total += AvoidPoint(point.Pos, radius);
        }
        return total;
    }
}
