using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Config : MonoBehaviour
{

    GameObject flow;
    Scene[] scenes;
    public bool CustumTerrainGeneretion;
    //GameObject player = GameObject.FindGameObjectWithTag("Player");
    //GameObject Generator = GameObject.FindGameObjectWithTag("Generator");
    //Toggle custumTerrainGeneretion = GameObject.Find("terrain_generation").GetComponent<Toggle>();

    void Start()
    {
        flow = GameObject.Find("flow");
        scenes = flow.GetComponent<Flow>().GameScenes;



    }
    void Toggle_CustumTerrainGeneretion()
    {
        CustumTerrainGeneretion = !CustumTerrainGeneretion;
        print("toggled");
    }

}