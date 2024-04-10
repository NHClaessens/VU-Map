using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class NavUIConfig : MonoBehaviour
{
    public VisualTreeAsset instructionTemplate;
    public float snapSpeed = 1;

    private VisualElement rootVisualElement;
    private ScrollView instructionList;
    private List<VisualElement> children = new List<VisualElement>();
    private int scrollIndex = 0;
    private List<Instruction> instructions = new List<Instruction>();

    private Coroutine smoothSnapCoroutine = null;

    void OnEnable() {
        NavigationController.Instance.navigationUpdated.AddListener(BuildInstructions);
        rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        rootVisualElement.Q<VisualElement>("safe-area-content-container").AddToClassList("nonUI");

        instructionList = rootVisualElement.Q<ScrollView>("instructionList");

        Coroutine snapCoroutine = null;
        instructionList.RegisterCallback<PointerMoveEvent>((evt) => {
            if(snapCoroutine != null) {
                StopCoroutine(snapCoroutine);
            }

            snapCoroutine = StartCoroutine(SnapAlign());
            // Debug.Log("Delta: " + evt.deltaPosition + " Pos: " + evt.localPosition);
            // SnapToElement(evt.localPosition);
        });

        Debug.Log("List: " + instructionList);
    }

    private void BuildInstructions(List<Instruction> instructions) {
        this.instructions = instructions;



        Debug.Log(instructions.Count);



        instructionList.Clear();
        // float maxWidth = 406;

        for(int i = 0; i < instructions.Count; i++) {
            Instruction instruction = instructions[i];
            VisualElement clone = instructionTemplate.Instantiate().Children().FirstOrDefault();

            clone.Q<Label>("title").text = instruction.toString();

            clone.Q<Label>("distance").text = DistToString(instruction.distance);
            Debug.Log(instruction.getIcon());
            clone.Q<VisualElement>("icon").style.backgroundImage = new StyleBackground(instruction.getIcon());

            if(i < instructions.Count - 1) {
                Instruction nextInstruction = instructions[i+1];
                clone.Q<VisualElement>("nextIcon").style.backgroundImage = new StyleBackground(nextInstruction.getIcon());
            } else {
                clone.Q<VisualElement>("nextInstruction").style.visibility = Visibility.Hidden;
                clone.Q<VisualElement>("currentInstruction").style.borderBottomLeftRadius = 10;
            }



            instructionList.Add(clone);
            children.Add(clone);
        }
    }

    private IEnumerator SnapAlign() {
        if(smoothSnapCoroutine != null) {
            StopCoroutine(smoothSnapCoroutine);
        }

        while(Input.touchCount > 0) {
            yield return new WaitForSeconds(0.5f);
        }


        float scrollOffset = instructionList.scrollOffset.x;
        float itemWidth = children[0].resolvedStyle.width;

        float closestDistance = float.MaxValue;
        int closestIndex = 0;
        
        for (int i = 0; i < children.Count; i++)
        {
            float itemPosition = children[i].layout.x; // X position of the item
            float distance = Mathf.Abs(scrollOffset - itemPosition);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        Debug.Log("Snap to " + closestIndex);
        
        scrollIndex = closestIndex;
        smoothSnapCoroutine = StartCoroutine(SmoothSnap(new Vector2(children[closestIndex].layout.x, 0)));
    }

    public void ScrollToInstructionOffset(int offset) {
        ScrollToInstruction(scrollIndex + offset);
    }
    public void ScrollToInstruction(int index) {
        VisualElement el = instructionList.Children().ElementAtOrDefault(scrollIndex);
        el.Q<Label>("distance").text = DistToString(instructions[scrollIndex].distance);


        index = Mathf.Clamp(index, 0, children.Count - 1);
        scrollIndex = index;
        smoothSnapCoroutine = StartCoroutine(SmoothSnap(new Vector2(children[index].layout.x, 0)));
    }
   
    private IEnumerator SmoothSnap(Vector2 destination) {
        while(instructionList.scrollOffset != destination) {
            instructionList.scrollOffset = Vector2.MoveTowards(instructionList.scrollOffset, destination, snapSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public void SetDistance(float distance) {
        VisualElement el = instructionList.Children().ElementAtOrDefault(scrollIndex);
        el.Q<Label>("distance").text = DistToString(distance);
    }

    private string DistToString(float distance) {
        return Mathf.FloorToInt(distance).ToString() + " m";
    }
}
