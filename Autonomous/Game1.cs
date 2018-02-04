﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameTry.GameObjects;

namespace MonoGameTry
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        private Model model;

        private Texture2D texture;

        private List<ViewportWrapper> viewports = new List<ViewportWrapper>();
        private List<GameObject> gameObjects = new List<GameObject>();
        private Car player;
        private Road road;
        private Texture2D metal;

        private BuildingA building;
        private bool collision;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            model = Content.Load<Model>("Low-Poly-Racing-Car");
            metal = Content.Load<Texture2D>("red_metal");

            Road.LoadContent(Content, graphics);

            player = new Car(model, metal);

            road = new Road();

            var buildingModel = Content.Load<Model>("BuildingA");
            BuildingA[] buildings = {
                    new BuildingA(buildingModel, 8f, 30f),
                    new BuildingA(buildingModel, 10f, 60f),
                    new BuildingA(buildingModel, 8f, 70f),
                    new BuildingA(buildingModel, 10f, 90f),
                    new BuildingA(buildingModel, 10f, 100f),
                    new BuildingA(buildingModel, 10f, 120f),
                    new BuildingA(buildingModel, 8f, 130f),
                    new BuildingA(buildingModel, 10f, 145f),
                    new BuildingA(buildingModel, 8f, 150f),

                    new BuildingA(buildingModel, -8f, 30f),
                    new BuildingA(buildingModel, -10f, 60f),
                    new BuildingA(buildingModel, -8f, 70f),
                    new BuildingA(buildingModel, -10f, 90f),
                    new BuildingA(buildingModel, -10f, 100f),
                    new BuildingA(buildingModel, -10f, 120f),
                    new BuildingA(buildingModel, -8f, 130f),
                    new BuildingA(buildingModel, -10f, 145f),
                    new BuildingA(buildingModel, -8f, 150f),

            };

            gameObjects = new List<GameObject>() { road, player };
            gameObjects.AddRange(buildings);
            gameObjects.AddRange(GenerateInitialCarAgents());

            gameObjects.ForEach(go => go.Initialize());

            int numViewports = 1;
            int width = graphics.PreferredBackBufferWidth / numViewports;
            int height = graphics.PreferredBackBufferHeight;
            int x = 0;

            viewports.Add(new GameObjectViewport(x, 0, width, height, player));
            x += width;
            viewports.Add(new BirdsEyeViewport(x, 0, width, height));
        }

        private IEnumerable<GameObject> GenerateInitialCarAgents()
        {
            var vanModel = Content.Load<Model>("kendo");
            var busModel = Content.Load<Model>("bus");

            const float laneWidth = 3f;
            const float vanWidth = 2f;
            for (int i = 0; i < 10; i++)
            {
                var van = new CarAgent(vanModel, 90, vanWidth, false)
                {
                    VY = 70f / 3.6f,
                    MaxVY = 120f / 3.6f,
                    X = laneWidth / 2,
                    Y = i * 100
                };
                yield return van;

                van = new CarAgent(vanModel, 90, vanWidth, true)
                {
                    VY = 70f / 3.6f,
                    MaxVY = 120f / 3.6f,
                    X = -laneWidth / 2,
                    Y = i * 200
                };
                yield return van;

                var bus = new CarAgent(busModel, 180f, 2.6f, false)
                {
                    VY = 50f / 3.6f,
                    MaxVY = 100f / 3.6f,
                    X = laneWidth * 1.45f,
                    Y = i * 100 + 20
                };

                yield return bus;

                bus = new CarAgent(busModel, 180f, 2.6f, true)
                {
                    VY = 50f / 3.6f,
                    MaxVY = 100f / 3.6f,
                    X = -laneWidth * 1.45f,
                    Y = i * 200 + 20
                };

                yield return bus;

            }
        }
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            gameObjects.ForEach(go => go.Update(gameTime.ElapsedGameTime));


            collision = (gameObjects.OfType<CarAgent>().Any(x => CollisionDetector.IsCollision(x, player))) ;
            if (!collision)
            {
                if (player.X - player.Width / 2 < -6)
                    collision = true;
                if (player.X + player.Width / 2 > 6)
                    collision = true;
            }
            viewports.ForEach(vp => vp.Update());
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(collision ? Color.Red : Color.CornflowerBlue);

            Viewport original = graphics.GraphicsDevice.Viewport;

            foreach (var viewport in viewports)
            {
                graphics.GraphicsDevice.Viewport = viewport.Viewport;
                Draw(gameTime, viewport.View, viewport.Projection);
            }

            graphics.GraphicsDevice.Viewport = original;

            base.Draw(gameTime);
        }

        private void Draw(GameTime gameTime, Matrix view, Matrix projection)
        {
            gameObjects.ForEach(go => go.Draw(gameTime.ElapsedGameTime, view, projection, GraphicsDevice));
        }

    }
}
