using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class SearchUIConfig : MonoBehaviour
{
    public List<Floor> floors = new List<Floor>();
    public float inputCompleteDelay;
    public VisualTreeAsset floorSelectorElement;
    public VisualTreeAsset searchResultTemplate;
    public POIController pOIController;
    public SearchController searchController;
    public CameraController cameraController;
    public NavigationController navigationController;
    public UIStateManager uiStateManager;
    public UnityEvent onUIClick = new UnityEvent();
    public UnityEvent onNonUIClick = new UnityEvent();

    private UIDocument ui;
    private ListView floorsList;
    private ListView resultList;
    private List<GameObject> searchResults = new List<GameObject>();
    private GameObject selectedPOI;
    private int selectedFloor = 0;

    private void OnEnable() {
        ui = GetComponent<UIDocument>();

        ui.rootVisualElement.Q<VisualElement>("safe-area-content-container").AddToClassList("nonUI");

        initFloorList();

        initSearchBar();

        onNonUIClick.AddListener(() => {
            hideSearchResults();
            cameraController.cancelMoveTo();
        });

        ui.rootVisualElement.RegisterCallback<ClickEvent>((evt) => {
            if(!(evt.target as VisualElement).ClassListContains("nonUI")) return;
            Debug.Log("Non UI Click");
            onNonUIClick.Invoke();
        });

        ui.rootVisualElement.Q<TextField>().RegisterCallback<ClickEvent>((evt) => {
            resultList.style.height = StyleKeyword.Auto;
            evt.StopPropagation();
        });

    }

    private void initFloorList() {
        floorsList = ui.rootVisualElement.Q<ListView>("floors");
        floorsList.itemsSource = floors;
        floorsList.makeItem = () => {
            TemplateContainer temp = floorSelectorElement.CloneTree();
            Button button = temp.Q<Button>();
            button.RegisterCallback<ClickEvent>((evt) => {
                selectFloor(button);
                evt.StopPropagation();
            });
            return temp;
        };
        floorsList.fixedItemHeight = 36;
        floorsList.Q<ScrollView>().verticalScrollerVisibility = ScrollerVisibility.Hidden;

        // Set ListView.bindItem to bind an initialized entry to a data item.
        floorsList.bindItem = (VisualElement element, int index) => {


            Button button = element.Q<Button>();
            button.text = floors[index].num.ToString();

            if(selectedFloor == floors[index].num) {
                button.style.backgroundColor = new Color(0, 0.502f, 0.788f);
                button.style.color = Color.white;
            }
            else if(floors[index].enabled) {
                button.style.backgroundColor = Color.white;
                button.style.color = Color.black;
            } else {
                button.style.backgroundColor = Color.grey;
                button.style.color = Color.black;
            }
        };

        ui.rootVisualElement.Q<Button>("floorsUp").RegisterCallback<ClickEvent>((evt) => {
            scrollToIdx(0);
            evt.StopPropagation();
        });
        ui.rootVisualElement.Q<Button>("floorsDown").RegisterCallback<ClickEvent>((evt) => {
            scrollToIdx(-1);
            evt.StopPropagation();
        });
    }

    private void initSearchBar() {
        TextField input = ui.rootVisualElement.Q<TextField>();
        ui.rootVisualElement.Q<Button>("searchBarGo").clicked += () => {
            if(selectedPOI) {
                uiStateManager.SetUIState("NavUI");
                navigationController.StartNavigation(selectedPOI.transform.position);
            }
        };

        resultList = ui.rootVisualElement.Q<ListView>("searchResults");

        resultList.itemsSource = searchResults;
        resultList.makeItem = () => {
            return searchResultTemplate.CloneTree();
        };
        resultList.bindItem = (VisualElement element, int index) => {
            element.Q<Label>("title").text = searchResults[index].GetComponent<POI>().title;
        };

        resultList.selectionChanged += (IEnumerable<object> obj) => {
            List<object> list = obj as List<object>;
            if(list.Count > 0) {
                selectedPOI = list[0] as GameObject;

                cameraController.moveTo(selectedPOI.transform.position, 1);
                VisualElement selectedPOICard = ui.rootVisualElement.Q<VisualElement>("selectedPOI");
                selectedPOICard.style.translate = new Translate(0, 0);
                selectedPOICard.Q<Label>("title").text = selectedPOI.GetComponent<POI>().title;
                input.value = selectedPOI.GetComponent<POI>().title;
                selectedPOICard.Q<Button>().RegisterCallback<ClickEvent>((evt) => {
                    navigationController.StartNavigation(selectedPOI.transform.position);
                });
                hideSearchResults();
            }
        };

        Coroutine waitForInputComplete = null;

        input.RegisterValueChangedCallback((ChangeEvent<string> e) => {
            search(e);

            if(waitForInputComplete != null) {
                StopCoroutine(waitForInputComplete);
            }
            waitForInputComplete = StartCoroutine(waitForInput());
        });
    }

    private IEnumerator waitForInput() {
        yield return new WaitForSeconds(inputCompleteDelay);
        cameraController.showAllElements(searchResults);
    }

    private void hideSearchResults() {
        resultList.style.height = 0;
    }

    private void search(ChangeEvent<string> e) {
        foreach(GameObject obj in searchResults) {
            obj.GetComponent<POI>().setVisibility(false);
        }

        string query = e.newValue;

        if(query.Length < 1) {
            resultList.Clear();
            searchResults.Clear();
            resultList.itemsSource = searchResults;
            resultList.Rebuild();
            return;
        }

        searchResults.Clear();
        searchResults = searchController.search(query, pOIController.transform);
        resultList.itemsSource = searchResults;
        resultList.Rebuild();

        foreach(GameObject obj in searchResults) {
            obj.GetComponent<POI>().setVisibility(true);
        }
        // resultList.style.height = searchResults.Count * 30;
        // sort results on distance??
    }

    private void selectFloor(Button button) {
        int floor = int.Parse(button.text);
        if(floors[floor].enabled) {
            selectedFloor = floor;
            Debug.Log("Select floor " + button.text);

            GameObject models = GameObject.Find("3D models");

            foreach(Transform child in models.transform) {
                if(child.name == "F"+button.text) {
                    child.gameObject.SetActive(true);
                } else {
                    child.gameObject.SetActive(false);
                }
            }

            floorsList.Rebuild();
        }
    }

    private void scrollToIdx(int idx) {
        if(idx < 0) idx = floors.Count - 1;

        floorsList.ScrollToItem(idx);
    }
}

[System.Serializable]
public class Floor {
    public int num;
    public bool enabled;
}
