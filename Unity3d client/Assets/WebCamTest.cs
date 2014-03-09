using UnityEngine;
using System.Collections;
using System.IO;

public class WebCamTest : MonoBehaviour {
	public string deviceName;
	public string path;
	public Texture2D texture;
	public float pollTime = 2;
	public string uploadURL = "http://path.tacticalspace.org/paths/upload.processor.php";
	public AudioClip[] dtmf;
	public string pathname = "1";
	int _CaptureCounter = 0;
	WebCamTexture wct;
	string _SavePath;
	float _timer;
	string message = "";

	
	// Use this for initialization
	void Start () {
		WebCamDevice[] devices = WebCamTexture.devices;
		deviceName = devices[0].name;
		wct = new WebCamTexture(deviceName);
		renderer.material.mainTexture = wct;
		wct.Play();
		_timer = pollTime;
		_SavePath = Application.persistentDataPath+"//"; //Change the path here!
		path = Application.persistentDataPath;
	}
	
	// For photo varibles
	
	public Texture2D heightmap;
	public Vector3 size = new Vector3(100, 10, 100);
	
	
	void OnGUI() {      
		//if (GUI.Button(new Rect(10, 70, 50, 30), "Click"))
		//	TakeSnapshot();
		string displaymessage = "Counter: " + _timer + "\r\n" + "Image Count: " + _CaptureCounter;
		GUI.Label (new Rect (10, 70, 200, 80), displaymessage+"\r\n"+message);
		
	}

	void Update() {
		_timer -= Time.deltaTime;
		if (_timer < 0) {
			_timer = pollTime;
			message = "";
			TakeSnapshot ();
			move (2);
		}

	}

	// For saving to the _savepath

	void StartUpload()
		
	{
		StartCoroutine("UploadImage()");
	}

	void move(int i) {
		audio.PlayOneShot (dtmf [i]);
	}

	string GetNextFilename() {
		string filename = _SavePath + _CaptureCounter.ToString ("0000") + ".png";
		while (File.Exists(filename)) {
			++_CaptureCounter;
			filename = _SavePath + _CaptureCounter.ToString ("0000") + ".png";
		}
		_CaptureCounter++;
		return filename;
	}

	void TakeSnapshot()
	{
		Texture2D snap = new Texture2D(wct.width, wct.height);
		snap.SetPixels(wct.GetPixels());
		snap.Apply();
		texture = snap;
		string filename = GetNextFilename ();
		System.IO.File.WriteAllBytes(filename, snap.EncodeToPNG());
		UploadPNG(_CaptureCounter.ToString("0000")+".png", snap.EncodeToPNG());
	}

	void UploadPNG (string filename, byte[] bytes) {
		// Create a Web Form

		WWWForm form = new WWWForm ();
		
		form.AddField ( "submit", "submit" );
		form.AddField( "pathname", pathname );
		form.AddBinaryData( "file", bytes, filename, "image/png" );

		// Upload to a cgi script    
		
		WWW w = new WWW (uploadURL, form );
		
		StartCoroutine(WaitForRequest(w));
		
	}

	IEnumerator WaitForRequest(WWW www)
	{
		yield return www;
		
		// check for errors
		if (www.error == null)
		{
			Debug.Log("WWW Ok!: " + www.data);
			message = "WWW Ok!: " + www.data;
		} else {
			Debug.Log("WWW Error: "+ www.error);
			message = "WWW Error: "+ www.error;
		}    
	}

}

