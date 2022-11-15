using UnityEngine;
using UnityEngine.UI;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Linq;

public class Hotbar : MonoBehaviour
{
    //hotbar navigating
    public int HighlightedSlot;
    Color normalColor;
    Color highLightColor;
    private PlayerControls playercontrolsReference;
    private int hotbarWidth;

    public void Start()
    {
        highLightColor = new Color(1f, 1f, 1f, 1f);
        normalColor = new Color(.6f, .6f, .6f, 1f);
        playercontrolsReference = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControls>();
        hotbarWidth = transform.GetComponentInParent<Inventory>().width - 1;
        HighlightedSlot = 0;
    }
    

    [SerializeField]
    public KeyCode[] keyCodesForSlots = new KeyCode[999];
    [SerializeField]
    public int slotsInTotal;

#if UNITY_EDITOR
    [MenuItem("Master System/Create/Hotbar")]        //creating the menu item
    public static void menuItemCreateInventory()       //create the inventory at start
    {
        GameObject Canvas = null;
        if (GameObject.FindGameObjectWithTag("Canvas") == null)
        {
            GameObject inventory = new GameObject();
            inventory.name = "Inventories";
            Canvas = (GameObject)Instantiate(Resources.Load("Prefabs/Canvas - Inventory") as GameObject);
            Canvas.transform.SetParent(inventory.transform, true);
            GameObject panel = (GameObject)Instantiate(Resources.Load("Prefabs/Panel - Hotbar") as GameObject);
            panel.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            panel.transform.SetParent(Canvas.transform, true);
            GameObject draggingItem = (GameObject)Instantiate(Resources.Load("Prefabs/DraggingItem") as GameObject);
            Instantiate(Resources.Load("Prefabs/EventSystem") as GameObject);
            draggingItem.transform.SetParent(Canvas.transform, true);
            Inventory inv = panel.AddComponent<Inventory>();
            panel.AddComponent<InventoryDesign>();
            panel.AddComponent<Hotbar>();
            inv.getPrefabs();
        }
        else
        {
            GameObject panel = (GameObject)Instantiate(Resources.Load("Prefabs/Panel - Hotbar") as GameObject);
            panel.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, true);
            panel.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            Inventory inv = panel.AddComponent<Inventory>();
            panel.AddComponent<Hotbar>();
            DestroyImmediate(GameObject.FindGameObjectWithTag("DraggingItem"));
            GameObject draggingItem = (GameObject)Instantiate(Resources.Load("Prefabs/DraggingItem") as GameObject);
            draggingItem.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, true);
            panel.AddComponent<InventoryDesign>();
            inv.getPrefabs();
        }
        
    }
#endif

    void Update()
    {
        for (int i = 0; i < slotsInTotal; i++)
        {
            if (Input.GetKeyDown(keyCodesForSlots[i]))
            {
                if (transform.GetChild(1).GetChild(i).childCount != 0 && transform.GetChild(1).GetChild(i).GetChild(0).GetComponent<ItemOnObject>().item.itemType.ToString() == "food")
                {
                    if (transform.GetChild(1).GetChild(i).GetChild(0).GetComponent<ConsumeItem>().duplication != null && transform.GetChild(1).GetChild(i).GetChild(0).GetComponent<ItemOnObject>().item.maxStack == 1)
                    {
                        Destroy(transform.GetChild(1).GetChild(i).GetChild(0).GetComponent<ConsumeItem>().duplication);
                    }
                    transform.GetChild(1).GetChild(i).GetChild(0).GetComponent<ConsumeItem>().consumeIt();
                }
                else
                {
                    transform.GetChild(1).GetChild(HighlightedSlot).GetComponent<Image>().color = normalColor;
                    HighlightedSlot = i;
                    if (HighlightedSlot < 0) HighlightedSlot = hotbarWidth;
                    transform.GetChild(1).GetChild(HighlightedSlot).GetComponent<Image>().color = highLightColor;
                    playercontrolsReference.hotbarsItemSwitch = true;
                }
                    
            }
        }
        
        if (Input.mouseScrollDelta.y > 0)
        {
            transform.GetChild(1).GetChild(HighlightedSlot).GetComponent<Image>().color = normalColor;
            HighlightedSlot -= 1;
            if (HighlightedSlot < 0) HighlightedSlot = hotbarWidth;
            transform.GetChild(1).GetChild(HighlightedSlot).GetComponent<Image>().color = highLightColor;
            playercontrolsReference.hotbarsItemSwitch = true;
        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            
            transform.GetChild(1).GetChild(HighlightedSlot).GetComponent<Image>().color = normalColor;
            HighlightedSlot += 1;
            if (HighlightedSlot >= hotbarWidth+1 ) HighlightedSlot = 0;
            transform.GetChild(1).GetChild(HighlightedSlot).GetComponent<Image>().color = highLightColor;
            playercontrolsReference.hotbarsItemSwitch = true;
        }


    }

    public int getSlotsInTotal()
    {
        Inventory inv = GetComponent<Inventory>();
        return slotsInTotal = inv.width * inv.height;
    }
}
