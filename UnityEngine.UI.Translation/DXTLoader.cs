extern alias U;
using System;
using System.IO;
using Texture2D = U::UnityEngine.Texture2D;
using TextureFormat = U::UnityEngine.TextureFormat;

internal static class DXTLoader
{
    public static bool LoadDXT(string fileName, out U::UnityEngine.Texture2D texture)
    {
        try
        {
            return LoadDXT(File.ReadAllBytes(fileName), out texture);
        }
        catch
        {
            texture = null;
            return false;
        }
    }

    public static bool LoadDXT(byte[] ddsBytes, out Texture2D texture)
    {
        try
        {
            if (ddsBytes[4] != 0x7c)
            {
                throw new Exception("Invalid DDS DXTn texture. Unable to read");
            }
            TextureFormat format = (TextureFormat) 0;
            if (((ddsBytes[0x54] == 0x44) && (ddsBytes[0x55] == 0x58)) && (ddsBytes[0x56] == 0x54))
            {
                if (ddsBytes[0x57] == 0x31)
                {
                    format = TextureFormat.DXT1;
                }
                else if (ddsBytes[0x57] == 0x35)
                {
                    format = TextureFormat.DXT5;
                }
            }
            if ((format != TextureFormat.DXT1) && (format != TextureFormat.DXT5))
            {
                throw new Exception("Invalid TextureFormat. Only DXT1 and DXT5 formats are supported by this method.");
            }
            int height = (ddsBytes[13] * 0x100) + ddsBytes[12];
            int width = (ddsBytes[0x11] * 0x100) + ddsBytes[0x10];
            byte[] dst = new byte[ddsBytes.Length - 0x80];
            Buffer.BlockCopy(ddsBytes, 0x80, dst, 0, ddsBytes.Length - 0x80);
            texture = new Texture2D(width, height, format, false);
            texture.LoadRawTextureData(dst);
            texture.Apply();
            return true;
        }
        catch
        {
            texture = null;
            return false;
        }
    }
}

