extern alias U;
using System;
using System.IO;
using Color32 = U::UnityEngine.Color32;
using Texture2D = U::UnityEngine.Texture2D;
using TextureFormat = U::UnityEngine.TextureFormat;

internal static class TGALoader
{
    public static bool LoadTGA(Stream TGAStream, out U::UnityEngine.Texture2D texture)
    {
        bool flag;
        try
        {
            using (BinaryReader reader = new BinaryReader(TGAStream))
            {
                Texture2D textured;
                reader.BaseStream.Seek(12L, SeekOrigin.Begin);
                short width = reader.ReadInt16();
                short height = reader.ReadInt16();
                int num3 = reader.ReadByte();
                reader.BaseStream.Seek(1L, SeekOrigin.Current);
                Color32[] colors = new Color32[width * height];
                if (num3 == 0x20)
                {
                    textured = new Texture2D(width, height, TextureFormat.RGBA32, false);
                    for (int i = 0; i < (width * height); i++)
                    {
                        byte b = reader.ReadByte();
                        byte g = reader.ReadByte();
                        byte r = reader.ReadByte();
                        byte a = reader.ReadByte();
                        colors[i] = new Color32(r, g, b, a);
                    }
                }
                else
                {
                    if (num3 != 0x18)
                    {
                        throw new Exception("TGA texture had non 32/24 bit depth.");
                    }
                    textured = new Texture2D(width, height, TextureFormat.RGB24, false);
                    for (int j = 0; j < (width * height); j++)
                    {
                        byte num10 = reader.ReadByte();
                        byte num11 = reader.ReadByte();
                        byte num12 = reader.ReadByte();
                        colors[j] = new Color32(num12, num11, num10, 0xff);
                    }
                }
                textured.SetPixels32(colors);
                textured.Apply();
                texture = textured;
                flag = true;
            }
        }
        catch
        {
            texture = null;
            flag = false;
        }
        return flag;
    }

    public static bool LoadTGA(string fileName, out Texture2D texture)
    {
        bool flag;
        try
        {
            using (FileStream stream = File.OpenRead(fileName))
            {
                flag = LoadTGA(stream, out texture);
            }
        }
        catch
        {
            texture = null;
            flag = false;
        }
        return flag;
    }
}

