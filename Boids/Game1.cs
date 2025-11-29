using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Boids;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    public static int screenWidth;
    public static int screenHeight;

    private List<Boid> boids;
    private List<Avoid> avoids;
    private MouseState prevMouse;

    private Texture2D boidImg;
    private Texture2D avoidImg;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here


        screenWidth = GraphicsDevice.Viewport.Width;
        screenHeight = GraphicsDevice.Viewport.Height;

        boids = new List<Boid>();
        avoids = new List<Avoid>();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        boidImg = Content.Load<Texture2D>("boid");
        avoidImg = Content.Load<Texture2D>("avoid");

    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here


        MouseState mouse = Mouse.GetState();
        int mouseX = mouse.X;
        int mouseY = mouse.Y;

        bool leftClick = mouse.LeftButton == ButtonState.Pressed &&
                 prevMouse.LeftButton == ButtonState.Released;

        bool rightClick = mouse.RightButton == ButtonState.Pressed &&
                 prevMouse.RightButton == ButtonState.Released;

        if (leftClick)
        {
            boids.Add(new Boid(new Vector2(mouseX, mouseY), boidImg));
        }

        if (rightClick)
        {
            avoids.Add(new Avoid(new Vector2(mouseX, mouseY), avoidImg));
        }

        prevMouse = mouse;

        for (int i = 0; i < boids.Count; i++)
        {
            boids[i].Update(boids, avoids);
        }


        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        for (int i = 0; i < boids.Count; i++)
        {
            boids[i].Draw(_spriteBatch);
        }

        for (int i = 0; i < avoids.Count; i++)
        {
            avoids[i].Draw(_spriteBatch);
        }
        _spriteBatch.End();

        // TODO: Add your drawing code here

        base.Draw(gameTime);
    }
}
