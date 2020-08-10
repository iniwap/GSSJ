using UnityEngine;

namespace ImageEffects
{
    [ExecuteInEditMode]
#if UNITY_5_4_OR_NEWER
    [ImageEffectAllowedInSceneView]
#endif
    [RequireComponent(typeof(Camera))]
    public class Tonemapper : ImageEffectBase
    {
        public enum TonemappingMethod { Photographic, Reinhard, HPDuiker, HejlDawson, Hable, ACES };

        public float MethodValue { get; set; }//用于计算method,需要和method默认值一致
        public TonemappingMethod method;
        [Range(0, 16)]
        public float exposure = 1;
       
        [SerializeField, Tooltip("Look-up texture used in Haarm-Peter Duiker’s tonemapping method")]
        private Texture filmLut;


        private TonemappingMethod _method;
        private float _MethodValue;
        private float _exposure = 1;


        protected override void Awake()
        {
            _shaderName = "Custom/Tonemapper";
            MethodValue = (int)method;

            _method = method;
            _MethodValue = MethodValue;
            _exposure = exposure;
        }

        public void ResetDefault()
        {
            method = _method;
            MethodValue = _MethodValue;
            exposure = _exposure;
        }
        // Called by the camera to apply the image effect
        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            _material.SetFloat("_Exposure", exposure);
            _material.SetTexture("_FilmLut", filmLut);
            Graphics.Blit(source, destination, _material, (int)method);
        }
    }
}
