using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : MonoBehaviour {

    public int gravity = 12;
    public Rigidbody rb;
	
	// Update is called once per frame
	void Update () {

        rb.AddForce(0, -gravity, 0);
	}
}
