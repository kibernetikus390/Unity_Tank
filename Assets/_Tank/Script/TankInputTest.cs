using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Doozy.Engine;


public class TankInputTest: MonoBehaviour
{
    private PlayerInput input;
    public Vector2 move { get; private set; }
    public Vector2 look { get; private set; }
    public Vector2 barrel { get; private set; }
    public bool fire { get; private set; }
    public bool menu { get; private set; }
    public bool fireCurrentFrame { get; private set; }
    private bool fire_previous;

    void Start()
    {
        input = GetComponent<PlayerInput>();
    }
    void Update()
    {
        move = input.actions["Move"].ReadValue<Vector2>();
        look = input.actions["Look"].ReadValue<Vector2>();
        barrel = input.actions["Barrel"].ReadValue<Vector2>();
        fire = input.actions["Fire"].ReadValue<float>() > 0;
        menu = input.actions["Menu"].ReadValue<float>() > 0;
        fireCurrentFrame = fire && !fire_previous;
        fire_previous = fire;
    }
    void OnCursorLock()
    {
        Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
    }
    void OnMenu()
    {
        //SceneManager.LoadScene("TankTest");
        
        GameEventMessage.SendEvent("MenuPressed");
    }

    public void CursorLock()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void CursorUnLock()
    {
        Cursor.lockState = CursorLockMode.None;
    }
    public void TimeStop()
    {
        Time.timeScale = 0;
    }
    public void TimeStart()
    {
        Time.timeScale = 1f;
    }
}