using UnityEngine;

public class ChunkData
{
    public BlockType[,,] Blocks; //= new BlockType[ChunkWidth, ChunkHeight, ChunkWidth];
    public ChunkRenderer Renderer;

    public Vector2Int ChunkPosition { get; set; }
}
