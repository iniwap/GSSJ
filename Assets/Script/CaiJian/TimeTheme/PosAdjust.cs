/*
 *用于控制处理和装饰有关的逻辑
 *
 */
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.Events;
using System.Collections.Generic;
using System.IO;
using Reign;

public class PosAdjust : MonoBehaviour
{
    public void Start()
    {

    }

    public Text _CameraRXText;
    public Text _CameraRZText;
    public Text _LightRXText;
    public Text _LightRYText;
    public Slider _CameraRXSlider;
    public Slider _CameraRZSlider;
    public Slider _LightRXSlider;
    public Slider _LightRYSlider;

    public GameObject _Camera;
    public GameObject _Light;

    public void ResetPosAdjust()
    {
        Material theme = RenderSettings.skybox;
        float cameraRX = -22;
        float cameraRZ = 0;
        float lightRX = 12;
        float lightRY = 168;
        if (theme.HasProperty("_CameraRX"))
        {
            cameraRX = theme.GetFloat("_CameraRX");
            cameraRZ = theme.GetFloat("_CameraRZ");
            lightRX = theme.GetFloat("_LightRX");
            lightRY = theme.GetFloat("_LightRY");
        }

        _CameraRXText.text = "" + (int)(100 * (cameraRX + 90) / 180) + "%";
        _CameraRZText.text = "" + (int)(100 * (cameraRZ + 180) / 360) + "%";
        _LightRXText.text = "" + (int)(100 * (lightRX + 90) / 180) + "%";
        _LightRYText.text = "" + (int)(100 * (lightRY - 120) / 120) + "%";

        _CameraRXSlider.value = cameraRX;
        _CameraRZSlider.value = cameraRZ;
        _LightRXSlider.value = lightRX;
        _LightRYSlider.value = lightRY;

        SetCameraXZ(cameraRX, cameraRZ);
        SetLightXY(lightRX, lightRY);
    }

    public void InitAdjust()
    {
        ResetPosAdjust();
    }

    public void OnCameraRXValueChange(float v)
    {
        _CameraRXText.text = "" + (int)(100 * (v + 90) / 180) + "%";
        SetCameraXZ(v, _Camera.transform.rotation.eulerAngles.z);

        Material theme = RenderSettings.skybox;
        theme.SetFloat("_CameraRX", v);
    }
    public void OnCameraRZValueChange(float v)
    {
        _CameraRZText.text = (int)(100*(v+180)/360) + "%";
        SetCameraXZ(_Camera.transform.rotation.eulerAngles.x,v);

        Material theme = RenderSettings.skybox;
        theme.SetFloat("_CameraRZ", v);
    }
    public void OnLightRXValueChange(float v)
    {
        _LightRXText.text = (int)(100*(v + 90)/180)+"%";
        SetLightXY(v, _Light.transform.localRotation.eulerAngles.y);

        Material theme = RenderSettings.skybox;
        theme.SetFloat("_LightRX", v);
    }
    public void OnLightRYValueChange(float v)
    {
        _LightRYText.text = (int)(100*(v-120)/120)+"%";
        SetLightXY(_Light.transform.localRotation.eulerAngles.x,v);

        Material theme = RenderSettings.skybox;
        theme.SetFloat("_LightRY", v);
    }
    private void SetCameraXZ(float x, float z)
    {
        Quaternion r = Quaternion.Euler(x, 0, z);
        _Camera.transform.rotation = r;
    }
    private void SetLightXY(float x, float y)
    {
        Quaternion r = Quaternion.Euler(x, y, 0);
        _Light.transform.localRotation = r;
    }
    public void OnSaveSetting()
    {
        string data = "";

        data += _CameraRXSlider.value + "#";
        data += _CameraRZSlider.value + "#";
        data += _LightRXSlider.value + "#";
        data += _LightRYSlider.value;

        Material theme = RenderSettings.skybox;

        Setting.setPlayerPrefs(theme.name + Setting.SETTING_KEY.POS_ADJUST_SETTING, data);
    }
}
