using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonClicks : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private int _characterIndex;
    private CharacterTabUI _characterTabUI;

    void Start()
    {
        _characterTabUI = transform.parent.parent.GetComponent<CharacterTabUI>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            _characterTabUI.MainCharacterBTN(_characterIndex);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            _characterTabUI.SecondaryCharacterBTN(_characterIndex);
        }   
    }
}
