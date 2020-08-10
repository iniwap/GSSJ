using UnityEngine;

namespace ImageEffects
{
    [ExecuteInEditMode]
#if UNITY_5_4_OR_NEWER
    [ImageEffectAllowedInSceneView]
#endif
    [RequireComponent(typeof(Camera))]
    public class Mosaic : ImageEffectBase
    {
        [Range(10, 100)]
        public float Size = 30;
        private float _Size = 30;

        protected override void Awake()
        {
            _shaderName = "Custom/Mosaic";

            _Size = Size;
        }

        public void ResetDefault()
        {
            Size = _Size;
        }
        ////////////////////////////////////////
        // Post-processing effect application //
        ////////////////////////////////////////

        // Called by the camera to apply the image effect
        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            //_material.SetTexture("_Param", param);
            _material.SetFloat("_Size", Size);

            Graphics.Blit(source, destination, _material);
        }
    }
}
