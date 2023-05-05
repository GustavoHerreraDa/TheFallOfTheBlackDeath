using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private GameStateManager()
    {

    }

    public void Setstate(GameState.Gamestate newgamestate)
    {
        if (newgamestate == Currentgamestate)
            return;

        Currentgamestate = newgamestate;
        Ongamestatechanged?.Invoke(newgamestate);


    }



}

    
    




