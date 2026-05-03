using System;
using UnityEngine;

[Serializable]
public class AnimalData
{
    public string animalId;
    public string displayName;
    public string correctHomeId;

    [TextArea(2, 4)]
    public string wrongHomeMessage;

    [TextArea(2, 4)]
    public string correctHomeMessage;

    public GameObject cardAnimalPrefab;
}
