using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;


namespace first3D
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Model player;
        Model barrel;
        Model grass;
        Model ground;
        Vector3 playerPos;
        //two options when adding player rotation
        //simple one only allows left/right rotation
        float playerRot;

        //the more complex case allows rotation in all axes
        Vector3 playerDir;
        Vector3 playerUp;

        //the fixed offset (player -> camera)
        Vector3 camOffset;

        List<Vector3> barrelPos;
        List<Vector3> bluffPos;

        List<Enemy> enemies;
        
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
            //not in world position
            //with respect to player position
            camOffset = new Vector3(0, 5, -25);
            playerRot = 0;

            barrelPos = new List<Vector3>();

            enemies = new List<Enemy>();

            for(int e = 0; e < 500; e++)
            {
                enemies.Add(new Enemy());
            }

            //set up all the barrels
            /* barrelPos.Add(new Vector3(100, 0, 0));
             barrelPos.Add(new Vector3(20, 0, 0));
             barrelPos.Add(new Vector3(120, 0, 0));
             barrelPos.Add(new Vector3(200, 0, 0));
             barrelPos.Add(new Vector3(210, 0, 0));
             barrelPos.Add(new Vector3(300, 0, 0));
             barrelPos.Add(new Vector3(400, 0, 0));
 */

            //not needed when reading from XML file
            /*bluffPos = new List<Vector3>();

            for (int i = 0; i < 15; i++)
            {
                bluffPos.Add(new Vector3(i * 50, -38, -2));
            }
            */
        
            

            XmlSerializer bluffSerializer
                = new XmlSerializer(typeof(List<Vector3>));

            #region write xml to disk
            /*TextWriter bluffWriter = new StreamWriter("bluff.xml");
            bluffSerializer.Serialize(bluffWriter, bluffPos);
            bluffWriter.Close();
            */
            #endregion
            
            #region read xml from disk
            TextReader bluffReader = new StreamReader("bluff.xml");
            bluffPos = (List<Vector3>)bluffSerializer.Deserialize(bluffReader);
            bluffReader.Close();
            
            #endregion

            playerPos = Vector3.Zero;
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

            player = Content.Load<Model>("Animal_Rigged_Zebu_01");
            barrel = Content.Load<Model>("Barrel_Sealed_01");
            grass = Content.Load<Model>("Bluff_Grass_01");
            ground = Content.Load<Model>("Uneven_Ground_Dirt_01");
        }

        //C:\Users\EKUStudent\source\repos\first3D\first3D\Content

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


            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                playerRot -= 0.05f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                playerRot += 0.05f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                playerPos += Vector3.Transform(
                    Vector3.Backward, 
                    Matrix.CreateRotationY(playerRot)
                    );
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Space))// && Math.Abs(playerPos.Y) < 0.001f )
            {
                playerPos.Y += 1.0f;
            }
            else
            {
                playerPos.Y -= 1.0f;
                // if playerPos.Y is ever negative (then 0 is bigger) and the answer to max
               playerPos.Y = Math.Max(playerPos.Y, 0.0f);
            }

            foreach(Vector3 pos in bluffPos)
            {
                //true if near the top of a bluff
                //not the bluffPos directly because its shifted down (-Y)
                //only the X position of the bluff
                //23f is the radius of the top of the bluff
                if(Vector3.Distance(new Vector3(pos.X, 0, pos.Z), playerPos) < 23f)
                    playerPos.Y = Math.Max(playerPos.Y, 0.0f);
            }

           // if(gameTime.TotalGameTime.TotalMilliseconds % 2 == 0)
           //     barrelPos.Add(new Vector3(100, 0, 0));

            for(int b = 0; b< barrelPos.Count; b++)
            {
                barrelPos[b] -= new Vector3(0.1f, 0, 0);

                if (Vector3.Distance(barrelPos[b], playerPos) < 3.5f)
                    //barrelPos[b] += new Vector3(0.2f, 0, 0);
                    playerPos += new Vector3(0, 3, 0);

            }

            foreach(Enemy e in enemies)
            {
                e.Update(gameTime);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //like the properties of lens on a camera
            Matrix proj = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(60),
                (float)graphics.PreferredBackBufferWidth / graphics.PreferredBackBufferHeight,
                0.001f,
                5000f);

            //how the camera is positioned in world
            Matrix view = Matrix.CreateLookAt(
                // this set our camera off to the player's right side
               // new Vector3(0 + playerPos.X, 10, 20) ,
               // set camera behind player
               // new Vector3(playerPos.X - 20, 10, 0),
               playerPos + Vector3.Transform(
                   camOffset,
                   Matrix.CreateRotationY(playerRot)
                   ),
               playerPos,
                new Vector3(0, 1, 0) // same as Vector3.up
                );

            if(Keyboard.GetState().IsKeyDown(Keys.OemTilde))
                view = Matrix.CreateLookAt(
                new Vector3(0 + playerPos.X, 0, 200),
                new Vector3(playerPos.X, 0, 0),
                new Vector3(0, 1, 0) // same as Vector3.up
                );

            #region draw ground plane
            ground.Draw(Matrix.CreateScale(10, .01f, 10), view, proj);
            #endregion

            //Vector3 is just a data type containing X,Y,Z
            //this can be a 3D position in space
            //this can be a vector -> direction and magnitude
            //this can also be a normalized vector (e.g. unit vector)
            //    vector with direction only


            //how the model is positioned in the world
            // SRT -> Scale*Rotation*Translation
            Matrix world = Matrix.CreateScale(0.005f)
                * Matrix.CreateRotationY(playerRot)
                * Matrix.CreateTranslation(playerPos);

               // * Matrix.CreateRotationZ((float)gameTime.TotalGameTime.TotalSeconds);

           //This is the simple way of drawing
           //player.Draw(world, view, proj);
           // we can replace this single line with a more complex draw call

            foreach(ModelMesh mesh in player.Meshes)
            {
                foreach(BasicEffect effect in mesh.Effects)
                {
                    effect.World = world;
                   // effect.DiffuseColor = Color.CornflowerBlue.ToVector3();
                    effect.View = view;
                    effect.Projection = proj;
                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }
        

           foreach(Vector3 pos in barrelPos) { 
                world = Matrix.CreateScale(0.01f)
                   * Matrix.CreateTranslation(pos);

                // barrel.Draw(world, view, proj);
                foreach (ModelMesh mesh in barrel.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.World = world;
                        effect.View = view;
                        effect.Projection = proj;
                        effect.EnableDefaultLighting();
                    }
                    mesh.Draw();
                }
            }


            foreach (Vector3 pos in bluffPos)
            {
                world = Matrix.CreateScale(0.01f)
                   * Matrix.CreateTranslation(pos);

                // barrel.Draw(world, view, proj);
                foreach (ModelMesh mesh in grass.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.World = world;
                        effect.View = view;
                        effect.Projection = proj;
                        effect.EnableDefaultLighting();
                    }
                    mesh.Draw();
                }
            }

            foreach(Enemy e in enemies)
            {
                world = Matrix.CreateScale(0.005f)
                * Matrix.CreateRotationY(e.Rot)
                * Matrix.CreateTranslation(e.Pos);
                player.Draw(world, view, proj);
            }
            base.Draw(gameTime);
        }
    }
}
