using UnityEngine;
using ImageEffects;

[ExecuteInEditMode]
public class Vortex : ImageEffectBase
{
    [Range(0, 3600)]
    public float angle = 360;
    private float _angle = 360;

    protected override void Awake()
    {
        _shaderName = "Custom/Vortex";
        _angle = angle;
    }

    public void ResetDefault()
    {
        angle = _angle;
    }

    //此函数在当完成所有渲染图片后被调用，用来渲染图片后期效果
    protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {

        Matrix4x4 rotationMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, angle), Vector3.one);

        //设置shader 的外部变量
        _material.SetMatrix("_RotationMatrix", rotationMatrix);
        _material.SetFloat("_Angle", angle * Mathf.Deg2Rad);
        _material.SetVector("_CenterRadius", new Vector4(0.5f, 0.5f, 0.5f,0.5f));

        //复制源纹理到目标纹理，加上材质效果
        Graphics.Blit(source, destination, _material);

    }

}
