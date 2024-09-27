using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
        public static void ConfigurePrefabTileSprites (Tilemap map, GameObject go, bool is_trans)
        {
            TilemapRenderer renderer = map.GetComponent<TilemapRenderer>();
            int layer = renderer.sortingOrder;
            SpriteRenderer[] sprites = go.GetComponentsInChildren<SpriteRenderer>();
            
            foreach (SpriteRenderer sprite in sprites)
            {   
                // Trans tile handling
                if (is_trans)
                    sprite.color = new Color(1,1,1,Constants.TRANS_TILE_ALPHA);

                // Sorting layer handling
                if (sprite.tag == Constants.PRIORITY_TILE_TAG)
                    sprite.sortingOrder = layer + Constants.PRIORITY_TILE_OFFSET;
                else if (sprite.tag == Constants.DEPRIORITY_TILE_TAG)
                    sprite.sortingOrder = layer - Constants.PRIORITY_TILE_OFFSET;
                else if (sprite.tag == Constants.EXTRA_PRIORITY_TILE_TAG)
                    sprite.sortingOrder = layer + (2 * Constants.PRIORITY_TILE_OFFSET);
                else
                    sprite.sortingOrder = layer;
            }
        }

        #endregion
    }
}


