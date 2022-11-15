using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public String chunk;
    //map ID
    public int x, z, y, type, orientation = 0;
    //block breaking variables
    public int breakingDamage , breakingDamageCapacity;
    public float instTime;
    //game position
    public float X, Z, Y;
    public bool up, down, south, north, east, west, isBreaking, broken ;
    public GameObject blockGameObject = new GameObject("block");
    public Item blockItem;

    public Block(int type, Vector3 position, int x, int y, int z)
    {
        instTime = Time.time;
        this.x = x;
        this.y = y;
        this.z = z;
        X = position.x;
        Y = position.y;
        Z = position.z;
        this.type = type;
        blockItem = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControls>().ItemDataBase.getItemByID(type);
        
    }


    public Block(int type, Vector3 position, int x, int y, int z, Transform chunk, int orientable = 0)
    {
        instTime = Time.time;
        this.x = x;
        this.y = y;
        this.z = z;
        X = position.x;
        Y = position.y;
        Z = position.z;
        this.type = type;
        blockItem = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControls>().ItemDataBase.getItemByID(type);
        blockGameObject.name = "block";
        blockGameObject.AddComponent<BoxCollider>();
        blockGameObject.transform.SetPositionAndRotation(position, Quaternion.identity);
        blockGameObject.GetComponent<BoxCollider>().center = new Vector3(0.25f, 0.25f, 0.25f);
        blockGameObject.GetComponent<BoxCollider>().size = new Vector3(0.5f, 0.5f, 0.5f);
        blockGameObject.transform.SetParent(chunk);
        this.chunk = chunk.name;
        orientation = orientable;
    }
    
    


}