using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePlatform_Rotate : MonoBehaviour
{
    public Vector3 RotEuler;
    public float RotSpeed;

    private Rigidbody MyRB; 

    void Start()
    {
        MyRB = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Quaternion deltaRotation = Quaternion.Euler(RotEuler * Time.deltaTime * RotSpeed);
        MyRB.MoveRotation(MyRB.rotation*deltaRotation);
    }
}
