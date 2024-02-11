using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagAnimation : MonoBehaviour
{
    public float Speed;
    public Vector3 Rot;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(gameObject.activeSelf == false) return;

        transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1f,1f,1f), Time.deltaTime);

        Vector3 newRot = new Vector3(
            transform.rotation.eulerAngles.x + Mathf.Sin(Time.time*Speed) * Rot.x,
            transform.rotation.eulerAngles.y + Mathf.Sin(Time.time*Speed) * Rot.y,
            transform.rotation.eulerAngles.z + Mathf.Sin(Time.time*Speed) * Rot.z
        );

        transform.rotation = Quaternion.Euler(newRot);
    }
}
