using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public Configurator configurator;
    public NavigationController navigator;
    public GameObject ui;
    // public State[] states;
    
    // public void UpdateUIState(CurrentState newState) {
    //     foreach(State state in states) {
    //         if(state.state == newState) {
    //             ui.transform.Find(state.ui).gameObject.SetActive(true);
    //         } else {
    //             ui.transform.Find(state.ui).gameObject.SetActive(false);
    //         }
    //     }
    // }
}
