using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using static UGMDataTypes;

public class InstantiatableInventoryControl : MonoBehaviour
{
    private static InstantiatableInventoryControl _instance;

    // Public property to access the instance
    public static InstantiatableInventoryControl Instance { get { return _instance; } }

    [SerializeField]
    private GameObject hologramPrefab;
    [SerializeField]
    private float rotateSpeed;

    private Camera mainCamera;
    private Vector3 screenCenter;
    private GameObject hologram;
    private Renderer hologramRenderer;
    private bool showHologram;
    private Vector3 modelScale = Vector3.zero;
    private bool canPlace;
    protected TokenInfo currentTokenInfo;
    private Vector3 scrollOffset;

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

    private void EnableHologram()
    {
        GetModelScale();
        showHologram = true;
    }

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
                hologram.transform.position = hit.point + new Vector3(0, 0.05f, 0);
                hologram.transform.rotation = Quaternion.Euler(scrollOffset);
                //Set the lossy scale of the box from the metadata
                hologram.transform.localScale = modelScale;
                //Increase the y position by half the height
                var height = hologram.transform.localScale;
                hologram.transform.position += new Vector3(0, height.y / 2, 0);

                int layerMask = ~LayerMask.GetMask("Player");
                Vector3 halfExtents = hologram.transform.localScale / 2f;
                Collider[] colliders = Physics.OverlapBox(hologram.transform.position, halfExtents, Quaternion.identity, layerMask);

                if (colliders.Length > 0)
                {
                    // Set the color to red if the boxcast hits something
                    hologramRenderer.material.color = new Color(1, 0, 0, 0.25f);
                    canPlace = false;
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

    private void ScrollToRotate()
    {
        scrollOffset += (Vector3)Input.mouseScrollDelta * rotateSpeed;
    }

    private void UpdateRaycast(out RaycastHit hit, out bool didHit)
    {
        if (mainCamera)
        {
            //Change this to mouse position
            screenCenter = Input.mousePosition;
        }
        int layerMask = ~LayerMask.GetMask("Player");
        // Raycast from the camera
        Ray ray = mainCamera.ScreenPointToRay(screenCenter);
        didHit = Physics.Raycast(ray, out hit, 100, layerMask);
    }
    private void PlaceOrCancel(RaycastHit hit)
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (canPlace && !EventSystem.current.IsPointerOverGameObject())
            {
                LoadModel(hit.point, hit.transform);
            }
        }
        if (Input.GetMouseButtonUp(1))
        {
            currentTokenInfo = null;
            DisableHologram();
        }
    }
    private async Task<GameObject> LoadModel(Vector3 hitPoint, Transform hitParent)
    {
        if (currentTokenInfo == null) return null;
        hitPoint.y += 0.05f;
        var ugmDownloader = new GameObject(currentTokenInfo.metadata.name).AddComponent<UGMDownloader>();
        ugmDownloader.SetLoadOptions(false, true, true, true, true);
        ugmDownloader.transform.position = hitPoint;
        ugmDownloader.transform.rotation = Quaternion.Euler(scrollOffset);
        ugmDownloader.transform.SetParent(hitParent);
        await ugmDownloader.LoadAsync(currentTokenInfo.token_id);
        return ugmDownloader.gameObject;
    }

    private void DisableHologram()
    {
        showHologram = false;
        if (hologram.activeInHierarchy)
        {
            hologram.SetActive(false);
        }
    }
}
