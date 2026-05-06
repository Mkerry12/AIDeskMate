using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Mouseclick : MonoBehaviour
{
    Ray ray;
    RaycastHit hit;
    GameObject Obj;
    public GameObject Chat_Box;
    public GameObject MusicPlayer;
    public bool isClicked = false;
    [SerializeField]
    public bool IS_MENU_OPEN = false;
    static public bool IS_MUSIC_OPEN = false;
    

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //此处设计左键为拖动，所以只使用中键和右键为点击
        if (Input.GetMouseButtonDown(2))
        {
            Debug.Log("Mouse Clicked");
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("中键Hit " + hit.collider.gameObject.name);
                Obj = hit.collider.gameObject;

                if (Obj.tag == "ClickObj")
                {
                    Debug.Log("ClickObj Clicked" + Obj.name);
                    isClicked = true;
                }


            }
        }
        //右键测试
        if (Input.GetMouseButtonDown(1))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                OpenMenu();

                Debug.Log("右键Hit " + hit.collider.gameObject.name);
            }

        }
    }

    //右键打开ChatBox
    public void OpenMenu()
    {

        if (IS_MENU_OPEN)
        {
            Chat_Box.SetActive(false);
        }
        if (!IS_MENU_OPEN)
        {
            Chat_Box.SetActive(true);
        }

        IS_MENU_OPEN = !IS_MENU_OPEN;
    }

    public void OnClickMusicPlayer()
    {
        if (IS_MUSIC_OPEN)
        {
            MusicPlayer.SetActive(false);
        }
        if (!IS_MUSIC_OPEN)
        {
            MusicPlayer.SetActive(true);
        }
        IS_MUSIC_OPEN = !IS_MUSIC_OPEN;
    }

    public void OnApplicationQuit()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}
