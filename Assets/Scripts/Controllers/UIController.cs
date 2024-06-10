using UnityEngine;

public class UIController : MonoBehaviour
{
    public static void SetUIState(string newState) {
        foreach(Transform child in GameObject.Find("UI").transform) {
            if(child.gameObject.name == newState) {
                child.gameObject.SetActive(true);
            } else {
                child.gameObject.SetActive(false);
            }
        }
    }
}