#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
	#define UNITY_OLD_LINE_RENDERER
#endif
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Lean.Touch
{
	// This script will draw the path each finger has taken since it started being pressed
	public class FXLeanFingerTrail : LeanFingerTrail
	{
		// This stores all the links
		private List<Link> links = new List<Link>();

#if UNITY_EDITOR
		protected override void Reset()
		{
			Start();
		}
#endif

		protected override void Start()
		{
			if (RequiredSelectable == null)
			{
				RequiredSelectable = GetComponent<LeanSelectable>();
			}
		}

		protected override void OnEnable()
		{
			// Hook events
			LeanTouch.OnFingerSet += FingerSet;
			LeanTouch.OnFingerUp  += FingerUp;
		}

		protected override void OnDisable()
		{
			// Unhook events
			LeanTouch.OnFingerSet -= FingerSet;
			LeanTouch.OnFingerUp  -= FingerUp;
		}

		// Override the WritePositions method from LeanDragLine
		protected override void WritePositions(LineRenderer line, LeanFinger finger)
		{
			// Reserve one vertex for each snapshot
#if UNITY_OLD_LINE_RENDERER
			line.SetVertexCount(finger.Snapshots.Count);
#else
			line.positionCount = finger.Snapshots.Count;
#endif
			// Loop through all snapshots
			for (var i = 0; i < finger.Snapshots.Count; i++)
			{
				var snapshot = finger.Snapshots[i];

				// Get the world postion of this snapshot
				var worldPoint = ScreenDepth.Convert(snapshot.ScreenPosition, Camera, gameObject);

				// Write position
				line.SetPosition(i, worldPoint);
			}
		}

		private void FingerSet(LeanFinger finger)
		{
			// ignore?
			if (MaxLines > 0 && links.Count >= MaxLines)
			{
				return;
			}

			if (IgnoreStartedOverGui == true && finger.StartedOverGui == true)
			{
				return;
			}

			if (IgnoreIsOverGui == true && finger.IsOverGui == true)
			{
				return;
			}

			if (RequiredSelectable != null && RequiredSelectable.IsSelectedBy(finger) == false)
			{
				return;
			}

			// Get link for this finger and write positions
			var link = FindLink(finger, true);

			if (link != null && link.Line != null)
			{
				WritePositions(link.Line, link.Finger);
			}
		}

		private void FingerUp(LeanFinger finger)
		{
            /*
			// Find link for this finger, and clear it
			var link = FindLink(finger, false);

			if (link != null)
			{
				links.Remove(link);

				LinkFingerUp(link);

				if (link.Line != null)
				{
					Destroy(link.Line.gameObject);
				}
			}
            */
		}

		protected override void LinkFingerUp(Link link)
		{
		}

		// Searches through all links for the one associated with the input finger
		private Link FindLink(LeanFinger finger, bool createIfNull)
		{
			// Find existing link?
			for (var i = 0; i < links.Count; i++)
			{
				var link = links[i];

				if (link.Finger == finger)
				{
					return link;
				}
			}

			// Make new link?
			if (createIfNull == true)
			{
				var link = new Link();

				link.Finger = finger;
				link.Line   = Instantiate(LinePrefab,gameObject.transform);

				links.Add(link);

				return link;
			}

			return null;
		}
	}
}