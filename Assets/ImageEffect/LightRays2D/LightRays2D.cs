using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(MeshFilter)),RequireComponent(typeof(MeshRenderer))]
public class LightRays2D : MonoBehaviour{

    public enum LightRaysParamType
    {
        NONE,
        BeginR,
        BeginG,
        BeginB,
        BeginA,
        EndR,
        EndG,
        EndB,
        EndA,
        Contrast,
        Speed,
        Size,
        Skew,
        Shear,
        Fade,

        END
    }

    private Material mat;

    public Color color1=Color.white;
	private Color _color1;

    public Color color2=new Color(0,0.46f,1,0);
	private Color _color2;

    [Range(0f,5f)]
	public float speed=0.5f;
	private float _speed;

    [Range(1f,30f)]
	public float size=15f;
	private float _size;

    [Range(-1f,1f)]
	public float skew=0f;
	private float _skew;

    [Range(0f,5f)]
	public float shear=3f;
	private float _shear;

    [Range(0f,1f)]
	public float fade=1f;
	private float _fade;

    [Range(0f,50f)]
	public float contrast=2f;
	private float _contrast;


	void Update()
    {
        mat.SetColor("_Color1", color1);
        mat.SetColor("_Color2", color2);
        mat.SetFloat("_Speed", speed);
        mat.SetFloat("_Size", size);
        mat.SetFloat("_Skew", skew);
        mat.SetFloat("_Shear", shear);
        mat.SetFloat("_Fade", fade);
        mat.SetFloat("_Contrast", contrast);
	}

    void Awake()
    {
        mat = GetComponent<MeshRenderer>().materials[0];

        _color1 = color1;
        _color2 = color2;
        _speed = speed;
        _size = size;
        _skew = skew;
        _shear = shear;
        _fade = fade;
        _contrast = contrast;
    }

    public void ResetDefault()
    {
        color1 = _color1;
        color2 = _color2;
        speed = _speed;
        size = _size;
        skew = _skew;
        shear = _shear;
        fade = _fade;
        contrast = _contrast;
    }

}
