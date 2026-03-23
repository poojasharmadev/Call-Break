using UnityEngine;
using UnityEngine.UI;

public class BidButton : MonoBehaviour
{
    public Button button;
    public Image bg;

    public Color normalColor = Color.white;
    public Color selectedColor = Color.red;

    public int bidValue;

    System.Action<int> onClick;

    public void Setup(int value, System.Action<int> callback)
    {
        bidValue = value;
        onClick = callback;

        if (button == null)
            button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);

        SetSelected(false); // reset color
    }

    void OnClick()
    {
        Debug.Log("Clicked: " + bidValue);
        onClick?.Invoke(bidValue);
    }
    
    
        
   

    public void SetSelected(bool selected)
    {
        if (bg != null)
            bg.color = selected ? selectedColor : normalColor;
        transform.localScale = selected ? Vector3.one * 1.15f : Vector3.one;
    }
}