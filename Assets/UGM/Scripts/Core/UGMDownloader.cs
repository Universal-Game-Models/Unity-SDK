using GLTFast.Loading;
using NaughtyAttributes;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using static UGMDataTypes;

public class UGMDownloader : MonoBehaviour
{
    [SerializeField]
    protected string nftId;

    #region Load Options
    [SerializeField] [Foldout("Options")]
    protected bool loadOnStart = true;
    [SerializeField][Foldout("Options")]
    protected bool addBoxColliders = true;
    [SerializeField][Foldout("Options")]
    protected bool addMeshColliders = false;
    [SerializeField][Foldout("Options")]
    protected bool loadModel = true;
    [SerializeField][Foldout("Options")]
    protected bool loadMetadata = true;
    [SerializeField][Foldout("Options")]
    protected bool loadImage = true;
    #endregion

    #region Public Events
    [Foldout("Events")]
    public UnityEvent<GameObject> onModelSuccess = new UnityEvent<GameObject>();
    [Foldout("Events")]
    public UnityEvent onModelFailure = new UnityEvent();
    [Foldout("Events")]
    public UnityEvent<Metadata> onMetadataSuccess = new UnityEvent<Metadata>();
    [Foldout("Events")]
    public UnityEvent onMetadataFailure = new UnityEvent();
    [Foldout("Events")] 
    public UnityEvent<Texture2D> onImageSuccess = new UnityEvent<Texture2D>();
    [Foldout("Events")] 
    public UnityEvent onImageFailure = new UnityEvent();
    [Foldout("Events")]
    public UnityEvent<string> onAnimationStart = new UnityEvent<string>();
    [Foldout("Events")]
    public UnityEvent<string> onAnimationEnd = new UnityEvent<string>();
    #endregion

    #region Private Variables
    private GLTFast.GltfAsset asset;
    private Metadata metadata;
    private Texture2D image;
    private bool isLoading = false;
    private GameObject instantiated;
    private Animation embeddedAnimationsComponent;
    private string currentEmbeddedAnimationName;
    #endregion

