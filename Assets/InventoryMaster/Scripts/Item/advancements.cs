using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class advancements : MonoBehaviour
{
    public GameObject player;
    public string[] advText = new string[6];
    public static List<int> itemIDs = new List<int> { 6, 8, 9, 10, 4, 11 };
    List<Item> items;
    public static List<bool> advBool = new List<bool> { false, false, false, false, false, false } ;
    public static bool check = false;
    AudioSource rewardsound;
    public GameObject WinPanel;
    public GameObject LosePanel;
    public GameObject progBar;
    public GameObject timerBar;
    private int counter;
    private float timer;
    private bool timerIsRunning;
    // Start is called before the first frame update
    void Start()
    {
        timer = 60;
        counter = 0;
        timerIsRunning = true;
        for (int i = 0; i < 6; i++)
            advBool[i] = false;
        advText[0] = "Get logs";
        advText[1] = "Craft planks";
        advText[2] = "Craft sticks";
        advText[3] = "Craft pickaxe";
        advText[4] = "Get stone";
        advText[5] = "Craft stone pickaxe";
        transform.Find("Goals").Find("Current goal").GetComponent<UnityEngine.UI.Text>().text = advText[0];
        rewardsound = transform.GetComponent<AudioSource>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (timerIsRunning)
        {
            if (timer < 0)
            {
                timerIsRunning = false;
                EndGame();

            }
            else if (timer - Time.deltaTime <= Mathf.Floor(timer))
            {
                UpdateTimerBar();
            }
            
            timer = timer - Time.deltaTime;
        }
        if (check)
        {
            List<Item> listInventory = player.GetComponent<PlayerControls>().inventory.GetComponent<Inventory>().getItemList(); 
            List<Item> listHotbar = player.GetComponent<PlayerControls>().hotbar.GetComponent<Inventory>().getItemList(); 
            print("CHECK");
            //print(player.GetComponent<Inventory>().getItemList());
            for (int i=0; i<6; i++)
            {
                print("itemIDs[ "+i+" ] = "+ itemIDs[i]);
                if (i == 0
                    & advBool[i] == false
                    & (listInventory.Find(item => item.itemID == itemIDs[i])!=null
                        | listHotbar.Find(item => item.itemID == itemIDs[i]) != null
                    ))
                {
                    transform.Find("Goals").Find("Current goal").GetComponent<UnityEngine.UI.Text>().text = advText[i + 1];
                    advBool[i] = true;
                    counter = i + 1;
                    rewardsound.Play();
                }
                else if (i!=0 && advBool[i] == false && advBool[i - 1] == true
                         && (listInventory.Find(item => item.itemID == itemIDs[i]) != null
                        | listHotbar.Find(item => item.itemID == itemIDs[i]) != null
                         ))
                {
                    if(i!=5)
                        transform.Find("Goals").Find("Current goal").GetComponent<UnityEngine.UI.Text>().text = advText[i + 1];
                    advBool[i] = true;
                    counter = i + 1;
                    rewardsound.Play();
                }
                if (!advBool.Contains(false))
                {
                    transform.gameObject.SetActive(false);
                    WinPanel.SetActive(true);
                    WinPanel.GetComponent<AudioSource>().Play();
                }
            }
            check = false;
            updateProgBar();
        }
    }

    void updateProgBar()
    {        
        float sizeProgBar = progBar.GetComponent<RectTransform>().rect.size.x;
        progBar.transform.GetChild(0).GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge( RectTransform.Edge.Right, (6 - counter) * sizeProgBar / 6, counter * sizeProgBar / 6) ;
        progBar.transform.GetChild(1).GetComponent<Text>().text = (counter*100/6).ToString()+ " %";
    }
    void UpdateTimerBar()
    {
        float sizeTimerBar = timerBar.GetComponent<RectTransform>().rect.size.x;
        timerBar.transform.GetChild(0).GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, (60 - timer) * sizeTimerBar / 60, timer * sizeTimerBar / 60);
        timerBar.transform.GetChild(1).GetComponent<Text>().text = (Mathf.Floor(timer)).ToString() + " seconds left";
    }
    
    void EndGame()
    {
        transform.gameObject.SetActive(false);
        LosePanel.SetActive(true);
        LosePanel.GetComponent<AudioSource>().Play();
        if (SceneManager.GetSceneByBuildIndex(1).IsValid())
        {
            SceneManager.LoadScene(0);
            
        }
    }
}
