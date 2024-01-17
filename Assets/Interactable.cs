using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * This script contains information about the interactable and the related ink node
 * */

public class Interactable : MonoBehaviour
{
    [Tooltip("The name of the Ink knot associated to this object")]
    public string knotName = "";

    [Tooltip("The text that appears when in range")]
    public string actionText = "Interact";

    [Tooltip("Set to true if you want the interactable to disable itself after the first interaction")]
    public bool onlyOnce = false;

    void Start()
    {

        //check if there is a collider
        Collider[] cols = transform.GetComponentsInChildren<Collider>();

        //check if there is a collider
        Collider2D[] cols2D = transform.GetComponentsInChildren<Collider2D>();


        if (cols.Length == 0 && cols2D.Length == 0)
        {
            Debug.LogWarning("Warning: the interactable " + gameObject.name + " doesn't have any colliders attached");
        }

    }


}
