using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class StartUIControl : MonoBehaviour
{
    public Button play;
    public Button options;
    public Button about;
    public Button quit;

    public Button back;
    public Button back_about;
    public GameObject optionsMenu;
    public GameObject aboutMenu;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Button play_btn = play.GetComponent<Button>();
        play_btn.onClick.AddListener(startGame);

        Button options_btn = options.GetComponent<Button>();
        options_btn.onClick.AddListener(toggleOptions);

        Button back_btn = back.GetComponent<Button>();
        back_btn.onClick.AddListener(toggleOptions);

        Button back_about_btn = back_about.GetComponent<Button>();
        back_about_btn.onClick.AddListener(toggleAbout);

        Button about_btn = about.GetComponent<Button>();
        about_btn.onClick.AddListener(toggleAbout);

        Button quit_btn = quit.GetComponent<Button>();
        quit_btn.onClick.AddListener(new UnityEngine.Events.UnityAction(QuitGame) );
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("p"))
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#endif
            Application.Quit();
        } 
    }

    void startGame()
    {
        
        if (SceneManager.GetSceneByBuildIndex(0).IsValid())
        {
            print("Starting play scene" );
            SceneManager.LoadScene(1);
            //SceneManager.UnloadSceneAsync(0);
        }
        else
        {
            print(SceneManager.GetSceneByBuildIndex(1).name);
            print("No valid play scene");
        }
    }
    void toggleAbout()
    {
        print("neh.. this button is just to add to the GUI ");
        aboutMenu.SetActive(!aboutMenu.activeSelf);
    }
    void toggleOptions()
    {
        print("Move to options GUI");
        optionsMenu.SetActive(!optionsMenu.activeSelf);
    }

    void QuitGame()
    {
        print("Quit game :(");
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode() ;
#endif
        Application.Quit();
    }

}