﻿using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using OutlineEffect;

namespace MapEditor
{
    //A singleton class for managing the currently selected objects
    public class ObjectSelection : MonoBehaviour
    {
        #region Data Members
        //A self-reference to the singleton instance of this script
        public static ObjectSelection Instance { get; private set; }
        private Camera mainCamera;

        //A hash set containing the objects that can be selected
        private HashSet<GameObject> selectableObjects = new HashSet<GameObject>();
        //A hash set containing the objects currently selected
        private HashSet<GameObject> selectedObjects = new HashSet<GameObject>();
        //The average point of all the selected objects
        private Vector3 selectionAverage;
        //The sum of the points of all the selected objects for calculating the average
        private Vector3 positionSum;
        #endregion

        #region Instantiation
        void Awake()
        {
            //Set this script as the only instance of the ObjectSelection script
            if (Instance == null)
                Instance = this;
        }

        private void Start()
        {
            //Find and store the main camrea in the scene
            mainCamera = Camera.main;

            //Add listners to events in the SelectionHandle class
            SelectionHandle.Instance.OnHandleFinish += endSelection;
            SelectionHandle.Instance.OnHandleMove += editSelection;
        }
        #endregion

        #region Selection Edit Commands
        //Add an object to the current selection
        private class SelectAdditive : EditCommand
        {
            private GameObject selectedObject;

            public SelectAdditive(GameObject selectedObject)
            {
                this.selectedObject = selectedObject;
            }

            public override void executeEdit() { Instance.selectObject(selectedObject); }
            public override void revertEdit() { Instance.deselectObject(selectedObject); }
        }

        //Deselect the current selection and select a single object
        private class SelectReplace : EditCommand
        {
            private GameObject selectedObject;
            private GameObject[] previousSelection;

            public SelectReplace(GameObject selectedObject)
            {
                this.selectedObject = selectedObject;
                previousSelection = Instance.selectedObjects.ToArray();
            }

            //Deselect all map objects and select the new object
            public override void executeEdit()
            {
                Instance.deselectAll();
                Instance.selectObject(selectedObject);
            }

            //Deselect the new object and re-select the objects that were previously selected
            public override void revertEdit()
            {
                Instance.deselectObject(selectedObject);

                foreach (GameObject mapObject in previousSelection)
                    Instance.selectObject(mapObject);
            }
        }

        private class SelectAll : EditCommand
        {
            //The objects that were previously unselected
            private GameObject[] unselectedObjects;

            public SelectAll()
            {
                //Find the unselected objects by excluding the selected objects from a set of all objects
                unselectedObjects = Instance.selectableObjects.ExcludeToArray(Instance.selectedObjects);
            }

            //Select the objects that aren't selected
            public override void executeEdit()
            {
                foreach (GameObject mapObject in unselectedObjects)
                    Instance.selectObject(mapObject);
            }

            //Deselect the objects that weren't previously selected
            public override void revertEdit()
            {
                foreach (GameObject mapObject in unselectedObjects)
                    Instance.deselectObject(mapObject);
            }
        }

        private class DeselectObject : EditCommand
        {
            private GameObject deselectedObject;

            public DeselectObject(GameObject deselectedObject)
            {
                this.deselectedObject = deselectedObject;
            }

            public override void executeEdit() { Instance.deselectObject(deselectedObject); }
            public override void revertEdit() { Instance.selectObject(deselectedObject); }
        }

        private class DeselectAll : EditCommand
        {
            private GameObject[] previousSelection;

            public DeselectAll()
            {
                previousSelection = new GameObject[Instance.selectedObjects.Count];
                Instance.selectedObjects.CopyTo(previousSelection);
            }

            //Deselect all objects
            public override void executeEdit()
            {
                Instance.deselectAll();
            }

            //Select all of the previously selected objects
            public override void revertEdit()
            {
                foreach (GameObject mapObject in previousSelection)
                    Instance.selectObject(mapObject);
            }
        }

        private class InvertSelection : EditCommand
        {
            public override void executeEdit() { Instance.invertSelection(); }
            public override void revertEdit() { Instance.invertSelection(); }
        }
        #endregion

