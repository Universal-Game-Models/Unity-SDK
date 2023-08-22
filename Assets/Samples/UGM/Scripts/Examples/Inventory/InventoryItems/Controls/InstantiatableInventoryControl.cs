using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UGM.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using static UGM.Core.UGMDataTypes;

namespace UGM.Examples.Inventory.InventoryItems.Controls
{
    /// <summary>
    /// Controls the behavior of instantiatable inventory items.
    /// Manages the hologram display, raycasting, rotation, and placement of the instantiated model.
    /// </summary>
    public class InstantiatableInventoryControl : MonoBehaviour
    {
        private static InstantiatableInventoryControl _instance;

        /// <summary>
        /// Public property to access the instance of InstantiatableInventoryControl.
        /// Provides a getter to retrieve the instance.
        /// </summary>
        public static InstantiatableInventoryControl Instance { get { return _instance; } }

        [Tooltip("Reference to the hologram prefab used for visualization.")]
        [SerializeField]
        private GameObject hologramPrefab;

        [Tooltip("The speed at which the hologram rotates from scrolling.")]
        [SerializeField]
        private float rotateSpeed;

        /// <summary>
        /// The main camera in the scene.
        /// Used for raycasting and screen-to-world coordinate conversions.
        /// </summary>
        private Camera mainCamera;

        /// <summary>
        /// The center position of the screen in pixel coordinates.
        /// Used for raycasting.
        /// </summary>
        private Vector3 screenCenter;

        /// <summary>
        /// The instantiated hologram object.
        /// </summary>
        private GameObject hologram;

        /// <summary>
        /// The renderer component of the hologram.
        /// </summary>
        private Renderer hologramRenderer;

        /// <summary>
        /// The temporary placement of the object.
        /// </summary>
        private UGMDownloader tempPlacement;

        /// <summary>
        /// Flag indicating whether the hologram should be shown.
        /// </summary>
        private bool showHologram;

        /// <summary>
        /// The scale of the model to be instantiated.
        /// </summary>
        private Vector3 modelScale = Vector3.zero;

        /// <summary>
        /// Flag indicating whether the hologram can be placed in the current position.
        /// </summary>
        private bool canPlace;

        /// <summary>
        /// The TokenInfo of the currently selected inventory item.
        /// </summary>
        protected TokenInfo currentTokenInfo;

        /// <summary>
        /// Offset applied to the hologram rotation when scrolling the mouse wheel.
        /// </summary>
        private Vector3 scrollOffset;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// Checks if an instance of the class already exists.
        /// If a duplicate instance is found, it destroys the duplicate.
        /// Otherwise, sets the instance and performs necessary initialization.
        /// </summary>
        private void Awake()
        {
            // Check if an instance already exists
            if (_instance != null && _instance != this)
            {
                // Destroy the duplicate instance
                Destroy(this.gameObject);
            }
            else
            {
                // Set the instance if it doesn't exist
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
                mainCamera = Camera.main;
                if (!hologram)
                {
                    hologram = Instantiate(hologramPrefab);
                    hologramRenderer = hologram.GetComponent<Renderer>();
                }          
            }
        }

        /// <summary>
        /// Sets the TokenInfo for the current inventory item.
        /// Calculates the scroll offset based on the camera rotation.
        /// Enables the hologram for display.
        /// </summary>
        public void SetTokenInfo(TokenInfo tokenInfo)
        {
            currentTokenInfo = tokenInfo;
            var cameraRotation = mainCamera.transform.rotation.eulerAngles;
            cameraRotation.x = 0;
            cameraRotation.z = 0;
            cameraRotation.y -= 90;
            scrollOffset = cameraRotation;
            EnableHologram();
        }

        /// <summary>
        /// Enables the hologram for display.
        /// Retrieves the model scale from the TokenInfo metadata.
        /// Initializes the hologram to be shown
        /// </summary>
        private void EnableHologram()
        {
            GetModelScale();
            GetTempModel();
            showHologram = true;
        }

        private void GetTempModel()
        {
            if (tempPlacement) Destroy(tempPlacement);
            tempPlacement = LoadModel(new Vector3(0, 0, 0), null);
            tempPlacement.onModelSuccess.AddListener(SetRecursiveLayer);
        }

        private void SetRecursiveLayer(GameObject go)
        {
            if (go)
            {
                SetLayerRecursivelyForTransform(go.transform, LayerMask.NameToLayer("Ignore Raycast"));
                tempPlacement.onModelSuccess.RemoveListener(SetRecursiveLayer);
            }
        }

        void SetLayerRecursivelyForTransform(Transform currentTransform, int targetLayer)
        {
            currentTransform.gameObject.layer = targetLayer;

            foreach (Transform child in currentTransform)
            {
                SetLayerRecursivelyForTransform(child, targetLayer);
            }
        }

        /// <summary>
        /// Retrieves the model scale from the TokenInfo metadata.
        /// If the size attribute is present, deserializes it into a Vector3.
        /// Otherwise, sets the model scale to Vector3.one.
        /// </summary>
        private void GetModelScale()
        {
            //Get the dimensions from metadata Size attribute
            var size = Array.Find(currentTokenInfo.metadata.attributes, a => a.trait_type == "Size");
            if (size != null)
            {
                modelScale = JsonConvert.DeserializeObject<Vector3>(size.value.ToString());
            }
            else
            {
                modelScale = Vector3.one;
            }
        }

