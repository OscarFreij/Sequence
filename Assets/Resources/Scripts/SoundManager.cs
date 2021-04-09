using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{

	public List<float> pitch { get; private set; }
	public AudioSource audioSource { get; private set; }
	public AudioClip miss { get; private set; }
	public AudioClip button { get; private set; }

	// Use this for initialization
	void Start ()
	{
		pitch = new List<float>();

		for (float  i = 2; i > 0.1; i -= 0.2f)
        {
			pitch.Add(i);
        }

		this.audioSource = transform.GetComponent<AudioSource>();
		this.miss = Resources.Load<AudioClip>("Sound/Miss");
		this.button = Resources.Load<AudioClip>("Sound/Button");
	}
	
	public void PlaySound(int id)
    {
		if (id >= 0 && id < 9)
        {
			audioSource.Stop();
			audioSource.clip = button;
			audioSource.pitch = pitch[id];
			audioSource.Play();
        }
		else if (id == 9)
        {
			audioSource.Stop();
			audioSource.clip = miss;
			audioSource.pitch = 1;
			audioSource.Play();
		}
    }
}
