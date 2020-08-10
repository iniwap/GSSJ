using UnityEngine;

namespace ImageEffects
{
    [ExecuteInEditMode]
#if UNITY_5_4_OR_NEWER
    [ImageEffectAllowedInSceneView]
#endif
    [RequireComponent(typeof(Camera))]
    public class Film : ColorGradingBase
    {
        public enum FilmParamType
        {
            None,

            Temperature,
            Tilt,
            Hue,
            Saturation,
            Vibrance,
            Value,
            Contrast,
            Gain,
            Gamma,

            END
        }

        protected override void Awake()
        {
            _shaderName = "Custom/Film";
            _internalLUT2D = GenerateIdentityLut(32);
            _internalLUT3D = LUT2DTo3D(_internalLUT2D);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            SetChanges();
        }
    }
}
