using System.Collections.Generic;
using UnityEngine;

// On Chunk Prefab
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkRenderer : MonoBehaviour
{
    public const int CHUNK_WIDTH = 10, CHUNK_HEIGHT = 128;

    public ChunkData ChunkData;
    public GameWorld World;
    private Mesh _chunkMesh;


    private readonly List<Vector3> _verticesMesh = new();
    private readonly List<int> _trianglesMesh = new();

    void Start()
    {
        _chunkMesh = new();

        CreateChunkMesh();

        GetComponent<MeshFilter>().mesh = _chunkMesh;
        GetComponent<MeshCollider>().sharedMesh = _chunkMesh;
    }

    private void CreateChunkMesh()
    {
        _verticesMesh.Clear();
        _trianglesMesh.Clear();

        for (int y = 0; y < CHUNK_HEIGHT; ++y)
            for (int x = 0; x < CHUNK_WIDTH; ++x)
                for (int z = 0; z < CHUNK_WIDTH; ++z)
                    GenerateBlockAt(x, y, z);

        _chunkMesh.vertices = _verticesMesh.ToArray();
        _chunkMesh.triangles = _trianglesMesh.ToArray();

        _chunkMesh.Optimize();
        _chunkMesh.RecalculateBounds();
        _chunkMesh.RecalculateNormals();
    }

    private void GenerateBlockAt(int x, int y, int z)
    {
        Vector3Int blockPos = new(x, y, z);

        if (GetBlockAt(blockPos) == BlockType.Air) return;

        if (GetBlockAt(blockPos + Vector3Int.right).Equals(BlockType.Air))   GetRightSide(blockPos);
        if (GetBlockAt(blockPos + Vector3Int.left).Equals(BlockType.Air))    GetLeftSide(blockPos);
        if (GetBlockAt(blockPos + Vector3Int.forward).Equals(BlockType.Air)) GetFrontSide(blockPos);
        if (GetBlockAt(blockPos + Vector3Int.back).Equals(BlockType.Air))    GetBackSide(blockPos);
        if (GetBlockAt(blockPos + Vector3Int.up).Equals(BlockType.Air))      GetTopSide(blockPos);
        if (GetBlockAt(blockPos + Vector3Int.down).Equals(BlockType.Air))    GetBottomSide(blockPos);
    }

    private BlockType GetBlockAt(Vector3Int coords)
    {
        if (coords.x >= 0 && coords.x < CHUNK_WIDTH &&
            coords.y >= 0 && coords.y < CHUNK_HEIGHT &&
            coords.z >= 0 && coords.z < CHUNK_WIDTH)
        {
            return ChunkData.Blocks[coords.x, coords.y, coords.z];
        }
        else
        {
            if (coords.y < 0 || coords.y > CHUNK_HEIGHT)
                return BlockType.Air;

            Vector2Int neighboringChunkPosition = ChunkData.ChunkPosition;

            if (coords.x < 0){
                --neighboringChunkPosition.x;
                coords.x += CHUNK_WIDTH;
            }
            else if (coords.x >= CHUNK_WIDTH){
                ++neighboringChunkPosition.x;
                coords.x -= CHUNK_WIDTH;
            }

            if (coords.z < 0){
                // координаты чанка располагаются в (x,z) относительно мировых коор-т, но в структуре (x,y)
                --neighboringChunkPosition.y;
                coords.z += CHUNK_WIDTH;
            }
            else if (coords.z >= CHUNK_WIDTH){
                ++neighboringChunkPosition.y;
                coords.z -= CHUNK_WIDTH;
            }

            if (World.ChunkDatas.TryGetValue(neighboringChunkPosition, out ChunkData neighboringChunk))
                return neighboringChunk.Blocks[coords.x, coords.y, coords.z];

            return BlockType.Air;
        }
    }

    private void GetRightSide(Vector3Int blockPosition)
    {
        // (1, 0, 0)
        // (1, 1, 0)
        // (1, 0, 1)
        // (1, 1, 1)
        for (int i = 0; i < 2; ++i)
            for (int j = 0; j < 2; ++j)
                _verticesMesh.Add(new Vector3(1, j, i) + blockPosition);
        AddLast_verticesMeshSquare();
    }

    private void GetLeftSide(Vector3Int blockPosition)
    {
        // (0, 0, 0)
        // (0, 0, 1)
        // (0, 1, 0)
        // (0, 1, 1)
        for (int i = 0; i < 2; ++i)
            for (int j = 0; j < 2; ++j)
                _verticesMesh.Add(new Vector3(0, i, j) + blockPosition);

        AddLast_verticesMeshSquare();
    }

    private void GetFrontSide(Vector3Int blockPosition)
    {
        // (0, 0, 1)
        // (1, 0, 1)
        // (0, 1, 1)
        // (1, 1, 1)

        for (int i = 0; i < 2; ++i)
            for (int j = 0; j < 2; ++j)
                _verticesMesh.Add(new Vector3(j, i, 1) + blockPosition);

        AddLast_verticesMeshSquare();
    }

    private void GetBackSide(Vector3Int blockPosition)
    {
        // (0, 0, 0)
        // (0, 1, 0)
        // (1, 0, 0)
        // (1, 1, 0)

        for (int i = 0; i < 2; ++i)
            for (int j = 0; j < 2; ++j)
                _verticesMesh.Add(new Vector3(i, j, 0) + blockPosition);

        AddLast_verticesMeshSquare();
    }

    private void GetTopSide(Vector3Int blockPosition)
    {
        // (0, 1, 0)
        // (0, 1, 1)
        // (1, 1, 0)
        // (1, 1, 1)

        for (int i = 0; i < 2; ++i)
            for (int j = 0; j < 2; ++j)
                _verticesMesh.Add(new Vector3(i, 1, j) + blockPosition);

        AddLast_verticesMeshSquare();
    }

    private void GetBottomSide(Vector3Int blockPosition)
    {
        // (0, 0, 0)
        // (1, 0, 0)
        // (0, 0, 1)
        // (1, 0, 1)

        for (int i = 0; i < 2; ++i)
            for (int j = 0; j < 2; ++j)
                _verticesMesh.Add(new Vector3(j, 0, i) + blockPosition);

        AddLast_verticesMeshSquare();
    }

    private void AddLast_verticesMeshSquare()
    {
        _trianglesMesh.Add(_verticesMesh.Count - 4);
        _trianglesMesh.Add(_verticesMesh.Count - 3);
        _trianglesMesh.Add(_verticesMesh.Count - 2);

        _trianglesMesh.Add(_verticesMesh.Count - 3);
        _trianglesMesh.Add(_verticesMesh.Count - 1);
        _trianglesMesh.Add(_verticesMesh.Count - 2);
    }

    private void SetMaterial()
    {

    }
}
