﻿using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Generic modules.
/// </summary>
public class imageProcModules : MonoBehaviour
{
	
	public float[,] blockCopy (float[,] inputArray)
	{
		float[,] output = new float[inputArray.GetLength (0), inputArray.GetLength (1)];
		Buffer.BlockCopy (inputArray, 0, output, 0, inputArray.Length * sizeof(float));
		return output;
	}


	public Color[] RBGNormalize (Color[] inputImg)
	{
		Color[] output = new Color[inputImg.Length];
		for (int i = 0; i < inputImg.Length; i++) {
			float RGBTotal = inputImg [i].r + inputImg [i].g + inputImg [i].b;
			output [i].r = inputImg [i].r / RGBTotal;
			output [i].g = inputImg [i].g / RGBTotal;
			output [i].b = inputImg [i].b / RGBTotal;
		}
		return  output;
	}

	/// <summary>
	/// Perlin the specified inp.
	/// </summary>
	/// <param name="inp">Inp.</param>
	public float [,] perlin (float[,] inp, float baseHeight, float intensity, float density)
	{
		TimingModule.timer ("perlinModule", "start");
		float px, py;
		for (int y = 0; y < inp.GetLength (1); y++) {
			for (int x = 0; x < inp.GetLength (0); x++) {
				px = (float)x / density;
				py = (float)y / density;
				inp [x, y] += Mathf.PerlinNoise (px, py) / intensity + baseHeight;
			}
		}
		TimingModule.timer ("perlinModule", "end");

		return inp;
	}


	public bool [,] floodFillQueue (bool[,] inputPicture)
	{
		TimingModule.timer ("floodFillModule", "start");
		inputPicture = blackFrame (inputPicture);
		bool[,] inputPictureEdge = new bool[inputPicture.GetLength (0), inputPicture.GetLength (1)];
		Buffer.BlockCopy (inputPicture, 0, inputPictureEdge, 0, inputPicture.Length * sizeof(bool));

		Queue<int> qX = new Queue<int> ();
		Queue<int> qY = new Queue<int> ();

		qX.Enqueue (1);
		qY.Enqueue (1);

		int[] kernelX = { 0, 1, 0, -1 };
		int[] kernelY = { -1, 0, 1, 0 };

		while (qX.Count != 0 && qY.Count != 0) {
			for (int i = 0; i < 4; i++) {
				if (qX.Peek () < inputPicture.GetLength (0) - 2 && qX.Peek () > 0 && qY.Peek () < inputPicture.GetLength (1) - 2 && qY.Peek () > 0) {
					if (inputPicture [qX.Peek () + kernelX [i], qY.Peek () + kernelY [i]] == false) {
						qX.Enqueue (qX.Peek () + kernelX [i]);
						qY.Enqueue (qY.Peek () + kernelY [i]);
						inputPicture [qX.Peek () + kernelX [i], qY.Peek () + kernelY [i]] = true;
					}
				}

			}
			qX.Dequeue ();
			qY.Dequeue ();
		}

		for (int y = 2; y < inputPicture.GetLength (1) - 2; y++) {
			for (int x = 2; x < inputPicture.GetLength (0) - 2; x++) {
				if (inputPictureEdge [x, y] == true && inputPicture [x, y] == true) {
					inputPicture [x, y] = false;
				}
				
			}
		}
		TimingModule.timer ("floodFillModule", "end");
		return invert (inputPicture);
	}



	/// <summary>
	/// Invert the specified boolArray.
	/// </summary>
	/// <param name="boolArray">Bool array.</param>
	public bool[,] invert (bool[,] boolArray)
	{
		TimingModule.timer ("invertModule", "start");
		for (int y = 0; y < boolArray.GetLength (1); y++) {
			for (int x = 0; x < boolArray.GetLength (0); x++) {
				boolArray [x, y] = !boolArray [x, y];
			}
		}
		TimingModule.timer ("invertModule", "end");

		return boolArray;
	}


