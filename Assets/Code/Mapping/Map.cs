using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;
using Utilities;
using Eventing;

namespace Mapping
{
    #region Enums

    public enum Maps
    {
        None = 0,
        TestMap1 = 1
    }

    public enum MapType
    {
        None = 0,
        Outdoors = 1
    }

    #endregion


    #region Structs

    [Serializable]
    public struct NeighborTilemaps
    {
        public Tilemap ground;
        public Tilemap layer_up;
        public Tilemap layer_down;

        public Tilemap objects;
        public Tilemap objects_up;
        public Tilemap objects_down;
    }

    [Serializable]
    public struct NeighborTiles
    {
        public ParallaxTileBase on_tile;
        public ParallaxTileBase up_tile;
        public ParallaxTileBase left_tile;
        public ParallaxTileBase right_tile;
        public ParallaxTileBase down_tile;

        public ParallaxTileBase above_tile;
        public ParallaxTileBase below_tile;
    }

    #endregion


    [Serializable]
    public class Map : SerializedMonoBehaviour
    {
        #region Global

        public static Dictionary<Maps, Map> map_dict;

        #endregion
        

        #region Fields

        [Title("Basic Information")]
        public int map_id;
        [ValidateInput("NameMatchesGameObject", "Map's name must match the game object's name.")]
        public string map_name;
        public MapType map_type;

        [Title("Layer Information")]
        public Dictionary<int, Tilemap> map_layers;
        [ReadOnly]
        public Dictionary<int, Tilemap[]> object_layers;

        #endregion


        #region Validation Methods

        private bool NameMatchesGameObject()
        {
            return map_name.Equals(this.gameObject.name);
        }

        #endregion


        #region Mono Behavior

        private void Start()
        {
            object_layers = new Dictionary<int, Tilemap[]>();
            foreach (KeyValuePair<int, Tilemap> layer in map_layers)
            {
                int sorting_layer = layer.Value.GetComponent<TilemapRenderer>().sortingOrder;
                object_layers[layer.Key] = layer.Value.GetComponentsInChildren<Tilemap>().Skip(1).ToArray();
                for (int i = 0; i < object_layers[layer.Key].Length; i++)
                {
                    Tilemap object_layer = object_layers[layer.Key][i];
                    
                    // Dynamic set sorting layers
                    TilemapRenderer renderer = object_layer.GetComponent<TilemapRenderer>();
                    renderer.sortingOrder = sorting_layer + i;

                    // Instantiate Prefab Tiles
                    foreach (Vector3Int pos in object_layer.cellBounds.allPositionsWithin)
                    {
                        Vector3Int local_place = new Vector3Int(pos.x, pos.y, pos.z);

                        if (object_layer.HasTile(local_place))
                        {
                            // Get PrefabTile for this tile
                            PrefabTile prefab_tile = object_layer.GetTile<PrefabTile>(local_place);
                            if (!prefab_tile)
                                continue;

                            // Instantiate prefab game object
                            prefab_tile.InstantiatePrefab(local_place, object_layer);
                        }
                    }
                }
            }
        }
        
        private NeighborTilemaps GetNeighborTileMaps (Vector3 pos)
        {
            NeighborTilemaps neighbor_maps = new NeighborTilemaps{};

            foreach (Tilemap map in map_layers.Values)
            {
                if (map.transform.position.z == pos.z)
                {
                    neighbor_maps.ground = map;
                    neighbor_maps.objects = map.GetComponentsInChildren<Tilemap>()[1];
                }
                else if (map.transform.position.z == pos.z - Constants.MAP_LAYER_HEIGHT)
                {
                    neighbor_maps.layer_up = map;
                    neighbor_maps.objects_up = map.GetComponentsInChildren<Tilemap>()[1];
                }
                else if (map.transform.position.z == pos.z + Constants.MAP_LAYER_HEIGHT)
                {
                    neighbor_maps.layer_down = map;
                    neighbor_maps.objects_down = map.GetComponentsInChildren<Tilemap>()[1];
                }
            }
            
            return neighbor_maps;
        }

        public NeighborTiles GetNeighborTiles (MoveableObject character)
        {
            return GetNeighborTiles(character.transform.position);
        }

        public NeighborTiles GetNeighborTiles (Vector3 pos)
        {
            NeighborTilemaps neighbor_maps = GetNeighborTileMaps(pos);
            NeighborTiles neighbor_tiles = new NeighborTiles{};

            Vector3Int int_pos = new Vector3Int (
                (int)(pos.x - 0.5f),
                (int)(pos.y - 0.5f),
                0
            );

            neighbor_tiles.on_tile = GetTileAtPosition (neighbor_maps, int_pos);
            neighbor_tiles.up_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.up);
            neighbor_tiles.left_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.left);
            neighbor_tiles.right_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.right);
            neighbor_tiles.down_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.down);

            return neighbor_tiles;
        }

        private ParallaxTileBase GetTileAtPosition (NeighborTilemaps neighbor_maps, Vector3Int pos)
        {
            ParallaxTileBase tile = null;
            
            if (neighbor_maps.objects_up != null)
                tile = (ParallaxTileBase)neighbor_maps.objects_up.GetTile(pos);
            if (tile == null && neighbor_maps.layer_up != null)
                tile = (ParallaxTileBase)neighbor_maps.layer_up.GetTile(pos);
            if (tile == null && neighbor_maps.objects != null)
                tile = (ParallaxTileBase)neighbor_maps.objects.GetTile(pos);
            if (tile == null && neighbor_maps.ground != null)
                tile = (ParallaxTileBase)neighbor_maps.ground.GetTile(pos);
            if (tile == null && neighbor_maps.objects_down != null)
                tile = (ParallaxTileBase)neighbor_maps.objects_down.GetTile(pos);
            if (tile == null && neighbor_maps.layer_down != null)
                tile = (ParallaxTileBase)neighbor_maps.layer_down.GetTile(pos);

            return tile;
        }

        #endregion


        #region Static Methods

        public static Map getMapByID(int id)
        {
            return map_dict[(Maps)id];
        }

        #endregion
    }
}