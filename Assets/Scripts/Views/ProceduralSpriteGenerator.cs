using UnityEngine;

public static class ProceduralSpriteGenerator
{
    public static Sprite CreateRoundedRectSprite(int width, int height, int radius, int borderThickness = 0, float pixelsPerUnit = 100f)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dist = GetDistanceToRoundedRect(x, y, width, height, radius);
                if (borderThickness > 0)
                {
                    float alpha = Mathf.Clamp01(0.5f - dist) * Mathf.Clamp01(dist + borderThickness + 0.5f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                else
                {
                    float alpha = Mathf.Clamp01(0.5f - dist);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
        }

        tex.Apply();
        Vector4 border = new Vector4(radius, radius, radius, radius);
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), pixelsPerUnit, 0, SpriteMeshType.FullRect, border);
    }

    public static Sprite CreateTestTubeSprite(bool isMask)
    {
        int width = 128;
        int height = 384;
        int bodyHalfWidth = 46;
        int bottomRadius = 46;
        int border = 7;
        int rimHeight = 16;
        int rimHalfWidth = 54;
        int centerX = width / 2;
        int bottomCenterY = bottomRadius + 6;
        int topY = height - 6;

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = Mathf.Abs(x - centerX);
                float distBody;

                if (y < bottomCenterY)
                {
                    float dy = bottomCenterY - y;
                    distBody = Mathf.Sqrt(dx * dx + dy * dy) - bottomRadius;
                }
                else if (y <= topY)
                {
                    distBody = dx - bodyHalfWidth;
                }
                else
                {
                    distBody = 100f;
                }

                if (isMask)
                {
                    float innerDist = distBody + border;
                    float maskAlpha = (y <= topY) ? Mathf.Clamp01(0.5f - innerDist) : 0f;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, maskAlpha));
                }
                else
                {
                    float wallAlpha = Mathf.Clamp01(0.5f - distBody) * Mathf.Clamp01(distBody + border + 0.5f);
                    if (y > topY - border && y <= topY)
                    {
                        wallAlpha = 0f;
                    }

                    float rimAlpha = 0f;
                    if (y >= topY - rimHeight && y <= topY)
                    {
                        float dRimX = dx - rimHalfWidth;
                        float dRimY = Mathf.Abs(y - (topY - rimHeight * 0.5f)) - (rimHeight * 0.5f);
                        float dRim = Mathf.Max(dRimX, dRimY);
                        float outerRim = Mathf.Clamp01(0.5f - dRim);
                        float innerHole = Mathf.Clamp01((dx - (bodyHalfWidth - border)) + 0.5f);
                        rimAlpha = outerRim * innerHole;
                    }

                    float highlightAlpha = 0f;
                    if (y >= bottomCenterY + 10 && y <= topY - rimHeight - 10)
                    {
                        if (x >= centerX - bodyHalfWidth + border + 5 && x <= centerX - bodyHalfWidth + border + 11)
                        {
                            highlightAlpha = 0.38f;
                        }
                    }

                    float finalAlpha = Mathf.Max(wallAlpha, Mathf.Max(rimAlpha, highlightAlpha));
                    float glassTint = (distBody < -border) ? 0.08f : 0f;
                    float a = Mathf.Clamp01(finalAlpha + (y <= topY && distBody < 0 ? glassTint : 0f));

                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 128f);
    }

    public static Sprite CreateCircleSprite(int size, float pixelsPerUnit = 100f)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float center = (size - 1) * 0.5f;
        float radius = size * 0.5f - 1f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = Mathf.Clamp01(radius + 0.5f - dist);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    public static Sprite CreateGradientBackgroundSprite(int width, int height, Color centerColor, Color edgeColor)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2(width * 0.5f, height * 0.55f);
        float maxDist = Vector2.Distance(Vector2.zero, center);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float t = Mathf.Clamp01(dist / maxDist);
                t = t * t * (3f - 2f * t);
                tex.SetPixel(x, y, Color.Lerp(centerColor, edgeColor, t));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    private static float GetDistanceToRoundedRect(int x, int y, int width, int height, int radius)
    {
        float px = Mathf.Max(radius - x, 0, x - (width - 1 - radius));
        float py = Mathf.Max(radius - y, 0, y - (height - 1 - radius));
        return Mathf.Sqrt(px * px + py * py) - radius;
    }
}