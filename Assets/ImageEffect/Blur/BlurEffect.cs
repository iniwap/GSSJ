using UnityEngine;

namespace ImageEffects
{
    [ExecuteInEditMode]
#if UNITY_5_4_OR_NEWER
    [ImageEffectAllowedInSceneView]
#endif
    [RequireComponent(typeof(Camera))]
    public class BlurEffect : ImageEffectBase
    {
        public enum BloomParamType
        {
            None,

            Threshold,
            Intensity,
            Downsampling,
            Iterations,
            BlurSpread,

            END
        }

        [Header("Blur Settings")]
        [Range(0, 4), Tooltip("How much will the camera's render texture will be downsampled (1/2^downsampling).")]
        public int downsampling = 2;
        public float downsamplingValue { get; set; }
        [Range(0, 20), Tooltip("How many blur iterations will be applied.")]
        public int iterations = 3;

        public float iterationsValue { get; set; }
        [Range(0.0f, 1.0f), Tooltip("How much will be the blur samples spread.")]
        public float blurSpread = 0.6f;

        //默认值
        private int _downsampling = 2;
        private int _iterations = 3;
        private float _blurSpread = 0.6f;

        private Camera _camera;
        ////////////////////////////////////
        // Unity Editor related functions //
        ////////////////////////////////////

        // Creates a private material used to the effect and get the camera component.
        protected override void Awake()
        {
            // _material = new Material(Shader.Find(_shaderName));
            _camera = GetComponent<Camera>();
            _shaderName = "Custom/Blur";
            downsamplingValue = downsampling;
            iterationsValue = iterations;

            //默认值
            _downsampling = downsampling;
            _iterations = iterations;
            _blurSpread = blurSpread;
        }
        public void ResetDefault()
        {
            downsampling = _downsampling;
            iterations = _iterations;
            blurSpread = _blurSpread;

            downsamplingValue = downsampling;
            iterationsValue = iterations;
        }
        ////////////////////////////////////////
        // Post-processing effect application //
        ////////////////////////////////////////

        // Called by the camera to apply the image effect
        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            //Determine the size of the render texture we'll use.
            int rtW = source.width / (int) Mathf.Pow(2, downsampling);
            int rtH = source.height / (int) Mathf.Pow(2, downsampling);

            //Determine the format of the render texture we'll use.
            RenderTextureFormat rtFormat;
            if (_camera.allowHDR)
            {
                if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat)) rtFormat = RenderTextureFormat.ARGBFloat;
                else rtFormat = RenderTextureFormat.DefaultHDR;
            }
            else rtFormat = RenderTextureFormat.Default;

            //Downsample the camera's RenderTexture and store it in a smaller new one.
            RenderTexture finalBlurBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, rtFormat, RenderTextureReadWrite.sRGB);
            Graphics.BlitMultiTap(source, finalBlurBuffer, _material,
                                   new Vector2(-1, -1),
                                   new Vector2(-1, 1),
                                   new Vector2(1, 1),
                                   new Vector2(1, -1));

            //Blur the downsampled texture.
            for (int i = 0; i < iterations; i++)
            {
                RenderTexture temporaryBlurBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, rtFormat, RenderTextureReadWrite.sRGB);

                //Calculate the spread of the blur samples.
                float offset = i * blurSpread;
                Graphics.BlitMultiTap(finalBlurBuffer, temporaryBlurBuffer, _material,
                                   new Vector2(-offset, -offset),
                                   new Vector2(-offset, offset),
                                   new Vector2(offset, offset),
                                   new Vector2(offset, -offset));

                RenderTexture.ReleaseTemporary(finalBlurBuffer);

                finalBlurBuffer = temporaryBlurBuffer;
                if (i == iterations - 1) RenderTexture.ReleaseTemporary(temporaryBlurBuffer);
            }

            //Blit copy to the screen.
            Graphics.Blit(finalBlurBuffer, destination);
            return;
        }
    }
}
