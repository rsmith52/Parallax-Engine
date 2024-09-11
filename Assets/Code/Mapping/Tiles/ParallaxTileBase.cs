using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;

namespace Mapping
{
    #region Structs
    
    [Serializable]
    public struct HelperTile
    {
        public ParallaxTileBase tile;
        public Vector2Int offsets;
    }
    
    #endregion

    
    [Serializable]
    public class ParallaxTileBase : TileBase
    {
        #region Editor

        [Title("Tile Type")]
        public TerrainTags terrain_tag = TerrainTags.None;
        public bool has_helper_tiles = false;

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
        public bool is_trans = false;

        [Title("Helper Tiles")]
        [ShowIf("@this.has_helper_tiles")]
        public HelperTile[] helper_tiles;

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