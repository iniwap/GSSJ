// 2016 - Damien Mayance (@Valryon)
// Source: https://github.com/valryon/water2d-unity/
using UnityEngine;
/// <summary>
/// Water surface script (update the shader properties).
/// </summary>
public class Water : MonoBehaviour
{
    public Vector2 speed = new Vector2(0.01f, 0f);
    private Vector2 _speed = new Vector2(0.01f, 0f);

    private Renderer rend;
    private Material mat;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material;
        _speed = speed;
    }

    public Material GetMaterial()
    {
        return mat;
    }

    public void ResetDefault()
    {
        speed = _speed;
        mat.SetFloat("_Magnitude", 0.05f);
    }

    void LateUpdate()
    {
        Vector2 scroll = Time.deltaTime * speed;

        mat.mainTextureOffset += scroll;
    }
}