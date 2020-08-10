using UnityEngine;

namespace ImageEffects
{
    [ExecuteInEditMode]
#if UNITY_5_4_OR_NEWER
    [ImageEffectAllowedInSceneView]
#endif
    [RequireComponent(typeof(Camera))]
    public class Drunk : ImageEffectBase
    {
        [Range(0, 1)]
        public float magnitude = 0.05f;
        [Range(0.1f, 10f)]
        public float speed = 1f;

        private float _magnitude = 0.05f;
        private float _speed = 1f;
        ////////////////////////////////////
        // Unity Editor related functions //
        ////////////////////////////////////

        protected override void Awake()
        {
            _shaderName = "Custom/Drunk";
            _magnitude = magnitude;
            _speed = speed;
        }

        public void ResetDefault()
        {
            magnitude = _magnitude;
            speed = _speed;
        }

        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            _material.SetFloat("_Magnitude", magnitude);
            _material.SetFloat("_Speed", speed);
            base.OnRenderImage(source, destination);
        }
    }
}
