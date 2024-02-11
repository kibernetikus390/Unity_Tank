using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : MonoBehaviour
{
    public float FuseTime;
    public float BulletSpeed;
    public GameObject ExplodeEffect;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody>().AddForce( transform.rotation * new Vector3( 0,0,BulletSpeed) );
        StartCoroutine(Fuse());
    }

    void OnCollisionEnter(Collision collision)
    {
        Explode();
    }
    IEnumerator Fuse()
    {
        yield return new WaitForSeconds(FuseTime);
        Explode();
    }


    void Explode()
    {
        Instantiate(ExplodeEffect, transform.position, transform.rotation);
        Destroy(gameObject);
    }

}
