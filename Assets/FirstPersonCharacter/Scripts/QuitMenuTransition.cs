using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;


public class QuitMenuTransition : MonoBehaviour
{
    public GameObject quitmenu;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("escape"))
        {
            Cursor.visible = !Cursor.visible;
            if (Cursor.visible) Cursor.lockState = CursorLockMode.Confined;
            else Cursor.lockState = CursorLockMode.Locked;

            quitmenu.SetActive(!quitmenu.activeSelf) ;

        }
        
    }
}
