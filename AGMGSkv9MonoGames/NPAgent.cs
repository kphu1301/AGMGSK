/*  
    Copyright (C) 2017 G. Michael Barnes
 
    The file NPAgent.cs is part of AGMGSKv9 a port and update of AGXNASKv8 from
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
    /// A non-playing character that moves.  Override the inherited Update(GameTime)
    /// to implement a movement (strategy?) algorithm.
    /// Distribution NPAgent moves along an "exploration" path that is created by the
    /// from int[,] pathNode array.  The exploration path is traversed in a reverse path loop.
    /// Paths can also be specified in text files of Vector3 values, see alternate
    /// Path class constructors.
    /// 
    /// 1/20/2016 last changed
    /// </summary>
    public class NPAgent : Agent
    {
        private NavNode nextGoal;
        private Path path;
        private int snapDistance = 20;  // this should be a function of step and stepSize
                                        // If using makePath(int[,]) set WayPoint (x, z) vertex positions in the following array

        // Commented out original path.  Will be replaced with new exploaration path, designed for testing treasure hunt requirements. -JC
       /* private int[,] pathNode = { {505, 490}, {500, 500}, {490, 505},  // bottom, right
										 {435, 505}, {425, 500}, {420, 490},  // bottom, middle
										 {420, 450}, {425, 440}, {435, 435},  // middle, middle
                               {490, 435}, {500, 430}, {505, 420},  // middle, right
										 {505, 105}, {500,  95}, {490,  90},  // top, right
                               {110,  90}, {100,  95}, { 95, 105},  // top, left
										 { 95, 480}, {100, 490}, {110, 495},  // bottom, left
										 {495, 480} };           */                     // loop return

        // Can use comments to switch between original and new exploration paths.
        // Take NPAgent within 25 units of each treasure so that they will all eventually be "autodetected". -JC
        private int[,] pathNode = { {420, 445}, {300, 280}, {330, 300}, {460, 300}, {410, 500} };

        // Boolean for path toggle. True -> on treasure path.  False -> on regular path.
        protected bool pathToggle = false;
        // Keyboard state to make toggle consistent.
        protected KeyboardState oldKeyboardState;

        private Treasures treasures;
        private int treasureIndex;
        private int collectedTreasures = 0;



        /// <summary>
        /// Create a NPC. 
        /// AGXNASK distribution has npAgent move following a Path.
        /// </summary>
        /// <param name="theStage"> the world</param>
        /// <param name="label"> name of </param>
        /// <param name="pos"> initial position </param>
        /// <param name="orientAxis"> initial rotation axis</param>
        /// <param name="radians"> initial rotation</param>
        /// <param name="meshFile"> Direct X *.x Model in Contents directory </param>
        public NPAgent(Stage theStage, string label, Vector3 pos, Vector3 orientAxis, float radians, string meshFile, Treasures tList)
        : base(theStage, label, pos, orientAxis, radians, meshFile)
        {
            treasures = tList;

            // change names for on-screen display of current camera
            first.Name = "npFirst";
            follow.Name = "npFollow";
            above.Name = "npAbove";
            
            // path is built to work on specific terrain, make from int[x,z] array pathNode
            path = new Path(stage, pathNode, Path.PathType.LOOP); // continuous search path
            stage.Components.Add(path);
            nextGoal = path.NextNode;  // get first path goal
            agentObject.turnToFace(nextGoal.Translation);  // orient towards the first path goal
                                                           // set snapDistance to be a little larger than step * stepSize
            snapDistance = (int)(1.5 * (agentObject.Step * agentObject.StepSize));

            // Add
        }

        /// <summary>
        /// Simple path following.  If within "snap distance" of a the nextGoal (a NavNode) 
        /// move to the NavNode, get a new nextGoal, turnToFace() that goal.  Otherwise 
        /// continue making steps towards the nextGoal.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            float distance, distance2;


            // On treasure hunt path.
            if (pathToggle == true)
            {
                // If the treasure has already been found go back to regular path.
                if (treasures.getTreasure(treasureIndex).isFound() == true)
                {
                    pathToggle = false;
                }

                // face target treasure
                agentObject.turnToFace(new Vector3(treasures.getTreasure(treasureIndex).xPos *stage.Spacing, 0,
                    treasures.getTreasure(treasureIndex).zPos * stage.Spacing));

                // get distance between npAgent and target treasure.
                distance = Vector3.Distance(new Vector3(agentObject.Translation.X, 0, agentObject.Translation.Z),
                    new Vector3(treasures.getTreasure(treasureIndex).xPos *stage.Spacing, 0,
                        treasures.getTreasure(treasureIndex).zPos *stage.Spacing));

                // check if within snap range
                if (distance < 400)
                {
                    // increment number of collected treasures
                    collectedTreasures++;
                    Console.WriteLine("Treasure Count: "  + collectedTreasures);
                    treasures.getTreasure(treasureIndex).found = true;

                    // Stop moving if all four treasures have been updated.
                    // It would be better to use variables then a specific number for this, so this is something to fix.
                    if (collectedTreasures == 4)
                    {
                        instance[0].Step = 0;
                    }

                    pathToggle = false;
                }
            }


            // Not on treasure hunt path.
            if (pathToggle == false)
            { 
                agentObject.turnToFace(nextGoal.Translation); // adjust to face nextGoal every move
                                                              // agentObject.turnTowards(nextGoal.Translation);
                                                              // See if at or close to nextGoal, distance measured in 2D xz plane
                distance = Vector3.Distance(
                    new Vector3(nextGoal.Translation.X, 0, nextGoal.Translation.Z),
                    new Vector3(agentObject.Translation.X, 0, agentObject.Translation.Z));
                stage.setInfo(15, stage.agentLocation(this));

                stage.setInfo(16,
                    string.Format("          nextGoal ({0:f0}, {1:f0}, {2:f0})  distance to next goal = {3,5:f2})",
                        nextGoal.Translation.X / stage.Spacing, nextGoal.Translation.Y,
                        nextGoal.Translation.Z / stage.Spacing, distance));

                if (distance <= snapDistance)
                {
                    // snap to nextGoal and orient toward the new nextGoal 
                    nextGoal = path.NextNode;
                    // agentObject.turnToFace(nextGoal.Translation);
                }

                // Do a check to see if any treasure is withing 4000px deteciton range.
                for (int i = 0; i < 4; i++)
                {
                    if (treasures.getTreasure(i).isFound() == true)
                        continue;

                    distance = Vector3.Distance(new Vector3(agentObject.Translation.X, 0, agentObject.Translation.Z),
                        new Vector3(treasures.getTreasure(i).xPos * stage.Spacing, 0,
                            treasures.getTreasure(i).zPos * stage.Spacing));

                    // If within detection range, start doing automatic treasure hunt.
                    if (distance < 4000)
                    {
                        pathToggle = true;
                        treasureIndex = i;
                    }
                }
            }

            KeyboardState keyboardState = Keyboard.GetState();
            // If we press N, then we wish to change to treasure seeking path. 
            if (keyboardState.IsKeyDown(Keys.N) && !oldKeyboardState.IsKeyDown(Keys.N))
            {
                // set distance to some max value.
                distance = float.MaxValue;

                // Iterate through list of treasures
                for (int i = 0; i < 4; i++)
                {
                    // Check if we have already found a given treasure.
                    // If so, skip it.
                    if (treasures.getTreasure(i).isFound() == true)
                    {
                        continue;
                    }

                    // Get the distance between agent and treasure[i].
                    distance2 = Vector3.Distance(
                        new Vector3(agentObject.Translation.X, 0, agentObject.Translation.Z),
                        new Vector3(treasures.getTreasure(i).xPos, 0, treasures.getTreasure(i).zPos));

                    // this makes sure we get the closest treasure.
                    if (distance2 < distance)
                    {
                        treasureIndex = i;
                        distance = distance2;
                    }
                }

                // If we found a new treasure, then toggle treasure path.
                if (distance != float.MaxValue)
                {
                    pathToggle = true;
                }
            }

            base.Update(gameTime);  // Agent's Update();
        }
    }
}
