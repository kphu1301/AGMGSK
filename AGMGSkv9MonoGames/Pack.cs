/*  
    Copyright (C) 2017 G. Michael Barnes
 
    The file Pack.cs is part of AGMGSKv9 a port and update of AGXNASKv8 from
    MonoGames 3.5 to MonoGames 3.6  

    AGMGSKv9 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

#region Using Statements
using System;
using System.IO;  // needed for trace()'s fout
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace AGMGSKv9
{

    /// <summary>
    /// Pack represents a "flock" of MovableObject3D's Object3Ds.
    /// Usually the "player" is the leader and is set in the Stage's LoadContent().
    /// With no leader, determine a "virtual leader" from the flock's members.
    /// Model3D's inherited List<Object3D> instance holds all members of the pack.
    /// 
    /// 2/1/2016 last changed
    /// </summary>
    public class Pack : MovableModel3D
    {


        Object3D leader;
        private Random random = null;

        // Used to determine which boids will flock and which will not.
        // True for flocking, false for not flocking.
        private List<bool> flockMembers;

        private enum FlockingLevel { Level1 = 0, Level2 = 33, Level3 = 66, Level4 = 99 }

        private static FlockingLevel flockingLevel = FlockingLevel.Level1;

        public bool isFlockMember(Object3D current)
        {
            return flockMembers[instance.IndexOf(current)];
        }

        public static int getLevelValue()
        {
            return (int)flockingLevel;
        }

        public Object3D Leader
        {
            get { return leader; }
            set { leader = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="theStage"></param>
        /// <param name="label"></param>
        /// <param name="meshFile"></param>
        public Pack(Stage theStage, string label, string meshFile)
            : base(theStage, label, meshFile)
        {
            isCollidable = true;
            leader = null;
            random = new Random();
        }

        /// <summary>
        /// Construct a pack with an Object3D leader
        /// </summary>
        /// <param name="theStage"> the scene </param>
        /// <param name="label"> name of pack</param>
        /// <param name="meshFile"> model of a pack instance</param>
        /// <param name="aLeader"> Object3D alignment and pack center </param>
        public Pack(Stage theStage, string label, string meshFile, Object3D aLeader)
            : base(theStage, label, meshFile)
        {
            isCollidable = true;
            leader = aLeader;
            random = new Random();
            flockMembers = new List<Boolean>();
        }


       /* /// <summary>
        /// Construct a pack with an Object3D leader
        /// </summary>
        /// <param name="theStage"> the scene </param>
        /// <param name="label"> name of pack</param>
        /// <param name="meshFile"> model of a pack instance</param>
        /// <param name="xPos, zPos">  approximate position of the pack </param>
        /// <param name="aLeader"> alpha dog can be used for flock center and alignment </param>
        public Pack(Stage theStage, string label, string meshFile, int nDogs, int xPos, int zPos, Object3D theLeader)
           : base(theStage, label, meshFile)
        {

            isCollidable = true;
            random = new Random();
            leader = theLeader;
            int spacing = stage.Spacing;

            // setup flockMembers
            flockMembers = new List<bool>();

            // initial vertex offset of dogs around (xPos, zPos)
            int[,] position = { { 0, 0 }, { 7, -4 }, { -5, -2 }, { -7, 4 }, { 5, 2 } };
            for (int i = 0; i < position.GetLength(0); i++)
            {
                int x = xPos + position[i, 0];
                int z = zPos + position[i, 1];
                float scale = (float)(0.5 + random.NextDouble());
                addObject(new Vector3(x * spacing, stage.surfaceHeight(x, z), z * spacing),
                              new Vector3(0, 1, 0), 0.0f,
                              new Vector3(scale, scale, scale));
            }
        }*/

        public void setFlockMembers()
        {
            flockMembers = new List<bool>();
            foreach (Object3D obj in instance)
            {
                flockMembers.Add(((int)random.NextDouble() * 100) <= getLevelValue());
            }
        }

        public Vector3 getAlignment(Object3D current)
        {
            Vector3 alignment = leader.Forward;
            Vector3 averageDelta = Vector3.Zero;
            int N = 0;

            foreach (Object3D obj in instance)
            {
                if (obj != current)
                {
                    averageDelta += alignment - current.Forward;
                    N++;
                }
            }

            if (N > 0)
            {
                averageDelta /= N;
                averageDelta.Normalize();
            }

            return 0.45f * averageDelta;
        }

        public Vector3 getCohesion(Object3D current)
        {
            Vector3 cohesion = Vector3.Zero;
            cohesion = leader.Translation - current.Translation;
            cohesion.Normalize();

            // Multiply by random cohesion force between 0 and 15
            return cohesion * 15 * (float)random.NextDouble();
        }

        // Apply separation rules
        public Vector3 getSeparation(Object3D current)
        {
            Vector3 separation = Vector3.Zero;
            float distanceRadius = 700;
            foreach (Object3D obj in instance)
            {
                if (current != obj)
                {
                    Vector3 header = current.Translation - obj.Translation;
                    // If distance between current boid and another object is less than distanceRadius
                    // add to the separation force
                    if (header.Length() < distanceRadius)
                    {
                        separation += 5 * Vector3.Normalize(header) / (header.Length() / distanceRadius);
                    }
                }
            }

            // Add separation between leader and other boids
            if (Vector3.Distance(leader.Translation, current.Translation) < distanceRadius)
            {
                Vector3 header = current.Translation - leader.Translation;
                separation += 5 * Vector3.Normalize(header) / (header.Length() / distanceRadius);

            }
            return 2 * separation;
        }

        public void changeFlockLevel()
        {
            switch (flockingLevel)
            {
                case FlockingLevel.Level1:
                    flockingLevel = FlockingLevel.Level1;
                    break;
                case FlockingLevel.Level2:
                    flockingLevel = FlockingLevel.Level2;
                    break;
                case FlockingLevel.Level3:
                    flockingLevel = FlockingLevel.Level3;
                    break;
                case FlockingLevel.Level4:
                    flockingLevel = FlockingLevel.Level4;
                    break;
            }

            setFlockMembers();
        }

        /// <summary>
        /// Each pack member's orientation matrix will be updated.
        /// Distribution has pack of dogs moving randomly.  
        /// Supports leaderless and leader based "flocking" 
        /// </summary>      
        public override void Update(GameTime gameTime)
        {
            // if (leader == null) need to determine "virtual leader from members"
            float angle = 0.3f;

            if (leader == null)
            {
                foreach (Object3D obj in instance)
                {
                    obj.Yaw = 0.0f;
                    // change direction 4 time a second  0.07 = 4/60
                    if (random.NextDouble() < 0.07)
                    {
                        if (random.NextDouble() < 0.5) obj.Yaw -= angle; // turn left
                        else obj.Yaw += angle; // turn right
                    }

                    obj.updateMovableObject();
                    stage.setSurfaceHeight(obj);
                }
            }
            else
            {
                foreach (Object3D obj in instance)
                {
                    if (isFlockMember(obj))
                    {
                        obj.Translation += getCohesion(obj) + getSeparation(obj);
                        obj.Forward = getAlignment(obj);
                    }
                    else
                    {
                        obj.Yaw = 0.0f;
                        if (random.NextDouble() < 0.07)
                        {
                            if (random.NextDouble() < 0.05)
                            {
                                obj.Yaw -= angle;
                            }
                            else
                            {
                                obj.Yaw += angle;
                            }
                        }
                    }

                    obj.updateMovableObject();
                    stage.setSurfaceHeight(obj);
                }
            }

            base.Update(gameTime); // MovableMesh's Update();
        }
    }
}
