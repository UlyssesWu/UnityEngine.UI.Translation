extern alias U;
using System;
using System.IO;
using System.Text;
using Color32 = U::UnityEngine.Color32;
using Texture2D = U::UnityEngine.Texture2D;
using TextureFormat = U::UnityEngine.TextureFormat;

internal static class PSDLoader
{
    public static bool LoadPSD(string fileName, out U::UnityEngine.Texture2D texture)
    {
        try
        {
            return LoadPSD(File.ReadAllBytes(fileName), out texture);
        }
        catch
        {
            texture = null;
            return false;
        }
    }

    public static bool LoadPSD(byte[] psdBytes, out Texture2D texture)
    {
        bool flag;
        try
        {
            byte[] buffer;
            int index = 0;
            if (psdBytes == null)
            {
                throw new ArgumentNullException("psdBytes");
            }
            byte[] bytes = Encoding.UTF8.GetBytes("8BPS");
            if (psdBytes.Length < bytes.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
            for (int i = 0; i < bytes.Length; i++)
            {
                if (psdBytes[i] != bytes[i])
                {
                    throw new FormatException();
                }
            }
            index = Read(psdBytes, bytes.Length, 2, out buffer, true);
            if (BitConverter.ToInt16(buffer, 0) != 1)
            {
                throw new FormatException();
            }
            index += 6;
            index = Read(psdBytes, index, 2, out buffer, true);
            short num2 = BitConverter.ToInt16(buffer, 0);
            if ((num2 != 3) && (num2 != 4))
            {
                throw new NotSupportedException();
            }
            index = Read(psdBytes, index, 4, out buffer, true);
            int height = BitConverter.ToInt32(buffer, 0);
            index = Read(psdBytes, index, 4, out buffer, true);
            int width = BitConverter.ToInt32(buffer, 0);
            index = Read(psdBytes, index, 2, out buffer, true);
            if (BitConverter.ToInt16(buffer, 0) != 8)
            {
                throw new NotSupportedException();
            }
            index = Read(psdBytes, index, 2, out buffer, true);
            short num5 = BitConverter.ToInt16(buffer, 0);
            index = Read(psdBytes, index, 4, out buffer, true);
            int num6 = BitConverter.ToInt32(buffer, 0);
            index += num6;
            index = Read(psdBytes, index, 4, out buffer, true);
            int num7 = BitConverter.ToInt32(buffer, 0);
            index += num7;
            index = Read(psdBytes, index, 4, out buffer, true);
            int num8 = BitConverter.ToInt32(buffer, 0);
            index += num8;
            index = Read(psdBytes, index, 2, out buffer, true);
            short num9 = BitConverter.ToInt16(buffer, 0);
            TextureFormat format = (TextureFormat) 0;
            switch (num5)
            {
                case 0:
                    throw new NotSupportedException();

                case 1:
                    throw new NotSupportedException();

                case 2:
                    throw new NotSupportedException();

                case 3:
                    format = TextureFormat.RGBA32;
                    break;

                case 4:
                    throw new NotSupportedException();

                case 7:
                    throw new NotSupportedException();

                case 8:
                    throw new NotSupportedException();

                case 9:
                    throw new NotSupportedException();

                default:
                    throw new NotSupportedException();
            }
            switch (num9)
            {
                case 0:
                    throw new NotSupportedException();

                case 1:
                {
                    short[] numArray = new short[num2 * height];
                    for (int j = 0; j < numArray.Length; j++)
                    {
                        index = Read(psdBytes, index, 2, out buffer, true);
                        numArray[j] = BitConverter.ToInt16(buffer, 0);
                    }
                    int num11 = 0;
                    int num12 = 0;
                    Color32[] colors = new Color32[width * height];
                    for (int k = 0; k < numArray.Length; k++)
                    {
                        if (num11 == 4)
                        {
                            throw new NotSupportedException();
                        }
                        index = Read(psdBytes, index, numArray[k], out buffer, false);
                        for (int m = 0; m < buffer.Length; m++)
                        {
                            sbyte num16 = (sbyte) buffer[m];
                            if (num16 != -128)
                            {
                                if (num16 < 0)
                                {
                                    m++;
                                    int num17 = 1 - num16;
                                    for (int n = 0; n < num17; n++)
                                    {
                                        Color32 color = colors[num12];
                                        switch (num11)
                                        {
                                            case 0:
                                                color.a = 0xff;
                                                color.r = buffer[m];
                                                break;

                                            case 1:
                                                color.g = buffer[m];
                                                break;

                                            case 2:
                                                color.b = buffer[m];
                                                break;

                                            case 3:
                                                color.a = buffer[m];
                                                break;
                                        }
                                        colors[num12++] = color;
                                    }
                                }
                                else
                                {
                                    int num19 = 1 + num16;
                                    for (int num20 = 0; num20 < num19; num20++)
                                    {
                                        Color32 color2 = colors[num12];
                                        switch (num11)
                                        {
                                            case 0:
                                                color2.a = 0xff;
                                                color2.r = buffer[++m];
                                                break;

                                            case 1:
                                                color2.g = buffer[++m];
                                                break;

                                            case 2:
                                                color2.b = buffer[++m];
                                                break;

                                            case 3:
                                                color2.a = buffer[++m];
                                                break;
                                        }
                                        colors[num12++] = color2;
                                    }
                                }
                            }
                        }
                        if (num12 == colors.Length)
                        {
                            num11++;
                            num12 = 0;
                        }
                    }
                    texture = new Texture2D(width, height, format, false);
                    texture.SetPixels32(colors);
                    texture.Apply();
                    return true;
                }
                case 2:
                    throw new NotSupportedException();

                case 3:
                    throw new NotSupportedException();
            }
            throw new NotSupportedException();
        }
        catch
        {
            texture = null;
            flag = false;
        }
        return flag;
    }

    private static int Read(byte[] data, int index, int count, out byte[] buffer, bool reverse = true)
    {
        if ((index < 0) || (data.Length < (index + count)))
        {
            throw new IndexOutOfRangeException();
        }
        buffer = new byte[count];
        Array.Copy(data, index, buffer, 0, count);
        if (reverse)
        {
            Array.Reverse(buffer);
        }
        index += count;
        return index;
    }
}

