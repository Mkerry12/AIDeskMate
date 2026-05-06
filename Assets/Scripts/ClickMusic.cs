using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickMusic : MonoBehaviour
{
    public AudioSource clip;
    private Mouseclick mouseClick;

    // Start is called before the first frame update
    void Start()
    {
        clip = GetComponent<AudioSource>();
        mouseClick = FindObjectOfType<Mouseclick>(); // 查找场景中的 Mouseclick 实例
    }

    // Update is called once per frame
    void Update()
    {
        if (mouseClick.isClicked == true)
        {
            if(clip.isPlaying == false)
            {
                clip.Play();
            }

            mouseClick.isClicked = false;

        }
    }
}
