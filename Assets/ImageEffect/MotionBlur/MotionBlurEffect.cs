using UnityEngine;

namespace ImageEffects
{
    [ExecuteInEditMode]
#if UNITY_5_4_OR_NEWER
    [ImageEffectAllowedInSceneView]
#endif

    //纯粹为了记录参数用，没有其他用途
    public class MotionBlurEffect : MonoBehaviour
    {
        [Range(1,32)]
        public float sampleCnt = 4;
        private float _sampleCnt = 4;

        void Awake()
        {
            _sampleCnt = sampleCnt;
        }

        public void ResetDefault()
        {
            sampleCnt = _sampleCnt;
        }

        public void OnEnable()
        {
            
        }
        public void OnDisable()
        {
            
        }
    }
}