        #region Transform Edit Commands
        private class TranslateSelection : EditCommand
        {
            private Vector3 displacement;
            private Vector3 negativeDisplacement;

            public TranslateSelection(Vector3 posDisplacement)
            {
                //Save the displacement
                this.displacement = posDisplacement;

                //Negate the displacement and store it
                negativeDisplacement = new Vector3();

                for (int axis = 0; axis < 3; axis++)
                    negativeDisplacement[axis] = -displacement[axis];
            }

            public override void executeEdit()
            {
                TransformTools.TranslateSelection(Instance.selectedObjects, displacement);

                //Update the selection average
                Instance.translateSelectionAverage(displacement);
                SelectionHandle.Instance.Position = Instance.selectionAverage;
            }

            public override void revertEdit()
            {
                TransformTools.TranslateSelection(Instance.selectedObjects, negativeDisplacement);

                //Update the selection average
                Instance.translateSelectionAverage(negativeDisplacement);
                SelectionHandle.Instance.Position = Instance.selectionAverage;
            }
        }

        private class RotateSelection : EditCommand
        {
            private Quaternion startRotation;
            private Quaternion endRotation;

            private float angleDisplacement;
            private Vector3 rotationAxis;

            public RotateSelection(Quaternion originalRotation, Quaternion currentRotation)
            {
                startRotation = originalRotation;
                endRotation = currentRotation;

                //Calculate the displacement
                Quaternion rotDisplacement = Quaternion.Inverse(originalRotation) * currentRotation;
                //Store the angle and axis of rotation
                rotDisplacement.ToAngleAxis(out angleDisplacement, out rotationAxis);

                //Multiply the rotation axis by the original rotation to get the axis relative to the handle
                rotationAxis = originalRotation * rotationAxis;
            }

            public override void executeEdit()
            {
                TransformTools.RotateSelection(Instance.selectedObjects, Instance.selectionAverage, rotationAxis, angleDisplacement);
                SelectionHandle.Instance.Rotation = endRotation;
            }

            public override void revertEdit()
            {
                TransformTools.RotateSelection(Instance.selectedObjects, Instance.selectionAverage, rotationAxis, -angleDisplacement);
                SelectionHandle.Instance.Rotation = startRotation;
            }
        }

        private class ScaleSelection : EditCommand
        {
            private Vector3 scaleUpFactor;
            private Vector3 scaleDownFactor;

            public ScaleSelection(Vector3 scaleUpFactor)
            {
                this.scaleUpFactor = scaleUpFactor;

                //Calculate the scale down factor
                scaleDownFactor = new Vector3();

                for (int axis = 0; axis < 3; axis++)
                    scaleDownFactor[axis] = 1f / scaleUpFactor[axis];
            }

            public override void executeEdit()
            {
                TransformTools.ScaleSelection(Instance.selectedObjects, Instance.selectionAverage, scaleUpFactor, false);
            }

            public override void revertEdit()
            {
                TransformTools.ScaleSelection(Instance.selectedObjects, Instance.selectionAverage, scaleDownFactor, false);
            }
        }
        #endregion

        #region Update Selection Methods
        private void Update()
        {
            //Check for an object selection if in edit mode and nothing is being dragged
            if (EditorManager.Instance.currentMode == EditorMode.Edit &&
                EditorManager.Instance.shortcutsEnabled &&
                EditorManager.Instance.cursorAvailable)
                checkSelect();
        }

