////////////////////////////////////////////////////////////////
// Copyright 2013, CompuScholar, Inc.
//
// This source code is for use by the students and teachers who 
// have purchased the corresponding TeenCoder or KidCoder product.
// It may not be transmitted to other parties for any reason
// without the written consent of CompuScholar, Inc.
// This source is provided as-is for educational purposes only.
// CompuScholar, Inc. makes no warranty and assumes
// no liability regarding the functionality of this program.
//
////////////////////////////////////////////////////////////////

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
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.Timers;

using SpriteLibrary;

namespace Swarm
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Swarm : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        // the following constants control how many of each type of sprite are possible
        const int NUM_COLS = 5;
        const int NUM_BEES_PER_COL = 4;
        const int MAX_BEE_SHOTS = 5;
        const int MAX_SMOKE_SHOTS = 2;

        const int NUM_HIVES = 3;
        const int NUM_HONEYCOMBS = 20;

        // initialize a new random number generator
        Random randomNumGen = new Random(DateTime.Now.Millisecond);

        // font used to display user messages
        SpriteFont gameFont;

        // textures used in the game
        Texture2D beeTexture;
        Texture2D beeShotTexture;
        Texture2D beeStripTexture;
        Texture2D hive1Texture;
        Texture2D hive2Texture;
        Texture2D smokeTexture;
        Texture2D smokeSprayerTexture;
        Texture2D smokeSprayerStripTexture;
        Texture2D smokeStripTexture;

        SoundEffect buzz;
        SoundEffect explosion;
        SoundEffect smokeShot;
        SoundEffect stingerShot;

        SoundEffectInstance buzzInstance;


        // the following members form the Game State for this program!

        // this sprite represents the player at the bottom
        Sprite smokeSprayer;
        
        // these sprites represent the smoke balls fired from the smoke gun
        LinkedList<Sprite> smokeShots = new LinkedList<Sprite>();

        // these sprites represent the individual honeycomb sections of the hive bunkers
        LinkedList<Sprite> hiveSections = new LinkedList<Sprite>();

        // these sprites represent the bees in the swarm
        LinkedList<Sprite> bees = new LinkedList<Sprite>();

        // these sprites represent the stinger shots fired from the bees
        LinkedList<Sprite> beeShots = new LinkedList<Sprite>();

        // the time the most recent bee shot was fired
        double lastBeeShotFired = 0;

        // the current bee movement speed
        float beeSpeed = 1.0f;
        int smokeSprayerSpeed = 2;
        int projectileSpeedPlus = 4;
        int projectileSpeedMinus = -4;

        float BeeShotWait = 1000;
        float SmokeShotWait = 1000;
        float SmokeShotWaitDefault = 1000;
        // if true, the current game is over
        bool gameOver = false;

        // this string contains the message that is currently displayed to the user
        String displayMessage = "";
        bool Won = false;

        // previous keyboard state
        KeyboardState oldKeyboardState;

        //previous Xbox controller state
        GamePadState oldGamePadState;

        public Swarm()
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
        // This method is provided fully complete as part of the activity starter.
        protected override void Initialize()
        {
            // call base.Initialize() first to get LoadContent called
            // (and therefore all textures initialized) prior to starting game!
            base.Initialize();

            startGame();
        }

        // This method is provided partially complete as part of the activity starter.
        private void startGame()
        {
            // reset game over flag
            gameOver = false;

            // create new smoke sprayer, bees, and hives
            initializeSmokeSprayer();
            initializeBees();
            initializeHives();

            // clear all prior shots
            smokeShots.Clear();
            beeShots.Clear();

            buzzInstance = buzz.CreateInstance();
            buzzInstance.IsLooped = true;
            buzzInstance.Volume = 0.1f;
            buzzInstance.Play();

        }


        // The student will complete this function as part of an activity
        private void initializeSmokeSprayer()
        {

            smokeSprayer = new Sprite();

            smokeSprayer.SetTexture(smokeSprayerStripTexture, 5);

            smokeSprayer.ContinuousAnimation = false;
            smokeSprayer.AnimationInterval = 100;

            smokeSprayer.UpperLeft.X = GraphicsDevice.Viewport.Width / 2 - 50;
            smokeSprayer.UpperLeft.Y = GraphicsDevice.Viewport.Height - 80;

            smokeSprayer.IsAlive = true;

        }

        // This method is provided fully complete as part of the activity starter.
        private void initializeHives()
        {
            // this method will initialize all of the little hive sections as
            // individual sprites that can be destroyed by shots and bees
            hiveSections.Clear();

            // pick starting Y location
            float hiveStartingY = GraphicsDevice.Viewport.Height - 150;
            if (smokeSprayer != null)
            	hiveStartingY = smokeSprayer.UpperLeft.Y - 100;
            
            // spacing between hives
            float hiveSpacing = 200;

            // for each hive
            for (int i = 0; i < NUM_HIVES; i++)
            {
                // for each honeycomb in the hive
                for (int j = 0; j < NUM_HONEYCOMBS; j++)
                {
                    // create a new sprite
                    Sprite hiveSection = new Sprite();

                    // alternate the colors on odd/even blocks
                    if (j % 2 == 0)
                        hiveSection.SetTexture(hive1Texture);
                    else
                        hiveSection.SetTexture(hive2Texture);

                    // first 8 squares go along the bottom
                    if (j < 8)
                    {
                        hiveSection.UpperLeft.Y = hiveStartingY + 3 * hiveSection.GetHeight();
                        hiveSection.UpperLeft.X = ((hiveSpacing * (i + 1)) + (j * hiveSection.GetWidth()));
                    }
                    // next 6 squares go along the middle
                    else if (j < 14)
                    {
                        hiveSection.UpperLeft.Y = hiveStartingY + 2 * hiveSection.GetHeight();
                        hiveSection.UpperLeft.X = ((hiveSpacing * (i + 1)) + ((j - 7) * hiveSection.GetWidth()));
                    }
                    // next 4 squares along the top
                    else if (j < 18)
                    {
                        hiveSection.UpperLeft.Y = hiveStartingY + hiveSection.GetHeight();
                        hiveSection.UpperLeft.X = ((hiveSpacing * (i + 1)) + ((j - 12) * hiveSection.GetWidth()));
                    }
                    // small group of squares at the peak
                    else
                    {
                        hiveSection.UpperLeft.Y = hiveStartingY;
                        hiveSection.UpperLeft.X = ((hiveSpacing * (i + 1)) + ((j - 15) * hiveSection.GetWidth()));
                    }

                    // set hive section to alive and add to list
                    hiveSection.IsAlive = true;

                    hiveSections.AddLast(hiveSection);
                }
            }
        }


        // The student will complete this function as part of an activity
        private void initializeBees()
        {

            bees.Clear();

            for(int i = 0; i < NUM_COLS; i++)
            {
                for (int i1 = 0; i1 < NUM_BEES_PER_COL; i1++)
                {

                    Sprite bee = new Sprite();

                    bee.UpperLeft.Y = i1 * 75;
                    bee.UpperLeft.X = 100 + i * 100;

                    bee.SetTexture(beeStripTexture, 5);

                    int StartingFrame = randomNumGen.Next(0, 4);
                    bee.setCurrentFrame(StartingFrame);

                    bee.SetSpeedAndDirection(beeSpeed, 0);

                    bees.AddLast(bee);

                }
            }

        }

        // The student will complete this function as part of an activity
        private void stopGame(bool won, String message)
        {
            displayMessage = message;
            gameOver = true;

            buzzInstance.Stop();

            if (won == true)
            {
                beeSpeed += 1;
                smokeSprayerSpeed += 2;
                projectileSpeedPlus += 2;
                projectileSpeedMinus -= 2;

                BeeShotWait -= 100;
                SmokeShotWait -= 100;
                SmokeShotWaitDefault -= 100;

                Won = true;

                if (BeeShotWait == 0)
                    if (SmokeShotWait == 0)
                        if (SmokeShotWaitDefault == 0)
                            displayMessage = "You win!";
            }
            if (won == false)
            {
                beeSpeed -= 2;
                projectileSpeedPlus -= 2;
                projectileSpeedMinus += 2;

                BeeShotWait += 100;
                SmokeShotWait += 100;
                SmokeShotWaitDefault += 100;

                Won = false;

                if (beeSpeed <= 0)
                    beeSpeed = 1;

                if (smokeSprayerSpeed == 0)
                    smokeSprayerSpeed = 1;

                if (projectileSpeedPlus == 0)
                    projectileSpeedPlus = 2;

                if (projectileSpeedMinus == 0)
                    projectileSpeedMinus = -2;
            }
        }



        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        // This method is provided partially complete as part of the activity starter.
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // load all textures used in the game
            beeTexture = Content.Load<Texture2D>("Images\\Bee");
            beeShotTexture = Content.Load<Texture2D>("Images\\BeeShot");
            beeStripTexture = Content.Load<Texture2D>("Images\\BeeStrip");
            hive1Texture = Content.Load<Texture2D>("Images\\Hive1");
            hive2Texture = Content.Load<Texture2D>("Images\\Hive2");
            smokeTexture = Content.Load<Texture2D>("Images\\Smoke");
            smokeSprayerTexture = Content.Load<Texture2D>("Images\\SmokeSprayer");
            smokeSprayerStripTexture = Content.Load<Texture2D>("Images\\SmokeSprayerStrip");
            smokeStripTexture = Content.Load<Texture2D>("Images\\SmokeStrip");

            buzz = Content.Load<SoundEffect>("Audio\\Buzz");
            explosion = Content.Load<SoundEffect>("Audio\\explosion");
            smokeShot = Content.Load<SoundEffect>("Audio\\smokeShot");
            stingerShot = Content.Load<SoundEffect>("Audio\\stingerShot");

            // load all fonts used in the game
            gameFont = Content.Load<SpriteFont>("GameFont");

            
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        // This method is provided fully complete as part of the activity starter.
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        // This method is provided fully complete as part of the activity starter.
        protected override void Update(GameTime gameTime)
        {

            float Elapsed = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            SmokeShotWait -= Elapsed;

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // if the game is still going on
            if (!gameOver)
            { 
                // check to see if the player has beaten all the bees
                if (bees.Count == 0)
                {
                    int WinNum = randomNumGen.Next(0, 2);

                    if (WinNum == 0)
                        stopGame(true,"You can haz all the honey!");
                    if (WinNum == 1)
                        stopGame(true, "G-G-G-G-GENOCIDE!!!");
                    if (WinNum == 2)
                        stopGame(true, "Congrats!");
                }
                else
                {

                    // see if any more bee stingers need to be fired
                    checkBeeShots(gameTime);

                    // move the bees
                    moveBees(gameTime);

                    // move any bee shots that are on the screen
                    moveBeeShots();

                    // move any smoke shots that are on the screen
                    moveSmokeShots(gameTime);

                    // handle all collisions
                    checkCollisions();
                }
            }

            // if the smoke sprayer is currently animating, advance the frame
            if (smokeSprayer != null)
            	if (smokeSprayer.IsAnimating())
                	smokeSprayer.Animate(gameTime);

            // handle all user input
            handleKeyPress();

            // Optionally handle Xbox gamepad
            handleXboxGamepad();
            
            base.Update(gameTime);
        }

        // The student will complete this function as part of an activity
        private void checkBeeShots(GameTime gameTime)
        {
            if (gameTime.TotalGameTime.TotalMilliseconds > lastBeeShotFired + BeeShotWait)
            {
                fireBeeShot();
                lastBeeShotFired = gameTime.TotalGameTime.TotalMilliseconds;
            }
        }

        // The student will complete this function as part of an activity
        private void fireBeeShot()
        {
            int BeeNumber = randomNumGen.Next(0, bees.Count);
            Sprite Bee = bees.ElementAt(BeeNumber);

            Sprite NewBeeShot = new Sprite();

            NewBeeShot.SetTexture(beeShotTexture);

            NewBeeShot.SetVelocity(0, projectileSpeedPlus);

            NewBeeShot.IsAlive = true;

            NewBeeShot.UpperLeft.X = Bee.UpperLeft.X + Bee.GetCenter().X - NewBeeShot.GetWidth() / 2;
            NewBeeShot.UpperLeft.Y = Bee.UpperLeft.Y + Bee.GetHeight();

            stingerShot.Play(0.25f, 0, 0);

            beeShots.AddLast(NewBeeShot);
        }

        // This method is provided fully complete as part of the activity starter.
        private Sprite findEdgeBee(bool leftSide)
        {
            // create variable to trach the bee we think is on the edge
            Sprite edgeSprite = null;

            // check all remaining bees
            foreach (Sprite bee in bees)
            {
                // if this is the first bee, just save it as current edge bee
                if (edgeSprite == null)
                    edgeSprite = bee;
                else
                {
                    // if we are looking for the leftmost bee
                    if (leftSide)
                    {
                        // if this bee is to left of current edge bee
                        if (bee.UpperLeft.X < edgeSprite.UpperLeft.X)
                            edgeSprite = bee;   // we have a new edge bee
                    }
                    else // we are looking for the rightmost bee
                    {
                        // if this bee is to right of current edge bee
                        if (bee.UpperLeft.X > edgeSprite.UpperLeft.X)
                            edgeSprite = bee;   // we have a new edge bee
                    }
                }
            }
            
            // return the edge bee we found
            return edgeSprite;
        }

        // The student will complete this function as part of an activity
        private void moveSmokeShots(GameTime gameTime)
        {
            foreach (Sprite NewSmokeShot in smokeShots)
            {
                NewSmokeShot.Animate(gameTime);

                NewSmokeShot.MoveAndVanish(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            }
        }

        // The student will complete this function as part of an activity
        private void moveBeeShots()
        {
            foreach (Sprite NewBeeShot in beeShots)
                NewBeeShot.MoveAndVanish(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        }

        // The student will complete this function as part of an activity
        private void moveBees(GameTime gameTime)
        {

            Sprite LeftMostBee = findEdgeBee(true);
            Sprite RightMostBee = findEdgeBee(false);

            if ((LeftMostBee == null) || (RightMostBee == null))
                return;


            if (((LeftMostBee.UpperLeft.X) < 0) ||
                 ((RightMostBee.UpperLeft.X + RightMostBee.GetWidth()) > GraphicsDevice.Viewport.Width))
            {
                foreach (Sprite bee in bees)
                {
                    Vector2 CurrentVelocity = bee.GetVelocity();

                    Double VelocityX = CurrentVelocity.X;
                    Double VelocityY = CurrentVelocity.Y;

                    VelocityX = VelocityX * -1;

                    bee.SetVelocity(VelocityX, VelocityY);

                    bee.UpperLeft.Y = bee.UpperLeft.Y + 25;
                }
            }

            foreach (Sprite bee in bees)
            {

                bee.Animate(gameTime);

                bee.Move();

                if (bee.UpperLeft.Y >= smokeSprayer.UpperLeft.Y)
                {

                    smokeSprayer.StartAnimationShort(1, 4, 4);

                    int LoseNum = randomNumGen.Next(0, 2);
                    if (LoseNum == 0)
                        stopGame(false, "That looked like it hurt...");
                    if (LoseNum == 1)
                        stopGame(false, "Shall I get some vinegar?");
                    if (LoseNum == 2)
                        stopGame(false, "Bees can have all your life!");
                }
            }

        }

        // The student will complete this function as part of an activity
        private void checkCollisions()
        {

            // remove all dead sprites from their lists
            pruneList(beeShots);
            pruneList(smokeShots);
            pruneList(bees);
            pruneList(hiveSections);

            foreach (Sprite Bee in bees)
                foreach (Sprite SmokeShot in smokeShots)
                    if (Bee.IsCollided(SmokeShot))
                    {
                        Bee.IsAlive = false;
                        SmokeShot.IsAlive = false;

                        explosion.Play(1, 0, 0);
                    }

            foreach (Sprite Honeycomb in hiveSections)
            {
                foreach (Sprite BeeShot in beeShots)
                    if (Honeycomb.IsCollided(BeeShot))
                    {
                        Honeycomb.IsAlive = false;
                        BeeShot.IsAlive = false;

                        explosion.Play(1, 0, 0);
                    }
                foreach (Sprite SmokeShot in smokeShots)
                    if (Honeycomb.IsCollided(SmokeShot))
                    {
                        Honeycomb.IsAlive = false;
                        SmokeShot.IsAlive = false;

                        explosion.Play(1, 0, 0);
                    }
                foreach (Sprite Bee in bees)
                    if (Honeycomb.IsCollided(Bee))
                    {
                        Honeycomb.IsAlive = false;

                        explosion.Play(1, 0, 0);
                    }
            }
            foreach (Sprite BeeShot in beeShots)
            {
                if (BeeShot.IsCollided(smokeSprayer))
                {
                    smokeSprayer.StartAnimationShort(1, 4, 4);

                    int LoseNum = randomNumGen.Next(0, 2);
                    if (LoseNum == 0)
                        stopGame(false, "That looked like it hurt...");
                    if (LoseNum == 1)
                        stopGame(false, "Shall I get some vinegar?");
                    if (LoseNum == 2)
                        stopGame(false, "Bees can have all your life!");

                    explosion.Play(1, 0, 0);
                }
            }
        }

        // The student will complete this function as part of an activity
        private void handleKeyPress()
        {

            GamePadState CurrentGamePadState = GamePad.GetState(PlayerIndex.One);
            if (CurrentGamePadState.IsConnected)
                return;

            KeyboardState CurrentKeyboardState = Keyboard.GetState();
            if (oldKeyboardState == null)
                oldKeyboardState = CurrentKeyboardState;

            if (CurrentKeyboardState.IsKeyDown(Keys.Space))
            {
                if (oldKeyboardState.IsKeyUp(Keys.Space))
                    if (CurrentKeyboardState.IsKeyDown(Keys.Space))
                        if (gameOver == true)
                            startGame();

                if (SmokeShotWait <= 0)
                {

                    shootSmokeSprayer();

                    smokeShot.Play(1, 0, 0);

                    SmokeShotWait = SmokeShotWaitDefault;

                }
            }

            if (CurrentKeyboardState.IsKeyDown(Keys.A))
            {
                if (gameOver == true)
                    return;

                if (smokeSprayer.UpperLeft.X > 0)
                    smokeSprayer.UpperLeft.X = smokeSprayer.UpperLeft.X - smokeSprayerSpeed;
            }

            if (CurrentKeyboardState.IsKeyDown(Keys.Left))
            {
                if (gameOver == true)
                    return;

                if (smokeSprayer.UpperLeft.X > 0)
                    smokeSprayer.UpperLeft.X = smokeSprayer.UpperLeft.X - smokeSprayerSpeed;
            }

            Vector2 smokeSprayerRight = new Vector2();
            smokeSprayerRight.X = smokeSprayer.UpperLeft.X + smokeSprayer.GetWidth();

            if (CurrentKeyboardState.IsKeyDown(Keys.D))
            {
                if (gameOver == true)
                    return;

                if (smokeSprayerRight.X < GraphicsDevice.Viewport.Width)
                    smokeSprayer.UpperLeft.X = smokeSprayer.UpperLeft.X + smokeSprayerSpeed;
            }

            if (CurrentKeyboardState.IsKeyDown(Keys.Right))
            {
                if (gameOver == true)
                    return;

                if (smokeSprayerRight.X < GraphicsDevice.Viewport.Width)
                    smokeSprayer.UpperLeft.X = smokeSprayer.UpperLeft.X + smokeSprayerSpeed;
            }
           
            oldKeyboardState = CurrentKeyboardState;
        }

        //*************************************************************************************************************
        // OPTIONAL: If the student is using Xbox gamepads, they can optionally complete this method
        // If not, they should complete the handleKeyPress() method mentioned above.

        private void handleXboxGamepad()
        {

            GamePadState CurrentGamePadState = GamePad.GetState(PlayerIndex.One);
            if (CurrentGamePadState.IsConnected == false)
                return;
            
            if (oldGamePadState == null)
                oldGamePadState = CurrentGamePadState;

            if (CurrentGamePadState.IsButtonDown(Buttons.A))
            {
                if (oldGamePadState.IsButtonUp(Buttons.A))
                    if (CurrentGamePadState.IsButtonDown(Buttons.A))
                        if (gameOver == true)
                            startGame();

                if (SmokeShotWait <= 0)
                {

                    shootSmokeSprayer();

                    smokeShot.Play(1, 0, 0);

                    SmokeShotWait = SmokeShotWaitDefault;

                }
            }

            if (CurrentGamePadState.ThumbSticks.Left.X <= -0.1)
            {
                if (gameOver == true)
                    return;

                if (smokeSprayer.UpperLeft.X > 0)
                    smokeSprayer.UpperLeft.X = smokeSprayer.UpperLeft.X - smokeSprayerSpeed;
            }

            Vector2 smokeSprayerRight = new Vector2();
            smokeSprayerRight.X = smokeSprayer.UpperLeft.X + smokeSprayer.GetWidth();

            if (CurrentGamePadState.ThumbSticks.Left.X >= 0.1)
            {
                if (gameOver == true)
                    return;

                if (smokeSprayerRight.X < GraphicsDevice.Viewport.Width)
                    smokeSprayer.UpperLeft.X = smokeSprayer.UpperLeft.X + smokeSprayerSpeed;
            }

            oldGamePadState = CurrentGamePadState;
        }
        //*****************************************************************************************************************



        // The student will complete this function as part of an activity
        private void shootSmokeSprayer()
        {

            Sprite NewSmokeShot = new Sprite();

            NewSmokeShot.SetTexture(smokeStripTexture, 4);

            NewSmokeShot.SetVelocity(0, projectileSpeedMinus);

            NewSmokeShot.IsAlive = true;

            NewSmokeShot.UpperLeft.X = smokeSprayer.UpperLeft.X + smokeSprayer.GetCenter().X - NewSmokeShot.GetWidth() / 2;
            NewSmokeShot.UpperLeft.Y = smokeSprayer.UpperLeft.Y + smokeSprayer.GetCenter().Y - smokeSprayer.GetHeight();

            smokeShots.AddLast(NewSmokeShot);

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        // The student will complete this function as part of an activity
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.SkyBlue);

            spriteBatch.Begin();

            // draw the smoke sprayer
            if (smokeSprayer != null)
            	smokeSprayer.Draw(spriteBatch);

            // draw all active smoke shots
            foreach (Sprite smokeShot in smokeShots)
            {
                smokeShot.Draw(spriteBatch);
            }

            // draw all active bee shots
            foreach (Sprite beeShot in beeShots)
            {
                beeShot.Draw(spriteBatch);
            }

            // draw all active bees
            foreach (Sprite bee in bees)
            {
                bee.Draw(spriteBatch);
            }

            // draw all remaining hive sections
            foreach (Sprite hiveSection in hiveSections)
            {
                hiveSection.Draw(spriteBatch);
            }

            if (gameOver == true)
            {
                if (Won == true)
                    spriteBatch.DrawString(gameFont, "You win!!!", new Vector2(20, 20), Color.Black);
                if (Won == false)
                    spriteBatch.DrawString(gameFont, "You lose!!!", new Vector2(20, 20), Color.Black);

                spriteBatch.DrawString(gameFont, displayMessage, new Vector2(20, 40), Color.Black);

                GamePadState CurrentGamePadState = GamePad.GetState(PlayerIndex.One);
                if (CurrentGamePadState.IsConnected == false)
                    spriteBatch.DrawString(gameFont, "Press SPACE to try again!", new Vector2(20, 60), Color.Black);
                if (CurrentGamePadState.IsConnected == true)
                    spriteBatch.DrawString(gameFont, "Press A to try again!", new Vector2(20, 60), Color.Black);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }


        // This method is provided fully complete as part of the activity starter.
        private void pruneList(LinkedList<Sprite> spriteList)
        {
            // clip out any sprites from the list where IsAlive = false
            for (int i = spriteList.Count - 1; i >= 0; i--)
            {
                Sprite s = spriteList.ElementAt(i);
                if (!s.IsAlive)
                {
                    spriteList.Remove(s);
                }
            }
        }


    }
}
