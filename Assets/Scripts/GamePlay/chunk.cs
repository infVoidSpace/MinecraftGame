using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{

    public String chunkID;
    public int x, y;
    private static int height = generator.renderSizeY;
    public static int chunkSize = 10;
    public int[,,] table = new int[chunkSize, height, chunkSize];

    public GameObject chunkGameObject = new GameObject();
    public Chunk(String id,int x,int y, Vector3 position, Transform map)
    {
        this.x = x;
        this.y = y;
        chunkID = id;
        chunkGameObject.transform.SetPositionAndRotation(position,Quaternion.identity);
        chunkGameObject.name = id;
        chunkGameObject.transform.SetParent(map);
    }
}