        //Test if any objects were clicked
        private void checkSelect()
        {
            //Stores the command that needs to be executed
            EditCommand selectionCommand = null;

            //If the left control key is held, check for shortcuts
            if (Input.GetKey(KeyCode.LeftControl))
            {
                //If 'control + A' is pressed, either select or deselect all based on if anything is currently selected
                if (Input.GetKeyDown(KeyCode.A))
                {
                    if (selectedObjects.Count > 0)
                        selectionCommand = new DeselectAll();
                    else
                        selectionCommand = new SelectAll();
                }
                //If 'control + I' is pressed, invert the current selection
                if (Input.GetKeyDown(KeyCode.I))
                    selectionCommand = new InvertSelection();
            }

            //If the mouse was clicked and the cursor is not over the UI, check if any objects were selected
            if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject(-1))
            {
                RaycastHit hit;
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                bool rayHit = Physics.Raycast(ray, out hit, Mathf.Infinity);

                //Check if nothing was hit or the hit object isn't selectable
                if(!rayHit || hit.transform.gameObject.tag != "Selectable")
                {
                    //If not in additive mode and there is a selection, deselect all
                    if(!Input.GetKey(KeyCode.LeftControl) && Instance.selectedObjects.Count > 0)
                        selectionCommand = new DeselectAll();
                }
                //If an object was clicked, select it
                else
                {
                    //Select the parent of the object
                    GameObject parentObject = getParent(hit.transform.gameObject);

                    //If left control is not held, deselect all objects and select the clicked object
                    if (!Input.GetKey(KeyCode.LeftControl))
                    {
                        //If the clicked object is already the only selected object, skip it
                        if (Instance.selectedObjects.Count == 1 &&
                            Instance.selectedObjects.Contains(parentObject))
                            return;

                        selectionCommand = new SelectReplace(parentObject);
                    }
                    //If left control is held, select or deselect the object based on if its currently selected
                    else
                    {
                        if (!selectedObjects.Contains(parentObject))
                            selectionCommand = new SelectAdditive(parentObject);
                        else
                            selectionCommand = new DeselectObject(parentObject);
                    }
                }
            }

            //If a selection was made, execute its associated command and add it to the history
            if (selectionCommand != null)
            {
                selectionCommand.executeEdit();
                EditHistory.Instance.addCommand(selectionCommand);
            }
        }
        #endregion

        #region Event Handler Methods
        //When the handle is released, create a command 
        private void endSelection()
        {
            //The command to be executed
            EditCommand transformCommand = null;

            switch (SelectionHandle.Instance.currentTool)
            {
                case Tool.Translate:
                    //Get the translation displacement from the selection handle
                    Vector3 posDisplacement = SelectionHandle.Instance.Position -
                                           SelectionHandle.Instance.getStartPosition();
                    
                    //If the handle wasn't moved, don't create a command
                    if(posDisplacement.magnitude == 0)
                        return;

                    //Otherwise, create a translate command
                    transformCommand = new TranslateSelection(posDisplacement);
                    break;

                case Tool.Rotate:
                    //Get the previous and current rotation of the selection handle
                    Quaternion originalRotation = SelectionHandle.Instance.getStartRotation();
                    Quaternion currentRotation = SelectionHandle.Instance.Rotation;

                    //If the handle wasn't rotated, dont create a command
                    if (originalRotation == currentRotation)
                        return;

                    //Otherwise, create a rotation command
                    transformCommand = new RotateSelection(originalRotation, currentRotation);
                    break;

                default:
                    //Get the current scale of the tool handle
                    Vector3 scaleFactor = SelectionHandle.Instance.getEndScale();

                    //If the selection wasn't scaled, don't create a command
                    if (scaleFactor == Vector3.one)
                        return;

                    //Otherwise, create a scale command
                    transformCommand = new ScaleSelection(scaleFactor);
                    break;
            }

            if (transformCommand != null)
                EditHistory.Instance.addCommand(transformCommand);
        }

        //Update the position, rotation, or scale of the object selections based on the tool handle
        private void editSelection()
        {
            //Determine which tool was used and call the respective transform
            switch (SelectionHandle.Instance.currentTool)
            {
                case Tool.Translate:
                    //Get the position displacement and translate the selected objects
                    Vector3 posDisplacement = SelectionHandle.Instance.getPosDisplacement();
                    TransformTools.TranslateSelection(Instance.selectedObjects, posDisplacement);

                    //Update the selection average
                    translateSelectionAverage(posDisplacement);
                    break;

                case Tool.Rotate:
                    //Get the angle and axis and to rotate around
                    Vector3 rotationAxis;
                    float angle = SelectionHandle.Instance.getRotDisplacement(out rotationAxis);

                    //Rotate the selected objects around the seleciton average
                    TransformTools.RotateSelection(Instance.selectedObjects, selectionAverage, rotationAxis, angle);
                    break;

                case Tool.Scale:
                    //Get the scale displacement and scale the selected objects
                    Vector3 scaleDisplacement = SelectionHandle.Instance.getScaleDisplacement();
                    TransformTools.ScaleSelection(Instance.selectedObjects, selectionAverage, scaleDisplacement, false);
                    break;
            }
        }
        #endregion

