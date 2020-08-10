using UnityEngine;

namespace ImageEffects
{
    [ExecuteInEditMode]
#if UNITY_5_4_OR_NEWER
    [ImageEffectAllowedInSceneView]
#endif
    [RequireComponent(typeof(Camera))]
    public class ImageEffectBase : MonoBehaviour
    {
        protected  string _shaderName = "Custom/";
        public  bool _isDynamic = false;
        protected Material _material;

        public bool GetIsDynamic()
        {
            return _isDynamic;
        }

        public void SetIsDynamic(bool dynamic)
        {
            _isDynamic = dynamic;
        }

        // Initialization.
        protected virtual void Awake()
        {

        }

        protected virtual void OnDestroy()
        {
            if (_material) DestroyImmediate(_material);
            _material = null;
        }
        protected virtual void OnEnable()
        {
            if (!_material) _material = new Material(Shader.Find(_shaderName));
            _material.hideFlags = HideFlags.HideAndDontSave;

        }

        protected virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination, _material);
        }
    }
}
