using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionForce : MonoBehaviour
{
    public float radius = 5.0F;
    public float power = 10.0F;
    public float powerY = 10.0F;
    public float damage;
    
    //ダメージを与えるGameObjectのリスト
    private List<GameObject> ObjToDamage;

    void Start()
    {
        ObjToDamage = new List<GameObject>();
        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        foreach (Collider hit in colliders)
        {
            //プロジェクタイルは無視
            if(hit.tag == "Projectile") continue;

            GameObject obj = hit.gameObject.transform.root.gameObject;
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.AddExplosionForce(power, explosionPos, radius, powerY);
                
                //ボディにダメージ
                if(damage > 0 && hit.tag == "TankBody")
                {
                    float distance = Mathf.Max(1.0f, (hit.gameObject.transform.position - explosionPos).magnitude);
                    obj.SendMessage("ApplyDamage", damage / (distance), SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
}