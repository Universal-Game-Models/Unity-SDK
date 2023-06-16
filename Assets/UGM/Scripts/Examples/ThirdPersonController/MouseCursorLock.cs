using UnityEngine;
using UnityEngine.EventSystems;

namespace UGM.Examples.ThirdPersonController
{
    /// <summary>
    /// Controls the behavior of the mouse cursor, including locking, hiding, and showing.
    /// </summary>
    public class MouseCursorLock : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Defines the Cursor Lock Mode to apply")]
        private CursorLockMode cursorLockMode;
        [SerializeField]
        [Tooltip("If true will hide mouse cursor")]
        private bool hideCursor = true;
        [SerializeField]
        [Tooltip("If true it apply cursor settings on start")]
        private bool applyOnStart = true;

        private int cursorShowCount = 0;

        /// <summary>
        /// Called before the first frame update. Hides the cursor on start if applyOnStart is true and subscribes to the OnShowCursor event.
        /// </summary>
        void Start()
        {
            if (applyOnStart)
            {
                HideCursor();
            }
            ExampleUIEvents.OnShowCursor.AddListener(SetCursor);
        }

        /// <summary>
        /// Called when the script instance is being destroyed. Unsubscribes from the OnShowCursor event.
        /// </summary>
        private void OnDestroy()
        {
            ExampleUIEvents.OnShowCursor.RemoveListener(SetCursor);
        }

        /// <summary>
        /// Update is called once per frame. Detects mouse click and hides the cursor when the left mouse button is clicked.
        /// </summary>
        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) HideCursor();
            if (Input.GetKeyDown(KeyCode.Escape)) ShowCursor();
            if (Input.GetKeyUp(KeyCode.Escape)) ShowCursor();
        }

        /// <summary>
        /// Show the cursor and set its lock state to Confined.
        /// </summary>
        private void ShowCursor()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }

        /// <summary>
        /// Hide the cursor based on the hideCursor flag and the current pointer over UI condition.
        /// </summary>
        private void HideCursor()
        {
            if (cursorShowCount < 0) cursorShowCount = 0;
            if (cursorShowCount == 0)
            {
                Cursor.visible = hideCursor;
                Cursor.lockState = cursorLockMode;
            }
        }

        /// <summary>
        /// Set the cursor visibility based on the provided active flag.
        /// </summary>
        /// <param name="active">Flag indicating whether to show or hide the cursor.</param>
        public void SetCursor(bool active)
        {
            if (active)
            {
                ShowCursor();
                cursorShowCount++;
            }
            else
            {
                cursorShowCount--;
                HideCursor();
            }
        }

        /// <summary>
        /// Called when the application gains or loses focus. Hides the cursor when the application loses focus and shows it when the application gains focus.
        /// </summary>
        /// <param name="hasFocus">Flag indicating whether the application has focus.</param>
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
}