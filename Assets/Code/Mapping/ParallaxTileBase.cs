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
        public bool right_passage = true;
        [HideIf("@!allow_passage")]
        public bool down_passage = true;
        [HideIf("@!allow_passage")]
        public bool left_passage = true;

        [Title("Flags")]
        public bool is_bush = false;
        public bool is_counter = false;

        [Title("Helper Tiles")]
        [ShowIf("@this.has_helper_tiles")]
        public HelperTile[] helper_tiles;

        #endregion
    }
}