        #region Selection Average Methods
        //Add a point to the total average
        private void addAveragePoint(Vector3 point)
        {
            //Add the point to the total and update the average
            positionSum += point;
            selectionAverage = positionSum / selectedObjects.Count;
            SelectionHandle.Instance.Position = selectionAverage;

            //If the tool handle is not active, activate it
            SelectionHandle.Instance.show();
        }

        //Add all selected objects to the total average
        private void addAverageAll()
        {
            //Reset the total
            positionSum = Vector3.zero;

            //Count up the total of all the objects
            foreach (GameObject mapObject in selectedObjects)
                positionSum += mapObject.transform.position;

            //Average the points
            selectionAverage = positionSum / selectableObjects.Count;
            SelectionHandle.Instance.Position = selectionAverage;

            //If the tool handle is not active, activate it
            SelectionHandle.Instance.show();
        }

        //Remove a point from the total average
        private void removeAveragePoint(Vector3 point)
        {
            //Subtract the point to the total and update the average
            positionSum -= point;

            //If there are any objects selected, update the handle position
            if (selectedObjects.Count != 0)
            {
                selectionAverage = positionSum / selectedObjects.Count;
                SelectionHandle.Instance.Position = selectionAverage;
            }
            //Otherwise, disable the tool handle
            else
                SelectionHandle.Instance.hide();
        }

        //Remove all selected objects from the total average
        private void removeAverageAll()
        {
            //Reset the total and average
            positionSum = Vector3.zero;
            selectionAverage = Vector3.zero;

            //Hide the tool handle
            SelectionHandle.Instance.hide();
        }

        //Updates the average when the whole selection is translated by the same amount
        private void translateSelectionAverage(Vector3 displacement)
        {
            positionSum += displacement * Instance.selectedObjects.Count;
            selectionAverage += displacement;
        }
        #endregion

        #region Select Objects Methods
        //Return the parent of the given object. If there is no parent, return the given object
        private GameObject getParent(GameObject childObject)
        {
            //The tag of the parent object
            string parentTag = childObject.transform.parent.gameObject.tag;

            //If the parent isn't a map object, return the child
            if (parentTag == "Map" || parentTag == "Group")
                return childObject;

            //Otherwise return the parent of the child
            return childObject.transform.parent.gameObject;
        }

        //Add the given object to the selectable objects list
        public void addSelectable(GameObject objectToAdd)
        {
            selectableObjects.Add(getParent(objectToAdd));
        }

        //Remove the given object from both the selectable and selected objects lists
        public void removeSelectable(GameObject objectToRemove)
        {
            //Deselect the object
            deselectObject(objectToRemove);
            //Remove the object from the selectable objects list
            selectableObjects.Remove(getParent(objectToRemove));
        }

        //Remove any selected objects from both the selected and selectable objects lists
        public HashSet<GameObject> removeSelected()
        {
            //Remove the selected objects from the selectable set
            selectableObjects.ExceptWith(Instance.selectedObjects);

            //Clone the selected objects set
            HashSet<GameObject> originalSelection = new HashSet<GameObject>(Instance.selectedObjects);

            //Deselect each object
            foreach (GameObject mapObject in originalSelection)
                deselectObject(mapObject);

            //Reset the selection average
            removeAverageAll();

            //Return a reference to the selected objects list
            return originalSelection;
        }

        public void selectObject(GameObject objectToSelect)
        {
            //Get the parent of the object
            GameObject parentObject = getParent(objectToSelect);

            //If the object is arleady selected, skip it
            if (selectedObjects.Contains(parentObject))
                return;

            //Select the object
            selectedObjects.Add(parentObject);
            addOutline(parentObject);

            //Update the position of the tool handle
            addAveragePoint(parentObject.transform.position);
            //Reset the rotation on the tool handle
            resetToolHandleRotation();
        }

        public void selectAll()
        {
            //Select all objects by copying the selectedObjects list
            selectedObjects = new HashSet<GameObject>(selectableObjects);

            //Add the outline to all of the objects
            foreach (GameObject selectedObject in selectedObjects)
                addOutline(selectedObject);

            //Update the tool handle position
            addAverageAll();
            //Reset the rotation on the tool handle
            resetToolHandleRotation();
        }

