using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public class Tank : MonoBehaviour
{
    [Header("BASE CLASS")]
    [Header("Movement Setting")]
    [Tooltip("HP")]
    public float MaxHP;
    protected float HP;
    [Tooltip("移動速度（HingeJointのトルクの強さ）")]
    public float MoveSpeed;
    [Tooltip("ホイールを格納するグループへの参照")]
    public GameObject WheelsGroup_R, WheelsGroup_L;
    //HingeJointsへの参照を保管する
    protected List<HingeJoint> JointsR, JointsL;

    [Header("BarrelMovement Setting")]
    [Tooltip("本体部分のオブジェクト参照")]
    public GameObject TankBody;
    [Tooltip("ヨー回転を行うオブジェクト参照")]
    public GameObject BarrelYaw;
    [Tooltip("ピッチ回転を行うオブジェクト参照")]
    public GameObject BarrelPitch;

    [Header("Fire Setting")]
    [Tooltip("発砲を行う座標を示すオブジェクト参照")]
    public GameObject FirePosition;
    [Tooltip("発砲エフェクト")]
    public GameObject FireEffect;
    [Tooltip("弾丸Prefab")]
    public GameObject AmmoPrefab;
    [Tooltip("発砲間隔(sec)")]
    public float FireInterval;
    //クールダウン中であるか
    protected bool cooldown = false;

    [Header("DamageEffect Setting")]
    [Tooltip("ダメージエフェクト　最後は破壊後のエフェクト")]
    public GameObject[] DamageEffets;
    public GameObject[] ObjectsToDisableOnDeath;
    public GameObject[] ObjectsToEnableOnDeath;
    
    [Header("Animation Setting")]
    [Tooltip("プレイヤーキャラクター")]
    public GameObject PlayerCharacter;
    protected Animator PlayerAnimator;

    //落下時用に開始点を保存
    protected Vector3 StartPosition;

    virtual protected void Start()
    {
        HP = MaxHP;
        PlayerAnimator = PlayerCharacter.GetComponentInChildren<Animator>();  

        //◆ホイールへの参照を取得
        JointsR = new List<HingeJoint>();
        JointsL = new List<HingeJoint>();
        for(int i = 0; i < WheelsGroup_R.transform.childCount; i++)
            JointsR.Add( (WheelsGroup_R.transform.GetChild(i).GetComponent<HingeJoint>()) );
        for(int i = 0; i < WheelsGroup_L.transform.childCount; i++)
            JointsL.Add( WheelsGroup_L.transform.GetChild(i).transform.GetComponent<HingeJoint>() );
        
        //◆イベント：落下したら開始地点に戻る
        StartPosition = transform.position;
        TankBody.ObserveEveryValueChanged(x => x.transform.position.y).Where(x => x < -5f)
            .Subscribe(_ => {
                Rigidbody rb = TankBody.GetComponent<Rigidbody>();
                WheelsGroup_R.transform.position = StartPosition;
                WheelsGroup_L.transform.position = StartPosition;
                WheelsGroup_R.transform.rotation = Quaternion.identity;
                WheelsGroup_L.transform.rotation = Quaternion.identity;
                rb.isKinematic = true;
                rb.freezeRotation = true;
                rb.velocity = Vector3.zero;
                rb.angularVelocity  = Vector3.zero;
                rb.ResetInertiaTensor();
                rb.position = StartPosition;
                rb.rotation = Quaternion.identity;
                rb.isKinematic = false;
                rb.freezeRotation = false;
            });

        //◆イベント：　ダメージエフェクト
        this.ObserveEveryValueChanged(x => x.HP)
            .Subscribe(_ => {
                float damage = MaxHP - HP;
                int indexToActive = (int)(damage / (MaxHP / DamageEffets.Length));
                for(int i = 0; i < indexToActive; i++){
                    DamageEffets[i].SetActive(true);
                }
                if(HP <= 0) {
                    HP = 0;
                    Dead();
                }
            });
    }

    // Update is called once per frame
    virtual protected void Update(){}

    virtual protected void ApplyDamage(float damage)
    {
        if(HP > damage)
            HP -= damage;
        else
            HP = 0;
    }

    protected IEnumerator Cooldown()
    {
        /*
            主砲のクールダウン
        */
        yield return new WaitForSeconds(FireInterval);
        cooldown = false;
    }

    protected void RotateMotors(List<HingeJoint> motorList, float value)
    {
        /*
            モーターの回転
        */
        for(int i = 0; i < motorList.Count; i++)
        {
            JointMotor motor = motorList[i].motor;
            motor.targetVelocity = value;
            motorList[i].motor = motor;
        }
    }
    
    virtual protected void PreDead(){}

    protected void Dead()
    {
        PreDead();

        /*
            死亡時の処理
        */
        RotateMotors(JointsR, 0);
        RotateMotors(JointsL, 0);
        //グレーにする
        SetColor[] setColors = GetComponentsInChildren<SetColor>();
        foreach (SetColor sc in setColors)
        {
            Color col = new Color(0.25f, 0.25f, 0.25f, 1f);
            sc.SendMessage("ApplyColor", col, SendMessageOptions.DontRequireReceiver);
        }

        foreach(GameObject obj in ObjectsToEnableOnDeath)    obj.SetActive(true);
        foreach(GameObject obj in ObjectsToDisableOnDeath)   obj.SetActive(false);

        Destroy(this);
    }

    
    protected void Fire(){
        cooldown = true;
        StartCoroutine(Cooldown());
        Instantiate(FireEffect, FirePosition.transform.position, Quaternion.identity);
        Instantiate(AmmoPrefab, FirePosition.transform.position, FirePosition.transform.rotation);
    }
}
