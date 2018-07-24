using System;
using System.IO;  // needed for trace()'s fout
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AGMGSKv9
{
    /*
      Treasure Class
      xPos = position of treasure's x coordinate 
      zPos = position of treasure's z coordinate
      found = status of treasure being found by npc/human
     */
    public class Treasure
    {
        public float xPos;
        public float zPos;
        public bool found;


        public Treasure(int x, int z)
        {
            xPos = x;
            zPos = z;
            found = false;
        }

        public float getXPos()
        {
            return xPos;
        }

        public float getZPos()
        {
            return zPos;
        }

        public bool isFound()
        {
            return found;
        }
    }

    /*
     * Treasures Class
     * creates array of Treasure items  
     * positions of treasure items are hard coded
     */
    public class Treasures : Model3D
    {
        const int numberOfTreasures = 4;
        Treasure[] treasureList;

        public Treasures(Stage theStage, string label, string meshFile) : base(theStage, label, meshFile)
        {
            isCollidable = true;
            int spacing = stage.Terrain.Spacing;
            Terrain terrain = stage.Terrain;

            treasureList = new Treasure[numberOfTreasures];
            treasureList[0] = new Treasure(447, 453);
            treasureList[1] = new Treasure(320, 320);
            treasureList[2] = new Treasure(480, 320);
            treasureList[3] = new Treasure(400, 490);

            for (int i = 0; i < treasureList.Length; i++)
            {
                addObject(new Vector3(treasureList[i].getXPos() * spacing, terrain.surfaceHeight((int)treasureList[i].getXPos(), 
                        (int)treasureList[i].getZPos()), treasureList[i].getZPos() * spacing), Vector3.Up, 0.0f);
            }

        }

        public Treasure getTreasure(int index)
        {
            if (index < 0 || index > numberOfTreasures - 1)
            {
                Console.WriteLine("Invalid index");
                return null;
            }
            else
            {
                return treasureList[index];
            }
        }
    }
}
