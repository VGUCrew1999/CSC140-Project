using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace first3D
{
    class Enemy
    {
        //there is only random number generator declared
        //for ALL enemies (i.e. they share it)
        static Random rand;
        Vector3 pos;
        public Vector3 Pos
        {
            get { return pos; }
        }
        float rot;
        public float Rot
        {
            get { return rot; }
        }

        public Enemy()
        {
            if (rand == null)
                rand = new Random();
            pos = new Vector3(rand.Next(-30, 30), rand.Next(-30,30), rand.Next(-30, 30));
            rot = MathHelper.ToRadians(rand.Next(0, 360));
        }

        public void Update(GameTime gameTime)
        {
            rot += MathHelper.ToRadians(rand.Next(-3, 3));
            pos += Vector3.Transform(Vector3.Backward, Matrix.CreateRotationY(rot));
        }

    }
}
