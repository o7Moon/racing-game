using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public Flag flag;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float dist = (Camera.main.transform.position - transform.position).magnitude;
        transform.localScale = new Vector3(dist, dist, dist) * 0.28f;
        transform.rotation = Quaternion.identity;
    }
}
