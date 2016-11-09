﻿using UnityEngine;
using System.Collections;
using System;

public class MapCreator : MonoBehaviour
{
    imageProcModules modules;
    ColorDetection colorScanScript;
    MountainGeneration mg;
    Terrain currentTerrain;
	float update;
    int frame = 0;
	float[,] drawMap, myHeightMap; 
	public int heightOfMap = 100;
	public float [,] newHeightMap;
	public float[,] river;

	public TreeInstance tree;

    void Start() {

		//INITIALIZING: 	////////////
		modules = GetComponent<imageProcModules> ();
		colorScanScript = GameObject.Find ("colorScan").GetComponent<ColorDetection> ();
		mg = GetComponent<MountainGeneration> ();

		Debug.Log (colorScanScript.widthOfTex + ", " + colorScanScript.heightOfTex);

		myHeightMap = new float[colorScanScript.widthOfTex, colorScanScript.heightOfTex];
		drawMap = new float[colorScanScript.widthOfTex, colorScanScript.heightOfTex];
		float startit = Time.realtimeSinceStartup; //starting milli counter:




		//GENERATION: 		////////////
		bool[,] inputColorImage = colorScanScript.colorDetection(colorScanScript.originalImage, 0.20f, 0.15f, 0f, 0f); // getting colors from i	nput image

		currentTerrain = Terrain.activeTerrain; // getting terrain data
		//fixing texturescale issue:
		int biggestDimention = (colorScanScript.heightOfTex > colorScanScript.widthOfTex) ? colorScanScript.heightOfTex : colorScanScript.widthOfTex; //Simple if statement 
		currentTerrain.terrainData.size = new Vector3(biggestDimention, heightOfMap, biggestDimention); //setting size

        inputColorImage = modules.dilation(inputColorImage);
        inputColorImage = modules.floodFill(inputColorImage);



		colorScanScript.printBinary(inputColorImage); //printing to plane

		myHeightMap = modules.boolToFloat(inputColorImage);//float convertion
		//myHeightMap = modules.perlin(myHeightMap); 
		newHeightMap = new float[myHeightMap.GetLength(0),myHeightMap.GetLength(1)];
		colorScanScript.printBinary(myHeightMap);
		newHeightMap = modules.generateRiver(newHeightMap, myHeightMap);


		//MOUNTAINS: 		////////////
		myHeightMap = modules.gaussian(myHeightMap, 0); //gauss
//		colorScanScript.printBinary(myHeightMap); //printing to plane
//        myHeightMap = mg.midpointDisplacement(3, myHeightMap, 1.0f, 0);
//        myHeightMap = mg.midpointDisplacement(8, myHeightMap, 1.0f, 0);
//        myHeightMap = mg.midpointDisplacement(16, myHeightMap, 1.0f, 0);
//        myHeightMap = mg.midpointDisplacement(32, myHeightMap, 0.5f, 0);
//        myHeightMap = mg.midpointDisplacement(64, myHeightMap, 0.5f, 0);
//        myHeightMap = mg.midpointDisplacement(128, myHeightMap, 0.5f, 0);
//
//        myHeightMap = mg.finalMap(mg.mountainRemove(myHeightMap, modules.boolToFloat(inputColorImage)), 5);
//		myHeightMap = modules.perlin(myHeightMap); 

        Debug.Log("Total millis for all recursions: " + ((Time.realtimeSinceStartup - startit) * 1000));
    }




    bool stop = false;

    void Update() {
        if (!stop){
            if (frame % 2 == 0) {
                for (int i = 0; i < drawMap.GetLength(0); i++) {
                    for (int j = 0; j < drawMap.GetLength(1); j++) {
                        drawMap[i, j] = myHeightMap[i, j] * update * 0.5f;
                    }
                }
                currentTerrain.terrainData.SetHeights(0, 0, drawMap);
            }
            frame++;
			if (update < 1f) {
				update += 0.01f;
			} else {
				print(drawMap.GetLength (0) + ", " + drawMap.GetLength (0));
				stop = true;
			}
        }


    }

}
