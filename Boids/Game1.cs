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
    private Texture2D purpleBoid;
    private Texture2D yellowBoid;
    private Texture2D avoidImg;
    private Texture2D chaserImg;
    private Texture2D background;

    float redTimer = 0f;
    float redInterval = 0.5f;

    float purpleTimer = 0f;
    float purpleInterval = 1.5f;

    float yellowTimer = 0f;
    float yellowInterval = 3f;
    int maxBoids = 100;

    private float eatingRadius = 60f;
    Random rng = new Random();

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        var display = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        _graphics.PreferredBackBufferWidth = display.Width;
        _graphics.PreferredBackBufferHeight = display.Height;

        _graphics.IsFullScreen = true;
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

        boidImg = Content.Load<Texture2D>("red_fish");
        avoidImg = Content.Load<Texture2D>("jelly");
        chaserImg = Content.Load<Texture2D>("chaser");
        background = Content.Load<Texture2D>("background");
        purpleBoid = Content.Load<Texture2D>("purple_fish");
        yellowBoid = Content.Load<Texture2D>("yellow_fish");

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

        bool pressedR = kb.IsKeyDown(Keys.R) && prevKb.IsKeyUp(Keys.R);

        if (leftClick)
        {
            boids.Add(new RedFish(new Vector2(mouseX, mouseY), boidImg));
        }

        if (rightClick)
        {
            avoids.Add(new Avoid(new Vector2(mouseX, mouseY), avoidImg));
        }

        if (pressedE)
        {
            chasers.Add(new Chaser(new Vector2(mouseX, mouseY), chaserImg));
        }
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (boids.Count < maxBoids)
        {
            redTimer += dt;
            if (redTimer >= redInterval)
            {
                redTimer = 0f;
                boids.Add(new RedFish(RandomSpawnPos(), boidImg));
            }

            purpleTimer += dt;
            if (purpleTimer >= purpleInterval)
            {
                purpleTimer = 0f;
                boids.Add(new PurpleFish(RandomSpawnPos(), purpleBoid));
            }

            yellowTimer += dt;
            if (yellowTimer >= yellowInterval)
            {
                yellowTimer = 0f;
                boids.Add(new YellowFish(RandomSpawnPos(), yellowBoid));
            }
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
