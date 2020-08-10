﻿using UnityEngine;

namespace ImageEffects
{
    [ExecuteInEditMode]
#if UNITY_5_4_OR_NEWER
    [ImageEffectAllowedInSceneView]
#endif
    [RequireComponent(typeof(Camera))]
    public class BloomEffect : ImageEffectBase
    {
        [Header("Bloom Settings")]
        [Tooltip("Threshold value that specifies where the bloom will be applied (LDR: t<=1, HDR: t<=4)")]
        public float threshold = 0.5f;
        [Tooltip("Bloom's intensity multiplier.")]
        public float intensity = 1;

        [Header("Blur Settings")]
        [Range(0, 3), Tooltip("How much will the camera's render texture will be downsampled (1/2^downsampling).")]
        public int downsampling = 2;
        [Range(0, 10), Tooltip("How many blur iterations will be applied.")]
        public float iterations = 3;
        [Range(0.0f, 1.0f), Tooltip("How much will be the blur samples spread.")]
        public float blurSpread = 0.6f;

        public bool debug;

        private Material _materialBlur;
        private Material _materialBloom;
        private Camera _camera;

        private string _shaderNameBlur = "Custom/Blur";
        private string _shaderNameBloom = "Custom/Bloom";
        ////////////////////////////////////
        // Unity Editor related functions //
        ////////////////////////////////////

        // Creates the private materials used to the effect and get the camera component.
        protected override void Awake()
        {
            //_materialBlur = new Material(Shader.Find(_shaderNameBlur));
            //_materialBloom = new Material(Shader.Find(_shaderNameBloom));
            _camera = GetComponent<Camera>();

        }

        protected override void OnDestroy()
        {
            if (_materialBlur) DestroyImmediate(_materialBlur);
            _materialBlur = null;

            if (_materialBloom) DestroyImmediate(_materialBloom);
            _materialBloom = null;
        }
        protected override void OnEnable()
        {
            if (!_materialBlur) _materialBlur = new Material(Shader.Find(_shaderNameBlur));
            if (!_materialBloom) _materialBloom = new Material(Shader.Find(_shaderNameBloom));
            _materialBlur.hideFlags = HideFlags.HideAndDontSave;
            _materialBloom.hideFlags = HideFlags.HideAndDontSave;
            //if (!_camera) _camera = GetComponent<Camera>();
        }

        // Methods used to take care of the materials when enabling/disabling the effects in the inspector.

        ///<summary>
        /// Get the effect's camera HDR flag.
        ///</summary>
        public bool HDR()
        {
            return _camera.allowHDR;
        }
        public void ResetDefault()
        {
            threshold = 0.5f;
            intensity = 1;
            downsampling = 2;
            iterations = 3;
            blurSpread = 0.6f;
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
            RenderTexture finalBloomBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, rtFormat, RenderTextureReadWrite.Linear);
            Graphics.BlitMultiTap(source, finalBloomBuffer, _materialBlur,
                                   new Vector2(-1, -1),
                                   new Vector2(-1, 1),
                                   new Vector2(1, 1),
                                   new Vector2(1, -1));

            //Substract the threshold value to the sample texture's pixels.
            _materialBloom.SetFloat("_BloomThreshold", threshold);
            Graphics.Blit(source, finalBloomBuffer, _materialBloom,0);

            if(debug)
            {
                Graphics.Blit(finalBloomBuffer, destination);
                RenderTexture.ReleaseTemporary(finalBloomBuffer);
                return;
            }

            //Blur the downsampled texture.
            for (int i = 0; i < (int)iterations; i++)
            {
                RenderTexture temporaryBlurBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, rtFormat, RenderTextureReadWrite.Linear);

                //Calculate the spread of the blur samples.
                float offset = i * blurSpread;
                Graphics.BlitMultiTap(finalBloomBuffer, temporaryBlurBuffer, _materialBlur,
                                   new Vector2(-offset, -offset),
                                   new Vector2(-offset, offset),
                                   new Vector2(offset, offset),
                                   new Vector2(offset, -offset));

                RenderTexture.ReleaseTemporary(finalBloomBuffer);

                finalBloomBuffer = temporaryBlurBuffer;
                if (i == (int)iterations - 1) RenderTexture.ReleaseTemporary(temporaryBlurBuffer);
            }

            //Add the final blurred bloom texture to the source render texture.
            _materialBloom.SetFloat("_BloomIntensity", intensity);
            _materialBloom.SetTexture("_OriginalTex", source);
            Graphics.Blit(finalBloomBuffer, destination, _materialBloom, 1);
            return;
        }
    }
}
