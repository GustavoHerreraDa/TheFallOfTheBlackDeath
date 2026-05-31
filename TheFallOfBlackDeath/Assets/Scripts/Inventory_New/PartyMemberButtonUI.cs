using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace InventoryNew
{
    /// <summary>
    /// Componente interno para manejar el estado visual de cada botón de miembro.
    /// </summary>
    public class PartyMemberButtonUI : MonoBehaviour
    {
        public Image portraitImage;
        public TMP_Text nameText;
        public GameObject selectionOverlay;
        public Button button;

        public PlayerFighter Fighter { get; private set; }

        public void Setup(PlayerFighter fighter, Sprite portrait, Action onClick)
        {
            Fighter = fighter;
            if (portraitImage != null) portraitImage.sprite = portrait;
            if (nameText != null) nameText.text = fighter.idName;
            
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke());
            
            SetSelected(false);
        }

        public void SetSelected(bool isSelected)
        {
            if (selectionOverlay != null) selectionOverlay.SetActive(isSelected);
        }
    }
}
