using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering;
using Mapping;

namespace Utilities
{
    public static class SpriteUtils
    {
        #region Static Methods

        /*
        *   Doesn't work with a sprite atlas :(
        */
        public static Texture2D TextureFromSprite(Sprite sprite)
        {
            if (sprite.rect.width != sprite.texture.width){
                Texture2D newText = new Texture2D((int)sprite.rect.width,(int)sprite.rect.height);
                Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x, 
                                                            (int)sprite.textureRect.y, 
                                                            (int)sprite.textureRect.width, 
                                                            (int)sprite.textureRect.height );
                newText.SetPixels(newColors);
                newText.Apply();
                return newText;
            } else
                return sprite.texture;
        }

        /*
        *   Configure all sprites used in this tile
        *   - Set transparency if applicable
        *   - Set proper sorting layer
        */
        public static void ConfigurePrefabTileSprites (Tilemap map, GameObject go, bool is_trans, bool is_light_trans,
                                                        bool is_ground_anim = false, bool is_anim = false)
        {
            TilemapRenderer renderer = map.GetComponent<TilemapRenderer>();
            int layer = renderer.sortingOrder;
            SpriteRenderer[] sprites = go.GetComponentsInChildren<SpriteRenderer>();
            
            foreach (SpriteRenderer sprite in sprites)
            {   
                // Trans tile handling
                if (is_light_trans)
                    sprite.color = new Color(1,1,1,Constants.LIGHT_TRANS_TILE_ALPHA);
                else if (is_trans)
                    sprite.color = new Color(1,1,1,Constants.TRANS_TILE_ALPHA);

                // Sorting layer handling
                if (is_ground_anim)
                    sprite.sortingOrder = layer + Constants.GROUND_ANIM_SORTING_LAYER_OFFSET;
                else if (is_anim)
                    sprite.sortingOrder = layer + Constants.ANIM_SORTING_LAYER_OFFSET;

                else if (sprite.tag == Constants.PRIORITY_TILE_TAG)
                    sprite.sortingOrder = layer + Constants.PRIORITY_TILE_OFFSET;
                else if (sprite.tag == Constants.TERRAIN_EDGE_TILE_TAG || sprite.tag == Constants.TERRAIN_CORNER_EDGE_TILE_TAG)
                {
                    sprite.sortingOrder = layer + Constants.TERRAIN_EDGE_TILE_OFFSET;
                    SortingGroup group = sprite.GetComponent<SortingGroup>();
                    if (group != null) group.sortingOrder = layer + Constants.TERRAIN_EDGE_TILE_OFFSET;
                }
                else if (sprite.tag == Constants.WATER_TILE_TAG)
                {
                    sprite.sortingOrder = layer + Constants.WATER_TILE_OFFSET;
                    SortingGroup group = sprite.GetComponentInParent<SortingGroup>();
                    if (group != null) group.sortingOrder = layer + Constants.WATER_TILE_OFFSET;
                }
                else if (sprite.tag == Constants.DEPRIORITY_TILE_TAG)
                {
                    sprite.sortingOrder = layer - Constants.PRIORITY_TILE_OFFSET;
                    SortingGroup group = sprite.GetComponent<SortingGroup>();
                    if (group != null) group.sortingOrder = layer - Constants.PRIORITY_TILE_OFFSET;
                }
                else if (sprite.tag == Constants.EXTRA_PRIORITY_TILE_TAG)
                    sprite.sortingOrder = layer + (2 * Constants.PRIORITY_TILE_OFFSET);
                else if (sprite.tag == Constants.EXTRA_DEPRIORITY_TILE_TAG)
                    sprite.sortingOrder = layer - (2 * Constants.PRIORITY_TILE_OFFSET);
                else if (sprite.tag == Constants.COVER_PRIORITY_TILE_TAG)
                    sprite.sortingOrder = layer + Constants.SORTING_LAYERS_PER_MAP_LAYER - Constants.OBJECT_LAYER_START_OFFSET;
                else if (sprite.tag == Constants.UP_LAYER_PRIORITY_TILE_TAG)
                    sprite.sortingOrder = layer + Constants.SORTING_LAYERS_PER_MAP_LAYER;
                else if (sprite.tag == Constants.DOWN_LAYER_PRIORITY_TILE_TAG)
                    sprite.sortingOrder = layer - Constants.SORTING_LAYERS_PER_MAP_LAYER + (2 * Constants.PRIORITY_TILE_OFFSET); // Gap Fill Use
                else
                {
                    sprite.sortingOrder = layer;
                    SortingGroup group = sprite.GetComponent<SortingGroup>();
                    if (group != null) group.sortingOrder = layer;
                }
            }
        }

        public static IEnumerator FadeSprite(SpriteRenderer sprite, float target_alpha, float time)
        {
            float elapsed_time = 0;
            float start_alpha = sprite.color.a;
            while (elapsed_time < time)
            {
                elapsed_time += Time.deltaTime;
                float new_alpha = Mathf.Lerp(start_alpha, target_alpha, elapsed_time / time);
                sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, new_alpha);
                yield return null;
            }
        }

        public static IEnumerator FadeTile(TilePosition tile, float target_alpha, float time)
        {
            float elapsed_time = 0;
            Color color = tile.map.GetColor(tile.pos);
            float start_alpha = color.a;
            while (elapsed_time < time)
            {
                elapsed_time += Time.deltaTime;
                float new_alpha = Mathf.Lerp(start_alpha, target_alpha, elapsed_time / time);
                tile.map.SetColor(tile.pos, new Color(color.r, color.g, color.b, new_alpha));
                yield return null;
            }
        }

        /* 
        * Get material used for pixel snapping
        */
        public static Material GetPixelSnappingMaterial()
        {
            return Resources.Load<Material>(Path.Join(Settings.MATERIALS_PATH,Settings.PIXEL_SNAPPING_MATERIAL_FILENAME));
        }

        /*
        * Get Outline Materials
        */
        public static Material GetOutlineMaterial(OutlineColors color)
        {
            if (color != OutlineColors.None)
            {
                string filename = Settings.OUTLINE_MATERIAL_FILENAME_PREFIX + color.ToString();
                Material material = Resources.Load<Material>(Path.Join(Settings.MATERIALS_PATH,filename));

                if (material != null) return material;
            }
            
            return GetPixelSnappingMaterial();
        }

        #endregion
    }
}


