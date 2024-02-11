using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using TMPro;
using Doozy.Engine;

public class PlayerTank : Tank
{
    [Header("PLAYER: Movement")]
    [Tooltip("InputSystemへの参照")]
    public TankInputTest INPUT;
    [Tooltip("左右入力の遊び")]
    public float Asobi;
    
    [Header("PLAYER: BarrelMovement")]
    [Tooltip("ヨー感度")]
    public float BarrelYawSpeed;
    [Tooltip("ピッチ感度")]
    public float BarrelPitchSpeed;
    
    [Header("PLAYER: Audio")]
    [Tooltip("駆動音オブジェクトへの参照")]
    public AudioSource MyAudio;
    //音量
    private float AudioVolumeTarget = 0f;

    [Header("PLAYER: GUI")]
    [Tooltip("GUIラベルへの参照")]
    public GameObject labelHP;
    private TextMeshProUGUI tmProHP;
    public Slider sliderHP;
    public GameObject fillHP;
    private Image imageFillHP;

    [Header("UI Setting")]
    public Slider SettingYaw;
    public Slider SettingPitch;
    private bool IsSettingInitialized;

    public void SettingChanged()
    {
        if(!IsSettingInitialized) return;

        BarrelYawSpeed = SettingYaw.value;
        BarrelPitchSpeed = SettingPitch.value;
        PlayerPrefs.SetFloat("PlayerYaw", BarrelYawSpeed);
        PlayerPrefs.SetFloat("PlayerPitch", BarrelPitchSpeed);
    }

    override protected void Start()
    {
        base.Start();
        IsSettingInitialized = false;
        tmProHP = labelHP.GetComponent<TextMeshProUGUI>();
        imageFillHP = fillHP.GetComponent<Image>();

        
        if(PlayerPrefs.HasKey("PlayerYaw")) BarrelYawSpeed = PlayerPrefs.GetFloat("PlayerYaw");
        SettingYaw.value = BarrelYawSpeed;
        if(PlayerPrefs.HasKey("PlayerPitch")) BarrelPitchSpeed = PlayerPrefs.GetFloat("PlayerPitch");
        SettingPitch.value = BarrelPitchSpeed;


        IsSettingInitialized = true;

        //◆オーディオ設定
        MyAudio.Play(0);
        MyAudio.volume = 0;
        //縦駆動入力に応じて音量を変化させる
        this.ObserveEveryValueChanged(x => x.INPUT.move.y)
            .Subscribe(_ => {
                AudioVolumeTarget = (INPUT.move.y == 0) ? 0f : 1f;
            });

        //◆イベント：主砲発射
        this.ObserveEveryValueChanged(x => x.INPUT.fireCurrentFrame).Where(x => x && !cooldown)
            .Subscribe(_ => {
                base.Fire();
            });
            
        //◆イベント：GUI HP
        this.ObserveEveryValueChanged(x => x.HP)
            .Subscribe(_ => {
                UpdateHPBar(HP);
            });
    }

    public void UpdateHPBar(float val)
    {
        //tmProHP.SetText("HP: {0}", Mathf.Ceil(val));
        sliderHP.value = val;

        //HPバーの色を設定
        switch( Mathf.Floor(HP / (MaxHP / 4)) )
        {
            case 0:
                imageFillHP.color = Color.red;
                break;
            case 1:
                imageFillHP.color = Color.yellow;
                break;
            default:    
                imageFillHP.color = new Color(0.2f, 0.75f, 1f, 1f);
                break;
        }
    }

    override protected void Update()
    {
        //◆音量調整
        MyAudio.volume = Mathf.Lerp(MyAudio.volume, AudioVolumeTarget, 0.02f);

        //◆移動操作
        float speed = INPUT.move.y * MoveSpeed;
        float speedR = Mathf.Abs(INPUT.move.x) < Asobi ? speed : (INPUT.move.x >  Asobi ? 0 : speed);
        float speedL = Mathf.Abs(INPUT.move.x) < Asobi ? speed : (INPUT.move.x < -Asobi ? 0 : speed);
        RotateMotors(JointsR, speedR);
        RotateMotors(JointsL, speedL);

        //◆バレル操作
        BarrelYaw.transform.Rotate(0, INPUT.barrel.x * Time.deltaTime * BarrelYawSpeed, 0, Space.Self);
        float currentBarrelPitch = (BarrelPitch.transform.localRotation.eulerAngles.x + 180f) % 360 - 180f + INPUT.barrel.y * Time.deltaTime * BarrelPitchSpeed;
        BarrelPitch.transform.localRotation = Quaternion.Euler(new Vector3( currentBarrelPitch < -20f ? -20f : (currentBarrelPitch > 10f ? 10f : currentBarrelPitch ), 0, 0));
    }

    override protected void ApplyDamage(float damage)
    {
        base.ApplyDamage(damage);
        if(damage > 50f) PlayerAnimator.SetTrigger("Damaged");
    }
    
    override protected void PreDead(){
        GameEventMessage.SendEvent("Defeated");
        UpdateHPBar(0);
        EnemyTank[] enemies = FindObjectsOfType<EnemyTank>();
        Debug.Log(enemies);
        foreach (EnemyTank enemy in enemies)
        {
            enemy.SendMessage("StopSeeking", SendMessageOptions.DontRequireReceiver);
        }
        PlayerAnimator.SetTrigger("Lose");
        Destroy(MyAudio);
    }
}