	/// <summary>
	/// Creates random value 2D array.
	/// </summary>
	/// <returns>The value gen.</returns>
	/// <param name="height">Height.</param>
	/// <param name="length">Length.</param>
	public float[,] randomValGen (int height, int length)
	{
		TimingModule.timer ("randomValueGenerator", "start");
		float[,] randomValues = new float[height, length];
		for (int y = 0; y < randomValues.GetLength (1); y++) {
			for (int x = 0; x < randomValues.GetLength (0); x++) {
				randomValues [x, y] = UnityEngine.Random.Range (0f, 1f);
			}
		}
		TimingModule.timer ("randomValueGenerator", "end");

		return randomValues;
	}



	/// <summary>
	/// Gaussian the specified 2D Array by using smoothing.
	/// </summary>
	/// <param name="heightMap">2D float height map.</param>
	/// <param name="smoothing">Number of iterations.</param>
	public float[,] gaussian (float[,] heightMap, int smoothing)
	{
		TimingModule.timer ("gaussianModule", "start");
		for (int k = 0; k < smoothing; k++) {
			for (int i = 1; i < heightMap.GetLength (0) - 1; i++) {
				for (int j = 1; j < heightMap.GetLength (1) - 1; j++) {
					float blur = (
					                 heightMap [i, j]

					                 + heightMap [i + 1, j + 1] * 2
					                 + heightMap [i + 1, j] * 2
					                 + heightMap [i, j + 1] * 2

					                 + heightMap [i - 1, j - 1] * 2
					                 + heightMap [i - 1, j] * 2
					                 + heightMap [i, j - 1] * 2

					                 + heightMap [i + 1, j - 1] * 2
					                 + heightMap [i - 1, j + 1] * 2
					             )
					             / (9 + 8);
					heightMap [i, j] = blur;	
				}
			}
		}
		TimingModule.timer ("gaussianModule", "end");

		return heightMap;
	}



	/// <summary>
	/// Dilate the specified boolean array.
	/// </summary>
	/// <param name="bools">Bools.</param>
	public bool[,] dilation (bool[,] bools)
	{
		TimingModule.timer ("dilationModule", "start");

		bool[,] returnedBools = new bool[bools.GetLength (0), bools.GetLength (1)];

		for (int y = 1; y < bools.GetLength (1) - 2; y++) {
			for (int x = 1; x < bools.GetLength (0) - 2; x++) {
				bool hit = false;
				for (int i = -1; i < 2; i++) {
					for (int j = -1; j < 2; j++) {
						if (hit) {
							returnedBools [x - j, y - i] = true;
						} else if (bools [x - j, y - i]) {
							hit = true;
							i = -1;
							j = -1;
						}
					}
				}
			}
		}
		TimingModule.timer ("dilationModule", "end");

		return returnedBools;
	}


	public bool[,] erosion (bool[,] bools)
	{ // UNTESTED
		bool[,] returnedBools = new bool[bools.GetLength (0), bools.GetLength (1)];
		Array.Copy (bools, returnedBools, 0);
		for (int y = 1; y < bools.GetLength (1) - 1; y++) {
			for (int x = 1; x < bools.GetLength (0) - 1; x++) {
				if (bools [x - 1, y - 1] && bools [x - 1, y] && bools [x, y - 1] && bools [x + 1, y - 1] && bools [x - 1, y + 1] && bools [x + 1, y + 1] && bools [x + 1, y] && bools [x, y + 1] && bools [x, y]) {
					returnedBools [x, y] = true;
				} else {
					returnedBools [x, y] = false;
				}
			}
		}
		return returnedBools;
	}



	/// <summary>
	/// Converts boolean array to float array.
	/// </summary>
	/// <returns>The to float.</returns>
	/// <param name="toBeConverted">To be converted.</param>
	public float [,] boolToFloat (bool[,] toBeConverted)
	{
		TimingModule.timer ("boolToFloatModule", "start");

		float[,] outputFloatArray = new float[toBeConverted.GetLength (0), toBeConverted.GetLength (1)]; 
		for (int y = 0; y < toBeConverted.GetLength (1); y++) {
			for (int x = 0; x < toBeConverted.GetLength (0); x++) {
				if (toBeConverted [x, y] == true) {
					outputFloatArray [x, y] = 1; 
				}
			}
		}
		TimingModule.timer ("boolToFloatModule", "end");

		return outputFloatArray; 
	}



