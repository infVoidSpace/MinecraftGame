using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Flow : MonoBehaviour
{

    public Scene[] GameScenes;
    public int scenesCount=8;
    // Start is called before the first frame update
    void Start()
    {
        scenesCount = SceneManager.sceneCountInBuildSettings;
        GameScenes = new Scene[scenesCount];
        for (int i=0; i < scenesCount; i++)
        {
            print("scene "+i+" "+" is being loaded");
            GameScenes[i] = SceneManager.GetSceneByBuildIndex(i);
            print("loaded "+ SceneManager.GetSceneByBuildIndex(i).name);
        }

        SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
