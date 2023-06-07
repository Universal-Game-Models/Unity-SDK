using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseCursorLock : MonoBehaviour
{
    [SerializeField][Tooltip("Defines the Cursor Lock Mode to apply")] 
    private CursorLockMode cursorLockMode;
    [SerializeField][Tooltip("If true will hide mouse cursor")] 
    private bool hideCursor = true;
    [SerializeField][Tooltip("If true it apply cursor settings on start")]
    private bool applyOnStart = true;
    
    // Start is called before the first frame update
    void Start()
    {
        if (applyOnStart)
        {
            HideCursor();
        }
        ExampleUIEvents.OnShowCursor.AddListener(SetCursor);
    }
    private void OnDestroy()
    {
        ExampleUIEvents.OnShowCursor.RemoveListener(SetCursor);
    }
    private void Update()
    {
        if(Input.GetMouseButtonDown(0)) HideCursor();
        if (Input.GetKeyDown(KeyCode.Escape)) ShowCursor();
        if (Input.GetKeyUp(KeyCode.Escape)) ShowCursor();
    }
    private void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }
    private void HideCursor()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            Cursor.visible = hideCursor;
            Cursor.lockState = cursorLockMode;
        }
    }

    public void SetCursor(bool active)
    {
        if (active)
        {
            ShowCursor();
        }
        else
        {
            HideCursor();
        }
    }

    public void ToggleCursor()
    {
        if (Cursor.visible)
        {
            HideCursor();
        }
        else
        {
            ShowCursor();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            HideCursor();
        }
        else
        {
            ShowCursor();
        }
    }
}
