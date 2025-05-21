using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Biome
{
    public Temperature Temperature { get; set;}
    public BiomeType Height { get; set;}
    public BlockType SurfaceBlock { get; set; }

    public override string ToString()
    {
        return $"{Height} {Temperature}";
    }
}
