using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UniRx;
using System;

/*
    TODO
    ・座標を監視してスタックを検知
*/

public class EnemyTank : Tank
{
    [Header("ENEMY: NavAgent")]
    //オブジェクト参照
    [Tooltip("パスの計算を行わせるNavAgentへの参照")]
    public UnityEngine.AI.NavMeshAgent MyAgent;
    [Tooltip("パスの計算元")]
    public GameObject AgentParent;
    [Tooltip("パスのゴールとなるオブジェクト")]
    public GameObject AgentTarget;
    [Tooltip("エージェントの再初期化を行う時間間隔(ms)")]
    public float PathCalculateInterval;
    [Tooltip("パスの設定を行う時間隔(ms)")]
    public float PathVisualizeInterval;
    [Tooltip("コーナーに到達したと判定する距離(unit)")]
    public float RadiusCornerArrived;
    //計算されたパスを保存
    private NavMeshPath AgentPath;
    //パスが計算されたか
    private bool IsPathDrawed;
    //目標とするコーナーのインデックス
    private int CornerIndexBase;
    
    [Header("ENEMY: Behavior")]
    [Tooltip("対象への追跡をやめる距離(unit)")]
    public float FollowDistance;
    [Tooltip("攻撃を行う距離(unit)")]
    public float LimitDistance;
    [Tooltip("前進を行う角度(deg)")]
    public float DriveForwardThreshold;
    [Tooltip("前進⇔後退を切り替える角度(deg)")]
    public float ChangeDirectionThreshold;
    [Tooltip("旋回を行う角度(deg)")]
    public float TurnThreshold;
    //前フレームで前進していたか
    private bool IsPreviousMovingForward;
    //
    private bool IsSeekingPlayer = true;

    [Header("ENEMY: BarrelMovement")]
    [Tooltip("エイムの速さ")]
    public float AimSpeed;
    [Tooltip("エイムを遮るレイヤー")]
    public LayerMask LayerMask;


    override protected void Start()
    {
        base.Start();
        AgentPath = new NavMeshPath();

        //◆イベント：　エージェントをダミーの位置に移動する
        Observable.Interval(TimeSpan.FromMilliseconds(PathCalculateInterval)).Subscribe(_ =>
        {
            MyAgent.Warp(AgentParent.transform.position);
            MyAgent.destination = AgentTarget.transform.position;
            IsPathDrawed = false;
            CornerIndexBase = 1;
        }).AddTo(this);

        //◆イベント：　一定時間おきにパスを設定し、可視化（デバッグ用）
        Observable.Interval(TimeSpan.FromMilliseconds(PathVisualizeInterval)).Where(_ => !IsPathDrawed).Subscribe(_ =>
        {
            AgentPath = MyAgent.path;
            if(AgentPath.status == NavMeshPathStatus.PathComplete && AgentPath.corners.Length >= 2)
            {
                IsPathDrawed = true;
                //Debug.Log("corners:"+AgentPath.corners.Length+"  statu:"+AgentPath.status);
                for (int i = 0; i < AgentPath.corners.Length - 1; i++)
                {
                    Debug.DrawLine(AgentPath.corners[i], AgentPath.corners[i + 1], Color.red, PathVisualizeInterval/1000f, false);
                }
            }
        }).AddTo(this);

    }
    override protected void ApplyDamage(float damage)
    {
        if (HP > damage)
            HP -= damage;
        else
            HP = 0;
        if (!cooldown)
        {
            cooldown = true;
            StartCoroutine(Cooldown());
        }
    }
    public void StopSeeking()
    {
        Debug.Log("Stop");
        IsSeekingPlayer = false;
    }
    
