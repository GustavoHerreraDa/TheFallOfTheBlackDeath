using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Supports exploration and world-state flow by handling game state manager.
/// </summary>
public class GameStateManager : MonoBehaviour
{
    private static GameStateManager _instante;

    public static GameStateManager Instance
    {

        get
        {
            if (_instante == null)
            
                _instante = new GameStateManager();

                return _instante;
            
        }

    } 

    public GameState.Gamestate Currentgamestate { get; private set; }

    public delegate void gamestatechangehandler(GameState.Gamestate newgamestate);
    public event gamestatechangehandler Ongamestatechanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameStateManager"/> class.
    /// </summary>
    private GameStateManager()
    {

    }

    /// <summary>
    /// Sets the state.
    /// </summary>
    /// <param name="newgamestate">The newgamestate.</param>
    public void Setstate(GameState.Gamestate newgamestate)
    {
        if (newgamestate == Currentgamestate)
            return;

        Currentgamestate = newgamestate;
        Ongamestatechanged?.Invoke(newgamestate);


    }



}

    
    




