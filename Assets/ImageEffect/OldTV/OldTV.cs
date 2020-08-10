// KinoTube - Old TV/video artifacts simulation
// https://github.com/keijiro/KinoTube

using UnityEngine;

namespace ImageEffects
{
    [ExecuteInEditMode]
    public class OldTV : ImageEffectBase
    {
        #region Editable attributes

        [SerializeField, Range(0, 1)] public float _bleeding = 0.5f;
        [SerializeField, Range(0, 1)] public  float _fringing = 0.5f;
        [SerializeField, Range(0, 1)] public  float _scanline = 0.5f;

        private float _bleedingDefault = 0.5f;
        private float _fringingDefault = 0.5f;
        private float _scanlineDefault = 0.5f;

        #endregion

        #region MonoBehaviour methods

        protected override void Awake()
        {
            _shaderName = "Custom/OldTV";
            _bleedingDefault = _bleeding;
            _fringingDefault = _fringing;
            _scanlineDefault = _scanline;
        }


        public void ResetDefault()
        {
            _bleeding = _bleedingDefault;
            _fringing = _fringingDefault;
            _scanline = _scanlineDefault;
        }


        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            var bleedWidth = 0.04f * _bleeding;  // width of bleeding
            var bleedStep = 2.5f / source.width; // max interval of taps
            var bleedTaps = Mathf.CeilToInt(bleedWidth / bleedStep);
            var bleedDelta = bleedWidth / bleedTaps;
            var fringeWidth = 0.0025f * _fringing; // width of fringing

            _material.SetInt("_BleedTaps", bleedTaps);
            _material.SetFloat("_BleedDelta", bleedDelta);
            _material.SetFloat("_FringeDelta", fringeWidth);
            _material.SetFloat("_Scanline", _scanline);

            Graphics.Blit(source, destination, _material);
        }

        #endregion
    }
}