        /// <summary>
        /// Updates the hologram position and rotation based on raycasting.
        /// Checks for collisions with other objects using an overlap box.
        /// Updates the hologram color to indicate whether it can be placed or not.
        /// Handles the placement or cancellation of the hologram based on user input.
        /// </summary>
        private void Update()
        {
            //Raycast every frame for hologram position
            if (showHologram)
            {
                RaycastHit hit;
                bool didHit;
                UpdateRaycast(out hit, out didHit);
                if (didHit)
                {
                    hologram.transform.rotation = Quaternion.Euler(scrollOffset);
                    //Set the lossy scale of the box from the metadata
                    hologram.transform.localScale = modelScale;
                    var scale = hologram.transform.localScale;
                    
                    //Hack for wall mounting, only works with thiner objects such as picture frames
                    Vector3 hitNormal = hit.normal;

                    hitNormal.y = (scale.y / 2);
                    hitNormal.z *= (scale.z / 2) * Mathf.Abs(Mathf.Cos(scrollOffset.x * Mathf.Deg2Rad)) * Mathf.Abs(Mathf.Cos(scrollOffset.y * Mathf.Deg2Rad));
                    hitNormal.x *= (scale.x / 2) * Mathf.Abs(Mathf.Cos(scrollOffset.x * Mathf.Deg2Rad)) * Mathf.Abs(Mathf.Cos(scrollOffset.y * Mathf.Deg2Rad));

                    //Increase the height of the hologram to prevent unwanted ground collisions
                    hologram.transform.position = (hit.point + hitNormal);
                    if (tempPlacement)
                    {
                        tempPlacement.transform.position = hologram.transform.position - new Vector3(0, scale.y / 2, 0);
                        tempPlacement.transform.rotation = hologram.transform.rotation;
                    }
                    int layerMask = ~(LayerMask.GetMask("Player") | LayerMask.GetMask("Ignore Raycast"));
                    //Fudge the collision extents to allow a bit of overlap
                    Vector3 halfExtents = hologram.transform.localScale / 2f;
                    Collider[] colliders = Physics.OverlapBox(hologram.transform.position + new Vector3(0,0.05f,0), halfExtents, hologram.transform.rotation, layerMask);

                    if (colliders.Length > 0)
                    {
                        // Set the color to red if the boxcast hits something
                        hologramRenderer.material.color = new Color(1, 0, 0, 0.25f);
                        canPlace = false;
                        foreach (var col in colliders)
                        {
                            Debug.Log(col.gameObject);
                        }
                    }
                    else
                    {
                        // Set the default color to blue
                        hologramRenderer.material.color = new Color(0, 0, 1, 0.25f);
                        canPlace = true;
                    }

                    if (!hologram.activeInHierarchy)
                    {
                        hologram.SetActive(true);
                    }
                    PlaceOrCancel(hit);
                    ScrollToRotate();
                }
                else
                {
                    if (hologram.activeInHierarchy)
                    {
                        //Only visual disable but keep running
                        hologram.SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the scroll offset based on the mouse scroll delta.
        /// </summary>
        private void ScrollToRotate()
        {
            scrollOffset += (Vector3)Input.mouseScrollDelta * rotateSpeed;
        }

        /// <summary>
        /// Updates the raycast position and determines if it hits any objects.
        /// </summary>
        private void UpdateRaycast(out RaycastHit hit, out bool didHit)
        {
            if (mainCamera)
            {
                //Change this to mouse position
                screenCenter = Input.mousePosition;
            }
            int layerMask = ~(LayerMask.GetMask("Player") | LayerMask.GetMask("Ignore Raycast"));
            // Raycast from the camera
            Ray ray = mainCamera.ScreenPointToRay(screenCenter);
            didHit = Physics.Raycast(ray, out hit, 100, layerMask);
        }

        /// <summary>
        /// Places or cancels the hologram based on user input.
        /// If the hologram can be placed and no UI elements are being clicked, loads the model at the hit point.
        /// If the right mouse button is clicked, cancels the hologram and clears the current TokenInfo.
        /// </summary>
        private void PlaceOrCancel(RaycastHit hit)
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (canPlace && !EventSystem.current.IsPointerOverGameObject())
                {
                    SetLayerRecursivelyForTransform(tempPlacement.transform, LayerMask.NameToLayer("Default"));
                    tempPlacement = null;
                    GetTempModel();
                }
            }
            if (Input.GetMouseButtonUp(1))
            {
                currentTokenInfo = null;
                DisableHologram();
                DisableTemp();
            }
        }

        /// <summary>
        /// Asynchronously loads the model at the specified hit point with the current TokenInfo.
        /// </summary>
        private UGMDownloader LoadModel(Vector3 hitPoint, Transform hitParent)
        {
            if (currentTokenInfo == null) return null;

            var ugmDownloader = new GameObject(currentTokenInfo.metadata.name).AddComponent<UGMDownloader>();
            ugmDownloader.SetLoadOptions(false, true, true, true, true);
            ugmDownloader.transform.position = hitPoint;
            ugmDownloader.transform.rotation = Quaternion.Euler(scrollOffset);
            ugmDownloader.transform.SetParent(hitParent);
            ugmDownloader.LoadAsync(currentTokenInfo.token_id);

            return ugmDownloader;
        }

        /// <summary>
        /// Disables the hologram and stops its display.
        /// </summary>
        private void DisableHologram()
        {
            showHologram = false;
            if (hologram.activeInHierarchy)
            {
                hologram.SetActive(false);
            }
        }
        /// <summary>
        /// Disables the temp placement and stops its display.
        /// </summary>
        private void DisableTemp()
        {
            if (tempPlacement)
            {
                Destroy(tempPlacement.gameObject);
            }
        }
    }
}
