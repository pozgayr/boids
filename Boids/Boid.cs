using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Boids;

public interface IBoid
{
    Vector2 Pos { get; }
    public void Update(List<Boid> boids, List<Avoid> avoids, List<Chaser> chasers);
    public void Draw(SpriteBatch spriteBatch);

}

public enum Species { Red, Purple, Yellow }

public abstract class Boid : IBoid, IPoint
{
    public Vector2 Pos { get; protected set; }
    public Species Species { get; protected set; }
    protected virtual float MaxSpeed => 4f;
    protected virtual float MaxForce => 0.05f;
    protected virtual float Neighborhood => 80f;
    protected virtual float Size => 0.06f;
    protected virtual float AvoidRadius => 40f;
    protected virtual float FleeRadius => 150f;
    protected virtual float WanderStrength => 0.05f;

    protected Vector2 _velocity;
    protected Vector2 _acceleration;
    protected Texture2D _img;
    protected Vector2 origin;
    protected float rotation;
    protected float halfH;
    protected float halfW;


    static Random rng = new Random();


    public Boid(Vector2 pos, Texture2D img)
    {
        Pos = pos;
        _velocity = RandomDirection();
        _acceleration = RandomDirection();
        _img = img;

        halfW = _img.Width * Size * 0.5f;
        halfH = _img.Height * Size * 0.5f;
        origin = new Vector2(_img.Width * 0.5f, _img.Height * 0.5f);
    }

