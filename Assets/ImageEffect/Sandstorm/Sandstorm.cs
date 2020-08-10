using UnityEngine;

namespace ImageEffects
{
    [ExecuteInEditMode]
#if UNITY_5_4_OR_NEWER
    [ImageEffectAllowedInSceneView]
#endif
    [RequireComponent(typeof(Camera))]
    public class Sandstorm : ImageEffectBase
    {
        [Range(0, 1)]
        public float intensity = 0.5f;
        private float _intensity = 0.5f;

        protected override void Awake()
        {
            _shaderName = "Custom/Sandstorm";
            _intensity = intensity;
        }

        public void ResetDefault()
        {
            intensity = _intensity;
        }

        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            _material.SetFloat("_Intensity", intensity);
            base.OnRenderImage(source, destination);
        }
    }
}
