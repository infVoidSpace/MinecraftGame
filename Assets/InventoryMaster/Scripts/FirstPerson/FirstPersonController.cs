using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    public float movementspeed;// = 5.0f;
    public float mouseSensitivity;// = 2.0f;
    public float verticalAngleLimit;// = 90.0f;
    public float jumpSpeed;// = 5f;
    public float gravitymultiplier;//= 2f;
    float verticalRotation;// = 0;

    GameObject _inventory;
    GameObject _tooltip;
    GameObject _character;
    public GameObject _dropBox;
    public bool showInventory = false;
    public float verticalVelocity;
    public Quaternion rot;
    GameObject inventory;
    public GameObject quitmenu;
    GameObject craftSystem;
    GameObject characterSystem;

    Camera firstPersonCamera;

    CharacterController characterController;
    // Use this for initialization
    void Start()
    {

        movementspeed = 3.0f;
        mouseSensitivity = 1.8f;
        verticalAngleLimit = 90.0f;
        jumpSpeed = 3f;
        gravitymultiplier = 1f;
        verticalRotation = 0;
        verticalVelocity = 0;

        Physics.gravity *= gravitymultiplier;
        firstPersonCamera = Camera.main.GetComponent<Camera>();
        characterController = GetComponent<CharacterController>();
        
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            PlayerInventory playerInv = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInventory>();
            if (playerInv.inventory != null)
                inventory = playerInv.inventory;
            if (playerInv.craftSystem != null)
                craftSystem = playerInv.craftSystem;
            if (playerInv.characterSystem != null)
                characterSystem = playerInv.characterSystem;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (!lockMovement())
        {
            //Rotation
            float rotationLeftRight = Input.GetAxis("Mouse X") * mouseSensitivity;
            transform.Rotate(0, rotationLeftRight, 0);

            verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            verticalRotation = Mathf.Clamp(verticalRotation, -verticalAngleLimit, verticalAngleLimit);
            firstPersonCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);

            //Movement
            float forwardSpeed = Input.GetAxis("Vertical") * movementspeed;
            float sideSpeed = Input.GetAxis("Horizontal") * movementspeed;



            if ( characterController.isGrounded)
            {
                // keep a minimal -dy to stay Grounded
                verticalVelocity = -0.001f; 
                if (Input.GetButtonDown("Jump"))
                    verticalVelocity = jumpSpeed;
            }
            else verticalVelocity += Physics.gravity.y * Time.deltaTime;

            Vector3 speed = new Vector3(sideSpeed, verticalVelocity, forwardSpeed);
            rot = transform.rotation;
            speed = transform.rotation * speed;

            characterController.Move(speed * Time.deltaTime);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }

    }


    bool lockMovement()
    {
        if (inventory != null && inventory.activeSelf)
            return true;
        else if (characterSystem != null && characterSystem.activeSelf)
            return true;
        else if (craftSystem != null && craftSystem.activeSelf)
            return true;
        else if (quitmenu != null && quitmenu.activeSelf)
            return true;
        else
            return false;
    }
}
