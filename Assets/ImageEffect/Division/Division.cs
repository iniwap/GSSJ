using UnityEngine;

namespace ImageEffects
{
    [ExecuteInEditMode]
#if UNITY_5_4_OR_NEWER
    [ImageEffectAllowedInSceneView]
#endif
    [RequireComponent(typeof(Camera))]
    public class Division : ImageEffectBase
    {
        [Range(1, 32)]
        public float block = 2;
        private float _block = 2;

        protected override void Awake()
        {
            _shaderName = "Custom/Division";
            _block = block;
        }

        public void ResetDefault()
        {
            block = _block;
        }

        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            _material.SetFloat("_Block", block);
            base.OnRenderImage(source, destination);
        }
    }
}
