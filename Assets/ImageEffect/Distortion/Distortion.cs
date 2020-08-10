using UnityEngine;

namespace ImageEffects
{
    [ExecuteInEditMode]
#if UNITY_5_4_OR_NEWER
    [ImageEffectAllowedInSceneView]
#endif
    [RequireComponent(typeof(Camera))]
    public class Distortion : ImageEffectBase
    {
        [Range(0, 1)]
        public float distortion = 0.5f;

        [Range(0, 1)]
        public float posx = 0.5f;

        [Range(0, 1)]
        public float posy = 0.5f;


        //默认值
        private float _distortion = 0.5f;
        private float _posx = 0.5f;
        private float _posy = 0.5f;
        ////////////////////////////////////
        // Unity Editor related functions //
        ////////////////////////////////////

        protected override void Awake()
        {
            _shaderName = "Custom/Distortion";
            _distortion = distortion;
            _posx = posx;
            _posy = posy;
        }

        public void ResetDefault()
        {
            distortion = _distortion;
            posx = _posx;
            posy = _posy;
        }

        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            _material.SetFloat("_PosX",posx);
            _material.SetFloat("_PosY", posy);
            _material.SetFloat("_Distortion", distortion);
            base.OnRenderImage(source, destination);
        }
    }
}
