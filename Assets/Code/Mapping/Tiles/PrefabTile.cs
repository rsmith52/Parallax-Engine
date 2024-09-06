using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;

namespace Mapping
{
    [CreateAssetMenu(menuName = "Parallax Tiles/Prefab Tile", fileName = "Prefab Tile")]
    [Serializable]
    public class PrefabTile : ParallaxTileBase
    {
        #region Editor
        
        [Title("Prefab Tile Settings")]
        public Sprite preview_sprite;
        public GameObject prefab;
        [DetailedInfoBox("Offset Considerations", "If the tile has an offset in it's tile palette, this value should be the intended offset (default 0.5, 0.5) + that grid selection transform offset.")]
        public Vector2 prefab_offset = new Vector2(0.5f, 0.5f);
        public float prefab_local_z = 0;

        #endregion


        #region MonoBehavior

        public override void GetTileData(Vector3Int pos, ITilemap tilemap, ref TileData tile_data)
        {
            tile_data.sprite = preview_sprite;
            
            if (prefab && tile_data.gameObject == null)
            {
                tile_data.gameObject = Application.isPlaying ? null : prefab;
                if (Application.isPlaying && prefab)
                {
                    Tilemap map = tilemap.GetComponent<Tilemap>();
                    InstantiatePrefab(pos, map);
                    // Set original tile to be transparent (not shown)
                    map.SetColor(pos, Color.clear);
                }
            }
        }

        /*
        * Fix display in scene editor to not show rogue prefabs.
        */
        public override bool StartUp(Vector3Int pos, ITilemap tilemap, GameObject go)
        {
            // This prevents rogue prefab objects from appearing when the Tile palette is present
            #if UNITY_EDITOR
            if (go != null)
            {
                if (go.scene.name == null)
                    DestroyImmediate(go);
            }
            #endif

            if (go != null)
            {
                // Modify position of game object to match middle of Tile sprite
                go.transform.position = new Vector3(
                    pos.x + prefab_offset.x
                    , pos.y + prefab_offset.y
                    , pos.z);
                // Set proper Z Offset
                go.transform.localPosition = new Vector3(go.transform.localPosition.x,
                    go.transform.localPosition.y, prefab_local_z);
            }

            return true;
        }

        /*
        * Instantiate prefab in runtime.
        */
        public void InstantiatePrefab(Vector3Int pos, Tilemap map)
        {
            GameObject instance = Instantiate(prefab);
            instance.transform.SetParent(map.transform);

            // Modify position of object to match middle of tile sprite
            instance.transform.position = new Vector3(
                pos.x + prefab_offset.x,
                pos.y + prefab_offset.y,
                pos.z
            );
            // Set proper Z Offset
            instance.transform.localPosition = new Vector3(instance.transform.localPosition.x,
                instance.transform.localPosition.y, prefab_local_z);

            // Set proper layer ordering
            TilemapRenderer renderer = map.GetComponent<TilemapRenderer>();
            int layer = renderer.sortingOrder;
            SpriteRenderer[] sprites = instance.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sprite in sprites)
                sprite.sortingOrder = layer;
        }

        #endregion
    }
}