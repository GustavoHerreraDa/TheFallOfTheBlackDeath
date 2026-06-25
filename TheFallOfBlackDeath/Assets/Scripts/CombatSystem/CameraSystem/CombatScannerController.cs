using UnityEngine;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Maneja el Input (Navegación, Salida) y la UI de ayuda
/// cuando el CameraDirector está en estado Scanner.
/// </summary>
public class CombatScannerController : MonoBehaviour
{
    public static CombatScannerController Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI instructionText;

    private int _currentIndex = 0;
    private Fighter[] _enemies;
    private bool _isActive;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Inicia el modo de navegación del scanner con una lista de enemigos.
    /// </summary>
    public void Activate(Fighter[] enemies)
    {
        _enemies = enemies;
        _currentIndex = 0;
        _isActive = true;

        if (instructionText != null)
        {
            instructionText.text = "Click derecho para regresar";
            instructionText.gameObject.SetActive(true);
        }
        
        // Enfocamos el primero inmediatamente
        FocusCurrent();
    }

    public void Deactivate()
    {
        _isActive = false;
        _enemies = null;

        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!_isActive || CameraDirector.Instance.CurrentState != CameraState.Scanner)
        {
            if (_isActive) Deactivate();
            return;
        }

        // Si se presiona Tab, salimos del modo scanner para permitir que abra el panel de estado
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ExitScanner();
            return;
        }

        HandleNavigationInput();
        HandleExitInput();
    }

    private void HandleNavigationInput()
    {
        if (_enemies == null || _enemies.Length == 0) return;

        if (Input.GetKeyDown(KeyCode.D))
        {
            Navigate(1);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            Navigate(-1);
        }
    }

    private void HandleExitInput()
    {
        // Salida únicamente con Click Derecho
        if (Input.GetMouseButtonDown(1))
        {
            ExitScanner();
        }
    }

    private void Navigate(int direction)
    {
        if (_enemies == null || _enemies.Length == 0) return;

        int startIdx = _currentIndex;
        do
        {
            _currentIndex = (_currentIndex + direction + _enemies.Length) % _enemies.Length;
            
            // Si el enemigo está vivo, lo enfocamos
            if (_enemies[_currentIndex] != null && _enemies[_currentIndex].isAlive)
            {
                FocusCurrent();
                return;
            }
        } while (_currentIndex != startIdx);
    }

    private void FocusCurrent()
    {
        if (_enemies == null || _currentIndex < 0 || _currentIndex >= _enemies.Length) return;
        
        Fighter target = _enemies[_currentIndex];
        if (target != null && target.isAlive)
        {
            CameraDirector.Instance.FocusScannerOnTarget(target);
        }
    }

    private void ExitScanner()
    {
        if (CombatScannerSystem.Instance != null)
        {
            // ToggleScanner apagará todo limpiamente
            CombatScannerSystem.Instance.ToggleScanner();
        }
        else
        {
            // Fallback directo si por alguna razón no hay sistema de scanner
            CameraDirector.Instance.ChangeState(CameraState.Overview);
        }
    }
}
