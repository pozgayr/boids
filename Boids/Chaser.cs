using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Boids;

public class Chaser : Boid
{
    public Chaser(Vector2 pos, Texture2D img) : base(pos, img) { }

    protected override float MaxSpeed => 6f;
    protected override float Size => 0.08f;

    public override void Update(List<Boid> boids, List<Avoid> avoids, List<Chaser> chasers)
    {
        Vector2 chase = Chase(boids);
        Vector2 avoid = AvoidPoints(avoids, AvoidRadius);
        Vector2 avoidOther = AvoidPoints(chasers, AvoidRadius);

        _acceleration = Limit(chase + avoid, MaxForce * 2f);
        _velocity += _acceleration;
        _velocity = Limit(_velocity, MaxSpeed);


        Move();
        rotation = GetBoidRotation();
    }
    private Vector2 Chase(List<Boid> boids)
    {
        float bestScore = float.MaxValue;
        Boid target = null;

        foreach (var boid in boids)
        {
            if (boid is Chaser) continue;

            float d = Vector2.Distance(Pos, boid.Pos);

            float weight = boid.Species switch
            {
                Species.Yellow => 6.0f,
                Species.Purple => 2.0f,
                Species.Red => 1.0f,
                _ => 1.0f
            };

            float score = d * weight;

            if (score < bestScore)
            {
                bestScore = score;
                target = boid;
            }
        }

        if (target == null) return Vector2.Zero;

        return Seek(target.Pos);
    }
}
