using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UISelector : MonoBehaviour
{
    public static List<string> options = new List<string>();
    public List<string> _options = new List<string>();
    public static GameObject canvas;
    public TMP_Dropdown selector;

    public void Awake() {
        canvas = GameObject.Find("Canvas");
        selector.onValueChanged.AddListener(SelectUIElement);

        options = _options;
    }

    void OnValidate()
    {
        options = _options;
    }

    public static void SelectUIElement(string element) {
        foreach(Transform child in canvas.transform) {
            if(!options.Contains(child.name)) continue;

            if(child.name == element) child.gameObject.SetActive(true);
            else child.gameObject.SetActive(false);
        }
    }

    public void SelectUIElement(int index) {
        Debug.Log("WHAT: " + index + "legnth: " + options.Count);
        if(options.Count == 0)
            SelectUIElement(_options[index]);
        else
            SelectUIElement(options[index]);
    }
}