	/// <summary>
	/// Converts float array to boolean array.
	/// </summary>
	/// <returns>The to bool.</returns>
	/// <param name="toBeConverted">To be converted.</param>
	public bool [,] floatToBool (float[,] toBeConverted)
	{
		TimingModule.timer ("floatToBoolModule", "start");


		bool[,] outputBoolArray = new bool[toBeConverted.GetLength (1), toBeConverted.GetLength (0)];
		for (int y = 0; y < toBeConverted.GetLength (1); y++) {
			for (int x = 0; x < toBeConverted.GetLength (0); x++) {

				if (toBeConverted [x, y] > 0.5f) {
					outputBoolArray [x, y] = true; 
				}
			}
		}
		TimingModule.timer ("floatToBoolModule", "end");

		return outputBoolArray; 
	}

	public float[,] generateTreePositions (float[,] inputArea, int treeSpace)
	{
		// when treePositions[0, x_value], a tree's x position is accessed
		// when treePositions[1, y_value], a tree's y position is accessed
		// the y index is set to 10000 to ensure that there are space for enough trees to be spawned
		float[,] treePositions = new float[2, 10000];
		// Ensures that trees look randomly placed
		float nextXPosition = UnityEngine.Random.Range (2f, 5f); 
		float nextYPosition = UnityEngine.Random.Range (2f, 5f);
		// ensures that the trees are not too close to each other
   
		// keeps track of how many trees are spawned
		int index = 0; 

		for (int y = 0; y < inputArea.GetLength (1); y += treeSpace) {
			for (int x = 0; x < inputArea.GetLength (0); x += treeSpace) {
				// Checks if the pixel scanned is white, and if the x and y positions are within the map
				if (inputArea [x, y] == 1f && x + nextXPosition <= inputArea.GetLength (0) && y + nextYPosition <= inputArea.GetLength (1)) {
					treePositions [0, index] = ((float)x + nextXPosition) / inputArea.GetLength (0); 
					treePositions [1, index] = ((float)y + nextYPosition) / inputArea.GetLength (1);
					index++; 

					nextXPosition = UnityEngine.Random.Range (2f, 5f); 
					nextYPosition = UnityEngine.Random.Range (2f, 5f);
				} 
			}
		}
		// The array is resized to the number of tree's that are spawned, so 10000 trees are not spawned everytime.w
		float[,] output = new float[2, index];
		for (int i = 0; i < output.GetLength (1); i++) {
			output [0, i] = treePositions [0, i];
			output [1, i] = treePositions [1, i];
		}
		return output;
	}



	/// <summary>
	/// Adds true border, adds false inline border.
	/// </summary>
	/// <returns>The frame.</returns>
	/// <param name="boolArrayToBeFramed">Bool array to be framed.</param>
	public bool [,] blackFrame (bool[,] boolArrayToBeFramed)
	{
		TimingModule.timer ("blackframeModule", "start");

		for (int y = 0; y < boolArrayToBeFramed.GetLength (1); y++) {
			for (int x = 0; x < boolArrayToBeFramed.GetLength (0); x++) {

				if (y == 0)
					boolArrayToBeFramed [x, y] = true; 
				if (y == 1)
					boolArrayToBeFramed [x, y] = false; 
				
				if (y == boolArrayToBeFramed.GetLength (1) - 1)
					boolArrayToBeFramed [x, y] = true; 
				if (y == boolArrayToBeFramed.GetLength (1) - 2)
					boolArrayToBeFramed [x, y] = false; 
				
				if (x == 0)
					boolArrayToBeFramed [x, y] = true;
				if (x == 1)
					boolArrayToBeFramed [x, y] = false;
				
				if (x == boolArrayToBeFramed.GetLength (0) - 1)
					boolArrayToBeFramed [x, y] = true;
				if (x == boolArrayToBeFramed.GetLength (0) - 2)
					boolArrayToBeFramed [x, y] = false;

			}
		}
		TimingModule.timer ("blackframeModule", "end");

		return boolArrayToBeFramed; 
	}


