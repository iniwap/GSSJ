using UnityEngine;

namespace ImageEffects
{
    [ExecuteInEditMode]
#if UNITY_5_4_OR_NEWER
    [ImageEffectAllowedInSceneView]
#endif
    [RequireComponent(typeof(Camera))]
    public class GameBoy : ImageEffectBase
    {
        [Range(0, 10)]
        public float gamma = 1.5f;
        private float _gamma = 1.5f;

        protected override void Awake()
        {
            _shaderName = "Custom/GameBoy";
            _gamma = gamma;
        }

        public void ResetDefault()
        {
            gamma = _gamma;
        }

        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            _material.SetFloat("_Gamma", gamma);

            base.OnRenderImage(source, destination);
        }
    }
}
