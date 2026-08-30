using UnityEngine;

public class UnitSelection : MonoBehaviour
{
    SelectionRing selectionRing;
    bool isSelected;

    public bool IsSelected => isSelected;

    void Awake()
    {
        selectionRing = GetComponent<SelectionRing>();
    }

    public void Select()
    {
        isSelected = true;
        selectionRing?.Show();
    }

    public void Deselect()
    {
        isSelected = false;
        selectionRing?.Hide();
    }
}
