using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring : MonoBehaviour
{
    [SerializeField] float amount;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other){
        Rigidbody otherRb = other.GetComponent<Rigidbody>();
        if (otherRb == null) return;
        Vector3 velocity = otherRb.velocity;
        if (velocity.y < 0) {
            otherRb.AddForce(Vector3.up * -velocity.y,ForceMode.VelocityChange);
        } 
        otherRb.AddForce(transform.forward * amount, ForceMode.VelocityChange); 
    }
}
