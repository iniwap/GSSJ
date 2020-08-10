/*
	Created by Alexander Dadukin & Vladimir Polyakov
	26/08/2016
*/

using UnityEngine;

namespace ImageEffects
{
    [ExecuteInEditMode]
    public class Watercolor : ImageEffectBase
    {
        #region Constants
       // private const string ScreenResolutiuonVectorFlag = "_ScreenResolution";
        #endregion

        #region Settings
        [Range(0, 10)]
        public float Iterations;//这里强转，省的外面麻烦
        [Range(0, 4)]
        public int DownRes;
        #endregion

        private float _Iterations;//这里强转，省的外面麻烦
        private int _DownRes;

        protected override void Awake()
        {
            _shaderName = "Custom/Watercolor";
            _Iterations = Iterations;
            _DownRes = DownRes;
        }

        public void ResetDefault()
        {
            Iterations = _Iterations;
            DownRes = _DownRes;
        }

        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            int width = source.width >> DownRes;
            int height = source.height >> DownRes;

            RenderTexture rt = RenderTexture.GetTemporary(width, height);
            Graphics.Blit(source, rt);

            for (int i = 0; i < (int)Iterations; i++)
            {
                RenderTexture rt2 = RenderTexture.GetTemporary(width, height);
                Graphics.Blit(rt, rt2, _material);
                RenderTexture.ReleaseTemporary(rt);
                rt = rt2;
            }

            Graphics.Blit(rt, destination);
            RenderTexture.ReleaseTemporary(rt);

        }
    }
}