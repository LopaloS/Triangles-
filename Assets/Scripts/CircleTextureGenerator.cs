using UnityEngine;
using System.Collections;

public class CircleTextureGenerator
{
    public Texture GetTexture(int size, int innerCircleRad, Color color)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color colorA = Color.white;
        colorA.a = 0;
        Vector2 center = new Vector2(size, size) / 2;
        float sqrHalfSize = Mathf.Pow((size / 2), 2);
        float sqrInnerRad = Mathf.Pow(innerCircleRad, 2);

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                Vector2 curPixel = new Vector2(i, j);
                float sqrDist = (curPixel - center).sqrMagnitude;

                if (sqrDist > sqrInnerRad && sqrDist < sqrHalfSize)
                    texture.SetPixel(i, j, color);
                else
                    texture.SetPixel(i, j, colorA);
            }
        }

        texture.Apply();
        return texture;
    }
}
