using UnityEngine;
using Lean.Touch;
using System;
using UnityEngine.Events;

public class PicAdjust : LeanTranslate
{
    public enum PicAdjustType
    {
        ADJUST_PIC_ALPHA,
        ADJUST_PIC_POS,
        ADJUST_FILTER,
        ADJUST_FX,
    };

    [Serializable] public class OnPicAdjustEvent : UnityEvent<Vector2, Vector2, PicAdjustType> { }
    public OnPicAdjustEvent OnPicAdjust;

    public  PicAdjustType _picAdjustType = PicAdjustType.ADJUST_PIC_POS;

    protected override void Update()
    {
        // Get the fingers we want to use
        var fingers = LeanSelectable.GetFingers(IgnoreStartedOverGui, IgnoreIsOverGui, RequiredFingerCount, RequiredSelectable);

        // Calculate the screenDelta value based on these fingers
        var screenDelta = LeanGesture.GetScreenDelta(fingers);

        if (screenDelta != Vector2.zero)
        {
            float sh = Screen.height / FitUI.DESIGN_HEIGHT;

            RectTransform rt = transform.GetComponent<RectTransform>();
            Rect screenRect = new Rect(Screen.width/2 - rt.rect.width * sh / 2, Screen.height / 2 - rt.rect.height * sh / 2,
                rt.rect.width, rt.rect.height);

            if (fingers.Count > 0)
            {
                if (!screenRect.Contains(fingers[0].StartScreenPosition))
                {
                    return;
                }
            }
            else
            {
                return;
            }

            TranslateUI(screenDelta);
        }
    }

    protected override void TranslateUI(Vector2 screenDelta)
	{
		var screenPoint = RectTransformUtility.WorldToScreenPoint(Camera, transform.position);

		screenPoint += screenDelta;

		var worldPoint = default(Vector3);

		if (RectTransformUtility.ScreenPointToWorldPointInRectangle(transform.parent as RectTransform, screenPoint, Camera, out worldPoint) == true)
		{
            Vector2 prevPos = transform.localPosition;
            Vector2 tmpPos = transform.position;

            transform.position = worldPoint;
            //
            OnPicAdjust.Invoke(prevPos, transform.localPosition, _picAdjustType);
        }
	}

    public void SetPicAdjustType(PicAdjustType type)
    {
        _picAdjustType = type;
    }

    public PicAdjustType GetPicAdjustType()
    {
        return _picAdjustType;
    }
}