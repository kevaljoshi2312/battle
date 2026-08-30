using UnityEngine;

public class UnitSelection : MonoBehaviour
{
    [SerializeField] Color selectedColor = new Color(0.2f, 0.9f, 0.3f);

    Renderer unitRenderer;
    Color defaultColor;
    bool isSelected;

    public bool IsSelected => isSelected;

    void Awake()
    {
        unitRenderer = GetComponent<Renderer>();
    }

    void Start()
    {
        if (unitRenderer != null)
            defaultColor = unitRenderer.material.color;
    }

    public void Select()
    {
        isSelected = true;
        if (unitRenderer != null)
            unitRenderer.material.color = selectedColor;
    }

    public void Deselect()
    {
        isSelected = false;
        if (unitRenderer != null)
            unitRenderer.material.color = defaultColor;
    }
}
