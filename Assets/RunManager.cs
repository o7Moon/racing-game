using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunManager : MonoBehaviour
{
    public enum RunState { 
        Exploring,
        Running,
        Selecting,
    }

    public static RunManager Instance;
    float start_time;
    public RunState state;
    const float selectionCameraHeight = 50f;
    public Flag from;
    public Flag to;
    bool startingRun = false;
    bool cancelling = false;

    [SerializeField] LayerMask UI;
    
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state) {
            case RunState.Selecting:
                if (cancelling) PlayerController.Instance.transform.position = Vector3.Lerp(PlayerController.Instance.transform.position, from.runEndPosition.position, 0.02f);
                else PlayerController.Instance.transform.position = Vector3.Lerp(PlayerController.Instance.transform.position, from.runStartPosition.position, 0.01f);
                if (startingRun) PlayerController.Instance.camera.localPosition = Vector3.Lerp(PlayerController.Instance.camera.localPosition, Vector3.up * 0.5f, 0.2f);
                else PlayerController.Instance.camera.localPosition = Vector3.Lerp(PlayerController.Instance.camera.localPosition, Vector3.up * selectionCameraHeight, 0.1f);

                if (Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 1000, UI, QueryTriggerInteraction.Collide))
                {
                    Arrow a = hit.collider.GetComponent<Arrow>();
                    if (a == null) break;

                    startRun(a.flag);
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    cancelling = true;
                    StartCoroutine(cancelSelecting());
                }
                break;
            case RunState.Running:
                UIManager.Instance.setTime(Time.realtimeSinceStartup - start_time);
                if (Input.GetKeyDown(KeyCode.Escape)) {
                    from = null;
                    to = null;
                    UIManager.Instance.HideTimer();
                    state = RunState.Exploring;
                }
                break;
            default:
                break;
        }
    }

    public void startSelecting(Flag from) {
        cancelling = false;
        startingRun = false;
        this.from = from;
        state = RunState.Selecting;
        PlayerController.Instance.canMove = false;
        foreach (Flag f in Flag.flags) {
            if (f == from) continue;
            f.showToSelection();
        }
    }

    public void startRun(Flag to) {
        //Debug.Log(from.flagName + " to " + to.flagName);
        startingRun = true;
        this.to = to;
        start_time = Time.realtimeSinceStartup + 1;
        StartCoroutine(countdown());
        foreach (Flag f in Flag.flags) {
            f.hideToSelection();
        }
        UIManager.Instance.setTime(0);
        UIManager.Instance.ShowTimer();
    }

    IEnumerator countdown() {
        yield return new WaitForSecondsRealtime(1);
        PlayerController.Instance.canMove = true;
        state = RunState.Running;
        PlayerController.Instance.camera.localPosition = Vector3.up * 0.5f;
    }

    public void EndRun() {
        state = RunState.Exploring;
        from = null;
        to = null;
        TunesManager.Instance.PlayTune();
        StartCoroutine(hideTimerAfterEndRun());
    }

    IEnumerator hideTimerAfterEndRun(){
        yield return new WaitForSecondsRealtime(3);
        UIManager.Instance.HideTimer();
    }

    IEnumerator cancelSelecting() {
        Cursor.lockState = CursorLockMode.Locked;
        startingRun = true;
        UIManager.Instance.HideTimer();
        foreach (Flag f in Flag.flags)
        {
            f.hideToSelection();
        }
        yield return new WaitForSecondsRealtime(1);
        startingRun = false;
        PlayerController.Instance.canMove = true;
        PlayerController.Instance.camera.localPosition = Vector3.up * 0.5f;
        state = RunState.Exploring;
        from = null;
        to = null;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
