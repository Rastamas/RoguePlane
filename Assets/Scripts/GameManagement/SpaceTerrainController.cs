using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

public class SpaceTerrainController : MonoBehaviour
{
    private Dictionary<LayerDistance, SpaceLayer> _spaceLayers;
    private Dictionary<LayerDistance, Dictionary<int, GameObject>> _propDictionary;
    private List<Material> _planetMaterials;
    private static readonly Vector3 TerrainBlockSizeAt40Y = new Vector3(16.2f, 0, 28.8f);
    private static readonly Dictionary<LayerDistance, int> MaxPropsPerBlock = new Dictionary<LayerDistance, int> {
        {LayerDistance.Near, 14},
        {LayerDistance.Middle, 6},
        {LayerDistance.Far, 1},
        {LayerDistance.Distant, 3},
    };
    private const int TerrainLength = 20;
    private float _mainCameraHeight;
    private GameObject _terrainParent;

    public void Awake()
    {
        _terrainParent = new GameObject("Terrain");
        _mainCameraHeight = Camera.main.transform.position.y;

        _planetMaterials = Resources.LoadAll<Material>("Prefabs/SpaceTerrain/PlanetMaterials").ToList();

        var layerDistances = EnumExtensions.GetValues<LayerDistance>();

        _spaceLayers = layerDistances.ToDictionary(distance => distance, distance => SpaceLayer.Empty(distance));

        _propDictionary = layerDistances.ToDictionary(distance => distance, distance =>
        {
            var propIndex = 0;
            return Resources.LoadAll<GameObject>(GetPropPath(distance)).ToDictionary(_ => propIndex++, gameObject => gameObject);
        });

        foreach (var layer in _spaceLayers)
        {
            AddProps(layer.Key, layer.Value);
        }
    }

    private static string GetPropPath(LayerDistance distance)
    {
        return "Prefabs/SpaceTerrain/" + distance switch
        {
            LayerDistance.Distant => LayerDistance.Far.ToString(),
            _ => distance.ToString(),
        };
    }

    private void AddProps(LayerDistance distance, SpaceLayer layer)
    {
        var layerParent = new GameObject(distance.ToString());
        layerParent.transform.parent = _terrainParent.transform;

        var propDictionary = _propDictionary[distance];

        var layerRange = (_mainCameraHeight - layer.yOffset) * TerrainBlockSizeAt40Y.x / (_mainCameraHeight - 40);


        for (float i = 0; i < TerrainLength; i++)
        {
            var groupParent = new GameObject(i.ToString());
            groupParent.transform.parent = layerParent.transform;
            var propCount = UnityRandom.Range(Math.Max(1, MaxPropsPerBlock[distance] / 2), MaxPropsPerBlock[distance] + 1);

            layer.gameObjects.AddRange(Enumerable.Range(1, propCount).Select(_ =>
            {
                var random = UnityRandom.Range(0, propDictionary.Count);

                var randomPosition = new Vector3(
                    UnityRandom.Range(-layerRange / 2, layerRange / 2),
                    layer.yOffset,
                    UnityRandom.Range(-layerRange / 2, layerRange / 2) + i * layerRange
                );

                var randomRotation = Quaternion.Euler(UnityRandom.Range(0, 360), UnityRandom.Range(0, 360), UnityRandom.Range(0, 360));

                var prop = Instantiate(propDictionary[random], randomPosition, randomRotation, groupParent.transform);

                if (distance == LayerDistance.Far || distance == LayerDistance.Distant)
                {
                    var randomPlanetMaterial = _planetMaterials[UnityRandom.Range(0, _planetMaterials.Count)];

                    prop.GetComponent<MeshRenderer>().material = randomPlanetMaterial;
                }

                prop.layer = Constants.TerrainLayer;

                return prop;
            }).ToList());
        }
    }

    private enum LayerDistance
    {
        Near,
        Middle,
        Far,
        Distant,
    }

    private class SpaceLayer
    {
        public float yOffset;
        public List<GameObject> gameObjects;

        public static SpaceLayer Empty(LayerDistance distance) => new SpaceLayer
        {
            gameObjects = new List<GameObject>(),
            yOffset = distance switch
            {
                LayerDistance.Near => -100f,
                LayerDistance.Middle => -300f,
                LayerDistance.Far => -1500f,
                LayerDistance.Distant => -5000f,
                _ => throw new NotImplementedException()
            }
        };
    }
}
