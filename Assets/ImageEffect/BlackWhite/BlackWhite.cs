using UnityEngine;

namespace ImageEffects
{
    [ExecuteInEditMode]
#if UNITY_5_4_OR_NEWER
    [ImageEffectAllowedInSceneView]
#endif
    [RequireComponent(typeof(Camera))]
    public class BlackWhite : ColorGradingBase
    {
        public enum BlackWhiteParamType
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
            _shaderName = "Custom/BlackWhite";
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
