using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class NaturalWorldGen : MonoBehaviour {
    [Header (" TERRAIN: ")]
    [Space]
    [Tooltip (" Click and drag the Terrain you want to have Generated")]
    public GameObject primaryTerrain;
    private Terrain terr; // terrain to modify

    private float heightMapMax = 100.0f;
    private float startHeight = 1000.0f; //8000

    private int width, height = 300;

    private static float baseLine;
    private static bool setBase = false;

    public float[, ] heightMap;
    [Space]
    [Header (" ADVANCED SETTINGS: ")]

    [Tooltip("This setting changes how sloped the terrain is. Can emphasize terrain features well if higher...")]
    public float terrainSlopeModifier = 1.0f;

    [Tooltip ("The Generator outputs quite rocky terrain. This option specifically edits the ammount of times the smoothing function gets run over the whole map after a generation has finished. The higher this number is, the more smoothed a terrain is/superficially eroded. This game will run faster if this is lower, though lowering this makes terrain less smoothed and more fabricated. Preffered number for this is a 10 but it is workable as a 7...")]
    public int smootherIteration = 7;

    [Tooltip ("This setting changes how many iterations of smoothing happens in asteroid impact zones due to flattening. The lower this is, the faster the game will run but beware, lowering this decreases terrain realism and smoothness. Preffered number for this is 4, but it is passible with a 2...")]
    public int asteroidSmoothing = 5;

    [Tooltip ("This variable sets a baseline for random generation planet-wise. Leave as -1 for a random seed.")]
    public int seed = -1;
    //private readonly String seedDigits = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_-";
    //private int seedNum = 0;

    void Start(){
        setup();
        displayMap();
    }

    public void setup () {
        //Terrain initialization, terrain scripting is later
        terr = primaryTerrain.GetComponent<Terrain>();
        terr = Terrain.activeTerrain;
        width = terr.terrainData.heightmapResolution;
        height = terr.terrainData.heightmapResolution;
        Terrain.activeTerrain.heightmapMaximumLOD = 0;
        //End of terrain

        //This part sets a seed if one hasn't been provided.
        if (seed < 0) {
            seed = (int) SeedRandom(0, 2147483647);
        }
        seed = seed % 2147483647;
        Random.InitState(seed);

 
        //size(heightMapX,heightMapY)

        //Initialization of Heightmap
        buildHM (width, height);

        //Build Elements of Heightmap,Like blender
        asteroids (0.1f, 100, 0.5f);

        smoothCircle (smootherIteration, width, width / 2, height / 2);
        
        //veryRandom();

        //Display Heightmap
        //displayMap ();

        //Draw HM to screen
    }

    public void coneDown(){
        float medX,medY;
        medX = heightMap.GetLength(0)/2;
        medY = heightMap.GetLength(1)/2;

        for (int x = 0; x < heightMap.GetLength (0); x++) {
            for (int y = 0; y < heightMap.GetLength (1); y++) {
                //Get length from medX
                float distan = Mathf.Sqrt(Mathf.Pow((x-medX),2)+Mathf.Pow((y-medY),2))/2.0f;
                heightMap[x,y] = heightMap[x,y]-(int)distan+(int)Mathf.Sqrt(Mathf.Pow((0-medX),2)+Mathf.Pow((0-medY),2));
            }
        }
    }

    public void buildHM (int wid, int hig) {
        heightMap = new float[wid, hig];

        //fill heightmap with starting values
        for (int x = 0; x < wid; x++) {
            for (int y = 0; y < hig; y++) {
                heightMap[x, y] = startHeight;
            }
        }
    }

    public void asteroids (float ammount, float maxSize, float asteroidBias) // highest ammount of hits per 10 point squared distance on average  //apply asteroids and pockmarks to the planets surface
    {

        //Asteroid types: 0 is simple, 1 is complex. Affects randomness of asteroid. If it is 0 all the asteroids will be on the smaller side, regardless of the maxSize, but if it is 1 most asteroids will be very close to the maximum size
        //get true max ammount of hits 
        //Only do complex asteroids in large size and small ammount. Also smooth terrain under impact zone.

        ammount = ammount * ((heightMap.GetLength (0) / 10.0f) * (heightMap.GetLength (1) / 10.0f));
        int finalAmmount = (int) ammount;
        float midLine = Mathf.Floor ((Mathf.Cos (asteroidBias * Mathf.PI) / 2.0f + 0.5f) * maxSize);

        for (int i = 0; i < finalAmmount; i++) {
            //primary asteroid creator
            float size = Mathf.Floor (maxSize * Mathf.Pow ((float) SeedRandom (0f, 1f), Mathf.Pow (2, Mathf.Cos (asteroidBias * Mathf.PI)))); //2 is close to 0, 0.5 is close to 1
            //Crater x and y's
            int cX = (int) SeedRandom (0, heightMap.GetLength (0));
            int cY = (int) SeedRandom (0, heightMap.GetLength (1));

            if (size > midLine) {
                //complex
                float d1 = size - size * Mathf.Cos (Mathf.PI / 4);
                smoothCircle (asteroidSmoothing, size, cX, cY);
                int pixelsRemoved = 0;
                for (int j = (int) d1; j >= 0; j -= 2) {
                    //get impact circle widths
                    float w = size * Mathf.Sin (Mathf.Acos ((size - j) / size));
                    pixelsRemoved += drawCircle (w, cX, cY);
                }
                //Middle Lump

                for (int q = 0; q < d1 / 5.0f; q++) {
                    //get impact circle widths
                    float w = size / 5.0f * Mathf.Sin (Mathf.Acos ((size / 5.0f - q) / size / 5.0f));
                    inverseDrawCircle (w / 5.0f, cX, cY);
                }

            } else {
                //simple
                //quite rudimentary. It doesn't do anything for impact angle relative to whats already there but thats okay. Will come up for a solution later.
                float d1 = size - size * Mathf.Cos (Mathf.PI / 4);
                smoothCircle (asteroidSmoothing, size, cX, cY);
                int pixelsRemoved = 0;
                for (int j = (int) d1; j >= 0; j--) {
                    //get impact circle widths
                    float w = size * Mathf.Sin (Mathf.Acos ((size - j) / size));
                    pixelsRemoved += drawCircle (w, cX, cY);
                }
                /*
                //pixelsRemoved is for ejecta. For a later update, will be copy & pasteable to the complex craters. A simple change honestly
      
                //pixels removed
                for(int p = 0; p < pixelsRemoved; p++){
                    float direction = (float)Math.random()*360.0f;
                    float dist = size/2.0f + size*pow((float)Math.random(),1.0f);
        
                    addHeight(1,(int)(dist*cos(2*PI*direction/360.0f))+cX,(int)(dist*sin(2*PI*direction/360.0f))+cY);
        
                }
                */

            }
        }
    }

    public void veryRandom () { //Assigns really random numbers to the heightmap. For erosion and smoothing testing
        for (int x = 0; x < heightMap.GetLength (0); x++) {
            for (int y = 0; y < heightMap.GetLength (1); y++) {
                heightMap[x, y] = SeedRandom (0, heightMapMax);
            }
        }
    }

    public void displayMap () { //function individual to processing
        //read individual heightmap point data and draw
        float[, ] heights = terr.terrainData.GetHeights (0, 0, width, height);
        float min = heightMap[0, 0];
        float max = heightMap[0, 0];
        for (int x = 0; x < heightMap.GetLength (0); x++) {
            for (int y = 0; y < heightMap.GetLength (1); y++) {
                if (heightMap[x, y] > max) {
                    max = heightMap[x, y];
                }
                if (heightMap[x, y] < min) {
                    min = heightMap[x, y];
                }
            }
        }

        float dataSize = (max - min);
        terr.terrainData.size = new Vector3 (heightMap.GetLength(0) , dataSize * terrainSlopeModifier, heightMap.GetLength(1));

        if (!setBase) {
            baseLine = min * 10;
            setBase = true;
        }

        primaryTerrain.transform.position = new Vector3(primaryTerrain.transform.position.x, min * 10 - baseLine, primaryTerrain.transform.position.z);

        //REMOVABLE
        float maxxy = 0.0f;

        for (int x = 0; x < heightMap.GetLength (0); x++) {
            for (int y = 0; y < heightMap.GetLength (1); y++) {
                heights[x, y] = ((heightMap[x, y] - min) / dataSize); // * 0.5f + 0.25f;

                //REMOVABLE
                if(x == 0 || y == 0 || x == heightMap.GetLength(0) - 1 || y == heightMap.GetLength(1) - 1){
                    if(heights[x,y] > maxxy){
                        maxxy = heights[x,y];
                    }
                }
            }
        }

        terr.terrainData.SetHeights (0, 0, heights);
    }

    //This point on contains minor functions for making circles, and graphical things
    public void addHeight (float change, int x, int y) {
        if (x < heightMap.GetLength (0) && x >= 0 && y < heightMap.GetLength (1) && y >= 0) {
            heightMap[x, y] += change;
        }
    }

    public void setHeight (float change, int x, int y) {
        if (x < heightMap.GetLength (0) && x >= 0 && y < heightMap.GetLength (1) && y >= 0) {
            heightMap[x, y] = change;
        }
    }

    public float getHeight (int x, int y) {
        if (x < 0 || y < 0 || x >= heightMap.GetLength (0) || y >= heightMap.GetLength (1)) {
            return -1.0f;
        }
        return heightMap[x, y];
    }

    public void smoothCircle (int iterations, float circleRadius, int x, int y) {
        //Create tiny originial array of the circle values
        float[, ] originalMap = new float[(int) (circleRadius * 2), (int) (circleRadius * 2)];
        float[, ] newMap = new float[(int) (circleRadius * 2), (int) (circleRadius * 2)];
        bool[, ] ignoreMe = new bool[(int) (circleRadius * 2), (int) (circleRadius * 2)];

        //Assign current map values to original map
        for (int yy = 0; yy < (int) circleRadius * 2; yy++) {
            //float w = sqrt(pow(circleRadius,2)-pow((-yy+circleRadius),2)); //gets circle width at height
            for (int xx = 0; xx < (int) circleRadius * 2; xx++) {
                originalMap[xx, yy] = getHeight (x + xx - (int) circleRadius, y + yy - (int) circleRadius);
                newMap[xx, yy] = getHeight (x + xx - (int) circleRadius, y + yy - (int) circleRadius);
                ignoreMe[xx, yy] = false;
            }
        }

        //look at each pixel in the circle, and check heights of all pixels around it and move it just one bit closer to the median. Do I want to make the median the median of the impact circle? or the median height of the pixel and heights around it.
        for (int iteration = 0; iteration < iterations; iteration++) {
            //Individual pixel changing.
            for (int yyy = 0; yyy < circleRadius * 2; yyy++) {
                for (int xxx = (int) (circleRadius - Mathf.Sqrt (Mathf.Pow (circleRadius, 2) - Mathf.Pow ((-yyy + circleRadius), 2))); xxx < circleRadius + Mathf.Sqrt (Mathf.Pow (circleRadius, 2) - Mathf.Pow ((-yyy + circleRadius), 2)); xxx++) {
                    //Put the circle width limiter here
                    if (originalMap[xxx, yyy] != -1.0f && circleRadius - Mathf.Sqrt (Mathf.Pow (circleRadius, 2) - Mathf.Pow ((-yyy + circleRadius), 2)) < xxx && circleRadius + Mathf.Sqrt (Mathf.Pow (circleRadius, 2) - Mathf.Pow ((-yyy + circleRadius), 2)) > xxx) {
                        float mediumHeight = originalMap[xxx, yyy];
                        float values = 1.0f;

                        //add values of pixels around
                        
                        if (xxx - 1 >= 0 && yyy + 1 < circleRadius * 2) {
                            values++;
                            mediumHeight += originalMap[xxx - 1, yyy + 1];
                        }
                        if (xxx + 1 < circleRadius * 2 && yyy + 1 < circleRadius * 2) {
                            values++;
                            mediumHeight += originalMap[xxx + 1, yyy + 1];
                        }
                        if (xxx + 1 < circleRadius * 2 && yyy - 1 >= 0) {
                            values++;
                            mediumHeight += originalMap[xxx + 1, yyy - 1];
                        }
                        if (xxx - 1 >= 0 && yyy - 1 >= 0) {
                            values++;
                            mediumHeight += originalMap[xxx - 1, yyy - 1];
                        }
                        
                        //up down left right
                        if (xxx - 1 >= 0) {
                            values++;
                            mediumHeight += originalMap[xxx - 1, yyy];
                        }
                        if (xxx + 1 < circleRadius * 2) {
                            values++;
                            mediumHeight += originalMap[xxx + 1, yyy];
                        }
                        if (yyy - 1 >= 0) {
                            values++;
                            mediumHeight += originalMap[xxx, yyy - 1];
                        }
                        if (yyy + 1 < circleRadius * 2) {
                            values++;
                            mediumHeight += originalMap[xxx, yyy + 1];
                        }

                        mediumHeight = mediumHeight / values;
                        newMap[xxx, yyy] = mediumHeight;
                    } else {
                        //make all heights -1 on pixels outside of field
                        //UPDATE not proper coding style, upgraded.
                        ignoreMe[xxx, yyy] = true;
                    }
                }
            }
            //Apply Smoothing
            for (int yyyy = 0; yyyy < circleRadius * 2; yyyy++) {
                for (int xxxx = 0; xxxx < circleRadius * 2; xxxx++) {
                    //originalMap[xx][yy] = getHeight(x+xx-(int)circleRadius,y+yy-(int)circleRadius);
                    originalMap[xxxx, yyyy] = newMap[xxxx, yyyy];
                    if (!ignoreMe[xxxx, yyyy]) {
                        setHeight (newMap[xxxx, yyyy], x + xxxx - (int) circleRadius, y + yyyy - (int) circleRadius);
                    }
                }
            }
        }
    }

    public int drawCircle (float radius, int x, int y) {
        int aS = 0;
        for (int p = 0; p < 2 * radius; p++) {
            int w1 = (int) (radius * Mathf.Sin (Mathf.Acos ((radius - p) / radius)));
            int w2 = (int) (-(radius * Mathf.Sin (Mathf.Acos ((radius - p) / radius))));
            for (int h = w2; h < 0; h++) {
                addHeight (-1, (int) (x + h), (int) (y - radius + p));
                aS++;
            }
            for (int h = 0; h < w1; h++) {
                addHeight (-1, (int) (x + h), (int) (y - radius + p));
                aS++;
            }
        }
        return aS;
    }

    public int inverseDrawCircle (float radius, int x, int y) {
        int aS = 0;
        for (int p = 0; p < 2 * radius; p++) {
            int w1 = (int) (radius * Mathf.Sin (Mathf.Acos ((radius - p) / radius)));
            int w2 = (int) (-(radius * Mathf.Sin (Mathf.Acos ((radius - p) / radius))));
            for (int h = w2; h < 0; h++) {
                addHeight (1, (int) (x + h), (int) (y - radius + p));
                aS++;
            }
            for (int h = 0; h < w1; h++) {
                addHeight (1, (int) (x + h), (int) (y - radius + p));
                aS++;
            }
        }
        return aS;
    }

    //SeedRandom
    private float SeedRandom(float lower, float higher) {
        //Random does from lower value to almost the higher value (inclusive to both bottom and top)
        float output = Random.value;
        output *= higher;
        output += lower;
        return output;
    }

    public void tempTest () {
        using (StreamWriter sw = File.AppendText ("C:/Users/athdo/Desktop/test.txt")) {
            sw.WriteLine ("This");
            sw.WriteLine ("is Extra");
            sw.WriteLine ("Text");
        }
    }
}