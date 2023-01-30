using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

public class TerrainController : MonoBehaviour
{
    private Dictionary<int, GameObject> _terrainBlocks;
    private Vector3 _terrainBlockSize;

    private TerrainData _terrainData;
    private Terrain _terrainTemplate;
    private Dictionary<int, TreeInstance> _treeDictionary;
    private float _terrainOffsetZ = 10;

    private const int TerrainLength = 100;
    private const int MaxTreesPerBlock = 32;

    public void Start()
    {
        _terrainTemplate = Resources.FindObjectsOfTypeAll<Terrain>()[0];
        _terrainData = _terrainTemplate.terrainData;
        _terrainBlockSize = _terrainData.size;
        _terrainBlocks = new Dictionary<int, GameObject>();
        _terrainOffsetZ = _terrainBlockSize.z / 2;

        var treeIndex = 0;
        _treeDictionary = _terrainData.treeInstances.ToDictionary(x => treeIndex++, x => x);

        for (int i = 0; i < TerrainLength; i++)
        {
            _terrainBlocks.Add(i, CreateTerrain(i));
        }
    }


    private GameObject CreateTerrain(int index)
    {
        var currentTerrainData = CopyTerrainData(_terrainData);

        if (_treeDictionary.Count > 0)
        {
            AddTrees(currentTerrainData);
        }

        var terrainGameObject = Terrain.CreateTerrainGameObject(currentTerrainData);
        terrainGameObject.transform.position = new Vector3(_terrainTemplate.terrainData.size.x / -2, 0, index * _terrainTemplate.terrainData.size.z - _terrainOffsetZ);
        var terrain = terrainGameObject.GetComponent<Terrain>();
        terrain.materialTemplate = _terrainTemplate.materialTemplate;

        if (_terrainBlocks.Count > 0)
        {
            terrain.SetNeighbors(left: null, top: null, right: null, bottom: _terrainBlocks[index - 1].GetComponent<Terrain>());
        }

        return terrainGameObject;
    }

    private void AddTrees(TerrainData currentTerrainData)
    {
        var freshTrees = Enumerable.Range(MaxTreesPerBlock / 2, MaxTreesPerBlock).Select(i =>
        {
            var random = UnityRandom.Range(0, _treeDictionary.Count);
            var tree = _treeDictionary[random];

            tree.position = new Vector3(
                UnityRandom.Range(0f, 1f),
                UnityRandom.Range(0f, 1f),
                UnityRandom.Range(0f, 1f)
            );

            return tree;
        }).ToArray();

        currentTerrainData.SetTreeInstances(freshTrees, snapToHeightmap: true);
    }

    private TerrainData CopyTerrainData(TerrainData terrainData)
    {
        return new TerrainData
        {
            wavingGrassStrength = terrainData.wavingGrassStrength,
            wavingGrassAmount = terrainData.wavingGrassAmount,
            wavingGrassSpeed = terrainData.wavingGrassSpeed,
            wavingGrassTint = terrainData.wavingGrassTint,
            enableHolesTextureCompression = terrainData.enableHolesTextureCompression,
            heightmapResolution = terrainData.heightmapResolution,
            terrainLayers = terrainData.terrainLayers,
            detailPrototypes = terrainData.detailPrototypes,
            baseMapResolution = terrainData.baseMapResolution,
            alphamapResolution = terrainData.alphamapResolution,
            treePrototypes = terrainData.treePrototypes,
            size = terrainData.size,
        };
    }

}
