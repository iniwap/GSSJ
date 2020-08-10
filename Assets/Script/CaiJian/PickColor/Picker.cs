using UnityEngine;
using Lean.Touch;
using System;
using UnityEngine.Events;

public class Picker : LeanTranslate
{

    public UnityEvent _OnPickerMove;
    private Rect _PicRect;
    protected override void Update()
    {
        // Get the fingers we want to use
        var fingers = LeanSelectable.GetFingers(IgnoreStartedOverGui, IgnoreIsOverGui, RequiredFingerCount, RequiredSelectable);

        // Calculate the screenDelta value based on these fingers
        var screenDelta = LeanGesture.GetScreenDelta(fingers);

        if (screenDelta != Vector2.zero)
        {
            if (fingers.Count > 0)
            {
                if (!_PicRect.Contains(fingers[0].StartScreenPosition)
                    || !_PicRect.Contains(fingers[0].LastScreenPosition))
                {
                    return;
                }
            }
            else
            {
                return;
            }

            transform.position = fingers[0].LastScreenPosition;
            _OnPickerMove.Invoke();
        }
    }

    public void SetRect(Rect rt)
    {
        _PicRect = rt;
    }

    public Rect GetPicRect()
    {
        return _PicRect;
    }
}