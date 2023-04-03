using GLTFast.Loading;
using NaughtyAttributes;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class UGADownloader : MonoBehaviour
{
    [SerializeField]
    protected string assetName;
    [SerializeField]
    protected bool loadOnStart = true;

    [SerializeField]
    protected bool addBoxColliders = false;
    [SerializeField]
    protected bool addMeshColliders = false;

    [SerializeField]
    protected UnityEvent<GameObject> onSuccess = new UnityEvent<GameObject>();
    [SerializeField]
    protected UnityEvent onFailure = new UnityEvent();

    private GLTFast.GltfAsset asset;

    protected virtual void OnSuccess(GameObject loadedGO) 
    {
        onSuccess.Invoke(loadedGO);
    }
    protected virtual void OnFailure()
    {
        onFailure.Invoke();
    }

    protected virtual void Start()
    {
        if (loadOnStart && !string.IsNullOrEmpty(assetName))
        {
            LoadAsset();
        }
    }
    protected virtual void OnDestroy()
    {

    }

    public async void LoadAsset()
    {
        // Remove any leading or trailing spaces
        assetName = assetName.Trim().Replace(" ", "").ToLower();
        var url = UGAAssetManager.UGA_URI + assetName + ".glb";
 
        if (asset == null)
        {
            asset = gameObject.AddComponent<GLTFast.GltfAsset>();
        }
        asset.InstantiationSettings = new GLTFast.InstantiationSettings() { Mask = GLTFast.ComponentType.Animation | GLTFast.ComponentType.Mesh };
        // Load the asset
        var didLoad = await asset.Load(url, new UgaDownloadProvider());
        if (didLoad)
        {
            if (asset != null ? asset.gameObject : null != null) AddColliders(asset);
            OnSuccess(asset.gameObject);
        }
        else
        {
            if(asset != null ? asset.gameObject : null != null) Destroy(asset.gameObject);
            OnFailure();
        }
    }

    private void AddColliders(GLTFast.GltfAsset asset)
    {
        if (addBoxColliders)
        {
            var meshFilters = asset.gameObject.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in meshFilters)
            {
                var boxCol = meshFilter.gameObject.AddComponent<BoxCollider>();
                boxCol.center = meshFilter.mesh.bounds.center;
                boxCol.size = meshFilter.mesh.bounds.size;
            }
        }
        else if (addMeshColliders)
        {
            var meshFilters = asset.gameObject.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in meshFilters)
            {
                var meshCol = meshFilter.gameObject.AddComponent<MeshCollider>();
                meshCol.sharedMesh = meshFilter.mesh;
            }
        }
    }
    [Button]
    public void ClearCache()
    {
        UGAAssetManager.ClearCache();
    }
}

class UgaDownloadProvider : GLTFast.Loading.IDownloadProvider
{
    public async Task<IDownload> Request(Uri url)
    {
        if(!Directory.Exists(Path.Combine(Application.persistentDataPath, "UGA")))
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "UGA"));
        }
        string cachePath = Path.Combine(Application.persistentDataPath, "UGA", url.ToString().GetHashCode().ToString());
        byte[] bytes;

        if (File.Exists(cachePath))
        {
            bytes = File.ReadAllBytes(cachePath);
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("x-api-key", UGAAssetManager.GetConfig().apiKey);
                httpClient.DefaultRequestHeaders.IfModifiedSince = File.GetLastWriteTimeUtc(cachePath);

                var response = await httpClient.GetAsync(url);

                if (response.StatusCode == HttpStatusCode.NotModified)
                {
                    return new Download(url.ToString(), bytes);
                }
                else if (response.IsSuccessStatusCode)
                {
                    bytes = await response.Content.ReadAsByteArrayAsync();

                    using (var fileStream = new FileStream(cachePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                    {
                        await fileStream.WriteAsync(bytes);
                    }
                }
                else
                {
                    return new Download(url.ToString(), null, $"HTTP error {response.StatusCode}");
                }
            }
        }
        else
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("x-api-key", UGAAssetManager.GetConfig().apiKey);

                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    bytes = await response.Content.ReadAsByteArrayAsync();

                    using (var fileStream = new FileStream(cachePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                    {
                        await fileStream.WriteAsync(bytes);
                    }
                }
                else
                {
                    return new Download(url.ToString(), null, $"HTTP error {response.StatusCode}");
                }
            }
        }

        return new Download(url.ToString(), bytes);
    }

    public class Download : IDownload
    {
        private string url;
        private byte[] data;
        private string errorMessage;

        public Download(string url, byte[] bytes)
        {
            this.url = url;
            this.data = bytes;
        }
        public Download(string url, byte[] bytes, string errorMessage) : this(url, bytes)
        {
            this.errorMessage = errorMessage;
        }

        private bool disposed = false;

        public byte[] Data
        {
            get
            {
                return data;
            }
        }

        public bool Success
        {
            get
            {
                return data != null && data.Length > 0 && string.IsNullOrEmpty(errorMessage);
            }
        }

        public string Error
        {
            get
            {
                return errorMessage;
            }
        }

        public string Text
        {
            get
            {
                return Encoding.UTF8.GetString(data);
            }
        }

        public bool? IsBinary
        {
            get
            {
                return url.Contains(".glb");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    url = null;
                    errorMessage = null;
                    data = null;
                }

                disposed = true;
            }
        }

        ~Download()
        {
            Dispose(false);
        }
    }

    public async Task<ITextureDownload> RequestTexture(Uri url, bool nonReadable)
    {
        var req = new AwaitableTextureDownload(url, nonReadable);
        await req.WaitAsync();
        return req;
    }
    public class AwaitableTextureDownload : AwaitableDownload, ITextureDownload
    {

        /// <summary>
        /// Parameter-less constructor, required for inheritance.
        /// </summary>
        protected AwaitableTextureDownload() { }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="url">Texture URI to request</param>
        /// <param name="nonReadable">If true, resulting texture is not CPU readable (uses less memory)</param>
        public AwaitableTextureDownload(Uri url, bool nonReadable)
        {
            Init(url, nonReadable);
        }

        /// <summary>
        /// Generates the UnityWebRequest used for sending the request.
        /// </summary>
        /// <param name="url">Texture URI to request</param>
        /// <param name="nonReadable">If true, resulting texture is not CPU readable (uses less memory)</param>
        /// <returns>UnityWebRequest used for sending the request</returns>
        protected static UnityWebRequest CreateRequest(Uri url, bool nonReadable)
        {
            return UnityWebRequestTexture.GetTexture(url, nonReadable);
        }

        void Init(Uri url, bool nonReadable)
        {
            m_Request = CreateRequest(url, nonReadable);
            m_AsyncOperation = m_Request.SendWebRequest();
        }

        /// <inheritdoc />
        public Texture2D Texture => (m_Request?.downloadHandler as DownloadHandlerTexture)?.texture;
    }
}