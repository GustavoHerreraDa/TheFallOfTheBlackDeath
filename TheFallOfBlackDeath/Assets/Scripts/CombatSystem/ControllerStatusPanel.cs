using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Supports the combat system by handling status panel controller.
/// Permite multi-selección entre los miembros del equipo mientras el panel está
/// abierto, sincronizando los datos en pantalla con la cámara diegética de UI.
/// </summary>
public class StatusPanelController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private KeyCode previousKey = KeyCode.A;
    [SerializeField] private KeyCode nextKey = KeyCode.D;

    private bool isOpen = false;

    // Lista dinámica de miembros del equipo navegables.
    private readonly List<Fighter> teamMembers = new List<Fighter>();
    private int selectedIndex = -1;

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    private void Start()
    {
        BuildTeamList();
        SetPanelsActive(false);
    }

    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleStatusPanel();
        }

        if (isOpen)
        {
            HandleSelectionInput();
        }
    }

    /// <summary>
    /// Construye la lista de miembros del equipo navegables a partir del GameManager.
    /// </summary>
    private void BuildTeamList()
    {
        teamMembers.Clear();

        if (GameManager.Instance == null) return;

        List<PlayerFighter> party = GameManager.Instance.GetPartyMembers();
        if (party != null)
        {
            foreach (PlayerFighter member in party)
            {
                if (member != null)
                    teamMembers.Add(member);
            }
        }

        // Fallback a las referencias de compatibilidad si la party está vacía.
        if (teamMembers.Count == 0)
        {
            if (GameManager.Instance.character1 != null)
                teamMembers.Add(GameManager.Instance.character1);
            if (GameManager.Instance.character2 != null)
                teamMembers.Add(GameManager.Instance.character2);
        }
    }

    /// <summary>
    /// Lee el input de navegación (flechas Izquierda/Derecha o teclas A/D) y alterna
    /// entre los miembros del equipo.
    /// </summary>
    private void HandleSelectionInput()
    {
        if (teamMembers.Count <= 1) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(previousKey))
        {
            CycleSelection(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(nextKey))
        {
            CycleSelection(1);
        }
    }

    /// <summary>
    /// Desplaza el índice seleccionado de forma circular y aplica la selección.
    /// </summary>
    /// <param name="direction">-1 para anterior, +1 para siguiente.</param>
    private void CycleSelection(int direction)
    {
        if (teamMembers.Count == 0) return;

        int count = teamMembers.Count;
        selectedIndex = ((selectedIndex + direction) % count + count) % count;
        SelectFighter(selectedIndex);
    }

    /// <summary>
    /// Executes the toggle status panel workflow.
    /// </summary>
    public void ToggleStatusPanel()
    {
        isOpen = !isOpen;

        // Reconstruimos la lista por si la composición del equipo cambió.
        if (isOpen)
        {
            BuildTeamList();
        }

        if (isOpen)
        {
            // Solo activar el panel del fighter que se va a seleccionar, no todos.
            // SelectFighter se encarga de activar el panel correcto.
            if (teamMembers.Count > 0)
            {
                selectedIndex = Mathf.Clamp(selectedIndex < 0 ? 0 : selectedIndex, 0, teamMembers.Count - 1);
                SelectFighter(selectedIndex);
            }

            // Disparar efecto de escaneo
            if (CameraFXManager.Instance != null)
                CameraFXManager.Instance.SetScanEffect(true);
        }
        else
        {
            // Desactivar solo el panel actualmente visible
            if (selectedIndex >= 0 && selectedIndex < teamMembers.Count)
            {
                Fighter current = teamMembers[selectedIndex];
                if (current != null && current.statusPanel != null)
                    current.statusPanel.gameObject.SetActive(false);
            }

            // Restaurar estado de cámara previo
            if (CameraDirector.Instance != null)
                CameraDirector.Instance.ChangeState(CameraDirector.Instance.StateBeforeUi);

            // Cancelar efecto si el decay no terminó
            if (CameraFXManager.Instance != null)
                CameraFXManager.Instance.SetScanEffect(false);
        }
    }

    /// <summary>
    /// Aplica la selección de un miembro: actualiza los datos en pantalla y mueve la
    /// cámara diegética de UI hacia el monitor del personaje seleccionado.
    /// </summary>
    /// <param name="index">Índice dentro de <see cref="teamMembers"/>.</param>
    private void SelectFighter(int index)
    {
        if (index < 0 || index >= teamMembers.Count) return;

        Fighter selected = teamMembers[index];
        if (selected == null) return;

        // Desactivar el panel del fighter anteriormente seleccionado
        // (selectedIndex ya fue actualizado en CycleSelection antes de llegar aquí,
        //  por eso usamos el parámetro index y recorremos para encontrar el previo)
        foreach (Fighter member in teamMembers)
        {
            if (member != null && member != selected && member.statusPanel != null)
                member.statusPanel.gameObject.SetActive(false);
        }

        // Activar y actualizar solo el panel del fighter seleccionado
        if (selected.statusPanel != null)
        {
            selected.statusPanel.gameObject.SetActive(true);
            selected.statusPanel.SetStats(selected.idName, selected.stats);
        }

        if (CameraDirector.Instance != null)
        {
            CameraDirector.Instance.FocusDiegeticUiOn(selected);
        }
    }

    /// <summary>
    /// Sets the panels active.
    /// </summary>
    /// <param name="value">The value.</param>
    private void SetPanelsActive(bool value)
    {
        foreach (Fighter member in teamMembers)
        {
            if (member != null && member.statusPanel != null)
                member.statusPanel.gameObject.SetActive(value);
        }
    }

    /// <summary>
    /// Refreshes the all ui.
    /// </summary>
    private void RefreshAllUI()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RefreshUI();
        }

        foreach (Fighter member in teamMembers)
        {
            if (member != null && member.statusPanel != null)
                member.statusPanel.SetStats(member.idName, member.stats);
        }
    }
}
