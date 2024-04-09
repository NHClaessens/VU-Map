using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStateManager : MonoBehaviour
{
    public void SetUIState(string newState) {
        foreach(Transform child in transform) {
            if(child.gameObject.name == newState) {
                child.gameObject.SetActive(true);
            } else {
                child.gameObject.SetActive(false);
            }
        }
    }
}
