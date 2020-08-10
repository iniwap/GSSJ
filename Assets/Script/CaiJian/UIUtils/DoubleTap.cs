using UnityEngine;
using UnityEngine.Events;
using Lean.Touch;

// This script calls the OnFingerTap event when a finger taps the screen
public class DoubleTap : LeanFingerTap
{
	public override void FingerTap(LeanFinger finger)
	{
		// Ignore?
		if (IgnoreStartedOverGui == true && finger.StartedOverGui == true)
		{
			return;
		}

		if (IgnoreIsOverGui == true && finger.IsOverGui == true)
		{
			return;
		}

		if (RequiredTapCount > 0 && finger.TapCount != RequiredTapCount)
		{
			return;
		}

		if (RequiredTapInterval > 0 && (finger.TapCount % RequiredTapInterval) != 0)
		{
			return;
		}

		if (RequiredSelectable != null && RequiredSelectable.IsSelected == false)
		{
			return;
		}

        //检查finger是否在要求的区域，不在则不认为是双击
        float sh = Screen.height / FitUI.DESIGN_HEIGHT;

        RectTransform rt = transform.GetComponent<RectTransform>();
        Rect screenRect = new Rect(Screen.width / 2 - rt.rect.width * sh / 2, Screen.height / 2 - rt.rect.height * sh / 2,
            rt.rect.width, rt.rect.height);

        if (!screenRect.Contains(finger.StartScreenPosition))
        {
            return;
        }

        // Call event
        if (OnTap != null)
		{
			OnTap.Invoke(finger);
		}
	}
	
}