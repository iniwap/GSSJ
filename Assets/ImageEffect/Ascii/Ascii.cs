using UnityEngine;

namespace ImageEffects
{
    [ExecuteInEditMode]
#if UNITY_5_4_OR_NEWER
    [ImageEffectAllowedInSceneView]
#endif
    [RequireComponent(typeof(Camera))]
    public class Ascii : ImageEffectBase
    {
        [Range(1, 100)]
        public float size = 50;
        private float _size = 50;//默认值

        public void ResetDefault()
        {
            size = _size;
        }

        protected override void Awake()
        {
            _shaderName = "Custom/Ascii";
            _size = size;
        }
        
        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            _material.SetFloat("_Size", size);
            base.OnRenderImage(source, destination);
        }
    }
}
