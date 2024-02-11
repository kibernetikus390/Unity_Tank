using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePlatform_Sin : MonoBehaviour
{
    public Vector3 TargetPosition;
    public float Speed = 1.0f;

    private Rigidbody MyRB;
    private Vector3 OriginalPosition;
    private Vector3 PreviousPosition;
    private Vector3 DeltaPosition;
    private Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        //オブジェクトの原点を保存しておく
        OriginalPosition = PreviousPosition = transform.position;
        MyRB = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //移動
        Vector3 newPos = new Vector3(
            OriginalPosition.x + (Mathf.Sin(Time.time*Speed) + 1) * TargetPosition.x / 2,
            OriginalPosition.y + (Mathf.Sin(Time.time*Speed) + 1) * TargetPosition.y / 2,
            OriginalPosition.z + (Mathf.Sin(Time.time*Speed) + 1) * TargetPosition.z / 2
        );
        MyRB.MovePosition(newPos);
        //いくら移動したか保存しておく
        DeltaPosition = newPos - PreviousPosition;
        //差分の計算用に座標を保存しておく
        PreviousPosition = newPos;
    }
}
