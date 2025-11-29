using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Boids;
public class Boid
{
    Vector2 _pos;
    Vector2 _velocity;
    Texture2D _img;
    float rotation;
    float size = 0.4f;
    float halfH;
    float halfW;


    public Boid(Vector2 pos, Texture2D img)
    {
        _pos = pos;
        _velocity = new Vector2(3, 0);
        _img = img;

        halfW = _img.Width * size * 0.5f;
        halfH = _img.Height * size * 0.5f;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_img,
                         _pos,
                         null,
                         Color.White,
                         rotation,
                         Vector2.Zero,
                         size,
                         SpriteEffects.None,
                         0f);
    }

    public void Update()
    {
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

}
