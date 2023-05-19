using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour
{
    public static List<Flag> flags;
    const float rotationSpeed = 70f;
    public string flagName;
    public Transform runStartPosition;
    public Transform runEndPosition;
    public GameObject toSelection;
    // Start is called before the first frame update
    void Start()
    {
        if (flags == null) flags = new List<Flag>();
        flags.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (RunManager.Instance.state == RunManager.RunState.Exploring && other.gameObject.layer == 8) {
            RunManager.Instance.startSelecting(this);
        }

        if (RunManager.Instance.state == RunManager.RunState.Running && RunManager.Instance.to == this) {
            RunManager.Instance.EndRun();
        }
    }

    public void showToSelection() {
        toSelection.SetActive(true);
    }

    public void hideToSelection() {
        toSelection.SetActive(false);
    }
}
