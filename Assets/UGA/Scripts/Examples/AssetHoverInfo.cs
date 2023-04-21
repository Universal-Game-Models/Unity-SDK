using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UGAAssetManager;

public class AssetHoverInfo : MonoBehaviour
{
    [SerializeField]
    private GameObject panelParent;
    [SerializeField]
    private TMPro.TextMeshProUGUI metadataText;
    [SerializeField]
    private Image image;
    [SerializeField]
    private Camera cam;

    private void Update()
    {
        RaycastUGADownloader();
    }

    private void RaycastUGADownloader()
    {
        // Cast a ray from the mouse position and check for UGADownloaders in the collider or any of its parents
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var downloader = hit.transform.GetComponentInParent<UGADownloader>();
            if (downloader != null)
            {
                if (downloader.Metadata != null)
                {
                    panelParent.SetActive(true);
                    SetText(downloader.Metadata);
                    if (downloader.Image != null)
                    {
                        SetImage(downloader.Image);
                    }
                    return;
                }
            }
            panelParent.SetActive(false);          
        }
    }

    private void SetText(Metadata metadata)
    {
        string text = "";
        if (!string.IsNullOrEmpty(metadata.name)) text += "Name: " + metadata.name + "\n\n";
        if(!string.IsNullOrEmpty(metadata.description)) text += "Description: " + metadata.description + "\n\n";

        if(metadata.attributes != null && metadata.attributes.Length > 0)
        {
            text += "Attributes:\n";
            foreach (var attribute in metadata.attributes)
            {
                text += attribute.trait_type + ": " + attribute.value + "\n";
            }
        }
        metadataText.text = text;
    }

    private void SetImage(Texture2D texture)
    {
        //Create a sprite from the texture and assign it to the image
        if (texture != null)
        {
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2f);
            image.sprite = sprite;
        }
    }
}
