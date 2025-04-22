using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class InfoChanger : MonoBehaviour
{
    [SerializeField] private GameWorld _world;
    [SerializeField] private Text _text;
    void Update()
    {
        var position = _world.CurrentPlayerChunk;
        var biome = _world.GetBiomeGenerator().GetBiome(position);

        _text.text = string.Format("Position: {0}\nBiome: {1}", position, biome);
    }
}
