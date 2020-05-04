using UnityEngine;

public interface ISelectable
{
    void OnValidate();
    void ToggleSelectionSprite(bool enabled);
    GameObject GetRootObject();
    SelectionHitBoxHandler GetHitBoxHandler();
}