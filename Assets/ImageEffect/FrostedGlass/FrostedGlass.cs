using UnityEngine;

namespace ImageEffects
{
    [ExecuteInEditMode]
#if UNITY_5_4_OR_NEWER
    [ImageEffectAllowedInSceneView]
#endif
    [RequireComponent(typeof(Camera))]
    public class FrostedGlass : ImageEffectBase
    {
        [Range(1, 100)]
        public float radius = 10;
        private float _radius = 10;

        ////////////////////////////////////
        // Unity Editor related functions //
        ////////////////////////////////////

        protected override void Awake()
        {
            _shaderName = "Custom/FrostedGlass";
            _radius = radius;
        }

        public void ResetDefault()
        {
            radius = _radius;
        }

        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            _material.SetFloat("_Radius", radius);
            base.OnRenderImage(source, destination);
        }
    }
}
