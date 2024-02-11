using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterOfMass : MonoBehaviour {

    public Vector3 center = new Vector3(0f, -0.2f, 0f);

    private Rigidbody rb;

    void Start () {
        rb = GetComponent<Rigidbody> ();
        rb.centerOfMass = center;
    }

    void Update () {
        Debug.DrawLine (transform.position , transform.position + transform.rotation * center);
    }

    void OnDrawGizmos(){
        Gizmos.color = Color.red;
        Gizmos.DrawSphere (transform.position + transform.rotation * center, 0.1f);
    }

}