using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;

namespace Mapping
{
    public class ParallaxTileBase : TileBase
    {
        #region Fields

        [Title("Movement")]
        public bool allow_passage = true;
        public bool up_passage = true;
        public bool right_passage = true;
        public bool down_passage = true;
        public bool left_passage = true;

        [Title("Flags")]
        public bool is_bush = false;
        public bool is_counter = false;

        [Title("Tile Type")]
        public TerrainTags terrain_tag = TerrainTags.None;

        #endregion
    }
}