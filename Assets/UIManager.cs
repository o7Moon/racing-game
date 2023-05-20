using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] Text timer;
    public GameObject pauseMenu;
    [SerializeField] Slider sens;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) && RunManager.Instance.state == RunManager.RunState.Exploring && !pauseMenu.activeSelf)
        {
            Cursor.lockState = CursorLockMode.None;
            pauseMenu.SetActive(true);
        }

        else if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) && RunManager.Instance.state == RunManager.RunState.Exploring && pauseMenu.activeSelf)
        {
            Cursor.lockState = CursorLockMode.Locked;
            pauseMenu.SetActive(false);
        }

        PlayerController.Instance.sensitivity = sens.value;
    }

    public void ShowTimer(){
        timer.gameObject.SetActive(true);
    }

    public void HideTimer(){
        timer.gameObject.SetActive(false);
    }

    public void setTime(float seconds) {
        TimeSpan time = TimeSpan.FromSeconds(seconds);
        timer.text = string.Format("{0:D}:{1:D2}.{2:D2}",(int)time.TotalMinutes, (int)time.Seconds, (int)time.Milliseconds);
    }

    public void quitGame(){
        Application.Quit();
    }
}
