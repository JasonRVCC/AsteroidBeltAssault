using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Asteroid_Belt_Assault
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        enum GameStates { TitleScreen, Playing, PlayerDead, GameOver };
        GameStates gameState = GameStates.TitleScreen;
        
        Texture2D titleScreen;
        Texture2D spriteSheet;

        StarField starField;
        AsteroidManager asteroidManager;
        PlayerManager playerManager;
        EnemyManager enemyManager;
        ExplosionManager explosionManager;
        CollisionManager collisionManager;

        SpriteFont pericles14;

        private float playerDeathDelayTime = 6f;
        private float playerDeathTimer = 0f;
        private float titleScreenTimer = 0f;
        private float titleScreenDelayTime = 1f;

        private int playerStartingLives = 3;
        private Vector2 playerStartLocation = new Vector2(390, 550);
        private Vector2 scoreLocation = new Vector2(20, 10);
        private Vector2 livesLocation = new Vector2(20, 25);

        //Added random game over text
        Random rand = new Random();
        private string gameOverText = "G A M E  O V E R !";

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
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            titleScreen = Content.Load<Texture2D>(@"Textures\TitleScreen");
            spriteSheet = Content.Load<Texture2D>(@"Textures\SpriteSheet");

            starField = new StarField(this.Window.ClientBounds.Width, this.Window.ClientBounds.Height,
                200, new Vector2(0, 30f), spriteSheet, new Rectangle(0, 450, 2, 2));

            asteroidManager = new AsteroidManager(10, spriteSheet, new Rectangle(0, 0, 50, 50), 20,
                this.Window.ClientBounds.Width, this.Window.ClientBounds.Height);

            playerManager = new PlayerManager(spriteSheet, new Rectangle(0, 150, 50, 50), 3,
                new Rectangle(0, 0, this.Window.ClientBounds.Width, this.Window.ClientBounds.Height));

            enemyManager = new EnemyManager(spriteSheet, new Rectangle(0,200,50,50), 6, playerManager,
                new Rectangle(0, 0, this.Window.ClientBounds.Width, this.Window.ClientBounds.Height));

            explosionManager = new ExplosionManager(spriteSheet, new Rectangle(0, 100, 50, 50), 3,
                new Rectangle(0, 450, 2, 2));

            collisionManager = new CollisionManager(asteroidManager, playerManager, enemyManager, 
                explosionManager);

            pericles14 = Content.Load<SpriteFont>(@"Fonts\Pericles14");

            SoundManager.Initialize(Content);

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
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
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            switch (gameState)
            {
                case GameStates.TitleScreen:
                    titleScreenTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if(titleScreenTimer >= titleScreenDelayTime)
                    {
                        if((Keyboard.GetState().IsKeyDown(Keys.Space)) || 
                            (GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed))
                        {
                            playerManager.livesRemaining = playerStartingLives;
                            playerManager.playerScore = 0;
                            resetGame();
                            gameState = GameStates.Playing;
                        }
                    }
                    break;

                case GameStates.Playing:
                    starField.Update(gameTime);
                    asteroidManager.Update(gameTime);
                    playerManager.Update(gameTime);
                    enemyManager.Update(gameTime);
                    explosionManager.Update(gameTime);
                    collisionManager.CheckCollisions();

                    if (playerManager.destroyed)
                    {
                        playerDeathTimer = 0f;
                        enemyManager.active = false;
                        playerManager.livesRemaining--;

                        if (playerManager.livesRemaining < 0)
                        {
                            switch (rand.Next(0,9))
                            {
                                case 0: case 6: case 7: case 8: case 9:
                                    gameOverText = "G A M E  O V E R !";
                                    break;
                                case 1:
                                    gameOverText = "Y E R  D E A D !";
                                    break;
                                case 2:
                                    gameOverText = "N O O O O O O O O O O O O O O O O !";
                                    break;
                                case 3:
                                    gameOverText = "M I S S I O N  F A I L E D";
                                    break;
                                case 4:
                                    gameOverText = "F I S S I O N  M A I L E D";
                                    break;
                                case 5:
                                    gameOverText = "T R Y  A G A I N ?";
                                    break;
                            }
                            gameState = GameStates.GameOver;
                        }
                        else
                        { gameState = GameStates.PlayerDead; }
                    }
                    break;

                case GameStates.PlayerDead:
                    playerDeathTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    starField.Update(gameTime);
                    asteroidManager.Update(gameTime);
                    enemyManager.Update(gameTime);
                    playerManager.PlayerShotManager.Update(gameTime);
                    explosionManager.Update(gameTime);

                    if (playerDeathTimer >= playerDeathDelayTime)
                    {
                        resetGame();
                        gameState = GameStates.Playing;
                    }
                    break;

                case GameStates.GameOver:
                    playerDeathTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    starField.Update(gameTime);
                    asteroidManager.Update(gameTime);
                    enemyManager.Update(gameTime);
                    playerManager.PlayerShotManager.Update(gameTime);
                    explosionManager.Update(gameTime);

                    if (playerDeathTimer >= playerDeathDelayTime)
                    {
                        gameState = GameStates.TitleScreen;
                    }
                    break;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            if (gameState == GameStates.TitleScreen)
            {
                spriteBatch.Draw(titleScreen, new Rectangle(0, 0, this.Window.ClientBounds.Width,
                    this.Window.ClientBounds.Height), Color.White);
            }

            if ((gameState == GameStates.Playing) || (gameState == GameStates.PlayerDead) ||
                (gameState == GameStates.GameOver))
            {
                starField.Draw(spriteBatch);
                asteroidManager.Draw(spriteBatch);
                playerManager.Draw(spriteBatch);
                enemyManager.Draw(spriteBatch);
                explosionManager.Draw(spriteBatch);

                spriteBatch.DrawString(pericles14, "Score: " + playerManager.playerScore.ToString(),
                    scoreLocation, Color.White);

                if (playerManager.livesRemaining >= 0)
                {
                    spriteBatch.DrawString(pericles14, "Ships Remaining: " +
                        playerManager.livesRemaining.ToString(), livesLocation, Color.White);
                }
            }

            if (gameState == GameStates.GameOver)
            {
                spriteBatch.DrawString(pericles14, gameOverText, 
                    new Vector2(this.Window.ClientBounds.Width / 2 -
                        pericles14.MeasureString(gameOverText).X / 2, 50), Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void resetGame()
        {
            playerManager.playerSprite.Location = playerStartLocation;
            foreach (Sprite asteroid in asteroidManager.Asteroids)
            {asteroid.Location = new Vector2 (-500,-500);}
            
            enemyManager.Enemies.Clear();
            enemyManager.active = true;
            playerManager.PlayerShotManager.Shots.Clear();
            enemyManager.EnemyShotManager.Shots.Clear();
            playerManager.destroyed=false;
        }

    }
}
