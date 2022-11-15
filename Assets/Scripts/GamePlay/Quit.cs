using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class Quit : MonoBehaviour
{
    public Button cancel;
    public Button quit;

    // Start is called before the first frame update
    void Start()
    {
        Button cancel_btn = cancel.GetComponent<Button>();
        cancel_btn.onClick.AddListener(new UnityEngine.Events.UnityAction(Cancel));
        Button quit_btn = quit.GetComponent<Button>();
        quit_btn.onClick.AddListener(new UnityEngine.Events.UnityAction(quitgame));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp("escape"))
        {
            //Cancel();
        }
        
    }

    void Cancel()
    {
        print("CANCEL");
        Cursor.visible = !Cursor.visible;
        gameObject.SetActive(false);
    }

    void quitgame()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#endif
        Application.Quit();

    }
}

