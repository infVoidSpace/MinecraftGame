using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerControls : MonoBehaviour
{
    private string[] holdingTool;
    private Dictionary<string, int> toolTypeDict = new Dictionary<string, int> { { "", 1 } ,{ "wood", 2 }, { "stone", 3 }, { "iron", 4 }, { "gold", 5 }, { "diamond", 6 }, };
    private bool holdingBlockBool;
    private int holdingBlockID;
    private int blockDamage, blockDamageCapacity ;
    private bool breakingBlockBool;
    private int blockBreakAnimationCounter;
    private int[] currentBlockIdXYZ = new int[3] { 0, 0, 0 };
    private int[] previousBlockXYZ = new int[3] { 0, 0, 0 };
    private Collider currentBlockHit;
    private Collider blockCollider;
    private RaycastHit hit;
    private bool inventoryIsDisplayed;
    private bool quitmenuIsDisplayed;
    private GameObject ActiveSlot;
    private Item activeItem;
    private bool generarateNewAreaBool;
    private float distanceToMapEdge;
        
    public ItemDataBaseList ItemDataBase;
    public GameObject drops;
    public GameObject Aim;
    public bool hotbarsItemSwitch;
    public GameObject objectHolder;
    public GameObject inventory;
    public GameObject quitmenu;
    public GameObject hotbar;
    

    void Start()
    {
       

        blockBreakAnimationCounter = -1;
        blockDamageCapacity = 1000;
        holdingTool = new string[] { "hand", ""};
        hotbarsItemSwitch = true;
        hotbar = GameObject.FindGameObjectWithTag("Hotbar");
        inventory = transform.GetComponent<PlayerInventory>().inventory;
        Aim = GameObject.FindGameObjectWithTag("aim");
        objectHolder = GameObject.FindGameObjectWithTag("objectHolder");
        //Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        if (Input.GetMouseButton(0) | Input.GetMouseButtonDown(0) | Input.GetMouseButtonUp(0) )
        {
            previousBlockXYZ[0] = currentBlockIdXYZ[0];
            previousBlockXYZ[1] = currentBlockIdXYZ[1];
            previousBlockXYZ[2] = currentBlockIdXYZ[2];
        }
    }

    void Update()
    {

        if (generator.ConsoleDebugInfo && Input.anyKeyDown)
        {
            print("KB Input : "+Input.inputString);

            print("Mouse Input left : " + Input.GetMouseButton(0));
            print("Mouse Input right : " + Input.GetMouseButton(1));
        }

        //check if inventory is open to not mine while it is
        inventoryIsDisplayed = inventory.activeSelf;
        quitmenuIsDisplayed = quitmenu.activeSelf;
        if (inventoryIsDisplayed | quitmenuIsDisplayed) Aim.SetActive(false);
        else Aim.SetActive(true);

        if (Input.GetMouseButtonUp(0) && breakingBlockBool)
        {
            print("MOUSE BUTTON UP , blockDamage is : " + blockDamage);
            blockDamage = 0;
            UpdateBlockBreakAnimation(true, false);
            blockBreakAnimationCounter = -1;
            breakingBlockBool = false;
        }
        
        //mine or place a block
        if (!inventoryIsDisplayed && !quitmenuIsDisplayed && (Input.GetMouseButton(0) ^ Input.GetMouseButtonDown(1)))
        {
            //create a ray and check if it hit anything
            Ray ray = new Ray(transform.GetChild(0).position, transform.GetChild(0).forward);
            if (Physics.Raycast(ray, out RaycastHit outhit, 2))
            {
                hit = outhit;
                if (generator.ConsoleDebugInfo) Debug.DrawRay(transform.GetChild(0).GetChild(0).position, transform.GetChild(0).forward, Color.red, 5f);
                if (generator.ConsoleDebugInfo) print("hit successful");

                if (hit.collider.transform.parent.name == "block")
                    blockCollider = hit.collider.transform.parent.GetComponent<Collider>();
                else if (hit.collider.name == "block")
                    blockCollider = hit.collider;
                else
                    return;
                int x = 0, y = 0, z = 0, orientation = 0;

                //the ray hit a block on the following mapID coordinates:
                x = (int)((float)blockCollider.transform.position.x * 2);
                y = (int)((float)blockCollider.transform.position.y * 2 + generator.renderSizeY / 2);
                z = (int)((float)blockCollider.transform.position.z * 2);
                currentBlockIdXYZ[0] = x ;
                currentBlockIdXYZ[1] = y;
                currentBlockIdXYZ[2] = z;

                //BREAK BLOCK
                //if ray hit and was left clicked, start to break the block : 
                //reference the block script: switch Block.isBreaking while operating
                //                            while (isBreaking) have a timer to increase the block damage (ref to holding tool and item attributes)
                //                            if block damage hit max then break block
                if (!inventoryIsDisplayed & Input.GetMouseButton(0))
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        print("MOUSE 0 DOWN");
                        previousBlockXYZ[0] = currentBlockIdXYZ[0];
                        previousBlockXYZ[1] = currentBlockIdXYZ[1];
                        previousBlockXYZ[2] = currentBlockIdXYZ[2];
                        blockDamage = 0;
                        breakingBlockBool = true;
                        currentBlockHit = blockCollider;
                        blockBreakAnimationCounter = -1;
                    }
                    if (currentBlockHit!=null && !currentBlockHit.Equals(blockCollider))
                    {
                        blockDamage = 0;
                        UpdateBlockBreakAnimation(true, false);
                        blockBreakAnimationCounter = -1;
                        //print("currentBlock : " + currentBlockHit.transform + "  !=  " + "last blockCollider : " + blockCollider.transform);
                        breakingBlockBool = false;
                        currentBlockHit = blockCollider;

                        return;
                    }

                    if (blockDamage == 0)
                    {
                        breakingBlockBool = true;
                        currentBlockHit = blockCollider;
                        blockBreakAnimationCounter = -1;
                    }
                    if (generator.blockMap[currentBlockIdXYZ[0], currentBlockIdXYZ[1], currentBlockIdXYZ[2]] != null &&
                        generator.blockMap[currentBlockIdXYZ[0], currentBlockIdXYZ[1], currentBlockIdXYZ[2]].blockItem.itemAttributes.Find(attribute => attribute.attributeName == "toolTypeRequired") != null &&
                        (generator.blockMap[currentBlockIdXYZ[0], currentBlockIdXYZ[1], currentBlockIdXYZ[2]].blockItem.itemAttributes.Find(attribute => attribute.attributeName == "toolTypeRequired").attributeValue <= toolTypeDict[holdingTool[1]] ||
                        generator.blockMap[currentBlockIdXYZ[0], currentBlockIdXYZ[1], currentBlockIdXYZ[2]].blockItem.itemAttributes.Find(attribute => attribute.attributeName == "uniqeToolId").attributeValue == activeItem.itemID))
                    {
                        int toolmultiplier = generator.blockMap[currentBlockIdXYZ[0], currentBlockIdXYZ[1], currentBlockIdXYZ[2]].blockItem.itemAttributes.Find(attribute => attribute.attributeName.Replace("MiningLevel", "") == holdingTool[0]).attributeValue;
                        int diamondmultiplier = toolTypeDict[holdingTool[1]];
                        blockDamage += (int)(10 * Time.deltaTime * toolmultiplier * diamondmultiplier);
                    }
                    else
                    {
                        print("Block is  "+ currentBlockIdXYZ[0] + " " + currentBlockIdXYZ[1] + " " + currentBlockIdXYZ[2]);
                        print(generator.blockMap[currentBlockIdXYZ[0], currentBlockIdXYZ[1], currentBlockIdXYZ[2]]);//.blockItem.itemAttributes.Find(attribute => attribute.attributeName.Replace("MiningLevel", "") == "toolTypeRequired").attributeValue);
                        blockDamage = 10;
                    }

                    //print("Tool Block attribute Value" + toolmultiplier);
                    //print("increase on the damaged block" + (int)(100 * Time.deltaTime * toolmultiplier));
                    //print("blockdamage " + blockDamage);
                    // update blockTexture to #0 breaking texture
                    if (blockBreakAnimationCounter == -1)
                    {
                        if (previousBlockXYZ != currentBlockIdXYZ)

                            print("block.breakingDamage =0 " + blockDamage);
                        UpdateBlockBreakAnimation(false, true, true);
                    }


                    //print("instTime " + generator.blockMap[currentBlockIdXYZ[0], currentBlockIdXYZ[1], currentBlockIdXYZ[2]].instTime);
                    if (blockDamage > blockDamageCapacity / 5 && blockBreakAnimationCounter == 0)
                    {
                        print("block.breakingDamage >1/3 " + blockDamage);
                        //update block texture to #2 breaking texture
                        UpdateBlockBreakAnimation();
                    }
                    if (blockDamage > blockDamageCapacity * 2 / 5 && blockBreakAnimationCounter == 1)
                    {
                        print("block.breakingDamage > 2/3 " + blockDamage);
                        //update block texture to last breaking texture
                        UpdateBlockBreakAnimation();

                    }
                    if (blockDamage > blockDamageCapacity * 3 / 5 && blockBreakAnimationCounter == 2)
                    {
                        print("block.breakingDamage > 2/3 " + blockDamage);
                        //update block texture to last breaking texture
                        UpdateBlockBreakAnimation();
                    }
                    if (blockDamage > blockDamageCapacity * 4 / 5 && blockBreakAnimationCounter == 3)
                    {
                        print("block.breakingDamage > 2/3 " + blockDamage);
                        //update block texture to last breaking texture
                        UpdateBlockBreakAnimation();
                    }
                    if (blockDamage >= blockDamageCapacity)
                    {
                        UpdateBlockBreakAnimation(true, false);
                        print("BLOCK BROKEN");
                        breakingBlockBool = false;
                        blockDamage = 0;
                        blockBreakAnimationCounter = -1;


                        //spawn block item on the ground
                        string blockName = blockCollider.gameObject.GetComponentInChildren<MeshRenderer>().materials[0].name.Split('_')[0];
                        GameObject newBlockSpawned = Instantiate(ItemDataBase.getItemByName(blockName + "_item").itemModel.gameObject, blockCollider.transform.position + new Vector3(0.25f, 0.25f, 0.25f), blockCollider.transform.rotation, drops.transform);
                        //destroy block object and update MapID reference
                        Destroy(generator.blockMap[currentBlockIdXYZ[0], currentBlockIdXYZ[1], currentBlockIdXYZ[2]].blockGameObject);
                        generator.mapIDs[currentBlockIdXYZ[0], currentBlockIdXYZ[1], currentBlockIdXYZ[2]] = 0;
                        generator.blockMap[currentBlockIdXYZ[0], currentBlockIdXYZ[1], currentBlockIdXYZ[2]] = null;
                        //cause a block update
                        generator.blockToUpdate[0] = currentBlockIdXYZ[0];
                        generator.blockToUpdate[1] = currentBlockIdXYZ[1];
                        generator.blockToUpdate[2] = currentBlockIdXYZ[2];
                        generator.blockToUpdate[3] = orientation;
                        generator.blockUpdateRequired = true;
                    }

                }

                //place block
                if (!inventoryIsDisplayed & Input.GetMouseButtonDown(1) && holdingBlockBool)
                {

                    Item HoldingItem = ActiveSlot.transform.GetComponentInChildren<ItemOnObject>().item;

                    currentBlockIdXYZ[0] += (int)hit.normal.x;
                    currentBlockIdXYZ[1]+= (int) hit.normal.y;
                    currentBlockIdXYZ[2] += (int)hit.normal.z;
                    bool orientable = false;
                    foreach (ItemAttribute att in HoldingItem.itemAttributes)
                    {
                        print(HoldingItem.itemName + " attribure " + att.attributeName + " is " + att.attributeValue);
                        if (att.attributeName == "orientable" && att.attributeValue != 0)
                        {
                            orientable = true;
                            break;
                        }
                    }
                    if (orientable)
                    {

                        print("hit.normal.x,y,z " + hit.normal.x + " " + hit.normal.y + " " + hit.normal.z);

                        if (hit.normal.x == 1 | hit.normal.x == -1)
                            orientation = 1;
                        if (hit.normal.z == 1 | hit.normal.z == -1)
                            orientation = 2;
                    }
                    //check for possible collisions and cause a block update
                    Collider[] intersections = Physics.OverlapBox(new Vector3(blockCollider.transform.position.x + 0.25f + hit.normal.x / 2, blockCollider.transform.position.y + 0.25f + hit.normal.y / 2, blockCollider.transform.position.z + 0.25f + hit.normal.z / 2), new Vector3(0.125f,0.125f,0.125f));
                    
                    print("intersections.Length : "+intersections.Length);
                    if (intersections.Length==0)
                    {
                        DecreaseHotbarItemValue();
                        generator.mapIDs[currentBlockIdXYZ[0], currentBlockIdXYZ[1], currentBlockIdXYZ[2]] = holdingBlockID;
                        generator.blockToUpdate[0] = currentBlockIdXYZ[0];
                        generator.blockToUpdate[1] = currentBlockIdXYZ[1];
                        generator.blockToUpdate[2] = currentBlockIdXYZ[2];
                        generator.blockToUpdate[3] = orientation;
                        generator.blockUpdateRequired = true;
                        if (generator.ConsoleDebugInfo) print("ray on : " + x + "," + y + "," + z);
                        
                    }
                    
                }

            }
        }
    
        //drop item on q press
        if (Input.GetKeyDown("q") && ActiveSlot.transform.childCount > 0 )
        {
            Item item = ActiveSlot.transform.GetComponentInChildren<ItemOnObject>().item;
            GameObject dropItem = Instantiate(item.itemModel);
            dropItem.AddComponent<PickUpItem>();
            dropItem.GetComponent<PickUpItem>().item = ItemDataBase.getItemByID(item.itemID);
            dropItem.transform.localPosition = transform.localPosition + transform.forward ;
            DecreaseHotbarItemValue();
        }

        //apply active hotbar item
        if (hotbarsItemSwitch)
        {
            if (objectHolder.transform.childCount > 0) Destroy(objectHolder.transform.GetChild(0).gameObject);
            
            int hotbarHighlightedSlot = hotbar.GetComponent<Hotbar>().HighlightedSlot;

            ActiveSlot = hotbar.transform.GetChild(1).GetChild(hotbarHighlightedSlot).gameObject;
            if (ActiveSlot.transform.childCount == 0)
            {
                holdingTool = new string[] { "hand", "" };
                holdingBlockID = -1;
                print("holdingTool : " + holdingTool[0] + " " + holdingTool[1]);
                hotbarsItemSwitch = false;
                holdingBlockBool = false;
                return;
            }
            activeItem = ActiveSlot.transform.GetChild(0).GetComponent<ItemOnObject>().item;
            if (activeItem.itemType.ToString().Equals("Blocks"))
            {
                holdingTool = new string[] { "hand", "" };
                holdingBlockID = activeItem.itemID;
                print("holdingTool : " + holdingTool[0] + " " + holdingTool[1]);
                holdingBlockBool = true;
            }
            else if (activeItem.itemType.ToString().Equals("Tools"))
            {
                holdingTool = activeItem.itemName.Split('_');
                print("holdingTool : "+ holdingTool[0]+" "+ holdingTool[1]);

                holdingBlockID = activeItem.itemID;
                holdingBlockBool = false;
            }
            else holdingBlockBool = false;

            HoldObject(activeItem);
            hotbarsItemSwitch = false;

        }
        
    }
            
    void UpdateBlockBreakAnimation(bool destroy = true, bool instantiate = true,bool newBlock = false)
    {
        blockBreakAnimationCounter++;
        int x, y, z;
        if (newBlock)
        {
            x = currentBlockIdXYZ[0];
            y = currentBlockIdXYZ[1];
            z = currentBlockIdXYZ[2];
       }
        else
        {
            x = previousBlockXYZ[0];
            y = previousBlockXYZ[1];
            z = previousBlockXYZ[2];
        }
        
        if (generator.blockMap[x, y, z] == null)
            return;
        
        int tileCount = generator.blockMap[x, y, z].blockGameObject.transform.childCount;
        if (tileCount == 0)
            return;
        int id = generator.blockMap[currentBlockIdXYZ[0], currentBlockIdXYZ[1], currentBlockIdXYZ[2]].blockItem.itemID;
        AudioSource breaking = transform.GetChild(1).GetChild(id - 1).GetComponent<AudioSource>();
        if (!breaking.isPlaying)
        {
            breaking.PlayOneShot(breaking.clip, 0.5f);
            
            breaking.PlayDelayed(0.5f);
            breaking.PlayDelayed(1);
        }
        for (int i = 0; i<tileCount; i++)
        {
            
            if (destroy && generator.blockMap[x, y, z].blockGameObject.transform.GetChild(i).childCount > 0)
            {
                breaking.Play();
                int breakcount = generator.blockMap[x, y, z].blockGameObject.transform.GetChild(i).childCount;
                for (int j = 0; j < breakcount; j++)
                    Destroy(generator.blockMap[x, y, z].blockGameObject.transform.GetChild(i).GetChild(j).gameObject); 
            }
            if (instantiate )
                Instantiate(generator.blockBreakAnimation[blockBreakAnimationCounter], generator.blockMap[x, y, z].blockGameObject.transform.GetChild(i));
        }

    }

    void DecreaseHotbarItemValue()
    {
        if (ActiveSlot.transform.childCount > 0 && ActiveSlot.transform.GetChild(0).GetComponent<ItemOnObject>().item.itemValue == 1)
        {
            Destroy(objectHolder.transform.GetChild(0).gameObject);
            Destroy(ActiveSlot.transform.GetChild(0).gameObject);
            holdingBlockBool = false;
        }
        else
            ActiveSlot.transform.GetChild(0).GetComponent<ItemOnObject>().item.itemValue -= 1;
    }

    void HoldObject(Item item)
    {
        GameObject Active_Item = Instantiate( item.itemModel,transform.GetChild(0).GetChild(0));
        //Active_Item.transform.position = objectHolder.transform.position;
        //Active_Item.transform.rotation = objectHolder.transform.rotation;
        //Active_Item.transform.localScale.Scale(new Vector3(2, 2, 2));
        Destroy(Active_Item.GetComponent<Rigidbody>());
        Destroy(Active_Item.GetComponent<PickUpItem>());
    }

}