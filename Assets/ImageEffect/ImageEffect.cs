using UnityEngine;

namespace ImageEffects
{
    [ExecuteInEditMode]
#if UNITY_5_4_OR_NEWER
    [ImageEffectAllowedInSceneView]
#endif
    [RequireComponent(typeof(Camera))]
    public class ImageEffect : ImageEffectBase
    {
        protected override void OnDestroy()
        {

        }
        protected override void OnEnable()
        {
 
        }

        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination);
        }
    }
}
