using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

    public  class ImageSourceEdited : MonoBehaviour
    {
        #region Public property

        public Texture Texture => OutputBuffer;
        public Vector2Int OutputResolution => _outputResolution;

        #endregion

        #region Editable attributes

        // Source type options
        public enum SourceType { Texture, Video, Webcam, Card, Gradient }
        [SerializeField] SourceType _sourceType = SourceType.Card;

        // Texture mode options
        [SerializeField] public Texture2D _texture = null;
        [SerializeField] public string _textureUrl = null;

        // Video mode options
        [SerializeField] VideoClip _video = null;
        [SerializeField] string _videoUrl = null;

        // Webcam options
        [SerializeField] string _webcamName = "";
        [SerializeField] Vector2Int _webcamResolution = new Vector2Int(1920, 1080);
        [SerializeField] int _webcamFrameRate = 30;

        // Output options
        [SerializeField] RenderTexture _outputTexture = null;
        [SerializeField] Vector2Int _outputResolution = new Vector2Int(1920, 1080);

        #endregion

        #region Package asset reference

        [SerializeField, HideInInspector] Shader _shader = null;

        #endregion

        #region Private members

        UnityWebRequest _webTexture;
        WebCamTexture _webcam;
        Material _material;
        RenderTexture _buffer;

        RenderTexture OutputBuffer
          => _outputTexture != null ? _outputTexture : _buffer;

        // Blit a texture into the output buffer with aspect ratio compensation.
        void Blit(Texture source, bool vflip = false)
        {
            if (source == null) return;

            var aspect1 = (float)source.width / source.height;
            var aspect2 = (float)OutputBuffer.width / OutputBuffer.height;

            var scale = new Vector2(aspect2 / aspect1, aspect1 / aspect2);
            scale = Vector2.Min(Vector2.one, scale);
            if (vflip) scale.y *= -1;

            var offset = (Vector2.one - scale) / 2;

            Graphics.Blit(source, OutputBuffer, scale, offset);
        }

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            // Allocate a render texture if no output texture has been given.
            if (_outputTexture == null)
                _buffer = new RenderTexture
                  (_outputResolution.x, _outputResolution.y, 0);

            // Create a material for the shader (only on Card and Gradient)
            if (_sourceType == SourceType.Card || _sourceType == SourceType.Gradient)
                _material = new Material(_shader);

            // Texture source type:
            // Blit a given texture, or download a texture from a given URL.
            if (_sourceType == SourceType.Texture)
            {
                if (_texture != null)
                {
                    Blit(_texture);
                }
                //else
                //{
                //    _webTexture = UnityWebRequestTexture.GetTexture(_textureUrl);
                //    _webTexture.SendWebRequest();
                //}
            }

            // Video source type:
            // Add a video player component and play a given video clip with it.
            //if (_sourceType == SourceType.Video)
            //{
            //    var player = gameObject.AddComponent<VideoPlayer>();
            //    player.source =
            //      _video != null ? VideoSource.VideoClip : VideoSource.Url;
            //    player.clip = _video;
            //    player.url = _videoUrl;
            //    player.isLooping = true;
            //    player.renderMode = VideoRenderMode.APIOnly;
            //    player.Play();
            //}

            // Webcam source type:
            // Create a WebCamTexture and start capturing.
            if (_sourceType == SourceType.Webcam)
            {
                _webcam = new WebCamTexture
                  (_webcamName,
                   _webcamResolution.x, _webcamResolution.y, _webcamFrameRate);
                _webcam.Play();
            }

            // Card source type:
            // Run the card shader to generate a test card image.
            if (_sourceType == SourceType.Card)
            {
                var dims = new Vector2(OutputBuffer.width, OutputBuffer.height);
                _material.SetVector("_Resolution", dims);
                Graphics.Blit(null, OutputBuffer, _material, 0);
            }
        }

        void OnDestroy()
        {
            if (_webcam != null) Destroy(_webcam);
            if (_buffer != null) Destroy(_buffer);
            if (_material != null) Destroy(_material);
        }

        void Update()
        {
            if (_sourceType == SourceType.Video)
                Blit(GetComponent<VideoPlayer>().texture);

            if (_sourceType == SourceType.Webcam && _webcam.didUpdateThisFrame)
                Blit(_webcam, _webcam.videoVerticallyMirrored);

            // Asynchronous image downloading
            if (_webTexture != null && _webTexture.isDone)
            {
                var texture = DownloadHandlerTexture.GetContent(_webTexture);
                _webTexture.Dispose();
                _webTexture = null;
                Blit(texture);
                Destroy(texture);
            }

            if (_sourceType == SourceType.Gradient)
                Graphics.Blit(null, OutputBuffer, _material, 1);
        }

        #endregion
    }

