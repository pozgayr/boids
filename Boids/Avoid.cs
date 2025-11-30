using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Boids;

public class Avoid : IPoint
{
    public Vector2 Pos { get; private set; }
    Texture2D _img;
    float size = 0.3f;
    Vector2 origin;

    public Avoid(Vector2 pos, Texture2D img)
    {
        Pos = pos;
        _img = img;
        origin = new Vector2(_img.Width * 0.5f, _img.Height * 0.5f);

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
}
