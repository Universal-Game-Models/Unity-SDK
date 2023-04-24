using GLTFast.Loading;
using NaughtyAttributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using static UGAAssetManager;

public class UGADownloader : MonoBehaviour
{
    [SerializeField]
    protected string assetName;

    #region Options
    [SerializeField] [Foldout("Options")]
    protected bool loadOnStart = true;
    [SerializeField][Foldout("Options")]
    protected bool addBoxColliders = false;
    [SerializeField][Foldout("Options")]
    protected bool addMeshColliders = false;
    [SerializeField][Foldout("Options")]
    protected bool loadModel = true;
    [SerializeField][Foldout("Options")]
    protected bool loadMetadata = true;
    [SerializeField][Foldout("Options")]
    protected bool loadImage = true;
    #endregion

    #region Events
    [SerializeField] [Foldout("Events")]
    protected UnityEvent<GameObject> onModelSuccess = new UnityEvent<GameObject>();
    [SerializeField] [Foldout("Events")]
    protected UnityEvent onModelFailure = new UnityEvent();
    [SerializeField] [Foldout("Events")]
    protected UnityEvent<Metadata> onMetadataSuccess = new UnityEvent<Metadata>();
    [SerializeField] [Foldout("Events")]
    protected UnityEvent onMetadataFailure = new UnityEvent();
    [SerializeField] [Foldout("Events")] 
    protected UnityEvent<Texture2D> onImageSuccess = new UnityEvent<Texture2D>();
    [SerializeField] [Foldout("Events")] 
    protected UnityEvent onImageFailure = new UnityEvent();
    #endregion

    #region Data
    private GLTFast.GltfAsset asset;
    private Metadata metadata;
    private Texture2D image;
    private bool isLoading = false;
    private GameObject instantiated;
    #endregion

