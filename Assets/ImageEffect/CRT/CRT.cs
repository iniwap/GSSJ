using UnityEngine;

namespace ImageEffects
{
    [ExecuteInEditMode]
#if UNITY_5_4_OR_NEWER
    [ImageEffectAllowedInSceneView]
#endif
    [RequireComponent(typeof(Camera))]
    public class CRT : ImageEffectBase
    {
        [Range(0.1f, 10)]
        public float factor = 1;
        private float _factor = 1;
        ////////////////////////////////////
        // Unity Editor related functions //
        ////////////////////////////////////

        protected override void Awake()
        {
            _shaderName = "Custom/CRT";
            _factor = factor;
        }

        public void ResetDefault()
        {
            factor = _factor;
        }

        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            _material.SetFloat("_Factor", factor);
            base.OnRenderImage(source, destination);
        }
    }
}