    protected Vector2 RandomDirection()
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
                         Size,
                         SpriteEffects.None,
                         0f);
    }

    public virtual void Update(List<Boid> boids, List<Avoid> avoids, List<Chaser> chasers)
    {

        Vector2 sep = Separation(boids) * 1.5f;
        Vector2 align = Alignment(boids);
        Vector2 coh = Cohesion(boids);

        Vector2 avoid = AvoidPoints(avoids, AvoidRadius);
        Vector2 fear = FleeMany(chasers, FleeRadius);
        Vector2 avoidOther = AvoidOtherSpecies(boids, AvoidRadius + 20f);
        Vector2 wander = RandomDirection() * WanderStrength;

        _acceleration = sep + align + coh + avoid + fear + avoidOther + wander;
        _velocity += _acceleration;
        if (_velocity.Length() < 1f)
            _velocity = Vector2.Normalize(_velocity) * 1f;

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

    protected Vector2 Separation(IEnumerable<Boid> boids)
    {
        float desiredDist = Neighborhood / 2.0f;
        Vector2 steer = Vector2.Zero;

        int count = 0;

        foreach (var other in boids)
        {
            if (other.Species != Species) continue;
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
            steer = Vector2.Normalize(steer) * MaxSpeed - _velocity;
            steer = Limit(steer, MaxForce);
        }
        return steer;
    }

    protected Vector2 Alignment(IEnumerable<Boid> boids)
    {
        Vector2 sum = Vector2.Zero;
        Vector2 steer = Vector2.Zero;
        int count = 0;

        foreach (var other in boids)
        {
            if (other.Species != Species) continue;
            float d = Vector2.Distance(Pos, other.Pos);
            if (other != this && d < Neighborhood)
            {
                sum += other._velocity;
                count++;
            }
        }

        if (count > 0)
        {
            sum /= count;
            sum = Vector2.Normalize(sum) * MaxSpeed;
            steer = sum - _velocity;
            steer = Limit(steer, MaxForce);
        }
        return steer;
    }

    protected Vector2 Cohesion(IEnumerable<Boid> boids)
    {
        Vector2 sum = Vector2.Zero;
        int count = 0;

        foreach (var other in boids)
        {
            if (other.Species != Species) continue;
            float d = Vector2.Distance(Pos, other.Pos);
            if (other != this && d < Neighborhood + 30f)
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
        desired = Vector2.Normalize(desired) * MaxSpeed;
        Vector2 steer = desired - _velocity;
        return Limit(steer, MaxForce);
    }

    protected Vector2 AvoidPoint(Vector2 target, float radius)
    {
        float d = Vector2.Distance(Pos, target);

        if (d < radius + 10f)
        {
            Vector2 desired = Pos - target;

            if (desired.LengthSquared() > 0)
                desired = Vector2.Normalize(desired);

            Vector2 steer = desired - _velocity;
            return Limit(steer, MaxForce * 2.0f);
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

    protected Vector2 FleeOne(Vector2 target, float radius)
    {
        Vector2 diff = Pos - target;
        float d = diff.Length();

        if (d < radius)
        {
            float strength = 1f - (d / radius);
            strength = strength * strength + 0.5f;

            diff.Normalize();

            Vector2 desired = diff * MaxSpeed * strength;

            Vector2 steer = desired - _velocity;
            return Limit(steer, MaxForce * 5f);
        }

        return Vector2.Zero;
    }

    protected Vector2 FleeMany(IEnumerable<Boid> enemies, float radius)
    {
        Vector2 total = Vector2.Zero;

        foreach (var e in enemies)
        {
            if (e == this) continue;
            total += FleeOne(e.Pos, radius);
        }

        return total;
    }
    protected Vector2 AvoidOtherSpecies(IEnumerable<Boid> boids, float radius)
    {
        Vector2 total = Vector2.Zero;
        int count = 0;

        foreach (var other in boids)
        {
            if (other == this) continue;
            if (other.Species == this.Species) continue;

            float d = Vector2.Distance(Pos, other.Pos);
            if (d < radius)
            {
                Vector2 diff = Pos - other.Pos;
                if (diff.LengthSquared() > 0)
                    diff.Normalize();

                float strength = 1f - (d / radius);
                diff *= strength * MaxSpeed;

                total += diff;
                count++;
            }
        }

        if (count > 0)
            total /= count;

        return Limit(total, MaxForce * 2.5f);
    }
}

public class RedFish : Boid
{
    public RedFish(Vector2 pos, Texture2D img) : base(pos, img)
    {
        Species = Species.Red;
    }
}

public class PurpleFish : Boid
{
    public PurpleFish(Vector2 pos, Texture2D img) : base(pos, img)
    {
        Species = Species.Purple;
    }
    protected override float Size => 0.03f;
    protected override float MaxSpeed => 6.0f;
    protected override float MaxForce => 0.09f;
    protected override float Neighborhood => 70f;
    protected override float AvoidRadius => 80f;

    protected virtual float CohesionWeight => 0.9f;
    protected virtual float AlignmentWeight => 0.9f;
    protected virtual float SeparationWeight => 1.4f;
    protected virtual float AvoidStrength => 1.5f;
    protected virtual float panic => 2.0f;

    protected override float WanderStrength => 0.01f;

    public override void Update(List<Boid> boids, List<Avoid> avoids, List<Chaser> chasers)
    {
        Vector2 sep = Separation(boids) * SeparationWeight;
        Vector2 align = Alignment(boids) * AlignmentWeight;
        Vector2 coh = Cohesion(boids) * CohesionWeight;

        Vector2 avoid = AvoidPoints(avoids, AvoidRadius) * AvoidStrength;
        Vector2 flee = FleeMany(chasers, FleeRadius) * panic;
        Vector2 avoidOther = AvoidOtherSpecies(boids, AvoidRadius) * AvoidStrength;

        Vector2 wander = RandomDirection() * WanderStrength;

        _acceleration = sep + align + coh + avoid + flee + avoidOther + wander;

        _velocity += _acceleration;
        Move();
        rotation = GetBoidRotation();
    }
}

public class YellowFish : Boid
{
    public YellowFish(Vector2 pos, Texture2D img) : base(pos, img)
    {
        Species = Species.Yellow;
    }
    protected override float Size => 0.065f;

    protected override float MaxSpeed => 2.0f;
    protected override float MaxForce => 0.03f;
    protected override float Neighborhood => 200f;

    protected virtual float CohesionWeight => 1.2f;
    protected virtual float AlignmentWeight => 0.8f;
    protected virtual float SeparationWeight => 1.8f;
    protected virtual float AvoidStrength => 0.9f;

    protected override float WanderStrength => 0.01f;

    protected virtual float AvoidOtherSpeciesRadius => 30f;

    public override void Update(List<Boid> boids, List<Avoid> avoids, List<Chaser> chasers)
    {
        Vector2 sep = Separation(boids) * SeparationWeight;
        Vector2 align = Alignment(boids) * AlignmentWeight;
        Vector2 coh = Cohesion(boids) * CohesionWeight;

        Vector2 avoid = AvoidPoints(avoids, AvoidRadius) * AvoidStrength;
        Vector2 flee = FleeMany(chasers, FleeRadius);

        Vector2 interSpecies = AvoidOtherSpecies(boids, AvoidOtherSpeciesRadius) * AvoidStrength;
        Vector2 wander = RandomDirection() * WanderStrength;

        _acceleration = sep + align + coh + avoid + interSpecies + flee + wander;

        _velocity += _acceleration;
        Move();
        rotation = GetBoidRotation();
    }
}
