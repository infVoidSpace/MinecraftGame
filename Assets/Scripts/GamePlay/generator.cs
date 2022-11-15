using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class generator : MonoBehaviour
{
    //map generation and rendering
    private static int WorldSizeX = 700, WorldSizeY = 40, WorldSizeZ = 700;
    public static int renderSizeX = 120, renderSizeY = 40, renderSizeZ = 120, blockTypes = 9;
    public static int renderOffsetX = 100, renderOffsetZ = 100;
    public GameObject Map;
    public GameObject[] Blocks ;
    public static Block[,,] blockMap = new Block[WorldSizeX,WorldSizeY,WorldSizeZ] ;
    public static int[,,] mapIDs = new int[WorldSizeX, WorldSizeY, WorldSizeZ]; // block type indexes for each cube in the world (0 is null or an air block)
    public void SetMapID(int x, int y, int z,int type) { mapIDs[x,y,z]= type; }

    public ItemDataBaseList database;    
    public GameObject FPScontroller;
    
    // Block update variables
    public static GameObject[] blockBreakAnimation = new GameObject[5];
    public static bool blockUpdateRequired;
    public static int[] blockToUpdate = new int[4];

    public static bool ConsoleDebugInfo;

    public bool UsePerlinNoiseTerrain;
    private Plane[] MapBoundaryPlanes = new Plane[4];
    private Vector3[] MapEdgePoints = new Vector3[4];
    public int blocksToGenerate;
    public int Distance_for_Generating_terrain;
    Scene play;

    void Start()
    {
        blocksToGenerate = 40;
        Distance_for_Generating_terrain = 10;
        //reference current scene
        play = SceneManager.GetActiveScene();

        //reference the Player and place in the middle of the init rendered map
        FPScontroller = GameObject.FindGameObjectWithTag("Player");
        FPScontroller.transform.position = new Vector3(renderOffsetX + renderSizeX / 4, WorldSizeY, renderOffsetZ + renderSizeZ / 2) / 2;

        //instantiate the blocks prefab types according to the database's "item.itemID":
        for (int i = 0; i < 5; i++)
            blockBreakAnimation[i] = gameObject.transform.Find("block breaking textures").GetChild(i).gameObject;
        Blocks = new GameObject[blockTypes];
        for (int i = 0; i < blockTypes; i++)
            Blocks[i] = database.itemList[i].blockModel;
            
        //generate terrain data
        if (!UsePerlinNoiseTerrain)
        {
            GenerateCustumTerrainSurface();
            GenerateCustumUnderGround();
        }
        else
            GenerateNewPerlinNoiseTerrain(0, 0, WorldSizeX, WorldSizeZ);
        
        GenerateChunks(renderOffsetX,renderOffsetZ, renderOffsetX+renderSizeX, renderOffsetZ+ renderSizeZ);
        Generate2DTreeMap((int)(renderSizeX * renderSizeZ/30),renderOffsetX/2,renderOffsetZ/2,renderSizeX*4,renderSizeZ*4);

        //build the terrain
        for (int x = renderOffsetX; x < renderOffsetX+ renderSizeX; x++)
        {
            for (int z = renderOffsetZ; z < renderOffsetZ+ renderSizeZ; z++) 
            {
                for (int y = 0; y < renderSizeY; y++)
                {
                    //print(x + renderOffsetX+" " + y+" "+ (z+ renderOffsetZ));
                    if (mapIDs[x, y, z] != 0)
                    {
                        CreateRenderedFakeBlock(x, y, z);
                        //print(" MapID :  " + mapIDs[x, y, z] + "  __at: " + x + "," + y + "," + z);

                    }
                    
                }
            }
        }

        //init map boundaries
        MapEdgePoints[0] = new Vector3(renderOffsetX, 0, renderOffsetZ)/2;

        MapEdgePoints[1] = new Vector3(renderOffsetX, 0, renderOffsetZ+ renderSizeZ)/2;
        MapEdgePoints[2] = new Vector3(renderOffsetX+renderSizeX, 0, renderOffsetZ)/2;
        MapEdgePoints[3] = new Vector3(renderOffsetX+renderSizeX , 0, renderOffsetZ+renderSizeZ)/2;
        Vector3 UpNormal = new Vector3(0,1,0);
        MapBoundaryPlanes[0] = new Plane(MapEdgePoints[0], MapEdgePoints[1], MapEdgePoints[0] + UpNormal);
        
        MapBoundaryPlanes[1] = new Plane(MapEdgePoints[0], MapEdgePoints[2], MapEdgePoints[0] + UpNormal);
        MapBoundaryPlanes[2] = new Plane(MapEdgePoints[3], MapEdgePoints[1], MapEdgePoints[3] + UpNormal);
        MapBoundaryPlanes[3] = new Plane(MapEdgePoints[3], MapEdgePoints[2], MapEdgePoints[3] + UpNormal);
        for (int i=0;i<4;i++)
        {
            if (MapBoundaryPlanes[i].GetSide(FPScontroller.transform.position))
                MapBoundaryPlanes[i].Flip();
        }
        
    }

    void Update()
    {
        // block update
        if (blockUpdateRequired)
        {
            if (ConsoleDebugInfo) print("blockToUpdate = " + blockToUpdate[0]+","+ blockToUpdate[1]+ "," + blockToUpdate[2]);
            int x = blockToUpdate[0];
            int y = blockToUpdate[1];
            int z = blockToUpdate[2];
            int orientable = blockToUpdate[3];
            CreateRenderedFakeBlock(x, y, z, orientable);
            if (z > 0) UpdateBlock(x, y, z - 1);
            if (z < WorldSizeZ - 1) UpdateBlock(x, y, z + 1);
            if (y > 0) UpdateBlock(x, y - 1, z);
            if (y<WorldSizeY-1) UpdateBlock(x, y + 1, z);
            if (x < WorldSizeX - 1) UpdateBlock(x + 1, y, z);
            if (x > 0) UpdateBlock(x - 1, y, z);
            blockUpdateRequired = false;
        }

        //precedural generation
        if (!GetPlayerToMapBoundariesNormal().Equals(Vector3.zero))
            UpdateNewChunks(GetPlayerToMapBoundariesNormal());

        if (Input.GetKey("u")) //for debug
        {
            print(GetPlayerToMapBoundariesdist());
            print("NORMAL VECTOR IS : " + GetPlayerToMapBoundariesNormal().x + " " + GetPlayerToMapBoundariesNormal().y + " " + GetPlayerToMapBoundariesNormal().z);
            for (int i = 0; i < 4; i++)
            {
                print("plane #" + i + " : " + 2 * MapBoundaryPlanes[i].GetDistanceToPoint(FPScontroller.transform.position));
            }
        }
    }

    //basic terrain precedural generation algorithm:
    void GenerateCustumTerrainSurface()
    {
        var rand = new System.Random();
        var y = renderSizeY / 2;
        var previousRow = new int[WorldSizeZ + 1, 2];

        for (int x = 0; x < WorldSizeX- renderOffsetX; x++)
        {
            var zAxisflatnessCounter = 0; //manage curves on the z axis ( the x axis is dealt with 'previousRow' array inside )

            for (int z = 0; z < WorldSizeZ- renderOffsetZ; z++)
            {
                
                mapIDs[x + renderOffsetX, y, z + renderOffsetZ] = 2;
                

                //deal with the horizontal x-Axis edges
                if (z == WorldSizeZ - 1 & x > 0)
                {
                    var tmprand = rand.Next(0, 2);
                    
                    if (tmprand == 0)
                    {
                        y = previousRow[0, 0];
                    }
                    else if (tmprand == 1 & y > 0)
                    {
                        y = previousRow[0, 0] - 1;
                    }
                    else if (tmprand == 2 & y < renderSizeY-1)
                    {
                        y = previousRow[0, 0] + 1;
                    }
                    break;
                }

                //initialize first z-Axis row
                var randomChance = rand.Next(10) + 1;
                if (x == 0)
                {

                    previousRow[z, 0] = y;
                    previousRow[z, 1] = 2;

                    if (randomChance == 1 & y > 0)
                    { y--; }
                    else if (randomChance == 2 & y < renderSizeY-1)
                    { y++; }
                    else
                    { zAxisflatnessCounter++; }
                    if (z == WorldSizeZ - 1)
                    {
                        previousRow[WorldSizeZ, 0] = y;
                        previousRow[WorldSizeZ, 1] = 3;
                    }
                }
                //build terrain row by row
                else
                {
                    //deal with x-Axis curve using the diff var
                    int diff = 0;
                    if (z < WorldSizeZ - 1)
                    { diff = previousRow[z + 1, 0] - y; }
                    var randomChance2 = rand.Next(10)+1;
                    if (previousRow[z + 1, 1] == 0 |
                        (previousRow[z + 1, 1] == 1 & randomChance2 > 5) |
                        (previousRow[z + 1, 1] == 2 & randomChance2 >= 8))
                    {
                        //keep flat
                        y = previousRow[z + 1, 0];
                    }
                    else
                    {
                        //curve
                        if (randomChance <= 5)
                        {
                            y = previousRow[z + 1, 0];
                        }
                        else //if (changeY>6 & changeY<10)
                        {
                            if (diff > 0 & y < renderSizeY-1)
                            {
                                y = previousRow[z + 1, 0] - 1;
                            }
                            else if (y > 0 & y < renderSizeY - 1)
                            {
                                y = previousRow[z + 1, 0] + 1;
                            }
                        }
                    }

                    // keep referance for next row
                    if (previousRow[z + 1, 0] == y)
                    {
                        previousRow[z + 1, 1] = 1;
                    }
                    else
                    {
                        previousRow[z + 1, 1] = 0;
                    }
                    previousRow[z + 1, 0] = y;
                }
            }
        }
    }

    void GenerateCustumUnderGround()
    {
        int dirtDepth = 5;
        for (int x = 0; x < WorldSizeX- renderOffsetX; x++)
        {
            for (int z = 0; z < WorldSizeZ- renderOffsetZ; z++)
            {
                int y = renderSizeY-1;
                while (y > 0 && mapIDs[x + renderOffsetX, y, z + renderOffsetZ] == 0 )
                {
                    //print("countin y down, y is : " + y);
                    y--;
                }
                if (y>0)
                    y--;
                for (int i = 1;i<= dirtDepth; i++)
                {
                    var r = UnityEngine.Random.Range(0f, (float)dirtDepth - 1);
                    if ( r< dirtDepth - i)
                    {
                        mapIDs[x + renderOffsetX, y, z + renderOffsetZ] = 1;
                    }
                    else
                    {
                        mapIDs[x + renderOffsetX, y, z + renderOffsetZ] = 4;
                    }
                    y--;
                    if (y < 0)
                        break;
                }
                while (y >= 0)
                {
                    mapIDs[x + renderOffsetX, y, z + renderOffsetZ] = 4;
                    y--;
                }
            }
        }
    }

    void GenerateChunks(int startX, int startZ, int endX, int endZ)
    {
        for (int i = startX / Chunk.chunkSize; i< endX / Chunk.chunkSize;i++)
        {
            for (int j = startZ / Chunk.chunkSize; j < endZ / Chunk.chunkSize; j++)
            {
                String name = i + "_" + j;
                if (ConsoleDebugInfo) print("chunkID ="+ name);
                Chunk chunk = new Chunk(name,i,j,new Vector3(i* Chunk.chunkSize/2, 0,j* Chunk.chunkSize/2) ,Map.transform);
            }
        }
    }

    void CreateRenderedFakeBlock(int x, int y, int z, int orienation = 0,int BlockID = 0, bool fromMapID = true)
    {
        float X = (float)x / 2;
        float Y = (float)(y - renderSizeY / 2) / 2;
        float Z = (float)z / 2;
        
        //if (blockUpdateRequired) print("making block : x,y,z = " + x + "," + y + "," + z);
        if (fromMapID)
            BlockID = mapIDs[x , y, z];
        if (BlockID == 0)
            return;
        if ((z<WorldSizeZ-1 && mapIDs[x, y, z + 1] != 0) &&
            (z >0 && mapIDs[x, y, z - 1] != 0 )&& 
            (x<WorldSizeX-1 && mapIDs[x + 1, y, z] != 0) && 
            (x>0 && mapIDs[x - 1, y, z] != 0) && 
            (y<WorldSizeY-1 && mapIDs[x, y + 1, z] != 0) &&
            (y>0 && mapIDs[x, y - 1, z] != 0 ))
        {
            return;
        }
        
        Transform currentChunk = Map.transform.Find((x)/ Chunk.chunkSize + "_" + (z) / Chunk.chunkSize); 
        blockMap[x, y, z] = new Block(BlockID, new Vector3(X, Y, Z), x, y, z, currentChunk, orienation);
        
        UpdateBlock(x, y, z, BlockID, fromMapID);

    }

    void UpdateBlock(int x, int y, int z, int BlockID = 0, bool fromMapID = true)
    {
        
           // print("Updating Block : " + x + "," + y + "," + z);

        if (fromMapID)
            BlockID = mapIDs[x, y, z ];
        if (BlockID == 0)
            return;
        
        float X = (float)(x) / 2;
        float Y = (float)(y - renderSizeY / 2) / 2;
        float Z = (float)(z) / 2;
        if (blockMap[x, y, z] == null)
        {
            Transform currentChunk = Map.transform.Find((x) / Chunk.chunkSize + "_" + (z) / Chunk.chunkSize);
            blockMap[x, y, z] = new Block(BlockID, new Vector3(X, Y, Z), x, y, z, currentChunk, blockToUpdate[3]);
        }
        Transform currentBlock = null;
        if (blockMap[x, y, z].blockGameObject != null)
            currentBlock = blockMap[x, y, z].blockGameObject.transform;

        //up
        if ((y == renderSizeY - 1 || mapIDs[x, y + 1, z] == 0) & !blockMap[x, y, z].up)
        {
            GameObject face;
            blockMap[x, y, z].up = true;
            if (blockMap[x, y, z].orientation == 1)
                face = Instantiate(Blocks[BlockID].transform.Find("east").gameObject, new Vector3(X + 0.25f, Y + 0.5f, Z + 0.25f), Quaternion.Euler((float)90, (float)90, (float)0), currentBlock);
            else if (blockMap[x, y, z].orientation == 2)
                face = Instantiate(Blocks[BlockID].transform.Find("north").gameObject, new Vector3(X + 0.25f, Y + 0.5f, Z + 0.25f), Quaternion.Euler((float)90, (float)180, (float)0), currentBlock);
            else
                face = Instantiate(Blocks[BlockID].transform.Find("top").gameObject, new Vector3(X + 0.25f, Y + 0.5f, Z + 0.25f), Quaternion.Euler((float)90, (float)90, (float)0), currentBlock);
            face.name = "up";
        }
        else if ((y != renderSizeY - 1 && mapIDs[x, y + 1, z] != 0) & blockMap[x, y, z].up)
        {
            blockMap[x, y, z].up = false;
            Destroy(currentBlock.Find("up").gameObject,0.6f);
        }

        //down
        if ((y == 0 || mapIDs[x, y - 1, z] == 0) & !blockMap[x, y, z].down)
        {
            GameObject face;
            blockMap[x, y, z].down = true;
            if (blockMap[x, y, z].orientation == 1)
                face = Instantiate(Blocks[BlockID].transform.Find("west").gameObject, new Vector3(X + 0.25f, Y, Z + 0.25f), Quaternion.Euler((float)270, (float)90, (float)0), currentBlock);
            else if (blockMap[x, y, z].orientation == 2)
                face = Instantiate(Blocks[BlockID].transform.Find("south").gameObject, new Vector3(X + 0.25f, Y, Z + 0.25f), Quaternion.Euler((float)270, (float)180, (float)0), currentBlock);
            else
                face = Instantiate(Blocks[BlockID].transform.Find("bottom").gameObject, new Vector3(X + 0.25f, Y, Z + 0.25f), Quaternion.Euler((float)270, (float)0, (float)0), currentBlock);
            face.name = "down";
        }
        else if ((y != 0 && mapIDs[x, y - 1, z] != 0) & blockMap[x, y, z].down)
        {
            blockMap[x, y, z].down = false;
            Destroy(currentBlock.Find("down").gameObject, 0.6f);
        }

        //north
        if ((z == renderSizeZ - 1 || mapIDs[x, y, z + 1] == 0) & !blockMap[x, y, z].north)
        {
            GameObject face;
            blockMap[x, y, z].north = true;
            if (blockMap[x, y, z].orientation == 1)
                face = Instantiate(Blocks[BlockID].transform.Find("north").gameObject, new Vector3(X + 0.25f, Y + 0.25f, Z + 0.5f), Quaternion.Euler((float)0, (float)180, (float)90), currentBlock);
            else if (blockMap[x, y, z].orientation == 2)
                face = Instantiate(Blocks[BlockID].transform.Find("top").gameObject, new Vector3(X + 0.25f, Y + 0.25f, Z + 0.5f), Quaternion.Euler((float)0, (float)180, (float)0), currentBlock);
            else
                face = Instantiate(Blocks[BlockID].transform.Find("north").gameObject, new Vector3(X + 0.25f, Y + 0.25f, Z + 0.5f), Quaternion.Euler((float)0, (float)180, (float)0), currentBlock);
            face.name = "north";
        }
        else if ((z != renderSizeZ - 1 && mapIDs[x, y, z + 1] != 0) & blockMap[x, y, z].north)
        {
            blockMap[x, y, z].north = false;
            Destroy(currentBlock.Find("north").gameObject, 0.6f);
        }

        //south
        if ((z == 0 || mapIDs[x, y, z - 1] == 0) & !blockMap[x, y, z].south)
        {
            GameObject face;
            blockMap[x, y, z].south = true;
            if (blockMap[x, y, z].orientation == 1 )
                face = Instantiate(Blocks[BlockID].transform.Find("south").gameObject, new Vector3(X + 0.25f, Y + 0.25f, Z), Quaternion.Euler((float)0, (float)0, (float)90), currentBlock);
            else if (blockMap[x, y, z].orientation == 2)
                face = Instantiate(Blocks[BlockID].transform.Find("bottom").gameObject, new Vector3(X + 0.25f, Y + 0.25f, Z), Quaternion.identity, currentBlock);
            else
                face = Instantiate(Blocks[BlockID].transform.Find("south").gameObject, new Vector3(X + 0.25f, Y + 0.25f, Z), Quaternion.identity, currentBlock);
            face.name = "south";
        }
        else if ((z != 0 && mapIDs[x, y, z - 1] != 0) & blockMap[x, y, z].south)
        {
            blockMap[x, y, z].south = false;
            Destroy(currentBlock.Find("south").gameObject, 0.6f);
        }

        //east
        if ((x == renderSizeX - 1 || mapIDs[x + 1, y, z] == 0) & !blockMap[x, y, z].east)
        {
            GameObject face;
            blockMap[x, y, z].east = true;
            if (blockMap[x, y, z].orientation == 1)
                face = Instantiate(Blocks[BlockID].transform.Find("top").gameObject, new Vector3(X + 0.5f, Y + 0.25f, Z + 0.25f), Quaternion.Euler((float)0, (float)270, (float)0), currentBlock);
            else if (blockMap[x, y, z].orientation == 2)
                face = Instantiate(Blocks[BlockID].transform.Find("east").gameObject, new Vector3(X + 0.5f, Y + 0.25f, Z + 0.25f), Quaternion.Euler((float)0, (float)270, (float)90), currentBlock);
            else
                face = Instantiate(Blocks[BlockID].transform.Find("east").gameObject, new Vector3(X + 0.5f, Y + 0.25f, Z + 0.25f), Quaternion.Euler((float)0, (float)270, (float)0), currentBlock);
            face.name = "east";
        }
        else if ((x != renderSizeX - 1 && mapIDs[x + 1, y, z] != 0) & blockMap[x, y, z].east)
        {
            blockMap[x, y, z].east = false;
            Destroy(currentBlock.Find("east").gameObject, 0.6f);
        }

        //west
        if ((x == 0 || mapIDs[x - 1, y, z] == 0) & !blockMap[x, y, z].west)
        {
            GameObject face;
            blockMap[x, y, z].west = true;
            if (blockMap[x, y, z].orientation == 1)
                face = Instantiate(Blocks[BlockID].transform.Find("bottom").gameObject, new Vector3(X, Y + 0.25f, Z + 0.25f), Quaternion.Euler((float)0, (float)90, (float)0), currentBlock);
            else if (blockMap[x, y, z].orientation == 2)
                face = Instantiate(Blocks[BlockID].transform.Find("west").gameObject, new Vector3(X, Y + 0.25f, Z + 0.25f), Quaternion.Euler((float)0, (float)90, (float)90), currentBlock);
            else
                face = Instantiate(Blocks[BlockID].transform.Find("west").gameObject, new Vector3(X, Y + 0.25f, Z + 0.25f), Quaternion.Euler((float)0, (float)90, (float)0), currentBlock);
            face.name = "west";
        }
        else if ((x != 0 && mapIDs[x - 1, y, z] != 0) & blockMap[x, y, z].west)
        {
            blockMap[x, y, z].west = false;
            Destroy(currentBlock.Find("west").gameObject, 0.6f);
        }
        
        if (ConsoleDebugInfo) print("Block Updated: " + x + "," + y + "," + z);
    }

    int GetGroundLevel(int x, int z)
    {
        if (x > WorldSizeX || x < 0 || z > WorldSizeZ || z < 0)
            return -1 ;
        for (int y = renderSizeY-1; y>0; y--)
        {
            if (mapIDs[x,y,z]>0 )
                return y+1 ;
        }
        return 0;
    }

    int[] GenerateTree(int x, int z)
    {
        int y = GetGroundLevel(x, z);
        if (y == -1)
            return new int[0];
        int height = (int)(7 * UnityEngine.Random.value) + 5;
        int width = (int)(4 * UnityEngine.Random.value) + 5;
        int[] vals = new int[2];
        vals[0] = height;
        vals[1] = width;
        MinecraftTree tree = new MinecraftTree(height,width,x,y,z);
        //print("TREE GENERATED IN : " + x +" " +y+" " + z);
        
        for (int i = 0; i<width;i++)
        {
            for (int j = 0;j  < height; j++)
            {
                for (int k = 0; k < width; k++)
                {
                    if (tree.treeIDMap[i, j, k] != 0 && x+i< WorldSizeX && y+j<renderSizeY-1 && z+k < WorldSizeZ)
                    {
                        mapIDs[x + i, y + j, z + k] = tree.treeIDMap[i, j, k];
                        //CreateRenderedFakeBlock(x + i, y + j, z + k, 0, tree.treeIDMap[i, j, k], false);
                        //blockMap[x + i, y + j, z + k].blockGameObject.transform.parent = tree.tree.transform;
                    }
                }
            }
        }
        return vals;
    }

    void GenerateNewPerlinNoiseTerrain(int startX, int startZ, int endX, int endZ,float scaleDivider = 2,int steapnessX = 50, int steapnessZ = 50)
    {
        float scaler = WorldSizeY/ scaleDivider;
        
        float Y = 0;
        for (int x = startX; x< endX; x++)
        {
            for (int z = startZ; z < endZ; z++)
            {
                Y = (WorldSizeY-scaler)/2 + (scaler * Mathf.PerlinNoise((float)x * steapnessX/WorldSizeX, (float)z* steapnessZ / WorldSizeZ));
                if (Y < 0)
                {
                    print("y<0");
                    Y = 0;
                }
                else if (Y >= renderSizeY)
                {
                    print("y>renderSizeY");
                    Y = renderSizeY-1;
                }
                //else print("y is fine : " + y);
                int y = (int)Y;
                mapIDs[x, y, z] = 2;
                y--;
                int dirtDepth = 5;
                for (int i = 1; i <= dirtDepth; i++)
                {
                    var r = UnityEngine.Random.Range(0f, (float)dirtDepth - 1);
                    if (r < dirtDepth - i)
                    {
                        mapIDs[x , y, z ] = 1;
                    }
                    else
                    {
                        mapIDs[x , y, z ] = 4;
                    }
                    y--;
                    if (y < 0)
                        break;
                }
                while (y >= 0)
                {
                    mapIDs[x , y, z ] = 4;
                    y--;
                }
            }
        }
        print("PERLIN NOISE GENERATED");
    }

    void Generate2DTreeMap(int amount, int startPositionX = 100, int startPositionZ = 100, int mapSizeX = 50, int mapSizeZ = 50, bool DEBUG = false) // startPosition is supposed to default to {renderOffsetX,renderOffsetZ},  and mapSize should to {renderSizeX,renderSizeZ}
    {
        int y = 10;// for debug
        int[,] treeMap = new int[mapSizeX, mapSizeZ];
        int count = 0;
        while (count < amount)
        {
            for (int x = 0; x < mapSizeX; x++)
            {
                for (int z = 0; z < mapSizeZ; z++)
                {
                    if (treeMap[x, z] == 0 && UnityEngine.Random.value > 0.999)
                    {
                        int[] treeSize = GenerateTree(x+ startPositionX, z+ startPositionZ);
                        if (DEBUG)
                        {
                            GameObject cube = Instantiate(GameObject.Find("Cube"), new Vector3(x / 2, y + 1 / 2, z / 2), Quaternion.identity);//debug
                            cube.GetComponent<MeshRenderer>().material.color = Color.green;//debug
                        }
                        for (int i = 0; i < treeSize[1] + 1 ; i++)
                        {
                            for (int j = 0; j < treeSize[1] + 1; j++)
                            {
                                if (x - i > 0 & z - j > 0 && treeMap[x - i, z - j] == 0)
                                {
                                    treeMap[x - i, z - j] = -1;
                                    if (DEBUG)
                                    {
                                        GameObject cube5 = Instantiate(GameObject.Find("Cube"), new Vector3(x / 2 - i / 2, y, z / 2 - j / 2), Quaternion.identity);//debug
                                        cube5.GetComponent<MeshRenderer>().material.color = Color.red;//debug
                                    }
                                }
                                if (x - i > 0 & z + j < mapSizeZ && treeMap[x - i, z + j] == 0)
                                {
                                    treeMap[x - i, z + j] = -1;
                                    if (DEBUG)
                                    {
                                        GameObject cube3 = Instantiate(GameObject.Find("Cube"), new Vector3(x / 2 - i / 2, y, z / 2 + j / 2), Quaternion.identity);//debug
                                        cube3.GetComponent<MeshRenderer>().material.color = Color.red;//debug
                                    }
                                }
                                if (x + i < mapSizeX & z + j < mapSizeZ && treeMap[x + i, z + j] == 0 )
                                {
                                    treeMap[x + i, z + j] = -1;
                                    if (DEBUG)
                                    {
                                        GameObject cube2 = Instantiate(GameObject.Find("Cube"), new Vector3(x / 2 + i / 2, y, z / 2 + j / 2), Quaternion.identity);//debug
                                        cube2.GetComponent<MeshRenderer>().material.color = Color.red;//debug
                                    }
                                }

                                if (x + i < mapSizeX & z - j > 0 && treeMap[x + i, z - j] == 0)
                                {
                                    treeMap[x + i, z - j] = -1;
                                    if (DEBUG)
                                    {
                                        GameObject cube4 = Instantiate(GameObject.Find("Cube"), new Vector3(x / 2 + i / 2, y, z / 2 - j / 2), Quaternion.identity);//debug
                                        cube4.GetComponent<MeshRenderer>().material.color = Color.red;//debug
                                    }
                                }
                            }
                        }
                        treeMap[x, z] = 1;
                        count++;
                        goto endwhile;
                    }
                }
            }
        endwhile: { }
        }
    }

    float GetPlayerToMapBoundariesdist()
    {
        int blocksToGenerate1 = blocksToGenerate;
        for (int i = 0; i < 4; i++)
        {
            
            if (2* MapBoundaryPlanes[i].GetDistanceToPoint(FPScontroller.transform.position) > -Distance_for_Generating_terrain)
                return 2*MapBoundaryPlanes[i].GetDistanceToPoint(FPScontroller.transform.position);
        }
        return -12345f;
    }

    Vector3 GetPlayerToMapBoundariesNormal()
    {
        for (int i=0;i<4;i++)
        {
            if (2*MapBoundaryPlanes[i].GetDistanceToPoint(FPScontroller.transform.position) > -Distance_for_Generating_terrain)
                return MapBoundaryPlanes[i].normal.normalized;
        }
        return new Vector3(0,0,0);
    }

    void UpdateNewChunks(Vector3 direction)
    {
        int startX, startZ, endX, endZ;
        if (direction.x > 0)
        {
            print("GENERATE CHUNKS ON X>0");
            if (renderOffsetX + blocksToGenerate >WorldSizeX)
                return;
            //update chunk GameObjects and destroy old ones
            startX = renderOffsetX + renderSizeX;
            startZ = renderOffsetZ;
            endX = renderOffsetX + renderSizeX + blocksToGenerate;
            endZ = renderOffsetZ + renderSizeZ;

            GenerateChunks(startX, startZ, endX, endZ);
            for (int x = startX - renderSizeX; x < endX - renderSizeX; x++)
            {
                for (int z = startZ; z < endZ; z++)
                {
                    string chunkName = (x / Chunk.chunkSize).ToString() + "_" + (z / Chunk.chunkSize).ToString();
                    //print("Destroy chunkName : "+chunkName);
                    Destroy(GameObject.Find(chunkName));
                }
            }
            
            //update new blocks
            for (int x=  startX ; x<  endX ; x++)
            {
                for (int z = startZ ; z < endZ; z++)
                {
                    for (int y=0;y<WorldSizeY;y++)
                    {
                        CreateRenderedFakeBlock(x , y, z,0,0);
                    }
                }
            }
           // Generate2DTreeMap(5, startX, startZ, endX - startX, endZ - startZ);
            // update renderoffset
            renderOffsetX += blocksToGenerate;
            print("renderOffsetX = " + renderOffsetX);
        }

        if (direction.x < 0)
        {
            print("GENERATE CHUNKS ON X<0");

            if (renderOffsetX - blocksToGenerate < 0)
                return;
            startX = renderOffsetX - blocksToGenerate;
            startZ = renderOffsetZ;
            endX = renderOffsetX;
            endZ = renderOffsetZ + renderSizeZ;

            //update chunk GameObjects and destroy old ones
            GenerateChunks(startX, startZ, endX, endZ);
            for (int x = startX+renderSizeX; x < endX+ renderSizeX; x++)
            {
                for (int z = startZ; z < endZ; z++)
                {
                    string chunkName = (x / Chunk.chunkSize).ToString() + "_" + (z / Chunk.chunkSize).ToString();
                    //print("Destroy chunkName : " + chunkName);
                    Destroy(GameObject.Find(chunkName));
                }
            }

            //update new blocks
            for (int x = startX; x < endX; x++)
            {
                for (int z = startZ; z < endZ; z++)
                {
                    for (int y = 0; y < WorldSizeY; y++)
                    {
                        CreateRenderedFakeBlock(x, y, z,0,0);
                    }
                }
            }
           // Generate2DTreeMap(5, startX, startZ, endX - startX, endZ - startZ);
            // update renderoffset
            renderOffsetX -= blocksToGenerate;
            print("renderOffsetX = " + renderOffsetX);

        }

        if (direction.z > 0)
        {
            print("GENERATE CHUNKS ON Z>0");
            if (renderOffsetZ + blocksToGenerate >WorldSizeZ)
                return;
            //update chunk GameObjects and destroy old ones
            GenerateChunks(renderOffsetX , renderOffsetZ + renderSizeZ, renderOffsetX + renderSizeX , renderOffsetZ + renderSizeZ + blocksToGenerate);
            for (int x = renderOffsetX; x < renderOffsetX + renderSizeX; x++)
            {
                for (int z = renderOffsetZ; z < renderOffsetZ + blocksToGenerate; z++)
                {
                    string chunkName = (x / Chunk.chunkSize).ToString() + "_" + (z / Chunk.chunkSize).ToString();
                    //print("Destroy chunkName : " + chunkName);
                    Destroy(GameObject.Find(chunkName));
                }
            }

            //update new blocks
            for (int x = renderOffsetX; x < renderOffsetX+renderSizeX; x++)
            {
                for (int z = renderOffsetZ+ renderSizeZ; z < renderOffsetZ+renderSizeZ + blocksToGenerate; z++)
                {
                    for (int y = 0; y < WorldSizeY; y++)
                    {
                        CreateRenderedFakeBlock(x, y, z,0,0);
                    }
                }
            }
            // update renderoffset
            renderOffsetZ += blocksToGenerate;
            print("renderOffsetZ = " + renderOffsetZ);

        }

        if (direction.z < 0)
        {
            print("GENERATE CHUNKS ON Z<0");
            if (renderOffsetZ - blocksToGenerate < 0)
                return;
            //update chunk GameObjects and destroy old ones
            GenerateChunks(renderOffsetX , renderOffsetZ - blocksToGenerate, renderOffsetX + renderSizeX, renderOffsetZ );
            for (int x = renderOffsetX ; x < renderOffsetX + renderSizeX; x++)
            {
                for (int z = renderOffsetZ + renderSizeZ - blocksToGenerate; z < renderOffsetZ+renderSizeZ ; z++)
                {
                    string chunkName = (x / Chunk.chunkSize).ToString() + "_" + (z / Chunk.chunkSize).ToString();
                    //print("Destroy chunkName : " + chunkName);
                    Destroy(GameObject.Find(chunkName));
                }
            }

            //update new blocks
            for (int x = renderOffsetX; x < renderOffsetX+ renderSizeX; x++)
            {
                for (int z = renderOffsetZ - blocksToGenerate; z < renderOffsetZ; z++)
                {
                    for (int y = 0; y < WorldSizeY; y++)
                    {
                        CreateRenderedFakeBlock(x, y, z, renderOffsetX, renderOffsetZ);
                    }
                }
            }
            // update renderoffset
            renderOffsetZ -= blocksToGenerate;
            print("renderOffsetZ = " + renderOffsetZ);

        }
        //move map boundaries
        if (play.isLoaded)
            for (int i = 0; i < 4; i++)
                MapBoundaryPlanes[i].Translate(-blocksToGenerate/2 * direction.normalized);
        
    }

}

