﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    #region Data Members
    //A reference to the empty map to add objects to
    [SerializeField]
    private GameObject mapRoot;
    //A reference to the billboard prefab
    [SerializeField]
    private GameObject billboardPrefab;

    //A hashtable mapping gameobjects to MapObject scripts
    public Dictionary<GameObject, MapObject> objectScriptTable { get; private set; }
    //Determines if the small map bounds have been disabled or not
    private bool boundsDisabled;
    //A list of objects cloned from the copied selection
    private List<GameObject> copiedObjects;
    #endregion

    #region Initialization
    //Get a references to components out of the scope of the script
    void Start()
    {
        objectScriptTable = new Dictionary<GameObject, MapObject>();
        testLoadMap();
    }
    #endregion

    #region Update
    void LateUpdate()
    {
        //If the game is in edit mode, check for keyboard shortcut inputs
        if (CommonReferences.editorManager.currentMode == EditorMode.Edit)
        {
            //Check delete keys
            if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete))
                deleteSelection();
            //Check for shortcuts that require the control key to be pressed down
            else if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftCommand))
            {
                if (Input.GetKeyDown(KeyCode.C))
                    copySelection();
                else if (Input.GetKeyDown(KeyCode.V))
                    pasteSelection();
            }
        }
    }

    //Copy a selection by cloning all of the selected objects and storing them
    private void copySelection()
    {
        //Get a reference to the list of selected objects
        ref List<GameObject> selectedObjects = ref CommonReferences.objectSelection.getSelection();

        //If there aren't any objects to copy, return
        if (selectedObjects.Count == 0)
            return;

        //Reset the old list of copied objects
        copiedObjects = new List<GameObject>();
        //Temporary GameObject to disable cloned objects before storing them
        GameObject objectClone;

        //Clone each selected object and save it in the copied objects list
        foreach (GameObject mapObject in selectedObjects)
        {
            //Instantiate and disable the copied objects
            objectClone = Instantiate(mapObject);
            objectClone.SetActive(false);
            //Get a reference to the cloned object's MapObject script
            MapObject mapObjectScript = objectClone.GetComponent<MapObject>();
            //Copy the values of the original map object script
            mapObjectScript.copyValues(mapObject.GetComponent<MapObject>());
            //Add the object to the copied objects list
            copiedObjects.Add(objectClone);
        }
    }

    //Paste the copied objects by instantiating them
    private void pasteSelection()
    {
        //Temporary GameObject to enable cloned objects before storing them
        GameObject objectClone;
        //Reset the current selection
        CommonReferences.objectSelection.deselectAll();

        //Loop through all of the copied objects
        foreach (GameObject mapObject in copiedObjects)
        {
            //Instantiate and enable the cloned object
            objectClone = Instantiate(mapObject);
            objectClone.SetActive(true);
            //Get a reference to the cloned object's MapObject script
            MapObject mapObjectScript = objectClone.GetComponent<MapObject>();
            //Copy the values of the original map object script
            mapObjectScript.copyValues(mapObject.GetComponent<MapObject>());
            //Add the object to the map and make it selectable
            addObjectToMap(objectClone, mapObjectScript);
            CommonReferences.objectSelection.selectObject(objectClone);
        }

        //Once the selection is pasted, change the tool type to translate
        CommonReferences.objectSelection.setTool(Tool.Translate);
    }

    //Delete the selected objects
    //To-Do: store deleted objects so the delete can be undone
    private void deleteSelection()
    {
        //Get a reference to the selected objects list
        ref List<GameObject> selectedObjects = ref CommonReferences.objectSelection.removeSelected();

        //Remove each selected object from the script table and destroy the object
        foreach (GameObject mapObject in selectedObjects)
        {
            objectScriptTable.Remove(mapObject);
            destroyObject(mapObject);
        }

        //Reset the selected objects lsit
        selectedObjects = new List<GameObject>();
    }
    #endregion

    #region Map Methods
    private void testLoadMap()
    {
        //Map with no whitespaces
        loadMap("map,disablebounds;custom,cuboid,stone3,750,1,750,0,1,1,1,128.0,128.0,0,-4.5,0,0,0,0,0;racing,endCuboid,15,8,5,-350,45,3075,0,0,0,0;racing,startCuboid,2,6,2,-15,80,60,0,0,0,0;racing,startCuboid,2,6,2,15,80,60,0,0,0,0;racing,checkpointCuboid,15,0.5,5,-350,92.5,3075,0,0,0,0;racing,killCuboid,120,3,20,-350,15,2760,0,0,0,0;spawnpoint,playerC,0,70,-10,0,0,0,0;spawnpoint,playerM,0,70,-10,0,0,0,0;spawnpoint,titan,-150,11,2950,0,0,0,0;spawnpoint,titan,-550,11,2950,0,0,0,0;photon,spawnTitan,30,0,-400,11,2950,0,0,0,0;photon,spawnTitan,30,0,-350,11,2950,0,0,0,0;photon,spawnTitan,30,0,-300,11,2950,0,0,0,0;custom,statue2,default,0.5,0.5,0.5,0,1,1,1,0.25,1.0,0,80,40,0,1,0,0;custom,statue2,default,0.75,0.75,0.75,0,1,1,1,0.25,1.0,-35,75,40,0,1,0,1;custom,statue2,default,0.75,0.75,0.75,0,1,1,1,0.25,1.0,35,75,40,0,1,0,-1;misc,barrier,32,1,50,190,65,-210,0,1,0,1;misc,barrier,50,1,50,380,65,200,0,0,0,0;misc,barrier,13,1,19,535,65,-115,0,1,0,1;misc,barrier,20,1,20,-55,65,-235,0,1,0,-0.414;misc,barrier,14,1,14,-130,65,-305,0,1,0,-1;misc,barrier,10,1,10,-200,65,-305,0,1,0,-0.414;misc,barrier,7,1,7,-240,65,-340,0,1,0,-1;misc,barrier,5,1,5,-275,65,-340,0,1,0,-0.414;misc,barrier,3,1,5,-292.5,65,-360,0,1,0,-1;misc,barrier,2,1,4,-300,65,-350,0,1,0,-0.414;misc,barrier,750,1,750,0,115,0,0,0,0,0;misc,barrier,0.5,5,7,-480.83,80,-63.92,0,0.38,0,-0.92;misc,barrier,0.5,5,7,-423.88,80,-105.74,0,0.85,0,0.51;misc,barrier,0.5,5,7,-357.63,80,-126.99,0,0.76,0,0.64;misc,barrier,0.5,5,7,-291.11,80,-126.50,0,0.64,0,0.76;misc,barrier,0.5,5,7,-227.10,80,-106.14,0,0.52,0,0.85;misc,barrier,0.5,5,7,-170.39,80,-65.66,0,0.38,0,0.92;misc,barrier,0.5,5,7,-129.23,80,-9.11,0,0.23,0,0.97;misc,barrier,0.5,5,7,-521.42,80,-7.84,0,0.23,0,-0.97;misc,barrier,0.5,5,7,-169.17,80,244.10,0,0.92,0,0.38;misc,barrier,0.5,5,7,-225.98,80,285.83,0,0.52,0,-0.85;misc,barrier,0.5,5,7,-289.89,80,306.66,0,0.64,0,-0.76;misc,barrier,0.5,5,7,-521.78,80,187.11,0,0.23,0,0.97;misc,barrier,0.5,5,7,-479.45,80,246.07,0,0.38,0,0.92;misc,barrier,0.5,5,7,-423.84,80,285.95,0,0.52,0,0.85;misc,barrier,0.5,5,6,-131.88,80,192.61,0,0.23,0,-0.97;misc,barrier,0.5,5,2,-384.01,80,302.86,0,0.77,0,-0.63;misc,barrier,12,5,0.5,-322.5,80,372,0,1,0,1;misc,barrier,14,5,0.5,-377.5,80,372,0,1,0,1;misc,barrier,5.5,5,1,-190,85,225,0,1,0,0.414;custom,cuboid,stone5,10,2,10,0,1,1,1,1.0,1.0,0,60,0,0,0,0,0;custom,cuboid,stone5,1,6,46,0,1,1,1,4.0,1.0,30,80,280,0,0,0,0;custom,cuboid,stone5,1,6,38,0,1,1,1,4.0,1.0,-30,80,240,0,0,0,0;custom,cuboid,stone5,1,6,12,0,1,1,1,2.0,1.0,50,80,0,0,0,0,0;custom,cuboid,stone5,1,6,12,0,1,1,1,2.0,1.0,-50,80,0,0,0,0,0;custom,cuboid,stone5,2.1,5.99,1.1,0,1,1,1,0.25,1.0,-35,80,55,0,0,0,0;custom,cuboid,stone5,2.1,5.99,1.1,0,1,1,1,0.25,1.0,35,80,55,0,0,0,0;custom,cuboid,stone5,9,6,1.1,0,1,1,1,2.0,1.0,0,80,-55,0,0,0,0;custom,cylinder,stone5,4,3,4,0,1,1,1,1.0,1.0,-45,80,430,0,0,0,0;custom,cuboid,stone5,1,11,36,0,1,1,1,4.0,2.0,-145,55,515,0,1,0,-1;custom,cuboid,stone3,6,1,46,0,1,1,1,1.0,4.0,0,55,280,0,0,0,0;custom,cuboid,stone3,7,1,15,0,1,1,1,1.0,2.0,-105,55,480,0,1,0,1;custom,cuboid,stone3,7,1,15,0,1,1,1,1.0,2.0,-247.5,26.5,480,-0.20,1,0.20,1;custom,cuboid,stone3,7,1,15,0,1,1,1,1.0,2.0,-595,55,480,0,1,0,1;custom,cuboid,stone3,7,1,15,0,1,1,1,1.0,2.0,-452.5,26.5,480,0.20,1,-0.20,1;custom,cuboid,stone5,1,11,29.5,0,1,1,1,4.0,2.0,-522.5,55,515,0,1,0,-1;custom,cuboid,stone3,7,1,8,0,1,1,1,1.0,1.0,-350,-2,480,0,1,0,-1;custom,cuboid,stone5,1,6,100,0,1,1,1,12.0,1.0,-675,80,20,0,0,0,0;custom,cuboid,stone5,1,11,28,0,1,1,1,4.0,2.0,-185,55,445,0,1,0,-1;custom,cuboid,stone5,1,11,18,0,1,1,1,4.0,2.0,-465,55,445,0,1,0,-1;custom,cylinder,stone5,4,3,4,0,1,1,1,1.0,1.0,-555,80,430,0,0,0,0;custom,cuboid,stone3,10,1,93,0,1,1,1,1.0,10.0,-620,55,-20,0,0,0,0;custom,cuboid,stone5,1,6,80,0,1,1,1,8.0,1.0,-570,80,30,0,0,0,0;custom,cuboid,stone5,1,6,100,0,1,1,1,12.0,1.0,-170,80,-485,0,1,0,1;custom,cuboid,stone3,11,1,90,0,1,1,1,1.0,8.0,-120,55,-430,0,1,0,1;custom,cuboid,stone5,1,6,90,0,1,1,1,12.0,1.0,-120,80,-375,0,1,0,1;custom,cuboid,stone5,2.5,2.5,2.5,0,1,1,1,0.5,0.5,-635,70,-440,0,0,0,0;custom,cuboid,stone5,2,6,6,0,1,1,1,1.0,1.0,-585,80,-390,0,0.414,0,1;custom,cuboid,stone5,1,5,11,0,1,1,1,1.0,1.0,335,25,-430,0,0,0,0;custom,cylinder,stone5,2,5.5,2,0,1,1,1,0.5,2.0,330,50,-430,0,1,1,0;custom,cuboid,stone5,1,5,11,0,1,1,1,1.0,1.0,435,85,-430,0,0,0,0;custom,cuboid,stone5,1,5,11,0,1,1,1,1.0,1.0,535,25,-430,0,0,0,0;custom,cuboid,stone5,1,5,11,0,1,1,1,1.0,1.0,635,85,-430,0,0,0,0;custom,cylinder,stone5,1,5.5,1,0,1,1,1,0.5,2.0,435,60,-430,0,1,1,0;custom,cylinder,stone5,1,5.5,1,0,1,1,1,0.5,2.0,535,50,-430,0,1,1,0;custom,cylinder,stone5,1,5.5,1,0,1,1,1,0.5,2.0,635,60,-430,0,1,1,0;custom,cuboid,stone5,1,11,2,0,1,1,1,1.0,1.0,385,55,-385,0,0,0,0;custom,cuboid,stone5,1,11,6,0,1,1,1,1.0,1.0,385,55,-455,0,0,0,0;custom,cuboid,stone5,1,11,4,0,1,1,1,1.0,1.0,485,55,-395,0,0,0,0;custom,cuboid,stone5,1,11,4,0,1,1,1,1.0,1.0,485,55,-465,0,0,0,0;custom,cuboid,stone5,1,11,6,0,1,1,1,1.0,1.0,585,55,-405,0,0,0,0;custom,cuboid,stone5,1,11,2,0,1,1,1,1.0,1.0,585,55,-475,0,0,0,0;custom,cuboid,stone5,1,11,31,0,1,1,1,4.0,2.0,485,55,-375,0,1,0,1;custom,cuboid,stone5,1,11,30,0,1,1,1,4.0,2.0,735,55,-335,0,0,0,0;custom,cuboid,stone5,1,11,30,0,1,1,1,4.0,2.0,590,55,-180,0,1,0,-1;custom,cuboid,stone5,1,11,14,0,1,1,1,1.5,1.5,645,55,-300,0,0,0,0;custom,cylinder,stone5,2,5.5,2,0,1,1,1,1.0,2.0,640,55,-370,0,0,0,0;custom,cuboid,stone5,1,11,41,0,1,1,1,4.0,2.0,535,55,-485,0,1,0,1;custom,cuboid,stone5,1,11,20,0,1,1,1,2.0,1.5,540,55,-225,0,1,0,1;custom,cylinder,stone5,2,5.5,2,0,1,1,1,1.0,2.0,640,55,-230,0,0,0,0;custom,cuboid,stone5,3.5,5,1,0,1,1,1,0.5,0.5,445,85,-202.5,0,1,0,1;custom,cuboid,stone5,8,4,1,0,1,1,1,1.0,1.0,690,20,-295,0,0,0,0;custom,cuboid,stone5,8,4,1,0,1,1,1,1.0,1.0,690,90,-295,0,0,0,0;custom,cuboid,stone5,2,3,1,0,1,1,1,0.5,0.5,660,55,-295,0,0,0,0;custom,cuboid,stone5,2,3,1,0,1,1,1,0.5,0.5,720,55,-295,0,0,0,0;custom,cuboid,stone5,2,6,1,0,1,1,1,0.5,1,435,30,-175,0,1,0,1;custom,cuboid,stone5,3,6,1,0,1,1,1,0.5,1,435,30,-235,0,1,0,1;custom,cuboid,stone5,1,6,10,0,1,1,1,1.0,1.0,380,30,-160,0,1,0,1;custom,cylinder,stone5,5,3,5,0,1,1,1,1.0,1.0,330,30,-140,0,0,0,0;custom,cuboid,stone5,1,6,20,0,1,1,1,2.0,1.0,430,30,-120,0,1,0,1;custom,cuboid,stone5,1,6,30,0,1,1,1,4.0,1.0,535,30,25,0,0,0,0;custom,cuboid,stone5,1,6,20,0,1,1,1,4.0,1.0,330,30,-250,0,1,0,1;custom,cuboid,stone5,1,6,19,0,1,1,1,4.0,1.0,235,30,-150,0,0,0,0;custom,cuboid,stone5,1,6,24,0,1,1,1,4.0,1.0,350,30,-50,0,1,0,1;custom,cuboid,stone5,1,6,30,0,1,1,1,4.0,1.0,475,30,95,0,0,0,0;custom,cuboid,stone5,1,6,10,0,1,1,1,2.0,1.0,520,30,250,0,1,0,1;custom,cuboid,stone5,1,6,10,0,1,1,1,2.0,1.0,580,30,180,0,1,0,1;custom,cuboid,stone5,1,6,20,0,1,1,1,2.0,1.0,635,30,275,0,0,0,0;custom,cuboid,stone5,1,6,50,0,1,1,1,6.0,1.0,390,30,380,0,1,0,1;custom,cuboid,stone5,1,6,8,0,1,1,1,1.0,1.0,575,30,285,0,0,0,0;custom,cuboid,stone5,1,6,30,0,1,1,1,2.0,1.0,430,30,330,0,1,0,1;custom,cylinder,stone5,10,3,10,0,1,1,1,2.0,1.0,280,30,285,0,0,0,0;custom,cuboid,stone5,1,6,33,0,1,1,1,4.0,1.0,235,30,120,0,0,0,0;custom,cuboid,stone5,1,6,34,0,1,1,1,4.0,1.0,115,30,-255,0,1,0,-0.414;custom,cuboid,stone5,1,6,28.5,0,1,1,1,4.0,1.0,45,30,-185,0,1,0,-0.414;custom,cuboid,stone5,1,6,46,0,1,1,1,4.0,1.0,145,30,145,0,0,0,0;custom,cuboid,stone5,1,6,20,0,1,1,1,2.0,1.0,-155,30,-282.5,0,1,0,1;custom,cuboid,stone5,1,2,14,0,1,1,1,1.5,0.5,-300,50,-330,0,1,0,-0.414;custom,cuboid,stone5,1,2,14,0,1,1,1,1.5,0.5,-300,10,-330,0,1,0,-0.414;custom,cuboid,stone5,1,5,14,0,1,1,1,1.5,0.5,-300,85,-330,0,1,0,-0.414;custom,cuboid,stone5,1,2,5,0,1,1,1,1.0,0.5,-330,30,-360,0,1,0,-0.414;custom,cuboid,stone5,1,2,5,0,1,1,1,1.0,0.5,-270,30,-300,0,1,0,-0.414;custom,cuboid,stone5,1,11,81,0,1,1,1,8.0,1.0,-540,55,35,0,0,0,0;custom,cuboid,stone5,1,11,20,0,1,1,1,2.0,1.0,-435,55,-365,0,1,0,-1;custom,cuboid,stone5,1.1,10.95,32,0,1,1,1,4.0,1.0,-140,55,-170,0,0.414,0,1;custom,cuboid,stone5,1,11,53,0,1,1,1,4.0,1.0,-60,55,175,0,0,0,0;custom,cuboid,stone5,1,5,33,0,1,1,1,4.0,1.0,-170,25,-375,0,1,0,1;custom,cylinder,stone5,34,5.5,34,0,1,1,1,8.0,1.0,-325,55,90,0,0,0,0;custom,cylinder,stone5,34,5.51,34,0,1,1,1,8.0,1.0,-325,55,90,0,1,0,0.414;custom,cylinder,stone3,44,0.5,44,0,1,1,1,8.0,8.0,-325,55,90,0,0,0,0;custom,cylinder,stone5,44.1,0.5,44.1,0,1,1,1,16.0,0.25,-325,54.49,90,0,0,0,0;custom,cylinder,stone5,44,0.5,44,0,1,1,1,8.0,8.0,-325,54.48,90,0,0,0,0;custom,cuboid,stone3,5.25,1.1,13.5,0,1,1,1,1.0,2.0,-90,55,90,0,0,0,0;custom,cuboid,stone5,6,5.95,1.0,0,1,1,1,4.0,0.5,-90,30,160,0,0,0,0;custom,cuboid,stone5,6,5.5,1.0,0,1,1,1,4.0,0.5,-90,27.5,170,0,0,0,0;custom,cuboid,stone5,6,5.0,1.0,0,1,1,1,4.0,0.5,-90,25.0,180,0,0,0,0;custom,cuboid,stone5,6,4.5,1.0,0,1,1,1,4.0,0.5,-90,22.5,190,0,0,0,0;custom,cuboid,stone5,6,4.0,1.0,0,1,1,1,4.0,0.5,-90,20.0,200,0,0,0,0;custom,cuboid,stone5,6,3.5,1.0,0,1,1,1,4.0,0.5,-90,17.5,210,0,0,0,0;custom,cuboid,stone5,6,3.0,1.0,0,1,1,1,4.0,0.5,-90,15.0,220,0,0,0,0;custom,cuboid,stone5,6,2.5,1.0,0,1,1,1,4.0,0.5,-90,12.5,230,0,0,0,0;custom,cuboid,stone5,6,2.0,1.0,0,1,1,1,4.0,0.5,-90,10.0,240,0,0,0,0;custom,cuboid,stone5,6,1.5,1.0,0,1,1,1,4.0,0.5,-90,07.5,250,0,0,0,0;custom,cuboid,stone5,6,1.0,1.0,0,1,1,1,4.0,0.5,-90,05.0,260,0,0,0,0;custom,cuboid,stone5,6,0.5,1.0,0,1,1,1,4.0,0.5,-90,02.5,270,0,0,0,0;custom,cuboid,stone3,5,0.99,200,0,1,1,1,1.0,32.0,-350,55,1300,0,0,0,0;custom,cuboid,stone5,5.0,0.99,200,0,1,1,1,1.0,32.0,-350,54.8,1300,0,0,0,0;custom,cuboid,stone5,5.05,0.99,200,0,1,1,1,32.0,0.25,-350,54.9,1300,0,0,0,0;custom,cuboid,stone5,1,11,178,0,1,1,1,24.0,2.0,-380,55,1410,0,0,0,0;custom,cuboid,stone5,1,11,178,0,1,1,1,24.0,2.0,-320,55,1410,0,0,0,0;custom,cuboid,stone5,1,5,8,0,1,1,1,2.0,1.0,-370,85,480,0,0,0,0;custom,cuboid,stone5,1,5,8,0,1,1,1,2.0,1.0,-330,85,480,0,0,0,0;custom,cylinder,stone5,1,5.5,1,0,1,1,1,1.0,4.0,-503,55,219,0,0,0,0;custom,cylinder,stone5,1,5.5,1,0,1,1,1,1.0,4.0,-454,55,268,0,0,0,0;custom,cylinder,stone5,1,5.5,1,0,1,1,1,1.0,4.0,-392,55,298,0,0,0,0;custom,cylinder,stone5,1,5.5,1,0,1,1,1,1.0,4.0,-325,55,311,0,0,0,0;custom,cylinder,stone5,1,5.5,1,0,1,1,1,1.0,4.0,-257,55,300,0,0,0,0;custom,cylinder,stone5,1,5.5,1,0,1,1,1,1.0,4.0,-196,55,267,0,0,0,0;custom,cylinder,stone5,1,5.5,1,0,1,1,1,1.0,4.0,-147,55,219,0,0,0,0;custom,cylinder,stone5,1,5.5,1,0,1,1,1,1.0,4.0,-196,55,-88,0,0,0,0;custom,cylinder,stone5,1,5.5,1,0,1,1,1,1.0,4.0,-257,55,-120,0,0,0,0;custom,cylinder,stone5,1,5.5,1,0,1,1,1,1.0,4.0,-325,55,-130,0,0,0,0;custom,cylinder,stone5,1,5.5,1,0,1,1,1,1.0,4.0,-394,55,-119,0,0,0,0;custom,cylinder,stone5,1,5.5,1,0,1,1,1,1.0,4.0,-454,55,-88,0,0,0,0;custom,cylinder,stone5,1,5.5,1,0,1,1,1,1.0,4.0,-503,55,-38,0,0,0,0;custom,cylinder,stone5,1,5.5,1,0,1,1,1,1.0,4.0,-147,55,-40,0,0,0,0;custom,cuboid,stone5,1,11,16,0,1,1,1,2.0,1.0,-120,55,240,0,0,0,0;custom,cylinder,stone5,1,5.51,1,0,1,1,1,0.5,1.0,-120,55,160,0,0,0,0;custom,cylinder,stone5,1,5.51,1,0,1,1,1,0.5,1.0,-120,55,320,0,0,0,0;custom,cuboid,stone5,6,11.1,1,0,1,1,1,1.0,2.0,-85,55,25,0,0,0,0;custom,cylinder,stone5,1,5.5,1,0,1,1,1,0.5,1.0,-115,55,25,0,0,0,0;custom,cuboid,stone5,6,5,1,0,1,1,1,1.0,1.0,-145,25,25,0,0,0,0;custom,cuboid,stone5,6,5,1,0,1,1,1,1.0,1.0,-150,25,160,0,0,0,0;custom,cylinder,stone5,47,0.51,47,0,1,1,1,8.0,8.0,-325,110,90,0,0,0,0;custom,cylinder,stone5,47.1,0.50,47.1,0,1,1,1,16.0,0.25,-325,110,90,0,0,0,0;custom,cuboid,stone5,5,5,1,0,1,1,1,1.0,1.0,-350,25,445,0,0,0,0;custom,cuboid,stone5,5,5,1,0,1,1,1,1.0,1.0,-350,25,515,0,0,0,0;custom,cuboid,stone5,2,6.1,2,0,1,1,1,0.5,1.0,-55,30,-285,0,0.414,0,-1;custom,cuboid,stone5,2,6.1,2,0,1,1,1,0.5,1.0,145,30,-85,0,1,0,-0.414;custom,cylinder,stone5,2,5.5,2,0,1,1,1,1.0,4.0,-325,55,440,0,0,0,0;custom,cylinder,stone5,2,5.5,2,0,1,1,1,1.0,4.0,-375,55,440,0,0,0,0;custom,cylinder,stone5,1,5.5,1,0,1,1,1,1.0,4.0,-325,55,395,0,0,0,0;custom,cylinder,stone5,1,5.5,1,0,1,1,1,1.0,4.0,-375,55,395,0,0,0,0;custom,cylinder,stone5,1,5.5,1,0,1,1,1,1.0,4.0,-325,55,355,0,0,0,0;custom,cylinder,stone5,1,5.5,1,0,1,1,1,1.0,4.0,-375,55,355,0,0,0,0;custom,cylinder,stone5,1,5.5,1,0,1,1,1,1.0,4.0,-375,55,302.5,0,0,0,0;custom,cuboid,stone5,8,0.9,13.98,0,1,1,1,1.0,2.0,-350,110,380,0,0,0,0;custom,cuboid,stone5,8.1,0.89,13.99,0,1,1,1,1.0,0.25,-350,110,380,0,0,0,0;custom,cuboid,stone5,4.99,1,7,0,1,1,1,1.0,1.0,-350,110,485,0,0,0,0;custom,cylinder,stone5,1,2.95,1,0,1,1,1,1.0,1.0,-330,85,520,0,0,0,0;custom,cylinder,stone5,1,2.95,1,0,1,1,1,1.0,1.0,-370,85,520,0,0,0,0;custom,cylinder,stone5,1,2.49,1,0,1,1,1,1.0,1.0,-350,110,520,-0.5,0.5,-0.5,-0.5;custom,cuboid,grass4,14,0.5,12,0,1,1,1,2.0,2.0,-465,2.50,380,0,0,0,0;custom,cuboid,brick2,14.1,0.5,12.1,0,1,1,1,0.01,0.01,-465,2.40,380,0,0,0,0;custom,cuboid,grass4,12,0.5,12,0,1,1,1,2.0,2.0,-250,2.50,380,0,0,0,0;custom,cuboid,brick2,12.1,0.5,12.1,0,1,1,1,0.01,0.01,-250,2.40,380,0,0,0,0;custom,cuboid,grass4,2.5,0.5,12,0,1,1,1,0.5,2.0,-350,2.50,380,0,0,0,0;custom,cuboid,brick2,2.6,0.5,12.1,0,1,1,1,0.01,0.01,-350,2.40,380,0,0,0,0;custom,cuboid,grass4,14,0.5,12,0,1,1,1,2.0,2.0,-465,2.50,-300,0,0,0,0;custom,cuboid,brick2,14.1,0.5,12.1,0,1,1,1,0.01,0.01,-465,2.40,-300,0,0,0,0;custom,cuboid,grass4,10,0.5,8,0,1,1,1,1.0,1.0,-485,2.50,-180,0,0,0,0;custom,cuboid,brick2,10.1,0.5,8.1,0,1,1,1,0.01,0.01,-485,2.40,-180,0,0,0,0;custom,cuboid,grass4,4,0.5,4,0,1,1,1,0.5,0.5,-515,2.50,-100,0,0,0,0;custom,cuboid,brick2,4.1,0.5,4.1,0,1,1,1,0.01,0.01,-515,2.40,-100,0,0,0,0;custom,cuboid,grass4,3,0.5,4,0,1,1,1,0.5,0.5,-365,2.50,-340,0,0,0,0;custom,cuboid,brick2,3.1,0.5,4.1,0,1,1,1,0.01,0.01,-365,2.40,-340,0,0,0,0;custom,cuboid,grass4,6,0.5,30,0,1,1,1,0.75,3.0,-190,2.5,-170,0,0.414,0,1;custom,cuboid,brick2,6.1,0.5,30.1,0,1,1,1,0.01,0.01,-190,2.4,-170,0,0.414,0,1;custom,cylinder,grass4,8,0.25,8,0,1,1,1,1.0,1.0,-355,2.5,-185,0,0,0,0;custom,cylinder,brick2,8.1,0.25,8.1,0,1,1,1,0.01,0.01,-355,2.4,-185,0,0,0,0;custom,cylinder,grass4,6,0.25,6,0,1,1,1,1.0,1.0,-495,2.5,275,0,0,0,0;custom,cylinder,brick2,6.1,0.25,6.1,0,1,1,1,0.01,0.01,-495,2.4,275,0,0,0,0;custom,cylinder,grass4,7,0.25,7,0,1,1,1,1.0,1.0,-125,2.5,380,0,0,0,0;custom,cylinder,brick2,7.1,0.25,7.1,0,1,1,1,0.01,0.01,-125,2.4,380,0,0,0,0;custom,cuboid,grass4,3,0.5,3,0,1,1,1,0.5,0.5,-165,2.5,290,0,1,0,-0.414;custom,cuboid,brick2,3.1,0.5,3.1,0,1,1,1,0.01,0.01,-165,2.4,290,0,1,0,-0.414;custom,cylinder,grass,37,0.25,37,1,0.20,0.10,0,8.0,8.0,-325,2.5,90,0,0,0,0;custom,cylinder,brick2,37.1,0.25,37.1,0,1,1,1,0.01,0.01,-325,2.4,90,0,0,0,0;custom,cylinder,stone5,8,3,8,0,1,1,1,2.0,1.0,-575,80,330,0,0,0,0;custom,cylinder,stone5,8,3,8,0,1,1,1,2.0,1.0,-670,80,250,0,0,0,0;custom,cylinder,stone5,4,3,4,0,1,1,1,2.0,1.0,-622.5,80,170,0,0,0,0;custom,cylinder,stone5,6,3,6,0,1,1,1,2.0,1.0,-575,80,120,0,0,0,0;custom,cylinder,stone5,6,3,6,0,1,1,1,2.0,1.0,-670,80,60,0,0,0,0;custom,cylinder,stone5,3,3,3,0,1,1,1,2.0,1.0,-622.5,80,0,0,0,0,0;custom,cylinder,stone5,4,3,4,0,1,1,1,1.0,1.0,-575,80,-35,0,0,0,0;custom,cylinder,stone5,4,3,4,0,1,1,1,1.0,1.0,-670,80,-75,0,0,0,0;custom,cylinder,stone5,2,3,2,0,1,1,1,1.0,1.0,-622.5,80,-115,0,0,0,0;custom,cylinder,stone5,10,0.95,14,0,1,1,1,2.0,0.25,-622.5,100.5,-250,0,0,0,0;custom,cylinder,stone5,10,1.45,14,0,1,1,1,2.0,0.25,-622.5,64.5,-250,0,0,0,0;custom,cylinder,stone5,10,0.05,14,0,1,1,1,2.0,2.0,-622.5,79.5,-250,0,0,0,0;custom,cylinder,stone5,10,0.05,14,0,1,1,1,2.0,2.0,-622.5,90.5,-250,0,0,0,0;custom,cylinder,stone5,10,0.05,14,0,1,1,1,2.0,2.0,-622.5,110.5,-250,0,0,0,0;custom,cuboid,stone5,1,6,1,0,1,1,1,0.5,1.0,255,80,-385,0,1,0,1;custom,cuboid,stone5,1,6,1,0,1,1,1,0.5,1.0,55,80,-385,0,1,0,1;custom,cuboid,stone5,1,6,1,0,1,1,1,0.5,1.0,-145,80,-385,0,1,0,1;custom,cuboid,stone5,1,6,1,0,1,1,1,0.5,1.0,-345,80,-385,0,1,0,1;custom,cuboid,stone5,1,6,1,0,1,1,1,0.5,1.0,-545,80,-385,0,1,0,1;custom,cuboid,stone5,1,6,1,0,1,1,1,0.5,1.0,-645,80,-475,0,1,0,1;custom,cuboid,stone5,1,6,1,0,1,1,1,0.5,1.0,-445,80,-475,0,1,0,1;custom,cuboid,stone5,1,6,1,0,1,1,1,0.5,1.0,-245,80,-475,0,1,0,1;custom,cuboid,stone5,1,6,1,0,1,1,1,0.5,1.0,-045,80,-475,0,1,0,1;custom,cuboid,stone5,1,6,1,0,1,1,1,0.5,1.0,155,80,-475,0,1,0,1;custom,cuboid,stone5,1,8,0.9,0,1,1,1,0.25,1.0,-345,77.5,-415,0.414,1,0.414,1;custom,cuboid,stone5,1,8,0.9,0,1,1,1,0.25,1.0,55,77.5,-415,0.414,1,0.414,1;custom,cuboid,stone5,1,8,0.9,0,1,1,1,0.25,1.0,-445,77.5,-445,0.414,1,-0.414,-1;custom,cuboid,stone5,1,8,0.9,0,1,1,1,0.25,1.0,-045,77.5,-445,0.414,1,-0.414,-1;custom,cuboid,stone5,1,1,9,0,1,1,1,2.0,0.5,-545,105,-435,0,0,0,0;custom,cuboid,stone5,1,1,9,0,1,1,1,2.0,0.5,-245,95,-425,0,0,0,0;custom,cuboid,stone5,1,1,9,0,1,1,1,2.0,0.5,-145,75,-435,0,0,0,0;custom,cuboid,stone5,1,1,9,0,1,1,1,2.0,0.5,155,65,-425,0,0,0,0;custom,cuboid,stone5,1,6,1,0,1,1,1,0.5,1.0,290,80,-430,0,0,0,0;custom,cuboid,stone5,1,6,1,0,1,1,1,0.5,1.0,190,80,-440,0,0,0,0;custom,cuboid,stone5,1,6,1,0,1,1,1,0.5,1.0,190,80,-420,0,0,0,0;custom,cuboid,stone5,1,6,1,0,1,1,1,0.5,1.0,90,80,-430,0,0,0,0;custom,cuboid,stone5,1,6,1,0,1,1,1,0.5,1.0,-10,80,-450,0,0,0,0;custom,cuboid,stone5,1,6,1,0,1,1,1,0.5,1.0,-10,80,-410,0,0,0,0;custom,cuboid,stone5,1,6,1,0,1,1,1,0.5,1.0,-110,80,-430,0,0,0,0;custom,cuboid,stone5,1,6,1,0,1,1,1,0.5,1.0,-210,80,-460,0,0,0,0;custom,cuboid,stone5,1,6,1,0,1,1,1,0.5,1.0,-210,80,-400,0,0,0,0;custom,cuboid,stone5,1,6,1,0,1,1,1,0.5,1.0,-310,80,-430,0,0,0,0;custom,cylinder,stone5,1,4.5,1,0,1,1,1,0.25,1.0,272.5,30,-140,0,0,0.52,1;custom,cylinder,stone5,1,4.5,1,0,1,1,1,0.25,1.0,272.5,30,-140,0,0,-0.52,1;custom,cylinder,stone5,1,4.5,1,0,1,1,1,0.5,1.0,475,30,-85,-0.52,0,0,1;custom,cylinder,stone5,1,3.5,1,0,1,1,1,0.5,1.0,505,30,-50,0,0,0.52,1;custom,cylinder,stone5,1,8,1,0,1,1,1,0.125,2.0,555,30,215,0.5,0.5,-0.5,0.5;custom,cylinder,stone5,1,3,1,0,1,1,1,0.5,1.0,605,30,290,0,0,0,0;custom,cuboid,stone5,1.5,12,1.5,0,1,1,1,0.25,1.0,80,100,-220,0,0,0,0;custom,cuboid,stone5,1.0,15,1.0,0,1,1,1,0.25,1.0,80,50,-220,0,0,0.414,1;custom,cuboid,stone5,1.0,15,1.0,0,1,1,1,0.25,1.0,80,50,-220,0,0,-0.414,1;custom,cuboid,stone5,1.0,15,1.0,0,1,1,1,0.25,1.0,80,50,-220,0,0.414,1,0;custom,cuboid,stone5,1.0,15,1.0,0,1,1,1,0.25,1.0,80,50,-220,0,-0.414,1,0;custom,cuboid,stone5,1.5,7.15,1.49,0,1,1,1,0.5,1.0,170,30,110,0,0,0.414,1;custom,cuboid,stone5,1.5,7.15,1.51,0,1,1,1,0.5,1.0,210,30,110,0,0,-0.414,1;custom,cuboid,stone5,1.5,7.15,1.48,0,1,1,1,0.5,1.0,210,30,110,0,0,0.414,1;custom,cuboid,stone5,1.5,7.15,1.52,0,1,1,1,0.5,1.0,170,30,110,0,0,-0.414,1;custom,cuboid,stone5,5.02,5.02,5.02,0,1,1,1,1.0,1.0,-465,30,-300,0,0,0,0;custom,cuboid,stone5,4.98,4.98,4.98,0,1,1,1,1.0,1.0,-465,30,-300,0,1,0.414,0;custom,cuboid,stone5,5.01,5.01,5.01,0,1,1,1,1.0,1.0,-465,30,-300,0,0,0.414,1;custom,cuboid,stone5,4.99,4.99,4.99,0,1,1,1,1.0,1.0,-465,30,-300,0,0.414,0,1;custom,cuboid,stone5,1,5,1,0,1,1,1,0.5,2.0,-465,30,380,0,0,0,0;custom,cuboid,stone5,1.5,1.5,1.5,0,1,1,1,0.5,0.5,-465,65,380,0.414,0,-0.414,1;custom,cuboid,stone5,5,1,1,0,1,1,1,2.0,0.5,-250,10,380,0,0,0,0;custom,cuboid,stone5,4,1,1,0,1,1,1,2.0,0.5,-250,10,400,0,0,0,0;custom,cuboid,stone5,4,1,1,0,1,1,1,2.0,0.5,-250,10,360,0,0,0,0;custom,cuboid,stone5,2,1,1,0,1,1,1,1.0,0.5,-250,10,415,0,0,0,0;custom,cuboid,stone5,2,1,1,0,1,1,1,1.0,0.5,-250,10,345,0,0,0,0;custom,cuboid,stone5,1,1,0.5,0,1,1,1,0.5,0.5,-350,10,430,0,0,0.414,1;custom,cuboid,stone5,1,1,0.5,0,1,1,1,0.5,0.5,-350,10,420,0,0,0.414,1;custom,cuboid,stone5,1,1,0.5,0,1,1,1,0.5,0.5,-350,10,410,0,0,0.414,1;custom,cuboid,stone5,1,1,0.5,0,1,1,1,0.5,0.5,-350,10,400,0,0,0.414,1;custom,cuboid,stone5,1,1,0.5,0,1,1,1,0.5,0.5,-350,10,390,0,0,0.414,1;custom,cuboid,stone5,1,1,0.5,0,1,1,1,0.5,0.5,-350,10,380,0,0,0.414,1;custom,cuboid,stone5,1,1,0.5,0,1,1,1,0.5,0.5,-350,10,370,0,0,0.414,1;custom,cuboid,stone5,1,1,0.5,0,1,1,1,0.5,0.5,-350,10,360,0,0,0.414,1;custom,cuboid,stone5,1,1,0.5,0,1,1,1,0.5,0.5,-350,10,350,0,0,0.414,1;custom,cuboid,stone5,1,1,0.5,0,1,1,1,0.5,0.5,-350,10,340,0,0,0.414,1;custom,cuboid,stone5,1,1,0.5,0,1,1,1,0.5,0.5,-350,10,330,0,0,0.414,1;custom,pyramid,stone5,4,2,2,0,1,1,1,1.0,1.0,-485,20,-180,0,0,0,0;custom,torus,stone5,3,3,3,0,1,1,1,2.0,2.0,-515,5,-98.5,0,0,0,0;custom,pyramid,stone5,0.5,3,0.5,0,1,1,1,0.5,1.0,-515,30,-100,0,0,0,0;custom,cuboid,stone5,1.5,1.5,1.5,0,1,1,1,0.5,0.5,-365,12.5,-350,0,0,0,0;custom,cuboid,stone5,1.25,1.25,1.25,0,1,1,1,0.5,0.5,-365,10.0,-340,0,0,0,0;custom,cuboid,stone5,1.0,1.0,1.0,0,1,1,1,0.5,0.5,-365,7.5,-330,0,0,0,0;custom,cuboid,stone5,2,2,2,0,1,1,1,0.5,0.5,-110,15,-90,0,0.414,0,1;custom,cuboid,stone5,2,2,2,0,1,1,1,0.5,0.5,-130,15,-110,0,0,0,0;custom,cuboid,stone5,2,2,2,0,1,1,1,0.5,0.5,-150,15,-130,0,0.414,0,1;custom,cuboid,stone5,2,2,2,0,1,1,1,0.5,0.5,-170,15,-150,0,0,0,0;custom,cuboid,stone5,2,2,2,0,1,1,1,0.5,0.5,-190,15,-170,0,0.414,0,1;custom,cuboid,stone5,2,2,2,0,1,1,1,0.5,0.5,-210,15,-190,0,0,0,0;custom,cuboid,stone5,2,2,2,0,1,1,1,0.5,0.5,-230,15,-210,0,0.414,0,1;custom,cuboid,stone5,2,2,2,0,1,1,1,0.5,0.5,-250,15,-230,0,0,0,0;custom,cuboid,stone5,2,2,2,0,1,1,1,0.5,0.5,-270,15,-250,0,0.414,0,1;custom,cuboid,stone5,1,1,1,0,1,1,1,0.5,0.5,-355,10,-185,0,0.414,0,1;custom,cuboid,stone5,1,1,1,0,1,1,1,0.5,0.5,-355,15,-185,0.414,0,-0.414,1;custom,cuboid,stone5,1,1,1,0,1,1,1,0.5,0.5,-355,20,-185,0,0,0,0;custom,cuboid,stone5,1,1,1,0,1,1,1,0.5,0.5,-355,25,-185,-0.414,0,-0.414,1;custom,cuboid,stone5,1,1,1,0,1,1,1,0.5,0.5,-355,30,-185,0,0.414,0,1;custom,cuboid,stone5,1,1,1,0,1,1,1,0.5,0.5,-355,35,-185,0.414,0,-0.414,1;custom,cuboid,stone5,1,1,1,0,1,1,1,0.5,0.5,-355,40,-185,0,0,0,0;custom,cuboid,stone5,1,1,1,0,1,1,1,0.5,0.5,-355,45,-185,-0.414,0,-0.414,1;custom,cuboid,stone5,1,1,1,0,1,1,1,0.5,0.5,-355,50,-185,0,0.414,0,1;custom,cuboid,stone5,1,1,1,0,1,1,1,0.5,0.5,-355,55,-185,0.414,0,-0.414,1;custom,cuboid,stone5,1,1,1,0,1,1,1,0.5,0.5,-355,60,-185,0,0,0,0;custom,cuboid,stone5,1,1,1,0,1,1,1,0.5,0.5,-355,65,-185,-0.414,0,-0.414,1;custom,cuboid,stone5,1,1,1,0,1,1,1,0.5,0.5,-355,70,-185,0,0.414,0,1;custom,cuboid,stone5,1,1,1,0,1,1,1,0.5,0.5,-355,75,-185,0.414,0,-0.414,1;custom,cuboid,stone5,1,1,1,0,1,1,1,0.5,0.5,-355,80,-185,0,0,0,0;custom,cuboid,stone5,1,1,1,0,1,1,1,0.5,0.5,-355,85,-185,-0.414,0,-0.414,1;custom,cuboid,stone5,1.5,1.5,1.5,0,1,1,1,0.5,0.5,-465,65,380,0.414,0,-0.414,1;custom,cuboid,stone5,3.0,0.1,3.0,0,1,1,1,0.5,0.25,-495,5.5,275,0,0,0,0;custom,cuboid,stone5,2.5,0.1,2.5,0,1,1,1,0.5,0.25,-495,6.5,275,0,0.414,0,1;custom,cuboid,stone5,2.0,0.1,2.0,0,1,1,1,0.5,0.25,-495,7.5,275,0,0,0,0;custom,cuboid,stone5,1.5,0.1,1.5,0,1,1,1,0.5,0.25,-495,8.5,275,0,0.414,0,1;custom,cuboid,stone5,1.0,0.1,1.0,0,1,1,1,0.5,0.25,-495,9.5,275,0,0,0,0;custom,cuboid,stone5,0.5,0.1,0.5,0,1,1,1,0.5,0.25,-495,10.5,275,0,0.414,0,1;custom,cuboid,stone5,0.1,0.5,0.1,0,1,1,1,0.5,0.5,-495,11.5,275,0,0,0,0;custom,cuboid,stone5,0.5,7.0,0.5,0,1,1,1,0.25,2.0,-150,40,380,0,0,0,0;custom,cuboid,stone5,0.5,7.0,0.5,0,1,1,1,0.25,2.0,-100,40,380,0,0,0,0;custom,cuboid,stone5,0.5,7.0,0.5,0,1,1,1,0.25,2.0,-125,40,355,0,0,0,0;custom,cuboid,stone5,0.5,7.0,0.5,0,1,1,1,0.25,2.0,-125,40,405,0,0,0,0;custom,cylinder,stone5,8,0.25,8,0,1,1,1,1.0,1.0,-125,75,380,0,0,0,0;custom,cylinder,stone5,7,0.25,7,0,1,1,1,1.0,1.0,-125,80,380,0,0,0,0;custom,cylinder,stone5,4,0.25,4,0,1,1,1,1.0,1.0,-125,85,380,0,0,0,0;custom,cuboid,stone5,0.5,4,0.5,0,1,1,1,0.5,2.0,-165,20,290,0,1,0,-0.414;custom,cylinder,stone5,0.5,3,0.5,0,1,1,1,0.5,2.0,-165,30,290,0,1,0,-0.414;custom,cuboid,stone5,0.49,0.5,1.0,0,1,1,1,0.5,0.5,-165,10,290,0,1,0,-0.414;custom,cuboid,stone5,0.49,0.5,2.0,0,1,1,1,1.0,0.5,-165,15,290,0,1,0,-0.414;custom,cuboid,stone5,0.49,0.5,3.0,0,1,1,1,2.0,0.5,-165,20,290,0,1,0,-0.414;custom,cuboid,stone5,0.49,0.5,2.5,0,1,1,1,2.0,0.5,-165,25,290,0,1,0,-0.414;custom,cuboid,stone5,3,3,34,0,1,1,1,0.5,4.5,-150,-4,-370,0.2,0.6,0.2,0.6;custom,cuboid,stone5,2,11,1,0,1,1,1,0.5,2.0,-318,55,-365,0,1,0,-0.5;custom,cuboid,stone5,2,2,2,0,1,1,1,0.5,0.5,-15,70,200,0,0,0,0;custom,cuboid,stone5,2,2,2,0,1,1,1,0.5,0.5,15,70,300,0,0,0,0;custom,cuboid,stone5,2,3,2,0,1,1,1,0.5,0.5,-15,75,400,0,0,0,0;custom,cuboid,stone5,2,3,2,0,1,1,1,0.5,0.5,-170,75,500,0,0,0,0;custom,cuboid,stone5,2,3,2,0,1,1,1,0.5,0.5,-600,75,500,0,0,0,0;custom,cuboid,stone5,2,3,2,0,1,1,1,0.5,0.5,160,15,330,0,0,0,0;custom,cuboid,stone5,2,2,2,0,1,1,1,0.5,0.5,190,10,-100,0,1,0,0.414;custom,cuboid,stone5,0.5,5,4,0,1,1,1,0.5,0.5,-350,82.5,500,0,0,0,0;custom,cuboid,stone5,4.0,0.5,4,0,1,1,1,0.5,0.5,-350,82.5,500,0,0,0,0;custom,cylinder,stone5,0.5,2.5,0.5,0,1,1,1,0.5,1.0,-350,82.5,480,0,0,0,0;custom,cylinder,stone5,0.5,2.5,0.5,0,1,1,1,0.5,1.0,-350,82.5,520,0,0,0,0;custom,cylinder,stone5,0.5,2.0,0.5,0,1,1,1,0.5,1.0,-350,82.5,480,0,0,-1,1;custom,cylinder,stone5,0.5,2.0,0.5,0,1,1,1,0.5,1.0,-350,82.5,520,0,0,-1,1;custom,cuboid,stone5,1.5,6.0,4.0,0,1,1,1,0.5,1.0,-367.5,80,800,0,0,0,0;custom,cuboid,stone5,1.5,6.0,4.0,0,1,1,1,0.5,1.0,-332.5,80,800,0,0,0,0;custom,cuboid,stone5,1.0,7.0,4.0,0,1,1,1,0.5,1.0,-350,85,1200,0,0,0.5,1;custom,cuboid,stone5,1.0,7.0,4.0,0,1,1,1,0.5,1.0,-350,85,1200,0,0,-0.5,1;custom,cylinder,stone5,1.0,3.5,1.0,0,1,1,1,0.5,1.0,-350,85,1180,0,0,-0.5,1;custom,cylinder,stone5,1.0,3.5,1.0,0,1,1,1,0.5,1.0,-350,85,1180,0,0,0.5,1;custom,cylinder,stone5,1.0,3.5,1.0,0,1,1,1,0.5,1.0,-350,85,1220,0,0,-0.5,1;custom,cylinder,stone5,1.0,3.5,1.0,0,1,1,1,0.5,1.0,-350,85,1220,0,0,0.5,1;custom,cuboid,stone5,3.0,6.0,4.0,0,1,1,1,0.5,1.0,-350,80,1600,0,0,0,0;custom,cuboid,stone5,2.0,6,4,0,1,1,1,1.0,1.0,-365,80,2280,0,0,0,0;custom,cuboid,stone5,2.0,6,4,0,1,1,1,1.0,1.0,-335,80,2280,0,0,0,0;custom,cuboid,stone5,1.0,2.0,4,0,1,1,1,0.5,0.5,-350,70,2280,0,0,0,0;custom,cuboid,stone5,1.0,2.0,4,0,1,1,1,0.5,0.5,-350,100,2280,0,0,0,0;custom,cuboid,stone5,16.0,7.0,4,0,1,1,1,1.0,0.5,-350,35.0,2320,0,0,0,0;custom,cuboid,stone5,24.0,6.5,4,0,1,1,1,2.0,0.5,-350,32.5,2360,0,0,0,0;custom,cuboid,stone5,32.0,6.0,4,0,1,1,1,4.0,0.5,-350,30.0,2400,0,0,0,0;custom,cuboid,stone5,40.0,5.5,4,0,1,1,1,6.0,0.5,-350,27.5,2440,0,0,0,0;custom,cuboid,stone5,48.0,5.0,4,0,1,1,1,8.0,0.5,-350,25.0,2480,0,0,0,0;custom,cuboid,stone5,56.0,4.5,4,0,1,1,1,10.0,0.5,-350,22.5,2520,0,0,0,0;custom,cuboid,stone5,64.0,4.0,4,0,1,1,1,12.0,0.5,-350,20.0,2560,0,0,0,0;custom,cuboid,stone5,72.0,3.5,4,0,1,1,1,14.0,0.5,-350,17.5,2600,0,0,0,0;custom,cuboid,stone5,80.0,3.0,4,0,1,1,1,16.0,0.5,-350,15.0,2640,0,0,0,0;custom,cuboid,stone5,2,10.0,80,0,1,1,1,8.0,1.0,-40,60,2575,0,0.414,0,1;custom,cuboid,stone5,2,10.0,80,0,1,1,1,8.0,1.0,-660,60,2575,0,-0.414,0,1;custom,cuboid,stone5,2,10.0,80,0,1,1,1,8.0,1.0,-660,60,3075,0,0.414,0,1;custom,cuboid,stone5,2,10.0,80,0,1,1,1,8.0,1.0,-40,60,3075,0,-0.414,0,1;custom,cuboid,stone5,110,3,1,0,1,1,1,16.0,1.0,-350,15,2865,0,0,0,0;custom,cuboid,stone5,110,2.5,1,0,1,1,1,16.0,0.25,-350,12.5,2875,0,0,0,0;custom,cuboid,stone5,110,2.0,1,0,1,1,1,16.0,0.25,-350,10,2885,0,0,0,0;custom,cuboid,stone5,110,1.5,1,0,1,1,1,16.0,0.25,-350,7.5,2895,0,0,0,0;custom,cuboid,stone5,110,1.5,1,0,1,1,1,16.0,1.0,-350,5.0,2905,0,0,0,0;custom,cuboid,stone5,5,8,0.5,0,1,1,1,1.0,1.0,-425,45,3075,0,1,0,1;custom,cuboid,stone5,5,8,0.5,0,1,1,1,1.0,1.0,-275,45,3075,0,1,0,1;custom,cuboid,stone5,5,0.5,15.5,0,1,1,1,1.0,2.0,-350,87.5,3075,0,1,0,1;custom,cuboid,stone5,1,10,1,0,1,1,1,0.25,1.0,-425,55,3050,0,0.414,0,1;custom,cuboid,stone5,1,10,1,0,1,1,1,0.25,1.0,-275,55,3050,0,0.414,0,1;custom,cuboid,stone5,1,10,1,0,1,1,1,0.25,1.0,-425,55,3100,0,0.414,0,1;custom,cuboid,stone5,1,10,1,0,1,1,1,0.25,1.0,-275,55,3100,0,0.414,0,1;custom,cuboid,stone5,18,1,1,0,1,1,1,2.0,0.075,-350,90,3050,0.414,0,0,1;custom,cuboid,stone5,18,1,1,0,1,1,1,2.0,0.075,-350,90,3100,0.414,0,0,1;custom,cuboid,stone5,8,1,1,0,1,1,1,1.0,0.25,-425,90,3075,0.414,1,-0.414,1;custom,cuboid,stone5,8,1,1,0,1,1,1,1.0,0.25,-275,90,3075,0.414,1,-0.414,1;custom,cuboid,stone5,1,0.2,2,0,1,1,1,0.01,0.01,-425,141,3375,0,1,0,1;custom,cuboid,stone5,1,0.2,2,0,1,1,1,0.01,0.01,-425,150,3375,0,1,0,1;custom,cuboid,stone5,1,0.2,2,0,1,1,1,0.01,0.01,-425,159,3375,0,1,0,1;custom,cuboid,stone5,0.2,0.7,1,0,1,1,1,0.01,0.01,-434,145.5,3375,0,0,0,0;custom,cuboid,stone5,0.2,0.7,1,0,1,1,1,0.01,0.01,-434,154.5,3375,0,0,0,0;custom,cuboid,stone5,1,0.2,2,0,1,1,1,0.01,0.01,-395,141,3375,0,1,0,1;custom,cuboid,stone5,0.2,1.8,1,0,1,1,1,0.01,0.01,-404,151,3375,0,0,0,0;custom,cuboid,stone5,1,0.2,2,0,1,1,1,0.01,0.01,-365,141,3375,0,1,0,1;custom,cuboid,stone5,1,0.2,2,0,1,1,1,0.01,0.01,-365,159,3375,0,1,0,1;custom,cuboid,stone5,0.2,1.6,1,0,1,1,1,0.01,0.01,-365,150,3375,0,0,0,0;custom,cuboid,stone5,1,0.2,2,0,1,1,1,0.01,0.01,-335,141,3375,0,1,0,1;custom,cuboid,stone5,1,0.2,2,0,1,1,1,0.01,0.01,-335,159,3375,0,1,0,1;custom,cuboid,stone5,0.2,1.6,1,0,1,1,1,0.01,0.01,-326,150,3375,0,0,0,0;custom,cuboid,stone5,0.2,1.6,1,0,1,1,1,0.01,0.01,-344,150,3375,0,0,0,0;custom,cuboid,stone5,1,0.2,2,0,1,1,1,0.01,0.01,-305,159,3375,0,1,0,1;custom,cuboid,stone5,1,0.2,2,0,1,1,1,0.01,0.01,-305,150,3375,0,1,0,1;custom,cuboid,stone5,0.2,0.7,1,0,1,1,1,0.01,0.01,-314,154.5,3375,0,0,0,0;custom,cuboid,stone5,0.2,0.7,1,0,1,1,1,0.01,0.01,-296,154.5,3375,0,0,0,0;custom,cuboid,stone5,0.2,0.9,1,0,1,1,1,0.01,0.01,-314,144.5,3375,0,0,0,0;custom,cuboid,stone5,1,0.2,2,0,1,1,1,0.01,0.01,-275,141,3375,0,1,0,1;custom,cuboid,stone5,1,0.2,2,0,1,1,1,0.01,0.01,-275,150,3375,0,1,0,1;custom,cuboid,stone5,1,0.2,2,0,1,1,1,0.01,0.01,-275,159,3375,0,1,0,1;custom,cuboid,stone5,0.2,0.7,1,0,1,1,1,0.01,0.01,-284,145.5,3375,0,0,0,0;custom,cuboid,stone5,0.2,0.7,1,0,1,1,1,0.01,0.01,-284,154.5,3375,0,0,0,0;custom,cuboid,stone3,120,1.0,50,0,1,1,1,16.0,8.0,-350,5,3160,0,0,0,0;custom,cuboid,stone5,8,10,1,0,1,1,1,1,1,-350,60,3345,0,0,0,0;custom,cylinder,metal3,25,0.25,5,1,0.0,0.5,0.0,1.0,1.0,-350,150,3380,-1,0,0,1;custom,cylinder,stone5,26,0.25,6,0,0.75,0.75,0.75,1.0,1.0,-350,150,3381,-1,0,0,1;custom,torus,stone5,30,5,8,0,1,1,1,4.0,4.0,-355,154,3380,-1,0,0,1;custom,cuboid,stone5,9,2.5,0.5,0,1,1,1,4.0,0.25,0,82.5,-47.5,0,0,0,0;custom,cuboid,stone5,9,2.0,0.5,0,1,1,1,4.0,0.25,0,80.0,-42.5,0,0,0,0;custom,cuboid,stone5,9,1.5,0.5,0,1,1,1,4.0,0.25,0,77.5,-37.5,0,0,0,0;custom,cuboid,stone5,9,1.0,0.5,0,1,1,1,4.0,0.25,0,75.0,-32.5,0,0,0,0;custom,cuboid,stone5,9,0.5,0.5,0,1,1,1,4.0,0.25,0,72.5,-27.5,0,0,0,0;custom,cuboid,stone5,9,0.1,0.5,0,1,1,1,4.0,0.25,0,70.0,-22.5,0,0,0,0;custom,cuboid,stone5,1,6,2,0,1,1,1,0.25,1.0,0,80,60,0,0,0,0;custom,cuboid,stone5,2,0.5,2,0,1,1,1,0.25,0.25,-35,72.5,40,0,0,0,0;custom,cuboid,stone5,2,0.5,2,0,1,1,1,0.25,0.25,35,72.5,40,0,0,0,0;custom,cuboid,stone5,1,1.0,2,0,1,1,1,0.25,0.25,0,75,40,0,0,0,0;custom,cylinder,stone5,1.0,5,1.0,0,1,1,1,0.25,1.0,-315,55,3380,0,1,0,-1;custom,cylinder,stone5,1.0,5,1.0,0,1,1,1,0.25,1.0,-385,55,3380,0,1,0,-1;custom,cuboid,stone5,0.75,10,0.75,0,1,1,1,0.25,2.0,-550,55,2980,0,0,0,0;custom,cuboid,stone5,0.75,10,0.75,0,1,1,1,0.25,2.0,-550,55,2980,0,0.414,0,1;custom,cuboid,stone5,0.75,10,0.75,0,1,1,1,0.25,2.0,-150,55,2980,0,0,0,0;custom,cuboid,stone5,0.75,10,0.75,0,1,1,1,0.25,2.0,-150,55,2980,0,0.414,0,1;custom,cuboid,stone5,1.5,1.5,-1.5,0,1,1,1,0.25,0.25,-200,67.5,215,0,0.414,0,1;custom,cuboid,stone5,1.5,1.5,1.5,0,1,1,1,0.25,0.25,-188,67.5,225,0,0.414,0,1;custom,cuboid,stone5,1.5,-1.5,1.5,0,1,1,1,0.25,0.25,-179,67.5,238,0,0.414,0,1;custom,cuboid,stone5,1.5,1.5,-1.5,0,1,1,1,0.25,0.25,-195,82.5,220,0,1,0,1;custom,cuboid,stone5,1.5,1.5,-1.5,0,1,1,1,0.25,0.25,-180,82.5,235,0,0.2,0,1;custom,cuboid,stone5,1.5,1.5,-1.5,0,1,1,1,0.25,0.25,-200,97.5,215,0,0.5,0,1;custom,cuboid,stone5,1.5,1.5,1.5,0,1,1,1,0.25,0.25,-188,97.5,225,0,0.3,0,1;custom,cuboid,stone5,1.5,-1.5,1.5,0,1,1,1,0.25,0.25,-179,97.5,238,0,0.414,0,1;custom,cuboid,stone5,5,1,20,0,1,1,1,2.0,4.0,-350,40,3315,0,1,0,1;custom,cuboid,stone5,10,1,40,0,1,1,1,3.0,6.0,-350,21.2,3245.8,-0.2,1,-0.2,1;custom,cuboid,grass,5,1.1,5,1,0.5,0,0,1.0,2.0,-350,40,3315,0,1,0,1;custom,cuboid,grass,10,1.1,5,1,0.5,0,0,1.0,2.0,-350,21.2,3246,-0.2,1,-0.2,1;custom,cuboid,grass,5,1,30,1,0.5,0,0,1.0,8.0,-350,6,3060,0,0,0,0;custom,cuboid,metal1,0.5,0.25,0.5,0,1,1,1,0.01,0.01,-350,47.0,3298.0,0,0,0,0;custom,cuboid,metal1,0.8,1.25,0.25,0,1,1,1,0.01,0.01,-350,51.5,3301,0,0,0,0;custom,cuboid,metal1,0.25,0.5,0.5,0,1,1,1,0.01,0.01,-353,48.0,3297.5,0,0,0,0;custom,cuboid,metal1,0.25,0.5,0.5,0,1,1,1,0.01,0.01,-347,48.0,3297.5,0,0,0,0;custom,cuboid,metal3,0.5,0.25,0.5,0,1,1,1,0.01,0.01,-365,47.0,3303.0,0,0,0,0;custom,cuboid,metal3,0.8,1.25,0.25,0,1,1,1,0.01,0.01,-365,51.5,3306,0,0,0,0;custom,cuboid,metal3,0.25,0.5,0.5,0,1,1,1,0.01,0.01,-368,48.0,3302.5,0,0,0,0;custom,cuboid,metal3,0.25,0.5,0.5,0,1,1,1,0.01,0.01,-362,48.0,3302.5,0,0,0,0;custom,cuboid,stone7,0.5,0.25,0.5,0,1,1,1,0.01,0.01,-335,47.0,3308.0,0,0,0,0;custom,cuboid,stone7,0.8,1.25,0.25,0,1,1,1,0.01,0.01,-335,51.5,3311,0,0,0,0;custom,cuboid,stone7,0.25,0.5,0.5,0,1,1,1,0.01,0.01,-332,48.0,3307.5,0,0,0,0;custom,cuboid,stone7,0.25,0.5,0.5,0,1,1,1,0.01,0.01,-338,48.0,3307.5,0,0,0,0;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-20,85,510,-0.5,0.5,-0.5,0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-10,85,510,-0.5,0.5,-0.5,0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,0,85,510,-0.5,0.5,-0.5,0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,10,85,510,-0.5,0.5,-0.5,0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,1.0,1.0,-325,65,480,0.5,0.5,0.5,-0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,1.0,1.0,-325,75,480,0.5,0.5,0.5,-0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,1.0,1.0,-325,85,480,0.5,0.5,0.5,-0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,1.0,1.0,-325,95,480,0.5,0.5,0.5,-0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-670,85,480,0,0,1,-1;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-670,85,470,0,0,1,-1;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-670,85,460,0,0,1,-1;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-670,85,450,0,0,1,-1;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-635,85,-480,-0.5,0.5,0.5,-0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-625,85,-480,-0.5,0.5,0.5,-0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-615,85,-480,-0.5,0.5,0.5,-0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-605,85,-480,-0.5,0.5,0.5,-0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,730,50,-455,-1,1,0,0;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,730,50,-445,-1,1,0,0;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,730,50,-435,-1,1,0,0;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,730,50,-425,-1,1,0,0;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,695,50,-185,-0.5,0.5,-0.5,0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,685,50,-185,-0.5,0.5,-0.5,0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,675,50,-185,-0.5,0.5,-0.5,0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,665,50,-185,-0.5,0.5,-0.5,0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-535,25,40,-1,1,0,0;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-535,25,50,-1,1,0,0;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-535,25,60,-1,1,0,0;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-535,25,70,-1,1,0,0;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-535,25,80,-1,1,0,0;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-535,25,90,-1,1,0,0;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-535,25,100,-1,1,0,0;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-65,50,410,0,0,1,1;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-65,50,400,0,0,1,1;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-65,50,390,0,0,1,1;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-65,50,380,0,0,1,1;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-65,50,370,0,0,1,1;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-65,50,360,0,0,1,1;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-65,50,350,0,0,1,1;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-65,50,340,0,0,1,1;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-65,50,330,0,0,1,1;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-350,70,2260,1,0,0,1;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-350,100,2260,-1,0,0,1;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-335,85,2260,0.5,-0.5,0.5,-0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-365,85,2260,-0.5,0.5,0.5,-0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,240,30,-220,1,1,0,0;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,240,30,-210,1,1,0,0;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,240,30,-200,1,1,0,0;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,240,30,-190,1,1,0,0;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,240,30,-180,1,1,0,0;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,150,30,367,0,0,1,-1;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,150,30,357,0,0,1,-1;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,150,30,347,0,0,1,-1;custom,cuboid,stone5,10,5,1,0,1,1,1,1.0,1.0,0,25,-40,0,0,0,0;custom,cuboid,stone5,10,5,1,0,1,1,1,1.0,1.0,0,25,40,0,0,0,0;custom,cuboid,stone5,10,5,1,0,1,1,1,1.0,1.0,-40,25,0,0,1,0,1;custom,cuboid,stone5,10,5,1,0,1,1,1,1.0,1.0,40,25,0,0,1,0,1;custom,cuboid,stone5,10,1,10,0,1,1,1,1.0,1.0,0,-3,0,0,1,0,1;custom,prism,metal3,1,0.15,1,1,1,1,0,0.01,0.01,-365,0,-250,1,0,0.414,0;custom,prism,metal3,1,0.15,1,1,1,1,0,0.01,0.01,-375,0,-240,1,0,0.414,0;custom,prism,metal3,1,0.15,1,1,1,1,0,0.01,0.01,-385,0,-230,1,0,0.414,0;custom,prism,metal3,1,0.15,1,1,1,1,0,0.01,0.01,-395,0,-220,1,0,0.414,0;custom,prism,metal3,1,0.15,1,1,1,1,0,0.01,0.01,-405,0,-210,1,0,0.414,0;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-510,55,440,0.5,-0.5,-0.5,0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-500,55,440,0.5,-0.5,-0.5,0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-490,55,440,0.5,-0.5,-0.5,0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-480,55,440,0.5,-0.5,-0.5,0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-440,55,440,0.5,-0.5,-0.5,0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-430,55,440,0.5,-0.5,-0.5,0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-420,55,440,0.5,-0.5,-0.5,0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-410,55,440,0.5,-0.5,-0.5,0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-240,55,440,0.5,-0.5,-0.5,0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-230,55,440,0.5,-0.5,-0.5,0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-220,55,440,0.5,-0.5,-0.5,0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-210,55,440,0.5,-0.5,-0.5,0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-200,55,440,0.5,-0.5,-0.5,0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-190,55,440,0.5,-0.5,-0.5,0.5;custom,prism,metal3,1,0.1,1,1,1,1,0,0.01,0.01,-180,55,440,0.5,-0.5,-0.5,0.5;custom,prism,metal3,1,0.05,1,1,1,1,0,0.01,0.01,-350,60,280,-1,0,0,0;custom,prism,metal3,1,0.05,1,1,1,1,0,0.01,0.01,-350,60,290,-1,0,0,0;custom,prism,metal3,1,0.05,1,1,1,1,0,0.01,0.01,-350,60,300,-1,0,0,0;custom,prism,metal3,1,0.05,1,1,1,1,0,0.01,0.01,-350,60,310,-1,0,0,0;");
    }

    //Delete all of the map objects
    public void clearMap()
    {
        //Remove all deleted objects from the selection lists
        CommonReferences.objectSelection.resetSelection();
        //Reset the hash table for MapObject scripts
        objectScriptTable = new Dictionary<GameObject, MapObject>();
        //Reset the boundaries disabled flag
        boundsDisabled = false;

        //Iterate over all children objects and delete them
        foreach (Transform child in mapRoot.GetComponentInChildren<Transform>())
            GameObject.Destroy(child.gameObject);
    }

    //Parse the given map script and load the map
    public void loadMap(string mapScript)
    {
        //Remove all of the new lines in the script
        mapScript = mapScript.Replace("\n", "");
        mapScript = mapScript.Replace("\r", "");

        //Seperate the map by semicolon
        string[] parsedMap = mapScript.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

        //Create each object and add it to the map
        for (int i = 0; i < parsedMap.Length; i++)
        {
            try
            {
                //Parse the object script and create a new map object
                MapObject mapObjectScript;
                GameObject newMapObject = loadObject(parsedMap[i], out mapObjectScript);

                //If the object is defined, add it to the map hierarchy and make it selectable
                if(newMapObject)
                    addObjectToMap(newMapObject, mapObjectScript);
            }
            catch(Exception e)
            {
                //If there was an issue parsing the object, log the error and skip it
                Debug.Log("Skipping object on line " + i);
                Debug.Log(e + ":\t" + e.Message);
            }
        }
    }

    //Add the given object to the map hierarchy and make it selectable
    private void addObjectToMap(GameObject objectToAdd, MapObject objectScript)
    {
        //Make the new object a child of the map root.
        objectToAdd.transform.parent = mapRoot.transform;
        //Make the new object selectable
        CommonReferences.objectSelection.addSelectable(objectToAdd);
        //Add the object and its MapObject script to the hashtable
        objectScriptTable.Add(objectToAdd, objectScript);
    }

    //Remove the given object to the map hierarchy and make object selection script
    private void removeObjectFromMap(GameObject objectToRemove)
    {
        //Remove the object from the object selection script
        CommonReferences.objectSelection.removeSelectable(objectToRemove);
        //Remove the object from the script hashtable
        objectScriptTable.Remove(objectToRemove);
        //Delete the object itself
        Destroy(objectToRemove);
    }

    //Parse the given object script and instantiate a new GameObject with the data
    private GameObject loadObject(string objectScript, out MapObject mapObjectScript)
    {
        //Seperate the object script by comma
        string[] parsedObject = objectScript.Split(',');
        //The GameObject loaded from RCAssets corresponding to the object name
        GameObject newObject = null;
        //The type of the object
        objectType type;

        try
        {
            //If the script is "map,disableBounds" then set a flag to disable the map boundries and skip the object
            if (parsedObject[0].StartsWith("map") && parsedObject[1].StartsWith("disablebounds"))
            {
                boundsDisabled = true;
                disableMapBounds();
                mapObjectScript = null;
                return null;
            }

            //If the length of the string is too short, raise an error
            if (parsedObject.Length < 9)
                throw new Exception("Too few elements in object script");

            //Parse the object type
            type = MapObject.parseType(parsedObject[0]);

            //Use the object name to load the asset
            newObject = createMapObject(type, parsedObject[1]);
            //Get the MapObject script attached to the new GameObject
            mapObjectScript = newObject.GetComponent<MapObject>();

            //Use the parsedObject array to set the reset of the properties of the object
            mapObjectScript.loadProperties(parsedObject);

            //Check if the object is a region
            if (type == objectType.misc && parsedObject[1] == "region")
            {
                //Give the region a default rotation
                mapObjectScript.Rotation = Quaternion.identity;

                //intantiate a billboard and set it as a child of the region
                GameObject billboard = Instantiate(billboardPrefab);
                billboard.GetComponent<TextMesh>().text = mapObjectScript.RegionName;
                billboard.transform.parent = newObject.transform;
            }

            return newObject;
        }
        //If there was an error converting an element to a float, destroy the object and pass a new exception to the caller
        catch(FormatException)
        {
            destroyObject(newObject);
            throw new Exception("Error conveting data");
        }
        //If there are any other errors, destroy the object and pass them back up to the caller
        catch (Exception e)
        {
            destroyObject(newObject);
            throw e;
        }
    }

    //Convert the map into a script
    public override string ToString()
    {
        //The exported map script
        string mapScript = "";

        //If bounds are disabled, add that script to the beginning of the script
        if (boundsDisabled)
            mapScript += "map,disablebounds;\n";

        //Add the script for each object to the map script
        foreach (MapObject objectScript in objectScriptTable.Values)
            mapScript += objectScript.ToString() + "\n";

        return mapScript;
    }
    #endregion

    #region Parser Helpers
    //Check if the object exists. Then disable and destroy it
    private void destroyObject(GameObject objectToDestroy)
    {
        if (objectToDestroy)
        {
            objectToDestroy.SetActive(false);
            Destroy(objectToDestroy);
        }
    }

    //Destroy the smaller bounds around the map and isntantiate the larger bounds
    private void disableMapBounds()
    {
        //
    }

    //Load the GameObject from RCAssets with the corresponding object name and attach a MapObject script to it
    private GameObject createMapObject(objectType type, string objectName)
    {
        //The GameObject loaded from RCAssets corresponding to the object name
        GameObject newObject;

        //Instantiate the object using the object name. If the object is a vanilla object, load a substitute model
        if(type == objectType.@base)
        {
            //To-DO
            newObject = null;
        }
        //If the object is a barrier or region, change it to the editor version
        else if (objectName == "barrier" || objectName == "region")
        {
            newObject = AssetManager.instantiateRcObject(objectName + "Editor");
        }
        //Otherwise, instantiate the object regularly
        else
            newObject = AssetManager.instantiateRcObject(objectName);

        //If the object name wasn't valid, raise an error
        if (!newObject)
            throw new Exception("The object '" + objectName + "' does not exist");

        //Attatch the MapObject script to the new object
        MapObject mapObjectScript = newObject.AddComponent<MapObject>();
        //Set the type of the mapObject
        mapObjectScript.Type = type;

        //Return the new object 
        return newObject;
    }
    #endregion
}