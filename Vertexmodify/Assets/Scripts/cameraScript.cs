﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class cameraScript : MonoBehaviour {

	private string savePath = "/Users/ejer/Desktop/Medialogy/3. Semester/P3/P3-3DMapGen/Vertexmodify/CamSnaps/"; 

	int _CaptureCounter = 0; 
	public GameObject targetPlane; 

    public RawImage rawimage;
	protected Color [] pixFromSnap; 
	Texture2D tex1; 
	WebCamTexture webcamTexture; 



	void Start () {
		StartCoroutine (fixBug ());    
    }
	

	void Update () {

		if (Input.GetKeyDown ("s")) {
			Snapshot();
		}
	}

	public void Snapshot()
{

		tex1.SetPixels(webcamTexture.GetPixels()); 
		tex1.Apply ();
		targetPlane.GetComponent<Renderer> ().material.mainTexture = tex1; 
		rawimage.enabled = false; 

		System.IO.File.WriteAllBytes(savePath + _CaptureCounter.ToString() + ".png", tex1.EncodeToPNG());
		_CaptureCounter ++; 
		pixFromSnap = tex1.GetPixels (); 

	}
	public void retake(){

		rawimage.enabled = true; 
		webcamTexture.Play (); 

	}

	IEnumerator fixBug(){

		webcamTexture = new WebCamTexture();
		webcamTexture.Play();

		if (webcamTexture.width <= 16 || webcamTexture.height <= 16) {		

			while (!webcamTexture.didUpdateThisFrame) {
				yield return new WaitForEndOfFrame (); 

			}

			webcamTexture.Pause (); 
			Color32[] colors = webcamTexture.GetPixels32 (); 
			webcamTexture.Stop (); 

			yield return new WaitForEndOfFrame ();

			webcamTexture.Play (); 

		}
			
		rawimage.texture = webcamTexture;
		rawimage.material.mainTexture = webcamTexture;

		tex1 = new Texture2D (webcamTexture.width,webcamTexture.height);

		Debug.Log ("The height of the webcam is: " + webcamTexture.height + " The width of the webcam is: " + webcamTexture.width); 
	}
}