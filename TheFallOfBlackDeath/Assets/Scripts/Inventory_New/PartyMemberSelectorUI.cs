using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;

namespace InventoryNew
{
    /// <summary>
    /// Gestiona la selección visual de miembros de la party en la interfaz de inventario y equipo.
    /// </summary>
    public class PartyMemberSelectorUI : MonoBehaviour
    {
        [Header("Configuración")]
        public GameObject buttonPrefab;
        public Transform container;
        
        [Header("Eventos")]
        public Action<PlayerFighter> OnMemberSelected;

        private List<PartyMemberButtonUI> buttons = new List<PartyMemberButtonUI>();
        private int selectedFighterIndex = -1;

        private void OnEnable()
        {
            PartyManager.OnPartyChanged += RefreshList;
            RefreshList();
        }

        private void OnDisable()
        {
            PartyManager.OnPartyChanged -= RefreshList;
        }

        /// <summary>
        /// Refresca la lista de botones basada en los miembros actuales de la party.
        /// </summary>
        public void RefreshList()
        {
            // Limpiar botones existentes
            foreach (var btn in buttons)
            {
                if (btn != null) Destroy(btn.gameObject);
            }
            buttons.Clear();

            if (GameManager.Instance == null) return;

            var members = GameManager.Instance.GetPartyMembers();
            foreach (var member in members)
            {
                if (member == null) continue;

                GameObject go = Instantiate(buttonPrefab, container);
                PartyMemberButtonUI btnUI = go.GetComponent<PartyMemberButtonUI>();
                
                Sprite portrait = GameManager.Instance.GetCharacterImage(member.figherIndex);
                btnUI.Setup(member, portrait, () => SelectMember(member));
                
                buttons.Add(btnUI);

                // Si no hay nada seleccionado, seleccionar al líder por defecto
                if (selectedFighterIndex == -1)
                {
                    selectedFighterIndex = member.figherIndex;
                    SelectMember(member);
                }
                else if (member.figherIndex == selectedFighterIndex)
                {
                    btnUI.SetSelected(true);
                }
            }
        }

        /// <summary>
        /// Selecciona un miembro y dispara el evento.
        /// </summary>
        public void SelectMember(PlayerFighter member)
        {
            selectedFighterIndex = member.figherIndex;
            
            foreach (var btn in buttons)
            {
                btn.SetSelected(btn.Fighter == member);
            }

            OnMemberSelected?.Invoke(member);
        }
    }

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