    override protected void Update(){
        if(!IsSeekingPlayer){
            RotateMotors(JointsL, 0);
            RotateMotors(JointsR, 0);
            return;
        }

        //◆エイム
        Vector3 posTarget = AgentTarget.transform.position;
        Vector3 posOrigin = BarrelPitch.transform.position;
        //遮るものがあるかチェック
        Ray ray = new Ray(posOrigin, posTarget - posOrigin);
        RaycastHit hitInfo;
        float distance = (posTarget-posOrigin).magnitude;
        if ( Physics.Raycast(ray, out hitInfo, distance, LayerMask))
        {
            //遮られている場合、クールダウンする
            if(!cooldown)
            {
                cooldown = true;
                StartCoroutine(Cooldown());
            }
        }
        else
        {
            Quaternion rotPitchToTarget = Quaternion.LookRotation(posTarget - posOrigin);
            Quaternion bodyToTargetLocalRot = Quaternion.FromToRotation(TankBody.transform.forward, posTarget - posOrigin);
            //ヨー調整
            BarrelYaw.transform.rotation = Quaternion.Lerp(BarrelYaw.transform.rotation, TankBody.transform.rotation * Quaternion.Euler( 0, bodyToTargetLocalRot.eulerAngles.y, 0 ), Time.deltaTime * AimSpeed);
            //ピッチ調整
            BarrelPitch.transform.rotation = Quaternion.Lerp(BarrelYaw.transform.rotation, TankBody.transform.rotation * Quaternion.Euler( bodyToTargetLocalRot.eulerAngles.x, 0, 0 ), Time.deltaTime * AimSpeed);

            //発砲
            if( distance <= LimitDistance && !cooldown )
            {
                float aimAngleDifference = Quaternion.Angle(BarrelPitch.transform.rotation, rotPitchToTarget);
                if( aimAngleDifference < 1f )
                {
                    base.Fire();
                }
            }
        }

        //パスが描画されていて、距離が離れていたら
        if(IsPathDrawed && MyAgent.remainingDistance > FollowDistance)
        {
            Vector3 PathTargetPosition = AgentParent.transform.position;
            //◆コーナーに到達したか判定し、到達したらインデックスを進める
            for(int cornerIndexToGo = CornerIndexBase; cornerIndexToGo < AgentPath.corners.Length; cornerIndexToGo++ )
            {
                if( (AgentPath.corners[cornerIndexToGo] - AgentParent.transform.position).magnitude > RadiusCornerArrived )
                {
                    CornerIndexBase = cornerIndexToGo;
                    PathTargetPosition = AgentPath.corners[cornerIndexToGo];
                    break;
                }
            }

            //◆戦車と移動先の最短角度(-180~180)を計算
            //Y座標については無視する？
            Vector3 fromVector = AgentParent.transform.forward;
            fromVector.y = 0;
            Vector3 toVector = Vector3.zero;
            toVector.x = PathTargetPosition.x - AgentParent.transform.position.x;
            toVector.z = PathTargetPosition.z - AgentParent.transform.position.z;
            float deltaAngle = Quaternion.FromToRotation(fromVector, toVector).eulerAngles.y;
            deltaAngle = (deltaAngle + 180) % 360 - 180;
            if(deltaAngle< -180)deltaAngle+= 360;

            //◆駆動操作
            float speed;
            float threshold = DriveForwardThreshold + (IsPreviousMovingForward ? TurnThreshold : -TurnThreshold );
            if(-threshold < deltaAngle && deltaAngle < threshold)
            {
                speed = MoveSpeed;
                IsPreviousMovingForward = true;
            }else
            {
                speed = -MoveSpeed;
                IsPreviousMovingForward = false;
            }
            float speedR = speed;
            float speedL = speed;
            if( (deltaAngle > TurnThreshold && DriveForwardThreshold > deltaAngle) ||           //1~3時
                (deltaAngle > DriveForwardThreshold && 180 - TurnThreshold > deltaAngle ) )     //3~6時
            {
                //右旋回
                speedR = 0;
                speedL = speed;
            }
            else if( (deltaAngle < -TurnThreshold && -DriveForwardThreshold < deltaAngle) ||        //9~12時
                     (deltaAngle < -DriveForwardThreshold && -180 + TurnThreshold < deltaAngle ) )  //6~9時
            {
                //左旋回
                speedR = speed;
                speedL = 0;
            }
            RotateMotors(JointsR, speedR);
            RotateMotors(JointsL, speedL);
        }else{
            //パスが設定されていないか、対象に近い場合停止する
            RotateMotors(JointsR, 0);
            RotateMotors(JointsL, 0);
        }
    }

    override protected void PreDead(){
        PlayerAnimator.SetTrigger("Victory");
    }
}
