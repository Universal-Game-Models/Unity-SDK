using UGM.Core;
using UnityEngine;
using UnityEngine.UI;
using static UGM.Core.UGMDataTypes;

namespace UGM.Examples
{
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
        [SerializeField]
        private bool showAttributes = false;

        private UGMDownloader current;
        private float timer = 0f;
        private float interval = 1f;

        private void Update()
        {
            timer += Time.deltaTime;

            if (timer >= interval)
            {
                timer = 0;
                RaycastUGADownloader();
            }
        }

        private void RaycastUGADownloader()
        {
            // Cast a ray from the mouse position and check for UGADownloaders in the collider or any of its parents
            Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
            Ray ray = cam.ScreenPointToRay(screenCenter);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var downloader = hit.transform.GetComponentInParent<UGMDownloader>();
                if (downloader != null)
                {
                    if (downloader.Metadata != null)
                    {
                        current = downloader;
                        panelParent.SetActive(true);
                        SetText(current.Metadata);
                        if (current.Image != null)
                        {
                            SetImage(current.Image);
                        }
                        return;
                    }
                }
                else
                {
                    panelParent.SetActive(false);
                }
            }
        }

        private void SetText(Metadata metadata)
        {
            string text = "";
            if (!string.IsNullOrEmpty(metadata.name)) text += "Name: " + metadata.name + "\n\n";
            if(!string.IsNullOrEmpty(metadata.description)) text += "Description: " + metadata.description + "\n\n";

            if(showAttributes && metadata.attributes != null && metadata.attributes.Length > 0)
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
                image.preserveAspect = true;
            }
        }
    }
}
