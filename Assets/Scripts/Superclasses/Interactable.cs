using UnityEngine;

public class Interactable : MonoBehaviour
{
    //Start and Update methods can be used by subclasses, so we don't need them here

    public virtual void OnHoverEnter()
    {
        //When the player is hovering over the object

    }

    public virtual void OnHoverExit()
    {
        //When the player stops hovering over the object
    }

    public virtual void Interact()
    {
        //When the player has clicked on the object
    }
}