    #region Private Functions
    private async Task<bool> DownloadModelAsync(string nftId)
    {
        if (asset == null)
        {
            asset = gameObject.AddComponent<GLTFast.GltfAsset>();
        }
        asset.InstantiationSettings = new GLTFast.InstantiationSettings() { Mask = GLTFast.ComponentType.Animation | GLTFast.ComponentType.Mesh };
        var childCount = transform.childCount;
        // Load the asset
        nftId = int.Parse(nftId).ToString("X").ToLower();
        var url = UGMManager.MODEL_URI + nftId.PadLeft(64, '0') + ".glb";
        var didLoad = await asset.Load(url, new UGMDownloadProvider());
        if (transform.childCount > childCount)
        {
            instantiated = transform.GetChild(childCount).gameObject;
        }
        return didLoad;
    }
    private void AddColliders(GLTFast.GltfAsset asset)
    {
        if (addMeshColliders)
        {
            var meshFilters = asset.gameObject.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in meshFilters)
            {
                var meshCol = meshFilter.gameObject.AddComponent<MeshCollider>();
                meshCol.sharedMesh = meshFilter.mesh;
            }
        }
        else if (addBoxColliders)
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
    }
    private IEnumerator WaitForAnimationEnd(string animationName, float clipLength)
    {
        yield return new WaitForSeconds(clipLength);
        OnAnimationEnd(animationName);
    }
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
    protected virtual void OnAnimationStart(string animationName)
    {
        currentEmbeddedAnimationName = animationName;
        onAnimationStart.Invoke(animationName);
    }
    protected virtual void OnAnimationEnd(string animationName)
    {
        currentEmbeddedAnimationName = "";
        onAnimationEnd.Invoke(animationName);
    }

    protected virtual void Start()
    {
        asset = GetComponent<GLTFast.GltfAsset>();
        if (loadOnStart && !string.IsNullOrEmpty(nftId))
        {
            Load(nftId);
        }
    }
    protected virtual void OnDestroy()
    {

    }
    #endregion

    #region Public Getters
    public Metadata Metadata { get => metadata; }
    public Texture2D Image { get => image; }
    public string AssetName { get => metadata?.name; }
    public bool IsLoading { get => isLoading; }
    public GameObject InstantiatedGO { get => instantiated; }

    public string CurrentEmbeddedAnimationName { get => currentEmbeddedAnimationName; }
    #endregion

    #region Public Functions
    public void SetLoadOptions(bool addBoxColliders, bool addMeshColliders, bool loadModel, bool loadMetadata, bool loadImage)
    {
        this.addBoxColliders = addBoxColliders;
        this.addMeshColliders = addMeshColliders;
        this.loadModel = loadModel;
        this.loadMetadata = loadMetadata;
        this.loadImage = loadImage;
    }

    public void Load(string nftId)
    {
        LoadAsync(nftId);
    }
    public async Task LoadAsync(string nftId)
    {
        this.nftId = nftId;
        //Prevent double load
        if (isLoading) return;
        isLoading = true;
        if (loadModel)
        {
            if(embeddedAnimationsComponent)
            {
                DestroyImmediate(embeddedAnimationsComponent);
            }
            if (InstantiatedGO != null)
            {
                DestroyImmediate(InstantiatedGO);
            }
            //Load the model
            bool didLoad = await DownloadModelAsync(nftId);
            if (didLoad)
            {
                if (asset != null ? asset.gameObject : null != null) AddColliders(asset);
                OnModelSuccess(asset.gameObject);
                embeddedAnimationsComponent = gameObject.GetComponentInChildren<Animation>();
            }
            else
            {
                OnModelFailure();
            }
        }
        if (loadMetadata)
        {
            //Load Metadata
            metadata = await DownloadMetadataAsync(nftId);
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

    public static async Task<Metadata> DownloadMetadataAsync(string nftId)
    {
        nftId = int.Parse(nftId).ToString("X").ToLower();
        var url = UGMManager.METADATA_URI + nftId.PadLeft(64, '0') + ".json";
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
    public static async Task<Texture2D> DownloadImageAsync(string url)
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

    public void PlayAnimation(string animationName = "", bool loop = false)
    {
        if (embeddedAnimationsComponent && embeddedAnimationsComponent.GetClipCount() > 0)
        {
            var animClipName = animationName;
            var clip = embeddedAnimationsComponent.GetClip(animationName);
            if (string.IsNullOrEmpty(animationName) || clip == null)
            {
                foreach (AnimationState animState in embeddedAnimationsComponent)
                {
                    clip = animState.clip;
                    animClipName = clip.name;
                    break;
                }
            }
            if (clip)
            {
                if (embeddedAnimationsComponent.isPlaying)
                {
                    embeddedAnimationsComponent.Stop();
                }
                OnAnimationStart(animationName);
                if (loop)
                {
                    embeddedAnimationsComponent.wrapMode = WrapMode.Loop;
                    embeddedAnimationsComponent.Play(animClipName);
                }
                else
                {
                    embeddedAnimationsComponent.wrapMode = WrapMode.Default;
                    embeddedAnimationsComponent.Play(animClipName);
                    StartCoroutine(WaitForAnimationEnd(animationName, clip.length));
                }
            }
        }
    }
    public void StopAnimation()
    {
        if (embeddedAnimationsComponent && embeddedAnimationsComponent.isPlaying)
        {
            var clip = embeddedAnimationsComponent.clip;
            embeddedAnimationsComponent.Stop();
            // Set the hand position to the first frame's pose
            AnimationState animState = embeddedAnimationsComponent[clip.name];
            animState.time = 0f;
            animState.enabled = true;
            animState.weight = 1f;
            embeddedAnimationsComponent.Sample();
            animState.enabled = false;
            OnAnimationEnd(clip.name);
        }
    }
    #endregion

    [Button]
    public void ClearCache()
    {
        UGMManager.ClearCache();
    }
}

class UGMDownloadProvider : GLTFast.Loading.IDownloadProvider
{
    public async Task<IDownload> Request(Uri url)
    {
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "UGM")))
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "UGM"));
        }
        string cachePath = Path.Combine(Application.persistentDataPath, "UGM", url.ToString().GetHashCode().ToString());
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
        request.SetRequestHeader("x-api-key", UGMManager.GetConfig().apiKey);
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