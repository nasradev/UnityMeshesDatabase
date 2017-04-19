using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR.WSA.Input;

public class PickCase : MonoBehaviour {

    // Setup on-click listener
    // if this object got selected, get its text value
    // (i.e. the Patient Case ID)
    // and pass it to RenderOBJ.CaseSelectionID.
    // Then hide the entire panel CaseSelectorPanel
    GestureRecognizer GRecognizer;
    GameObject panel;

    void Awake () {
        //GRecognizer = new GestureRecognizer();
        //GRecognizer.SetRecognizableGestures(GestureSettings.Tap);
        //GRecognizer.TappedEvent += GRecognizer_TappedEvent;	
	}

    void Start()
    {
        // consider moving this to another class that deals with 
        // the panel instantiation
        panel = GameObject.Find("CaseSelectorPanel");
    }

    //private void GRecognizer_TappedEvent(InteractionSourceKind source, int tapCount, Ray headRay)
    //{
    //    string id = gameObject.transform.GetChild(0).gameObject.GetComponent<Text>().text;
    //    RenderOBJ.CaseSelectionID = id;
    //    panel.SetActive(false);
    //    Debug.Log("Tapped");
    //}

    public void OnSelect()
    {
        string id = gameObject.GetComponent<Text>().text;
        RenderOBJ.CaseSelectionID = id;
        panel.SetActive(false);
    }

    private void OnDestroy()
    {
        //GRecognizer.TappedEvent -= GRecognizer_TappedEvent;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
