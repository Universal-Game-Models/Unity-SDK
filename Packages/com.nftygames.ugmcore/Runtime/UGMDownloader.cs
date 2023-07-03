using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GLTFast.Loading;
using NaughtyAttributes;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace UGM.Core
{
    public class UGMDownloader : MonoBehaviour
    {
        [SerializeField] [Tooltip("Identification of the NFT.")]
        protected string nftId;

        #region Load Options
        [SerializeField] [Foldout("Options")] [Tooltip("Determines if the model should be loaded automatically on start.")]
        protected bool loadOnStart = true;
        [SerializeField][Foldout("Options")] [Tooltip("Determines if box colliders should be added to the model.")]
        protected bool addBoxColliders = true;
        [SerializeField][Foldout("Options")] [Tooltip("Determines if mesh colliders should be added to the model.")]
        protected bool addMeshColliders = false;
        [SerializeField][Foldout("Options")] [Tooltip("Determines if the model should be loaded.")]
        protected bool loadModel = true;
        [SerializeField][Foldout("Options")] [Tooltip("Determines if the metadata should be loaded.")]
        protected bool loadMetadata = true;
        [SerializeField][Foldout("Options")] [Tooltip("Determines if the image should be loaded.")]
        protected bool loadImage = true;
        #endregion

        #region Public Events
        [Foldout("Events")] [Tooltip("Event invoked when the model is successfully loaded, passing the loaded GameObject.")]
        public UnityEvent<GameObject> onModelSuccess = new UnityEvent<GameObject>();
        [Foldout("Events")] [Tooltip("Event invoked when there is a failure in loading the model.")]
        public UnityEvent onModelFailure = new UnityEvent();
        [Foldout("Events")] [Tooltip(" Event invoked when the metadata is successfully loaded, passing the loaded Metadata object.")]
        public UnityEvent<UGMDataTypes.Metadata> onMetadataSuccess = new UnityEvent<UGMDataTypes.Metadata>();
        [Foldout("Events")] [Tooltip("Event invoked when there is a failure in loading the metadata.")]
        public UnityEvent onMetadataFailure = new UnityEvent();
        [Foldout("Events")] [Tooltip(" Event invoked when the image is successfully loaded, passing the loaded Texture2D.")]
        public UnityEvent<Texture2D> onImageSuccess = new UnityEvent<Texture2D>();
        [Foldout("Events")] [Tooltip("Event invoked when there is a failure in loading the image.")]
        public UnityEvent onImageFailure = new UnityEvent();
        [Foldout("Events")] [Tooltip("Event invoked when an animation starts, passing the animation name.")]
        public UnityEvent<string> onAnimationStart = new UnityEvent<string>();
        [Foldout("Events")] [Tooltip("Event invoked when an animation ends, passing the animation name.")]
        public UnityEvent<string> onAnimationEnd = new UnityEvent<string>();
        #endregion

        #region Private Variables
        /// <summary>
        /// The GLTFast.GltfAsset representing the loaded model.
        /// </summary>
        private GLTFast.GltfAsset asset;
        /// <summary>
        /// The Metadata associated with the loaded model.
        /// </summary>
        private UGMDataTypes.Metadata metadata;
        /// <summary>
        /// The Texture2D representing the loaded image.
        /// </summary>
        private Texture2D image;
        /// <summary>
        /// Indicates if the model is currently being loaded.
        /// </summary>
        private bool isLoading = false;
        /// <summary>
        /// The instantiated GameObject of the loaded model.
        /// </summary>
        private GameObject instantiated;
        /// <summary>
        /// The Animation component of the loaded model containing embedded animations.
        /// </summary>
        private Animation embeddedAnimationsComponent;
        /// <summary>
        /// The name of the currently playing embedded animation.
        /// </summary>
        private string currentEmbeddedAnimationName;
        #endregion

        #region Private Functions
        /// <summary>
        /// Asynchronously downloads the model associated with the specified NFT ID.
        /// <para />
        /// Providing flexibility in loading assets and handling success or failure scenarios.
        /// </summary>
        /// <param name="nftId">The NFT ID of the model to download.</param>
        /// <returns>A task representing the asynchronous operation, returning a bool indicating whether the download was successful.</returns>
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
    
        /// <summary>
        /// Adds colliders to the GameObjects in the GLTFast.GltfAsset based on the specified collider options.
        /// <para />
        /// Providing options for MeshColliders and BoxColliders based on the specified settings.
        /// </summary>
        /// <param name="asset">The GLTFast.GltfAsset containing the GameObjects to add colliders to.</param>
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
    
        /// <summary>
        /// Coroutine that waits for the specified animation to end and invokes the corresponding event.
        /// </summary>
        /// <param name="animationName">The name of the animation to wait for.</param>
        /// <param name="clipLength">The length of the animation clip in seconds.</param>
        /// <returns>An IEnumerator used for the coroutine.</returns>
        private IEnumerator WaitForAnimationEnd(string animationName, float clipLength)
        {
            yield return new WaitForSeconds(clipLength);
            OnAnimationEnd(animationName);
        }
        #endregion

        #region Virtual Functions
        /// <summary>
        /// Invokes the onModelSuccess event with the provided loaded GameObject.
        /// </summary>
        /// <param name="loadedGO">The loaded GameObject to pass to the onModelSuccess event.</param>
        protected virtual void OnModelSuccess(GameObject loadedGO) 
        {
            onModelSuccess.Invoke(loadedGO);
        }
        /// <summary>
        /// Invokes the onModelFailure event to handle model loading failure.
        /// </summary>
        protected virtual void OnModelFailure()
        {
            onModelFailure.Invoke();
        }
        /// <summary>
        /// Invokes the onMetadataSuccess event with the provided metadata.
        /// </summary>
        /// <param name="metadata">The metadata associated with the model.</param>
        protected virtual void OnMetadataSuccess(UGMDataTypes.Metadata metadata)
        {
            onMetadataSuccess.Invoke(metadata);
        }
        /// <summary>
        /// Invokes the onMetadataFailure event to handle metadata retrieval failure.
        /// </summary>
        protected virtual void OnMetadataFailure()
        {
            onMetadataFailure.Invoke();
        }
        /// <summary>
        /// Invokes the onImageSuccess event with the provided image.
        /// </summary>
        /// <param name="image">The image associated with the model.</param>
        protected virtual void OnImageSuccess(Texture2D image)
        {
            onImageSuccess.Invoke(image);
        }
        /// <summary>
        /// Invokes the onImageFailure event to handle image retrieval failure.
        /// </summary>
        protected virtual void OnImageFailure()
        {
            onImageFailure.Invoke();
        }
    
        /// <summary>
        /// Invokes the onAnimationStart event with the provided animationName.
        /// </summary>
        /// <param name="animationName">The name of the animation that started.</param>
        protected virtual void OnAnimationStart(string animationName)
        {
            currentEmbeddedAnimationName = animationName;
            onAnimationStart.Invoke(animationName);
        }
    
        /// <summary>
        /// Invokes the onAnimationEnd event with the provided animationName.
        /// </summary>
        /// <param name="animationName">The name of the animation that Ended.</param>
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
        /// <summary>
        /// Gets the metadata associated with the model.
        /// </summary>
        public UGMDataTypes.Metadata Metadata { get => metadata; }
        /// <summary>
        /// Gets the image associated with the model.
        /// </summary>
        public Texture2D Image { get => image; }
        /// <summary>
        /// Gets the name of the asset.
        /// </summary>
        public string AssetName { get => metadata?.name; }
        /// <summary>
        /// Gets a value indicating whether the model is currently being loaded.
        /// </summary>
        public bool IsLoading { get => isLoading; }
        /// <summary>
        /// Gets the instantiated GameObject of the loaded model.
        /// </summary>
        public GameObject InstantiatedGO { get => instantiated; }
        /// <summary>
        /// Gets the name of the currently playing embedded animation.
        /// </summary>
        public string CurrentEmbeddedAnimationName { get => currentEmbeddedAnimationName; }
        #endregion

        #region Public Functions
        /// <summary>
        /// Sets the load options for the model, such as whether to add colliders, load the model, load the metadata, and load the image.
        /// </summary>
        /// <param name="addBoxColliders">Whether to add box colliders to the model.</param>
        /// <param name="addMeshColliders">Whether to add mesh colliders to the model.</param>
        /// <param name="loadModel">Whether to load the model.</param>
        /// <param name="loadMetadata">Whether to load the metadata.</param>
        /// <param name="loadImage">Whether to load the image.</param>
        public void SetLoadOptions(bool addBoxColliders, bool addMeshColliders, bool loadModel, bool loadMetadata, bool loadImage)
        {
            this.addBoxColliders = addBoxColliders;
            this.addMeshColliders = addMeshColliders;
            this.loadModel = loadModel;
            this.loadMetadata = loadMetadata;
            this.loadImage = loadImage;
        }
        /// <summary>
        /// Loads the model synchronously by calling LoadAsync.
        /// </summary>
        /// <param name="nftId">The ID of the model to load.</param>
        public void Load(string nftId)
        {
            LoadAsync(nftId);
        }
        /// <summary>
        /// Loads the model asynchronously. It downloads the model, metadata,
        /// and image based on the load options.
        /// It also handles callbacks for success or failure events.
        /// </summary>
        /// <param name="nftId">The ID of the model to load.</param>
        public async Task LoadAsync(string nftId)
        {
            this.nftId = nftId;
            //Prevent double load
            if (isLoading)
            {
                Debug.LogWarning("Double load");
                return;
            }
            isLoading = true;
            try
            {
                if (loadModel)
                {
                    if (embeddedAnimationsComponent)
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
            }
            catch (Exception e)
            {
                isLoading = false;
            }
            isLoading = false;
        }
        /// <summary>
        /// Downloads the metadata of the model asynchronously.
        /// It parses the JSON response and returns the deserialized metadata object.
        /// </summary>
        /// <param name="nftId">The ID of the model.</param>
        /// <returns>The downloaded metadata.</returns>
        public static async Task<UGMDataTypes.Metadata> DownloadMetadataAsync(string nftId)
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
                    return JsonConvert.DeserializeObject<UGMDataTypes.Metadata>(jsonString);
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
    
        /// <summary>
        /// Downloads the image associated with the model asynchronously.
        /// It returns the downloaded image as a Texture2D object.
        /// </summary>
        /// <param name="url">The URL of the image.</param>
        /// <returns>The downloaded image as a Texture2D.</returns>
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
        /// <summary>
        /// Plays the specified animation on the model.
        /// It handles animation looping
        /// </summary>
        /// <param name="animationName">The name of the animation to play.</param>
        /// <param name="loop">Whether to loop the animation.</param>
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
        /// <summary>
        /// Stops the currently playing animation on the model.
        /// </summary>
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

    
        /// <summary>
        /// Clears the cache used by the UGMManager.
        /// </summary>
        [Button]
        public void ClearCache()
        {
            UGMManager.ClearCache();
        }
    }

    class UGMDownloadProvider : GLTFast.Loading.IDownloadProvider
    {
        /// <summary>
        /// Requests a download from the specified URL asynchronously.
        ///
        /// <para />
        /// The method first checks if the cache directory exists and creates it if not.
        /// It then checks if the file exists in the cache and sends a HEAD request to check if the file has been modified.
        /// If the file has not been modified, it returns the cached download.
        /// If the file has been modified or is not present in the cache, it performs a full download and saves the file.
        /// <para />
        /// Finally, it returns an instance of IDownload representing the download result.
        /// </summary>
        /// <param name="url">The URL to request the download from.</param>
        /// <returns>An instance of IDownload representing the download result.</returns>
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
        /// <summary>
        /// Saves the byte array to the specified file path.
        /// <para />
        /// The method opens or creates a file stream using the given path and writes the bytes to the file.
        /// After saving the bytes, it sets the "forceSave" PlayerPrefs key to an empty string to indicate that the save was successful.
        /// Finally, it saves the PlayerPrefs to persist the change.
        /// </summary>
        /// <param name="path">The file path to save the bytes to.</param>
        /// <param name="bytes">The byte array to be saved.</param>
        private void SaveBytes(string path, byte[] bytes)
        {
            // Check if the file already exists
            if (File.Exists(path))
            {
                // Delete the existing file
                File.Delete(path);
            }
            using (var fileStream = new FileStream(path, FileMode.OpenOrCreate))
            {
                fileStream.Write(bytes);
                PlayerPrefs.SetString("forceSave", string.Empty);
                PlayerPrefs.Save();
            }
        }
        /// <summary>
        /// Adds custom headers to the UnityWebRequest.
        /// <para />
        /// Specifically, it sets the "x-api-key" header using the API key retrieved from the UGMManager configuration.
        /// This method allows additional headers to be added to the request for authentication or other purposes.
        /// </summary>
        /// <param name="request">The UnityWebRequest to add headers to.</param>
        private void AddHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("x-api-key", UGMManager.GetConfig().apiKey);
        }

        public class Download : IDownload
        {
            private string url;
            private byte[] data;
            private string errorMessage;

            /// <summary>
            /// Constructor of Download Class
            /// <para />
            /// Creates a new Download object with the specified URL and byte array data.
            /// </summary>
            /// <param name="url">The URL of the download.</param>
            /// <param name="bytes">The byte array data of the download.</param>
            public Download(string url, byte[] bytes)
            {
                this.url = url;
                this.data = bytes;
            }
        
            /// <summary>
            /// Constructor of Download Class
            /// <para />
            /// Creates a new Download object with the specified URL, byte array data, and error message.
            /// </summary>
            /// <param name="url">The URL of the download.</param>
            /// <param name="bytes">The byte array data of the download.</param>
            /// <param name="errorMessage">The error message associated with the download, if any.</param>
            public Download(string url, byte[] bytes, string errorMessage) : this(url, bytes)
            {
                this.errorMessage = errorMessage;
            }

            private bool disposed = false;

            /// <summary>
            /// Gets the byte array data of the download.
            /// </summary>
            public byte[] Data
            {
                get
                {
                    return data;
                }
            }
            /// <summary>
            /// Gets a value indicating whether the download was successful.
            /// </summary>
            public bool Success
            {
                get
                {
                    return data != null && data.Length > 0 && string.IsNullOrEmpty(errorMessage);
                }
            }
            /// <summary>
            /// Gets the error message associated with the download, if any.
            /// </summary>
            public string Error
            {
                get
                {
                    return errorMessage;
                }
            }
            /// <summary>
            /// Gets the text representation of the downloaded data.
            /// </summary>
            public string Text
            {
                get
                {
                    return Encoding.UTF8.GetString(data);
                }
            }
            /// <summary>
            /// Gets a value indicating whether the download contains binary data.
            /// </summary>
            public bool? IsBinary
            {
                get
                {
                    return url.Contains(".glb");
                }
            }
            /// <summary>
            /// Disposes of the resources used by the Download object.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            /// <summary>
            /// Releases the managed and optionally the unmanaged resources used by the Download object.
            /// </summary>
            /// <param name="disposing">A boolean value indicating whether to dispose of managed resources.</param>
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
            /// <summary>
            /// Finalizes an instance of the Download class.
            /// </summary>
            ~Download()
            {
                Dispose(false);
            }
        }
        /// <summary>
        /// Requests a texture download from the specified URL.
        ///
        /// <para />
        ///  It takes the URL of the texture and a flag indicating whether the downloaded texture should be set as non-readable.
        /// The method creates a new AwaitableTextureDownload object to handle the texture download and awaits its completion.
        /// Finally, it returns the ITextureDownload object representing the downloaded texture.
        /// </summary>
        /// <param name="url">The URL of the texture to download.</param>
        /// <param name="nonReadable">A flag indicating whether the downloaded texture should be set as non-readable.</param>
        /// <returns>An awaitable task that resolves to an ITextureDownload object representing the downloaded texture.</returns>
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

            /// <summary>
            /// Initializes the texture download by creating a UnityWebRequest and starting the asynchronous operation.
            /// </summary>
            /// <param name="url">The URL of the texture to download.</param>
            /// <param name="nonReadable">A flag indicating whether the downloaded texture should be set as non-readable.</param>
            void Init(Uri url, bool nonReadable)
            {
                m_Request = CreateRequest(url, nonReadable);
                m_AsyncOperation = m_Request.SendWebRequest();
            }

            /// <inheritdoc />
            public Texture2D Texture => (m_Request?.downloadHandler as DownloadHandlerTexture)?.texture;
        }
    }
}