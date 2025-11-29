using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Boids;

public class Avoid
{
    public Vector2 _pos;
    Texture2D _img;
    float size = 0.3f;
    Vector2 origin;
    public Avoid(Vector2 pos, Texture2D img)
    {
        _pos = pos;
        _img = img;
        origin = new Vector2(_img.Width * 0.5f, _img.Height * 0.5f);

    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_img,
                          _pos,
                          null,
                          Color.White,
                          0.0f,
                          origin,
                          size,
                          SpriteEffects.None,
                          0f);
    }
}
