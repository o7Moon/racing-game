using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TunesManager : MonoBehaviour
{
    public static TunesManager Instance;
    [SerializeField] AudioClip[] tunes;
    [SerializeField] AudioSource source;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayTune() {
        int tune = Random.Range(0,tunes.Length);
        source.PlayOneShot(tunes[tune]);
    }
}