        public void deselectObject(GameObject objectToDeselect)
        {
            //Get the parent of the object
            GameObject parentObject = getParent(objectToDeselect);

            //If the object isn't selected, skip it
            if (!selectedObjects.Contains(parentObject))
                return;

            //Deselect the object
            selectedObjects.Remove(parentObject);
            removeOutline(parentObject);

            //Update the position of the tool handle
            removeAveragePoint(parentObject.transform.position);
            //Reset the rotation on the tool handle
            resetToolHandleRotation();
        }

        public void deselectAll()
        {
            //If there are no selected objects, return from the function
            if (selectedObjects.Count == 0)
                return;

            //Remove the outline on all selected objects
            foreach (GameObject selectedObject in selectedObjects)
                removeOutline(selectedObject);

            //Deselect all objects by deleting the selected objects list
            selectedObjects.Clear();

            //Update the position of the tool handle
            removeAverageAll();
            //Reset the rotation on the tool handle
            resetToolHandleRotation();
        }

        //Deselect the current seleciton and select all other objects
        public void invertSelection()
        {
            //Iterate over all selectable map objects
            foreach (GameObject mapObject in selectableObjects)
                invertObject(mapObject);
        }

        //Invert the selection on the given object
        public void invertObject(GameObject mapObject)
        {
            //If the map object is already selected, deselect it
            if (selectedObjects.Contains(mapObject))
                deselectObject(mapObject);
            //Otherwise, select it
            else
                selectObject(mapObject);
        }

        //Resets both the selected and selectable object lists
        public void resetSelection()
        {
            selectedObjects.Clear();
            selectableObjects.Clear();
            removeAverageAll();
        }
        #endregion

        #region Selection Getters
        public HashSet<GameObject> getSelection()
        {
            return Instance.selectedObjects;
        }

        public int getSelectionCount()
        {
            return Instance.selectedObjects.Count;
        }

        public HashSet<GameObject> getSelectable()
        {
            return Instance.selectableObjects;
        }
        #endregion

        #region Tool Methods
        //Set the type of the tool handle
        public void setTool(Tool toolType)
        {
            SelectionHandle.Instance.setTool(toolType);
            resetToolHandleRotation();
        }

        //Set the rotation of the tool handle based on how many objects are selected
        public void resetToolHandleRotation()
        {
            //If the tool handle is in rotate mode and only one object is selected, use the rotation of that object
            if ((SelectionHandle.Instance.currentTool == Tool.Rotate || SelectionHandle.Instance.currentTool == Tool.Scale) && selectedObjects.Count == 1)
            {
                GameObject[] selectedArray = new GameObject[1];
                selectedObjects.CopyTo(selectedArray, 0);
                SelectionHandle.Instance.Rotation = selectedArray[0].transform.rotation;
            }
            //Otherwise reset the rotation
            else
                SelectionHandle.Instance.Rotation = Quaternion.identity;
        }
        #endregion

        #region Outline Methods
        //Add a green outline around a GameObject
        private void addOutline(GameObject objectToAddOutline)
        {
            //If parent has an outline script, enable it
            if (objectToAddOutline.tag == "Selectable")
                objectToAddOutline.GetComponent<Outline>().enabled = true;

            //Go through the children of the object and enable the outline if it is a selectable object
            foreach (Transform child in objectToAddOutline.transform)
                if (child.gameObject.tag == "Selectable")
                    child.GetComponent<Outline>().enabled = true;
        }

        //Remove the green outline shader
        private void removeOutline(GameObject objectToRemoveOutline)
        {
            //Get the outline script of the parent object
            Outline outlineScript = objectToRemoveOutline.GetComponent<Outline>();

            //If parent has an outline script, disable it
            if (outlineScript != null)
                outlineScript.enabled = false;

            //Go through the children of the object and disable the outline if it is a selectable object
            foreach (Transform child in objectToRemoveOutline.transform)
                if (child.gameObject.tag == "Selectable")
                    child.GetComponent<Outline>().enabled = false;
        }
        #endregion
    }
}