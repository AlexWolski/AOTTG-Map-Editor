﻿using UnityEngine;
using System.Collections.Generic;

public class ObjectSelection : MonoBehaviour
{
    //A reference to the main object
    public GameObject mainObject;
    //The shader that adds an outline to an object
    public Shader outlineShader;
    //The defailt shader
    public Shader defaultShader;
    //A reference to the editorManager
    private EditorManager editorManager;
    //A list containing the objects that can be selected
    private List<GameObject> selectableObjects = new List<GameObject>();
    //A list containing the objects currently selected
    private List<GameObject> selectedObjects = new List<GameObject>();

    //Get references from other scripts
    void Start()
    {
        editorManager = mainObject.GetComponent<EditorManager>();
    }

    //If the editor is in edit mode, check for selections
    void Update()
    {
        if (editorManager.currentMode == "edit")
            checkSelect();
    }

    //Return a reference to the seleceted objects
    public List<GameObject> getSelectedObjects()
    {
        return selectedObjects;
    }

    //Test if any objects were clicked
    private void checkSelect()
    {
        //If the mouse was clicked, check if any objects were selected
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //If an object was clicked, select it
            if (Physics.Raycast(ray, out hit, 1000.0f))
            {
                //Select the parent of the object
                GameObject parentObject = getParent(hit.transform.gameObject);

                //Only select the object if it is selectable
                if (selectableObjects.Contains(parentObject))
                {
                    //If left control is not held, deselect all objects and select the clicked object
                    if (!Input.GetKey(KeyCode.LeftControl))
                    {
                        deselectAll();

                        //Select the object that was clicked on
                        selectObject(parentObject);
                    }
                    //If left control is held, select or deselect the object based on if its currently selected
                    else
                    {
                        if (!selectedObjects.Contains(parentObject))
                        {
                            selectObject(parentObject);
                        }
                        else
                            deselectObject(parentObject);
                    }
                }
            }
        }

        //If the 'A' key was pressed, either select or deselect all based on if anything is currently selected
        else if(Input.GetKeyDown(KeyCode.A))
        {
            if (selectedObjects.Count > 0)
                deselectAll();
            else
                selectAll();
        }
    }

    private GameObject getParent(GameObject childObject)
    {
        GameObject parentObject = childObject;

        while (parentObject.transform.parent != null)
            parentObject = childObject.transform.parent.gameObject;

        return parentObject;
    }

    //Add an object to the list of selectable objects
    public void addSelectable(GameObject objectToAdd)
    {
        selectableObjects.Add(getParent(objectToAdd));
    }

    public void selectObject(GameObject objectToSelect)
    {
        //Get the parent of the object
        GameObject parentObject = getParent(objectToSelect);

        //Select the object
        selectedObjects.Add(parentObject);
        addOutline(parentObject);
    }

    public void selectAll()
    {
        //Select all objects by copying the selectedObjects list
        selectedObjects = new List<GameObject>(selectableObjects);

        //Add the outline to all of the objects
        foreach (GameObject selectedObject in selectedObjects)
            addOutline(selectedObject);
    }

    public void deselectObject(GameObject objectToDeselect)
    {
        //Get the parent of the object
        GameObject parentObject = getParent(objectToDeselect);

        //Deselect the object
        selectedObjects.Remove(parentObject);
        removeOutline(parentObject);
    }

    public void deselectAll()
    {
        //Remove the outline on all selected objects
        foreach (GameObject selectedObject in selectedObjects)
            removeOutline(selectedObject);

        //Deselect all objects by deleting the selected objects list
        selectedObjects = new List<GameObject>();
    }

    //Add a green outline around a GameObject
    private void addOutline(GameObject objectToAddOutline)
    {
        //Iterate through all of the children in the GameObject and apply a green outline
        foreach (MeshRenderer renderer in objectToAddOutline.GetComponentsInChildren<MeshRenderer>())
        {
            renderer.material.shader = outlineShader;
            renderer.sharedMaterial.SetColor("_OutlineColor", Color.green);
        }
    }

    //Remove the green outline shader
    private void removeOutline(GameObject objectToRemoveOutline)
    {
        foreach (MeshRenderer renderer in objectToRemoveOutline.GetComponentsInChildren<MeshRenderer>())
            renderer.material.shader = defaultShader;
    }
}