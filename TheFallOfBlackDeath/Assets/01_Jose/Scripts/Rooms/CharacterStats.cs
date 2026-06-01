using UnityEngine;

/// <summary>
/// A placeholder data structure for character statistics.
/// Used to demonstrate stat modifications from room events.
/// </summary>
[System.Serializable]
public class CharacterStats
{
    [SerializeField] public int health = 100;
    [SerializeField] public float speed = 5.0f;
    [SerializeField] public int strength = 10;
}