    #region Virtual Functions
    protected virtual void OnModelSuccess(GameObject loadedGO) 
    {
        onModelSuccess.Invoke(loadedGO);
    }
    protected virtual void OnModelFailure()
    {
        onModelFailure.Invoke();
    }
    protected virtual void OnMetadataSuccess(Metadata metadata)
    {
        onMetadataSuccess.Invoke(metadata);
    }
    protected virtual void OnMetadataFailure()
    {
        onMetadataFailure.Invoke();
    }
    protected virtual void OnImageSuccess(Texture2D image)
    {
        onImageSuccess.Invoke(image);
    }
    protected virtual void OnImageFailure()
    {
        onImageFailure.Invoke();
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
    #endregion

    #region Public Getters
    public Metadata Metadata { get => metadata; }
    public Texture2D Image { get => image; }
    public string AssetName { get => assetName; }
    public bool IsLoading { get => isLoading; }
    public GameObject InstantiatedGO { get => instantiated; }
    #endregion

    public void Load(string assetName)
    {
        this.assetName = assetName;
        LoadAsset();
    }
    protected async void LoadAsset()
    {
        isLoading = true;
        if (loadModel)
        {
            //Load the model
            var modelUrl = UGAAssetManager.MODEL_URI + assetName + ".glb";
            bool didLoad = await DownloadModelAsync(modelUrl);
            if (didLoad)
            {
                if (asset != null ? asset.gameObject : null != null) AddColliders(asset);
                OnModelSuccess(asset.gameObject);
            }
            else
            {
                OnModelFailure();
            }
        }
        if (loadMetadata)
        {
            //Load Metadata
            var metadataUrl = UGAAssetManager.METADATA_URI + assetName + ".json";
            metadata = await DownloadMetadataAsync(metadataUrl);
            if (metadata != null)
            {
                OnMetadataSuccess(metadata);
                if (loadImage)
                {
                    //Load Image
                    var imageUrl = metadata.image;
                    image = await DownloadImageAsync(imageUrl);
                    if (image != null)
                    {
                        OnImageSuccess(image);
                    }
                    else
                    {
                        OnImageFailure();
                    }
                }
            }
            else
            {
                OnMetadataFailure();
            }
        }
        isLoading = false;
    }

    #region Private Functions
    private async Task<bool> DownloadModelAsync(string url)
    {
        if (asset == null)
        {
            asset = gameObject.AddComponent<GLTFast.GltfAsset>();
        }
        asset.InstantiationSettings = new GLTFast.InstantiationSettings() { Mask = GLTFast.ComponentType.Animation | GLTFast.ComponentType.Mesh };
        var childCount = transform.childCount;
        // Load the asset
        var didLoad = await asset.Load(url, new UgaDownloadProvider());
        if (transform.childCount > childCount)
        {
            instantiated = transform.GetChild(childCount).gameObject;
        }
        return didLoad;
    }
    private static async Task<Metadata> DownloadMetadataAsync(string url)
    {
        var request = UnityWebRequest.Get(url);

        var tcs = new TaskCompletionSource<bool>();
        var operation = request.SendWebRequest();

        operation.completed += (asyncOperation) =>
        {
            tcs.SetResult(true);
        };

        await tcs.Task;

        if (request.result == UnityWebRequest.Result.Success)
        {
            var jsonString = request.downloadHandler.text;
            try
            {
                return JsonConvert.DeserializeObject<Metadata>(jsonString);
            }
            catch (Exception e)
            {
                return null;
            }
        }
        else
        {
            throw new Exception($"HTTP error {request.responseCode}");
        }
    }
    private async Task<Texture2D> DownloadImageAsync(string url)
    {
        using (var request = UnityWebRequestTexture.GetTexture(url))
        {
            var tcs = new TaskCompletionSource<bool>();
            var operation = request.SendWebRequest();

            operation.completed += (asyncOperation) =>
            {
                tcs.SetResult(true);
            };

            await tcs.Task;

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to download image: {request.result}");
                return null;
            }

            return ((DownloadHandlerTexture)request.downloadHandler).texture;
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
                //boxCol.center = meshFilter.mesh.bounds.center;
                //boxCol.size = meshFilter.mesh.bounds.size;
            }
            //Only add box colliders to skinned meshes as they are expected to be animated
            //Could add an optional bone capsule colliders that could be used for specific use-cases
            var skinnedMeshes = asset.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var skinnedMesh in skinnedMeshes)
            {
                var boxCol = skinnedMesh.gameObject.AddComponent<BoxCollider>();
                // Convert the bounding box center to local space
                Vector3 center = skinnedMesh.transform.InverseTransformPoint(skinnedMesh.bounds.center);
                boxCol.center = center;
                // Calculate the scale factor needed to match the lossyScale
                boxCol.size = Vector3.Scale(boxCol.size, skinnedMesh.transform.lossyScale);
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
    #endregion

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
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "UGA")))
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "UGA"));
        }
        string cachePath = Path.Combine(Application.persistentDataPath, "UGA", url.ToString().GetHashCode().ToString());
        byte[] bytes;

        if (File.Exists(cachePath))
        {
            bytes = File.ReadAllBytes(cachePath);

            using (var webRequest = UnityWebRequest.Head(url))
            {
                webRequest.SetRequestHeader("If-Modified-Since", File.GetLastWriteTimeUtc(cachePath).ToString("r"));

                var downloadRequest = webRequest.SendWebRequest();

                var downloadTcs = new TaskCompletionSource<bool>();
                downloadRequest.completed += (asyncOp) =>
                {
                    downloadTcs.SetResult(true);
                };

                await downloadTcs.Task;

                if (webRequest.responseCode == (int)HttpStatusCode.NotModified)
                {
                    return new Download(url.ToString(), bytes);
                }
                else if (webRequest.responseCode == (int)HttpStatusCode.OK)
                {
                    var req = new CustomHeaderDownload(url, AddHeaders);
                    await req.WaitAsync();
                    bytes = req.Data;
                    SaveBytes(cachePath, bytes);
                }
                else
                {
                    return new Download(url.ToString(), null, $"HTTP error {webRequest.responseCode}");
                }
            }
        }
        else
        {
            var req = new CustomHeaderDownload(url, AddHeaders);
            await req.WaitAsync();
            bytes = req.Data;
            SaveBytes(cachePath, bytes);
            return req;
        }

        return new Download(url.ToString(), bytes);
    }

    private void SaveBytes(string path, byte[] bytes)
    {
        using (var fileStream = new FileStream(path, FileMode.OpenOrCreate))
        {
            fileStream.Write(bytes);
            PlayerPrefs.SetString("forceSave", string.Empty);
            PlayerPrefs.Save();
        }
    }

    private void AddHeaders(UnityWebRequest request)
    {
        request.SetRequestHeader("x-api-key", UGAAssetManager.GetConfig().apiKey);
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