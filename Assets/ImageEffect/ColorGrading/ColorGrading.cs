using UnityEngine;

namespace ImageEffects
{
    [ExecuteInEditMode]
#if UNITY_5_4_OR_NEWER
    [ImageEffectAllowedInSceneView]
#endif
    [RequireComponent(typeof(Camera))]
    public class ColorGrading : ColorGradingBase
    {
        public enum ColorGradingParamType
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

        // Initialization.
        protected override void Awake()
        {
            _shaderName = "Custom/ColorGrading";
            _internalLUT2D = GenerateIdentityLut(32);
            _internalLUT3D = LUT2DTo3D(_internalLUT2D);
        }

        protected override void  OnEnable()
        {
            base.OnEnable();

            SetChanges();
        }
    }
}
