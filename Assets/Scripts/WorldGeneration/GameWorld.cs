using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core;
using UnityEngine;

// On GameWorld
public class GameWorld : MonoBehaviour
{
    private const int VIEW_RADIUS = 5;
    public Dictionary<Vector2Int, ChunkData> ChunkDatas = new();
    public ChunkRenderer ChunkPrefab;
    public TerrainGenerator Generator;

    private Camera _mainCamera;
    [SerializeField] private Vector2Int _currentPlayerChunk;


    void Start(){
        _mainCamera = Camera.main;

        StartCoroutine(Generate(false));
    }

    private IEnumerator Generate(bool wait){
        for (int x = _currentPlayerChunk.x - VIEW_RADIUS; x < _currentPlayerChunk.x + VIEW_RADIUS; ++x){
            for (int y = _currentPlayerChunk.y - VIEW_RADIUS; y < _currentPlayerChunk.y + VIEW_RADIUS; ++y){
                var chunkPosition = new Vector2Int(x, y);
                if (ChunkDatas.ContainsKey(chunkPosition)) continue;

                LoadChunkAt(chunkPosition);

                if (wait) yield return new WaitForSecondsRealtime(0.2f);
            }
        }
    }

    private void LoadChunkAt(Vector2Int chunkCoords){
        float xPosWorld = chunkCoords.x * ChunkRenderer.CHUNK_WIDTH;
        float zPosWorld = chunkCoords.y * ChunkRenderer.CHUNK_WIDTH;

        ChunkData chunkData = new()
        {
            ChunkPosition = chunkCoords,
            Blocks = Generator.GenerateTerrain(xPosWorld, zPosWorld)
        };
        ChunkDatas.Add(chunkCoords, chunkData);

        var chunk = Instantiate(ChunkPrefab, new Vector3(xPosWorld, 0, zPosWorld), Quaternion.identity, transform);
        chunk.ChunkData = chunkData;
        chunk.World = this;

        chunkData.Renderer = chunk;
    }

    [ContextMenu("Regenerate world")]
    public void Regenerate()
    {
        Generator.Init();
        foreach (var chunkData in ChunkDatas)
            Destroy(chunkData.Value.Renderer.gameObject);

        ChunkDatas.Clear();

        StartCoroutine(Generate(false));
    }

    private void Update() {
        Vector3Int playerWorldPos = Vector3Int.FloorToInt(_mainCamera.transform.position);
        Vector2Int playerChunk = GetChunkAt(playerWorldPos);
        if (playerChunk != _currentPlayerChunk){
            _currentPlayerChunk = playerChunk;
            StartCoroutine(Generate(true));

        }
    }

    public Vector2Int GetChunkAt(Vector3Int blockWorldPos){
        Vector2Int chunkPosition = new (blockWorldPos.x / ChunkRenderer.CHUNK_WIDTH, blockWorldPos.z / ChunkRenderer.CHUNK_WIDTH);

        if (blockWorldPos.x < 0) chunkPosition.x++;
        if (blockWorldPos.y < 0) chunkPosition.y++;
        return chunkPosition;
    }
}
