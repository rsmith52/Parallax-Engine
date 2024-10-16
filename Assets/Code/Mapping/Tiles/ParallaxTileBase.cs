using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;

namespace Mapping.Tiles
{
    [Serializable]
    public class ParallaxTileBase : TileBase
    {
        #region Editor

        [Title("Tile Type")]
        public TerrainTags terrain_tag = TerrainTags.None;

        [Title("Movement")]
        public bool allow_passage = true;
        [HideIf("@!allow_passage")]
        public bool up_passage = true;
        [HideIf("@!allow_passage")]
        public bool left_passage = true;
        [HideIf("@!allow_passage")]
        public bool right_passage = true;
        [HideIf("@!allow_passage")]
        public bool down_passage = true;

        [Title("Flags")]
        public bool is_bush = false;
        public bool is_counter = false;
        [HideIf("@is_light_trans")]
        public bool is_trans = false;
        [HideIf("@is_trans")]
        public bool is_light_trans = false;
        public bool is_reflective = false;
        public bool is_jump = false;
        public bool is_hideable = false;
        [HideIf("@!is_hideable")]
        public bool strict_hiding = false;

        [Title("Multi Tile")]
        public bool multi_tile;
        [HideIf("@!multi_tile")]
        public TileSize tile_size;

        #endregion


        #region Fields

        [HideInInspector]
        public GameObject instantiated_object;
        public GameObject GetInstantiatedObject() { return instantiated_object; }

        #endregion

        
        #region MonoBehavior

        protected BasicTile MultiTileCopy(ParallaxTileBase base_tile)
        {
            BasicTile tile_copy = CreateInstance<BasicTile>();
            tile_copy.name = base_tile.name;
            tile_copy.terrain_tag = base_tile.terrain_tag;
            tile_copy.allow_passage = base_tile.allow_passage;
            tile_copy.is_bush = base_tile.is_bush;
            
            tile_copy.is_hideable = base_tile.is_hideable;
            tile_copy.strict_hiding = base_tile.strict_hiding;
            tile_copy.instantiated_object = base_tile.instantiated_object;

            return tile_copy;
        }

        protected Tilemap GetCopyToMap(Map map, Tilemap base_layer, int layers_up)
        {
            int layer_up_key = 0;
            foreach (KeyValuePair<int, Tilemap> layer in map.map_layers)
            {
                if (layer.Value.name == base_layer.name)
                {
                    layer_up_key = layer.Key + 1;
                }
            }

            if (map.object_layers.ContainsKey(layer_up_key))
            {
                Tilemap[] object_layers = map.object_layers[layer_up_key];
                return object_layers.Last();
            }
            else return null;
        }

        #endregion


        #region Detect Palette

        public const int PaletteTilemapLayer = 31;

        /// <summary>
        /// Overload for IsTilemapFromPalette(Tilemap)
        /// </summary>
        /// <param name="itilemap">An ITilemap instance</param>
        /// <returns>true if tilemap is from the palette</returns>
        public static bool IsTilemapFromPalette(ITilemap itilemap)
        {
            var component = itilemap.GetComponent<Tilemap>();
            return component != null && IsTilemapFromPalette(component);
        }
      
        /// <summary>
        /// Is this tilemap actually the palette?
        /// </summary>
        /// <param name="tilemap">tilemap ref</param>
        /// <returns>true if is from palette</returns>
        /// <remarks>It shouldn't be this obtuse... </remarks>
        public static bool IsTilemapFromPalette(Tilemap tilemap)
        {
            /*although it's tempting to use PrefabUtility.IsPartOfAnyPrefab() here
              because the palette is actually a Prefab, it won't work because any
              tilemap can be validly part of a prefab.

            aside from a Palette's name being Layer1, which could change,
            the hideflags are different. A normal tilemap should never
            have DontSave for flags.
            Also, the tilemap's transform.parent's layer is set to 31. So check for that too.
            Note: the tilemap within the Palette is inside a prefab. It's hide flags
            are set to HideAndDontSave which is DontSave | NotEditable | HideInHierarchy
            but when a palette prefab is opened in a prefab context, the tilemap created
            for the prefab stage is set to DontSave. Since HideAndDontSave includes DontSave
            its easier to use that. It's unlikely that a tilemap in a scene would have DontSave
            as a flag. (Note that in 2021.2 this may not be true, so the "Layer1" check is still used)
            */
            //so the tilemap is a palette if the hideflags DontSave bit is set
          
            if (tilemap.name == "Layer1" ||
                (tilemap.hideFlags & HideFlags.DontSave) == HideFlags.DontSave)
                return true;

            var parent = tilemap.transform.parent;
            if (parent == null) //should never happen since a tilemap always has a Grid as parent.
                return false;

            //or the tilemap's parent GO layer is 31
            return parent.gameObject.layer == PaletteTilemapLayer;
          
        }

        #endregion
    }
}