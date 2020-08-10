using UnityEngine;

namespace ImageEffects
{
    [ExecuteInEditMode]
#if UNITY_5_4_OR_NEWER
    [ImageEffectAllowedInSceneView]
#endif
    [RequireComponent(typeof(Camera))]
    public class HalftoneEffect : ImageEffectBase
    {
        [Tooltip("Printing paper texture used to achieve a better effect.")]
        public Texture2D printingPaper;
        [Tooltip("Number of halftone dots that fill the screen. It's clamped to a fifth of the minimum screen size (in pixels).")]
        public float frequency;
        [Tooltip("Enables BW halftone rendering.")]
        public bool BW;
        private Camera _camera;

        private float _frequency;
        private bool _BW;
        //private Texture2D _printingPaper;

        //Creates a private _material used to the effect
        protected override void Awake()
        {
            _shaderName = "Custom/Halftone";
            _camera = GetComponent<Camera>();

            _frequency = frequency;
            _BW = BW;

            //_printingPaper = printingPaper;
        }

        ///<summary>
        /// Get the effect's camera.
        ///</summary>
        public Camera GetCamera()
        {
            return _camera;
        }

        public void ResetDefault()
        {
            frequency = _frequency;
            BW = _BW;
            //printingPaper = _printingPaper;
        }

        ////////////////////////////////////////
        // Post-processing effect application //
        ////////////////////////////////////////

        // Postprocess the image
        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            _material.SetTexture("_paper", (printingPaper) ? printingPaper : Texture2D.whiteTexture);
            _material.SetFloat("_frequency", frequency);
            _material.SetInt("_BW", BW ? 1 : 0);
            Graphics.Blit(source, destination, _material);
        }
    }
}
