using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinecraftTree
{
    
    public int height , width, x, groundLevelY, z; //height starts from 0
    private readonly int LOG_id=6, LEAVES_id=7;
    public int[,,] treeIDMap;
    public GameObject tree;
    public MinecraftTree(int height, int width, int x, int groundLevelY, int z)
    {
        if (width < 3)
            width = 3;
        else
            this.width = width;

        if (height < 3)
            height = 3;
        else
            this.height = height;

        this.x = x;
        this.z = z;
        this.groundLevelY = groundLevelY;
        treeIDMap = new int[width, height, width];
        string chunk = (x / Chunk.chunkSize).ToString() + "_" + (z / Chunk.chunkSize).ToString();
        if (GameObject.Find(chunk))
        {
            tree = new GameObject("Oak_tree");
            //Debug.Log("TREE GAMEOBJECT " + tree);
            tree.transform.position = new Vector3((float)x / 2, (float)(groundLevelY - generator.renderSizeY) / 2, (float)z / 2);
            tree.transform.parent = GameObject.Find(chunk).transform;
        }

        //generate logs ID
        if (height < 3) return;
        

        //generate leaves ID
        for (int i = 0; i < width - 1; i++)
        {
            for (int j = 0; j < width - 1; j++)
            {
                treeIDMap[i, height - 3, j] = LEAVES_id;
                //GameObject _leaves0 = Instantiate(log, tree.transform);
                //_leaves0.transform.localPosition = new Vector3((float)i / 2, (float)(height - 2) / 2, (float)j / 2);
                if (i != 0 && i != width-2 && j != 0 && j != width-2)
                {
                    treeIDMap[i, height - 2, j] = LEAVES_id;
                    //GameObject _leaves1 = Instantiate(log, tree.transform);
                    //_leaves1.transform.localPosition = new Vector3((float)i / 2, (float)(height - 1) / 2, (float)j / 2);

                    if (i != 1 && i != width-3  && j != 1 && j != width-3 )
                    {
                        treeIDMap[i, height-1, j] = LEAVES_id;
                        //GameObject _leaves2 = Instantiate(log, tree.transform);
                        //_leaves2.transform.localPosition = new Vector3((float)i / 2, (float)(height - 1) / 2, (float)j / 2);
                    }
                }
            }
        }
        for (int i = 0; i < height - 2; i++)
        {
            treeIDMap[width / 2 - 1, i, width / 2 - 1] = LOG_id;
            //GameObject _log = Instantiate(log, tree.transform);
            //_log.transform.localPosition = new Vector3((float)(width / 2 - 1)/2, (float)i /2, (float)(width / 2 - 1)/2);
        }
    }

}
