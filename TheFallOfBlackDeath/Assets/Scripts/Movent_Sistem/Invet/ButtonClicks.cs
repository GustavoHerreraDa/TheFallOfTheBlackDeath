using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Supports inventory and interaction flow by handling button clicks.
/// </summary>
public class ButtonClicks : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private int _characterIndex;
    private CharacterTabUI _characterTabUI;

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    void Start()
    {
        _characterTabUI = transform.parent.parent.GetComponent<CharacterTabUI>();
    }

    /// <summary>
    /// Executes the on pointer click workflow.
    /// </summary>
    /// <param name="eventData">The event data.</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            _characterTabUI.MainCharacterBTN(_characterIndex);
        }
        /// <summary>
        /// Executes the if workflow.
        /// </summary>
        /// <param name="eventData.button">The event data.button.</param>
        /// <returns>The resulting value.</returns>
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            _characterTabUI.SecondaryCharacterBTN(_characterIndex);
        }   
    }
}
