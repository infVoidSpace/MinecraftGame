using UnityEngine;
using System.Collections;

public class PickUpItem : MonoBehaviour
{
    public Item item;
    private Inventory _inventory;
    private Inventory _hotbar;

    private GameObject _player;
    public ItemDataBaseList database;
    //public GameObject advPanel;
    //advancements adv;

    void Start()
    {
        //GameObject advPanel = GameObject.FindGameObjectWithTag("adv");
        //adv = advPanel.GetComponent<advancements>();
        _player = GameObject.FindGameObjectWithTag("Player");
        if (_player != null)
        {
            database = _player.GetComponent<PlayerControls>().ItemDataBase;
            _inventory = _player.GetComponent<PlayerInventory>().inventory.GetComponent<Inventory>();
            _hotbar = GameObject.FindGameObjectWithTag("Hotbar").GetComponent<Inventory>();
        }
        gameObject.name=gameObject.name.Replace("(Clone)", "");
        string name = gameObject.name;
        if (database & item==null || item.itemValue==1)
        {
            foreach (Item listitem in database.itemList)
            {
                if (listitem.itemName.Equals(name))
                {
                    item = listitem;
                    break;
                }
            }
        }
        

    }
    
    // Update is called once per frame
    void Update()
    {
        if (_inventory != null && _hotbar!=null )
        {
           float distance = Vector3.Distance(this.gameObject.transform.position, _player.transform.position);

            if (distance <= .8f)
            {
                //check if advancements are all done else check advancement:
                bool markforcheck = false;
                for (int i=0; i<6; i++)
                {
                    if (advancements.advBool[i] == false)
                        markforcheck = true;
                }
                advancements.check = markforcheck;
                bool checkInventory = _inventory.checkIfItemAllreadyExist(item.itemID, item.itemValue);
                bool checkHotbar = false;
                if (!checkInventory)
                {
                    checkHotbar = _hotbar.checkIfItemAllreadyExist(item.itemID, item.itemValue);
                }
                
                if (checkInventory | checkHotbar)
                {
                    _player.GetComponent<AudioSource>().Play();
                    Destroy(this.gameObject);
                }
                else if (_hotbar.ItemsInInventory.Count < (_hotbar.width * _hotbar.height))
                {
                    
                    _player.GetComponent<AudioSource>().Play();
                    _hotbar.addItemToInventory(item.itemID, item.itemValue);
                    _hotbar.updateItemList();
                    _hotbar.stackableSettings();
                    Destroy(this.gameObject);
                    _player.GetComponent<PlayerControls>().hotbarsItemSwitch = true;
                }
                else if (_inventory.ItemsInInventory.Count < (_inventory.width * _inventory.height))
                {
                    _player.GetComponent<AudioSource>().Play();
                    _inventory.addItemToInventory(item.itemID, item.itemValue);
                    _inventory.updateItemList();
                    _inventory.stackableSettings();
                    Destroy(this.gameObject);
                }
                

            }
        }
    }

}