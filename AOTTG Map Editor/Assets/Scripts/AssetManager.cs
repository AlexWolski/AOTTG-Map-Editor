﻿using UnityEngine;
using System.Collections;

public class AssetManager : MonoBehaviour
{
    //The unity3d file that contains the assets from RC mod
    private static AssetBundle RCAssets;

    //Load the RC mod assets from RCAssets.unity3d
    public static IEnumerator LoadRCAssets()
    {
        //Reference to the file. Compatible with windows and Mac.
        string url = "File:///" + Application.dataPath + "/RCAssets.unity3d";

        //Wait until caching is available
        while (!Caching.ready)
            yield return null;

        //Load the file
        using (WWW iteratorVariable2 = WWW.LoadFromCacheOrDownload(url, 1))
        {
            RCAssets = iteratorVariable2.assetBundle;

            if (!RCAssets)
            {
                Debug.Log("RC Assets Didn't Load Successfully");
                Debug.Log("RC Assets File Location: " + url);
            }
        }
    }

    //Instantiate the GameObject wtih the given name
    public static GameObject instantiateRcObject(string objectName)
    {
        //Instantiate the object
        GameObject newObject = Instantiate((GameObject)RCAssets.LoadAsset(objectName));

        //If the gameobject has a mesh, add the outline script
        if (newObject.GetComponent<Renderer>() != null)
            newObject.AddComponent<Outline>();

        //Go through the children of the object and add the outline script if it has a mesh
        foreach (Transform child in newObject.transform)
        {
            if (child.GetComponent<Renderer>() != null)
            {
                //Add the outline script to the object and give it a selectable tag
                child.gameObject.AddComponent<Outline>();
                child.gameObject.tag = "Selectable Object";
            }
        }

        //Find all mesh colliders in the object's children and alter the settings for the newer version of Unity
        foreach (MeshCollider meshCollider in newObject.GetComponentsInChildren<MeshCollider>())
        {
            meshCollider.convex = false;
            meshCollider.isTrigger = false;
        }

        //Add a Map Object tag to the 
        newObject.tag = "Map Object";

        return newObject;
    }

    //Load a material
    public static Material loadRcMaterial(string materialName)
    {
        return (Material)RCAssets.LoadAsset(materialName);
    }
}