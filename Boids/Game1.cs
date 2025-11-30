using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;


namespace Boids;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    public static int screenWidth;
    public static int screenHeight;

    private List<Boid> boids;
    private List<Avoid> avoids;
    private List<Chaser> chasers;
    private MouseState prevMouse;
    private KeyboardState prevKb;

    private Texture2D boidImg;
    private Texture2D avoidImg;
    private Texture2D chaserImg;
    private Texture2D background;

    float spawnTimer = 0f;
    float spawnInterval = 0.5f;
    int maxBoids = 50;

    private float eatingRadius = 50f;
    Random rng = new Random();

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1600;
        _graphics.PreferredBackBufferHeight = 900;

        _graphics.ApplyChanges();
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
        chasers = new List<Chaser>();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        boidImg = Content.Load<Texture2D>("rybka1");
        avoidImg = Content.Load<Texture2D>("jelly");
        chaserImg = Content.Load<Texture2D>("rybka2");
        background = Content.Load<Texture2D>("background");

    }

    Vector2 RandomSpawnPos()
    {
        float x = (float)rng.NextDouble() * Game1.screenWidth;
        float y = (float)rng.NextDouble() * Game1.screenHeight;
        return new Vector2(x, y);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here


        MouseState mouse = Mouse.GetState();
        KeyboardState kb = Keyboard.GetState();
        int mouseX = mouse.X;
        int mouseY = mouse.Y;

        bool leftClick = mouse.LeftButton == ButtonState.Pressed &&
                 prevMouse.LeftButton == ButtonState.Released;

        bool rightClick = mouse.RightButton == ButtonState.Pressed &&
                 prevMouse.RightButton == ButtonState.Released;

        bool pressedE = kb.IsKeyDown(Keys.E) && prevKb.IsKeyUp(Keys.E);

        if (leftClick)
        {
            boids.Add(new Boid(new Vector2(mouseX, mouseY), boidImg));
        }

        if (rightClick)
        {
            avoids.Add(new Avoid(new Vector2(mouseX, mouseY), avoidImg));
        }


        if (pressedE)
        {
            chasers.Add(new Chaser(new Vector2(mouseX, mouseY), chaserImg));
        }
        spawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (spawnTimer >= spawnInterval && boids.Count < maxBoids)
        {
            spawnTimer = 0f;
            boids.Add(new Boid(RandomSpawnPos(), boidImg));
        }

        prevMouse = mouse;
        prevKb = kb;

        for (int i = 0; i < boids.Count; i++)
        {
            boids[i].Update(boids, avoids, chasers);
        }

        for (int i = 0; i < chasers.Count; i++)
        {
            chasers[i].Update(boids, avoids, chasers);
        }

        for (int i = 0; i < avoids.Count; i++)
        {
            avoids[i].Update(boids, avoids, chasers);
        }

        List<Boid> eaten = new();

        foreach (var c in chasers)
        {
            foreach (var b in boids)
            {
                float dist = Vector2.Distance(c.Pos, b.Pos);

                if (dist < eatingRadius)
                {
                    eaten.Add(b);
                }
            }
        }

        foreach (var b in eaten)
        {
            boids.Remove(b);
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        _spriteBatch.Draw(
            background,
            new Rectangle(0, 0, screenWidth, screenHeight),
            Color.White
        );

        for (int i = 0; i < boids.Count; i++)
        {
            boids[i].Draw(_spriteBatch);
        }

        for (int i = 0; i < avoids.Count; i++)
        {
            avoids[i].Draw(_spriteBatch);
        }

        for (int i = 0; i < chasers.Count; i++)
        {
            chasers[i].Draw(_spriteBatch);
        }
        _spriteBatch.End();

        // TODO: Add your drawing code here

        base.Draw(gameTime);
    }
}
