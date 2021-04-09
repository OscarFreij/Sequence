using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OverlayControler : MonoBehaviour
{
	public bool run { get; set; }
	public bool isFirst1 { get; set; }
	public bool soundOn { get; set; }
	public GameManager gm { get; set; }
	public float timePassed { get; private set; }
	public int id { get; private set; }
	// Use this for initialization
	void Start ()
	{
		run = false;
		isFirst1 = false;
		soundOn = true;
		timePassed = 0;
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();

		string temp = this.transform.parent.name.Substring(this.transform.parent.name.IndexOf('(')+1);
		temp = temp.Substring(0, temp.Length-1);
		id = int.Parse(temp);

		this.transform.parent.GetComponent<Button>().onClick.AddListener(delegate { gm.CheckClick(id); });
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		if (run)
        {
			if (isFirst1)
            {
				timePassed = -1.5f;
				isFirst1 = false;
			}

			timePassed += Time.deltaTime;
			if (timePassed < gm.overlayOnTime && timePassed > 0)
            {
				if (soundOn)
                {
					gm.sm.PlaySound(id);
					soundOn = false;
				}
				transform.GetComponent<Image>().enabled = true;
			}
			else if (timePassed > gm.overlayOnTime && timePassed < gm.overlayOnTime+gm.overlayOnTimeDelay)
            {
				transform.GetComponent<Image>().enabled = false;
			}
			else if (timePassed > gm.overlayOnTime + gm.overlayOnTimeDelay)
            {
				run = false;
				soundOn = true;
				timePassed = 0;
				gm.showNextBox = true;
            }
		}
	}
}
