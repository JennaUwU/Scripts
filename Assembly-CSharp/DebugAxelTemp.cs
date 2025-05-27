using UnityEngine;

public class DebugAxelTemp : MonoBehaviour
{
	private int loopClipLength = 4096;

	private int sampleRate = 11025;

	private float[] clipData;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			clipData = new float[loopClipLength];
			for (int i = 0; i < clipData.Length; i++)
			{
				clipData[i] = Random.Range(-1f, 1f);
			}
			AudioClip.Create("Speech Loop", loopClipLength, 1, sampleRate, stream: true, callback_audioRead);
		}
	}

	private void callback_audioRead(float[] output)
	{
		for (int i = 0; i < output.Length; i++)
		{
			output[i] = clipData[i];
		}
	}
}
