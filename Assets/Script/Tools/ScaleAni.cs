using UnityEngine;
using Lean.Touch;

public class ScaleAni : MonoBehaviour
{
	public Vector3 BaseScale = Vector3.one;

	public float Size = 1.0f;

	public float PulseInterval = 1.0f;

	public float PulseSize = 1.0f;

	public float Dampening = 5.0f;

	[System.NonSerialized]
	private float counter;

	protected virtual void Update()
	{
		counter += Time.deltaTime;

		if (counter >= PulseInterval)
		{
			counter %= PulseInterval;

			Size += PulseSize;
		}

		var factor = LeanTouch.GetDampenFactor(Dampening, Time.deltaTime);

		Size = Mathf.Lerp(Size, 1.0f, factor);

		transform.localScale = Vector3.Lerp(transform.localScale, BaseScale * Size, factor);
	}
}