	/// <summary>
	/// Applies median filter.
	/// </summary>
	/// <returns>The filter.</returns>
	/// <param name="inputpicture">Inputpicture.</param>
	public bool [,] medianFilter (bool[,] inputpicture)
	{
		int[] kernel = { 1, 1, 1, 1, 1, 1, 1, 1, 1 };

		for (int y = 1; y < inputpicture.GetLength (1) - 1; y++) {
			for (int x = 1; x < inputpicture.GetLength (0) - 1; x++) {
				int sum = 0;

				for (int ky = 0; ky <= 2; ky++) {
					for (int kx = 0; kx <= 2; kx++) {
						if (inputpicture [x + kx - 1, y + ky - 1])
							sum++;

					}
				}

				if (sum >= 5)
					inputpicture [x, y] = true;
				else {
					inputpicture [x, y] = false;
				}
			}
		}
		return inputpicture;

	}


	/// <summary>
	/// Subtract the specified .
	/// </summary>
	/// <param name="Base">Base.</param>
	/// <param name="subtract">Subtract.</param>
	public float [,] subtract (float[,] Base, float[,] subtract)
	{
		TimingModule.timer ("subtractModule", "start");

		for (int y = 0; y < Base.GetLength (1); y++) {
			for (int x = 0; x < Base.GetLength (0); x++) {

				Base [x, y] -= subtract [x, y];

			}
		}
		TimingModule.timer ("subtractModule", "end");


		return Base;
	}




	/// <summary>
	/// Add the specified Base and add.
	/// </summary>
	/// <param name="Base">Base.</param>
	/// <param name="add">Add.</param>
	public float [,] add (float[,] Base, float[,] add)
	{
		TimingModule.timer ("addModule", "start");

		for (int y = 0; y < Base.GetLength (1); y++) {
			for (int x = 0; x < Base.GetLength (0); x++) {

				Base [x, y] += add [x, y];

			}
		}
		TimingModule.timer ("addModule", "end");

		return Base;
	}


	/// <summary>
	/// Compressing and subtracting subtractor from base.
	/// </summary>
	/// <returns>The and subtract.</returns>
	/// <param name="Base">Base.</param>
	/// <param name="subtractor">Subtractor.</param>
	public float [,] compressAndSubtract (float[,] Base, float[,] subtractor)
	{
		TimingModule.timer ("riverGenerateModule", "start");

		for (int y = 0; y < Base.GetLength (1); y++) {
			for (int x = 0; x < Base.GetLength (0); x++) {
				Base [x, y] -= (subtractor [x, y]) * Base [x, y];
			}
		}
		TimingModule.timer ("riverGenerateModule", "end");

		return Base;
	}



	/// <summary>
	/// Flips the X and y.
	/// </summary>
	/// <returns>The X and y.</returns>
	/// <param name="original">Original.</param>
	public Texture2D flipXAndY (Texture2D original)
	{
		TimingModule.timer ("flipX&Y", "start");
		Texture2D flipped = new Texture2D (original.height, original.height);

		int cropAreaOnEachSide = (original.width - original.height) / 2;

		int xN = original.width - 1 - cropAreaOnEachSide;
		int yN = original.height - 1;


		for (int i = cropAreaOnEachSide; i < xN; i++) {
			for (int j = 0; j < yN; j++) {
				flipped.SetPixel (yN - j, xN - i, original.GetPixel (i, j));

			}
		}
		flipped.Apply ();
		TimingModule.timer ("flipX&Y", "end");

		return flipped;
	}


}


/// <summary>
/// Timing module.
/// </summary>
public class TimingModule
{
	static float startTimeOfModule;
	static float endTimeOfModule;

	static float startTimeOfProgram;
	static float endTimeOfProgram;

	/// <summary>
	/// Timer the specified name and status.
	/// </summary>
	/// <param name="name">Name.</param>
	/// <param name="status">Status.</param>
	public static void timer (string name, string status)
	{
		if (name == "program" && status == "start") {
			startTimeOfProgram = Time.realtimeSinceStartup;
		}

		if (name == "program" && status == "end") {
			endTimeOfProgram = Time.realtimeSinceStartup;
			Debug.Log (name + " took " + (endTimeOfProgram - startTimeOfProgram) * 1000 + " miliseconds");
		}

		if (name != "program" && status == "start") {
			startTimeOfModule = Time.realtimeSinceStartup;
		}

		if (name != "program" && status == "end") {
			endTimeOfModule = Time.realtimeSinceStartup;
			//Debug.Log (name + " took " + (endTimeOfModule - startTimeOfModule) * 1000 + " miliseconds");
		}


	}



}
