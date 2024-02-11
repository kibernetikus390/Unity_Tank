using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Interactions;
#endif

public class SimpleCamera : MonoBehaviour
{
    [Header("Target Setting")]
    [Tooltip("追尾の対象オブジェクト")]
    public GameObject TargetObject;
    [Tooltip("追尾の対象の縦オフセット")]
    public float YOffset = 1f;
    [Tooltip("対象からの距離")]
    public float Distance = 10.0f;
    [Tooltip("補完の速さ(0 ~ 1.0)")]
    public float Smooth = 0.5f;
    //カメラのクォータニオン
    private Quaternion CameraRotation;

    [Header("Input Setting")]
    [Tooltip("InputSystemへの参照")]
    public TankInputTest INPUT;
    [Tooltip("横感度")]
    public float SensitivityX = 10.0f;
    [Tooltip("縦感度")]
    public float SensitivityY = 10.0f;
    
    [Header("Collision Setting")]
    [Tooltip("衝突判定を始める、原点からの距離")]
    public float CollStartDistance;
    [Tooltip("カメラを配置する、衝突点からのオフセット")]
    public float OffsetFromHit;
    [Tooltip("衝突判定を行うレイヤー")]
    public LayerMask LayerMask;

    [Header("UI Setting")]
    public Slider SettingSensitivityX;
    public Slider SettingSensitivityY;

    private bool uiSettingInitialized;

    // Start is called before the first frame update
    void Start()
    {
        uiSettingInitialized = false;
        //PlayerPrefs.SetInt("Max", maxFruits);
        if(PlayerPrefs.HasKey("SensX")) SensitivityX = PlayerPrefs.GetFloat("SensX");
        SettingSensitivityX.value = SensitivityX;
        if(PlayerPrefs.HasKey("SensY")) SensitivityY = PlayerPrefs.GetFloat("SensY");
        SettingSensitivityY.value = SensitivityY;
        uiSettingInitialized = true;

        CameraRotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        //◆入力
        Quaternion addYaw   = Quaternion.AngleAxis(  INPUT.look.x * Time.deltaTime * SensitivityX, Vector3.up);
        Quaternion addPitch = Quaternion.AngleAxis( -INPUT.look.y * Time.deltaTime * SensitivityY, transform.right);
        Quaternion newRotation = addYaw * addPitch * CameraRotation;
        float newPitch = newRotation.eulerAngles.x;
        if( 180f <= newPitch && newPitch < 280f) newPitch = 280f;
        else if( 80f < newPitch && newPitch <= 180f ) newPitch = 80f;
        CameraRotation = Quaternion.Euler( newPitch, newRotation.eulerAngles.y, newRotation.eulerAngles.z );
    
        //◆座標
        Vector3 posOrigin = TargetObject.transform.position + new Vector3(0,YOffset,0);
        Vector3 offsetFromOrigin   = CameraRotation * Vector3.back * Distance;
        Vector3 newCameraPos = Vector3.Lerp(transform.position, posOrigin + offsetFromOrigin, Smooth);
        
        //◆カメラの当たり判定
        Ray ray = new Ray(TargetObject.transform.position + offsetFromOrigin.normalized * CollStartDistance, offsetFromOrigin);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, Distance, LayerMask))
            transform.position  = hitInfo.point - offsetFromOrigin.normalized * OffsetFromHit + new Vector3(0,YOffset,0);
        else
            transform.position  = newCameraPos;

        //◆回転
        Quaternion newCamerarRot = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(-offsetFromOrigin), Smooth);
        transform.rotation       = newCamerarRot;
    }
    public void CameraSettingUpdated()
    {
        if( !uiSettingInitialized || SettingSensitivityX == null || SettingSensitivityY == null ) return;
        
        SensitivityX = SettingSensitivityX.value;
        SensitivityY = SettingSensitivityY.value;
        PlayerPrefs.SetFloat("SensX", SensitivityX);
        PlayerPrefs.SetFloat("SensY", SensitivityY);
        //Debug.Log("SensX:"+SensitivityX + "  SensY:"+SensitivityY);
    }
}
