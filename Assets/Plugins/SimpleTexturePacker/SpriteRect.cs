using UnityEngine;

namespace SimpleTexturePacker
{
    public class SpriteRect
    {
        public Sprite sprite;
        public int x, y, w, h;

        public SpriteRect(Sprite sprite, int padding)
        {
            this.sprite = sprite;
            this.x = 0;
            this.y = 0;
            this.w = (int)sw + padding * 2;
            this.h = (int)sh + padding * 2;
        }

        public int sw => (int)sprite.rect.width;
        public int sh => (int)sprite.rect.height;
        public int area => w * h;
    }
}