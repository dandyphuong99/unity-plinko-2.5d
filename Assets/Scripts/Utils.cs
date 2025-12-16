using System;
using System.Collections;
using System.Linq;
using TikTokLiveSharp.Events.Objects;
using TikTokLiveUnity;
using TikTokLiveUnity.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Utils
{

    public static void RequestSprite(Image img, Picture picture)
    {
        Dispatcher.RunOnMainThread(() =>
        {
            if (TikTokLiveManager.Exists)
                TikTokLiveManager.Instance.RequestSprite(picture, spr =>
                {
                    if (img != null && img.gameObject != null && img.gameObject.activeInHierarchy)
                        img.sprite = spr;
                });
        });
    }

    public static string GetBestImageUrl(Picture picture)
    {
        if (picture?.Urls == null || !picture.Urls.Any())
            return null;

        // Ưu tiên ảnh lớn nhất (thường là cuối list)
        var urls = picture.Urls.ToList();
        string bestUrl = urls.LastOrDefault(u => u.Contains("jpg") || u.Contains("jpeg") || u.Contains("png"))
                         ?? urls.LastOrDefault(u => u.Contains("webp"))
                         ?? urls.LastOrDefault();

        return bestUrl;
    }


    public static void RequestImage(Material mat, Picture picture)
    {
        Dispatcher.RunOnMainThread(() =>
        {
            if (TikTokLiveManager.Exists)
                TikTokLiveManager.Instance.RequestImage(picture, tex =>
                {
                    if (tex != null)
                    {
                        tex.filterMode = FilterMode.Point;      // hoặc FilterMode.Bilinear
                        tex.wrapMode = TextureWrapMode.Clamp;   // tránh lặp ảnh
                        tex.Apply();

                        mat.mainTexture = tex; // Gán texture vào material thứ 2
                    }
                });
        });
    }

    public static void RequestImageByUrl(Material mat, string url)
    {
        Dispatcher.RunOnMainThread(() =>
        {
            if (TikTokLiveManager.Exists)
            {
                TikTokLiveManager.Instance.RequestSprite(url, spr =>
                {
                    if (spr != null && mat != null)
                    {
                        Texture2D tex = spr.texture;

                        // Thiết lập texture properties
                        tex.filterMode = FilterMode.Trilinear;
                        tex.wrapMode = TextureWrapMode.Clamp;

                        // Kiểm tra shader type và sử dụng property phù hợp
                        if (mat.shader.name.Contains("Universal") || mat.shader.name.Contains("URP"))
                        {
                            // URP shaders sử dụng _BaseMap thay vì _MainTex
                            if (mat.HasProperty("_BaseMap"))
                            {
                                mat.SetTexture("_BaseMap", tex);
                            }
                            else if (mat.HasProperty("_MainTex"))
                            {
                                mat.SetTexture("_MainTex", tex);
                            }
                            else
                            {
                                mat.mainTexture = tex;
                            }

                            // Đảm bảo material sử dụng đúng shader mode
                            mat.SetFloat("_Surface", 0); // 0 = Opaque, 1 = Transparent
                        }
                        else
                        {
                            // Standard shader
                            mat.mainTexture = tex;
                        }

                        // Đặt màu sáng để texture hiển thị rõ
                        if (mat.HasProperty("_BaseColor"))
                        {
                            mat.SetColor("_BaseColor", Color.white);
                        }
                        else if (mat.HasProperty("_Color"))
                        {
                            mat.SetColor("_Color", Color.white);
                        }
                        else
                        {
                            mat.color = Color.white;
                        }
                    }
                });
            }
        });
    }


    public static void RequestImageByUrl(SpriteRenderer renderer, string url)
    {
        Dispatcher.RunOnMainThread(() =>
        {
            if (TikTokLiveManager.Exists)
                TikTokLiveManager.Instance.RequestSprite(url, spr =>
                {
                    if (spr != null && renderer != null && renderer.gameObject != null && renderer.gameObject.activeInHierarchy)
                        renderer.sprite = spr;
                });
        });
    }

    /// <summary>
    /// Lấy màu pixel cho texture hình tròn
    /// </summary>
    static public Color GetPixelColorForCircle(Texture2D originalTexture, int x, int y, int size, Vector2 center, float radius)
    {
        float distance = Vector2.Distance(new Vector2(x, y), center);

        if (distance <= radius)
        {
            float u = (float)x / size;
            float v = (float)y / size;
            return originalTexture.GetPixelBilinear(u, v);
        }

        return Color.clear;
    }

    /// <summary>
    /// Tạo texture hình tròn từ texture gốc
    /// </summary>
    /// <param name="originalTexture">Texture gốc</param>
    /// <returns>Texture hình tròn</returns>
    static public Texture2D CreateCircleTexture(Texture2D originalTexture)
    {
        int size = Mathf.Max(originalTexture.width, originalTexture.height);
        Texture2D circleTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Color pixelColor = GetPixelColorForCircle(originalTexture, x, y, size, center, radius);
                circleTexture.SetPixel(x, y, pixelColor);
            }
        }

        circleTexture.Apply();
        return circleTexture;
    }

    public static Sprite CreateCircleSprite(Color color)
    {
        int size = 64; // độ phân giải viền
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                tex.SetPixel(x, y, distance <= radius ? color : Color.clear);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }


    /// <summary>
    /// Tạo sprite từ texture
    /// </summary>
    static public Sprite CreateSpriteFromTexture(Texture2D texture)
    {
        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );
    }
}
