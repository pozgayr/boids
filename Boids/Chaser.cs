using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Boids;

public class Chaser : Boid
{
    public Chaser(Vector2 pos, Texture2D img) : base(pos, img)
    {
        _maxSpeed = 6f;
        size = 0.05f;
    }


    public override void Update(List<Boid> boids, List<Avoid> avoids, List<Chaser> chasers)
    {
        Vector2 chase = Chase(boids);
        Vector2 avoid = AvoidPoints(avoids, _radius);
        Vector2 avoidOther = AvoidPoints(chasers, _radius);

        _acceleration = Limit(chase + avoid, _maxForce * 2f);
        _velocity += _acceleration;
        _velocity = Limit(_velocity, _maxSpeed);


        Move();
        rotation = GetBoidRotation();
    }
    private Vector2 Chase(List<Boid> boids)
    {
        float closest = float.MaxValue;
        Boid target = null;

        foreach (var boid in boids)
        {
            if (boid is Chaser)
            {
                continue;
            }
            float d = Vector2.Distance(Pos, boid.Pos);
            if (d < closest)
            {
                closest = d;
                target = boid;
            }
        }

        if (target == null) return Vector2.Zero;
        return Seek(target.Pos);

    }
}
