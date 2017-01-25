﻿using UnityEngine;
using System.Runtime.InteropServices;
public struct MyVector
{
    public float x;
    public float y;

    MyVector(Vector2 InVec)
    {
        x = InVec.x;
        y = InVec.y;
    }

    public static implicit operator MyVector(Vector2 value)
    {
        return new MyVector(value);
    }

    public static implicit operator Vector2(MyVector value)
    {
        return new Vector2(value.x, value.y);
    }
};

public struct Brush
{
    public byte[] Data;
    public int Size;
    public int Spacing;
    //Direction Normalized
    public MyVector Direction;
    public float SpacingRatio;
    public Brush(Texture2D InputTex, float _SpacingRatio)
    {
        int DataLength = InputTex.width * InputTex.height;
        Data = new byte[DataLength];
        byte[] InputData = InputTex.GetRawTextureData();
        for (int i = 0; i < DataLength; i++)
        {
            int index = i << 2;
            Data[i] = ((InputData[index] + InputData[index + 1] + InputData[index + 2]) > 150) ? (byte)0 : (byte)1;
        }
        Size = InputTex.width;
        SpacingRatio = _SpacingRatio;
        Spacing = 1;
        Direction = new MyVector();
        CalcSpacing();
    }

    public void Resize(int NewSize)
    {
        float ratio = (float)Size / NewSize;
        byte[] NewData = new byte[NewSize * NewSize];
        for (int y = 0; y < NewSize; y++)
        {
            int thisY = (int)(ratio * y) * Size;
            int yw = y * NewSize;
            for (int x = 0; x < NewSize; x++)
            {
                NewData[yw + x] = Data[(int)(thisY + ratio * x)];
            }
        }
        Data = NewData;
        Size = NewSize;
        CalcSpacing();
    }

    public void Rotate(float Angle)
    {
        byte[] NewData = new byte[Size * Size];
        int x, y;
        float x1, y1, x2, y2;

        x1 = rot_x(Angle, -Size / 2.0f, -Size / 2.0f) + Size / 2;
        y1 = rot_y(Angle, -Size / 2.0f, -Size / 2.0f) + Size / 2;
        float dx_x = rot_x(Angle, 1.0f, 0.0f);
        float dx_y = rot_y(Angle, 1.0f, 0.0f);
        float dy_x = rot_x(Angle, 0.0f, 1.0f);
        float dy_y = rot_y(Angle, 0.0f, 1.0f);

        for (x = 0; x < Size; x++)
        {
            x2 = x1;
            y2 = y1;
            for (y = 0; y < Size; y++)
            {
                x2 += dx_x;
                y2 += dx_y;
                NewData[x + y * Size] = GetPixel((int)x2, (int)y2);
            }

            x1 += dy_x;
            y1 += dy_y;
        }

        Data = NewData;
    }

    byte GetPixel(int x, int y)
    {
        if (x >= Size || x < 0 || y >= Size || y < 0)
        {
            return 0;
        }
        else
        {
            return Data[x + y * Size];
        }
    }
    float rot_x(float angle, float x, float y)
    {
        float cos = Mathf.Cos(angle / 180.0f * Mathf.PI);
        float sin = Mathf.Sin(angle / 180.0f * Mathf.PI);
        return (x * cos + y * (-sin));
    }

    float rot_y(float angle, float x, float y)
    {
        float cos = Mathf.Cos(angle / 180.0f * Mathf.PI);
        float sin = Mathf.Sin(angle / 180.0f * Mathf.PI);
        return (x * sin + y * cos);
    }

    void CalcSpacing()
    {
        int S = (int)(SpacingRatio * Size);
        Spacing = S > 0 ? S : 1;
    }
}

public struct MyTexture
{
    public byte[] Data;
    public int Width;
    public int Height;

    public MyTexture(byte[] _Data, int _Width, int _Height)
    {
        Data = _Data;
        Width = _Width;
        Height = _Height;
    }

    public MyTexture(Texture2D NewTex)
    {
        Data = NewTex.GetRawTextureData();
        Width = NewTex.width;
        Height = NewTex.height;
    }

};

public struct ByteColor
{
    public byte R;
    public byte G;
    public byte B;
    public float A;
    public ByteColor(Color NewColor)
    {
        R = (byte)(NewColor.r * 255);
        G = (byte)(NewColor.g * 255);
        B = (byte)(NewColor.b * 255);
        A = NewColor.a;
    }

    public static implicit operator ByteColor(Color InCol)
    {
        return new ByteColor(InCol);
    }
};

public class Draw
{
    // Use this for initialization

    private delegate void DebugCallback(string message);

    [DllImport("DrawDLL")]
    public static extern void SeedRandomization();
    [DllImport("DrawDLL")]
    private static extern void FillWithColor(MyTexture InTex, int x, int y, ByteColor ReplacementColor);
    [DllImport("DrawDLL")]
    public static extern void GetBrightTexture(MyTexture InTex, ByteColor MainColor);
    [DllImport("DrawDLL")]
    public static extern void DrawBrushTip(MyTexture TexData, Brush BrushData, ByteColor DrawColor, int x, int y);
    [DllImport("DrawDLL")]
    public static extern void DrawSprayWithBrush(MyTexture TexData, Brush BrushData, ByteColor DrawColor, int x, int y);
    [DllImport("DrawDLL")]
    private static extern void DrawBrushTipWithTex(MyTexture TexData, Brush BrushData, MyTexture DrawColor, int x, int y);
    [DllImport("DrawDLL")]
    private static extern void DrawLine(MyTexture TexData, Brush BrushData, ByteColor DrawColor, int x0, int y0, int x1, int y1, ref MyVector FinalPos);
    [DllImport("DrawDLL")]
    private static extern void DrawLineWithTex(MyTexture TexData, Brush BrushData, MyTexture DrawColor, int x0, int y0, int x1, int y1, ref MyVector FinalPos);
    [DllImport("DrawDLL")]
    private static extern void RegisterDebugCallback(DebugCallback callback);

    public static void FloodFillArea(MyTexture OutTex, Vector2 Pos, Color aFillColor)
    {
        FillWithColor(OutTex, (int)Pos.x, (int)Pos.y, new ByteColor(aFillColor));
    }

    public static void DrawBrushTip(MyTexture OutTex, Brush _Brush, Color DrawColor, Vector2 Pos)
    {
        DrawBrushTip(OutTex, _Brush, DrawColor, (int)Pos.x, (int)Pos.y);
    }

    public static void DrawBrushTipWithTex(MyTexture OutTex, MyTexture PatternTex, Brush _Brush, Vector2 Pos)
    {
        DrawBrushTipWithTex(OutTex, _Brush, PatternTex, (int)Pos.x, (int)Pos.y);
    }

    public static Vector2 DrawLine(MyTexture OutTex, Brush _Brush, Color DrawColor, Vector2 Pos0, Vector2 Pos1)
    {
        MyVector Vect = new MyVector();
        DrawLine(OutTex, _Brush, new ByteColor(DrawColor), (int)Pos0.x, (int)Pos0.y, (int)Pos1.x, (int)Pos1.y, ref Vect);
        return Vect;
    }

    public static Vector2 DrawLineWithTex(MyTexture OutTex, MyTexture PatternsTex, Brush _Brush, Vector2 Pos0, Vector2 Pos1)
    {
        MyVector Vect = new MyVector();
        DrawLineWithTex(OutTex, _Brush, PatternsTex, (int)Pos0.x, (int)Pos0.y, (int)Pos1.x, (int)Pos1.y, ref Vect);
        return Vect;
    }

    public static void RegisterDrawCallbackDebugMessage()
    {
        RegisterDebugCallback(new DebugCallback(DebugMethod));
    }

    private static void DebugMethod(string message)
    {
        Debug.Log("Draw Plugin: " + message);
    }

    // Global static functions for usage
    public static Texture2D LoadImage(Sprite InImage)
    {
        Texture2D OutImage = new Texture2D((int)InImage.rect.width, (int)InImage.rect.height);
        Color[] pixels = InImage.texture.GetPixels((int)InImage.textureRect.x,
                                                (int)InImage.textureRect.y,
                                                (int)InImage.textureRect.width,
                                                (int)InImage.textureRect.height);
        OutImage.wrapMode = TextureWrapMode.Clamp;
        OutImage.SetPixels(pixels);
        OutImage.Apply();
        return OutImage;
    }

    public static Texture2D LoadImage(Texture2D InImage)
    {
        Texture2D OutImage = new Texture2D((int)InImage.width, (int)InImage.height);
        OutImage.wrapMode = TextureWrapMode.Clamp;
        Color[] pixels = InImage.GetPixels();
        OutImage.SetPixels(pixels);
        OutImage.Apply();
        return OutImage;
    }

    public static Texture2D GetWhiteTexture(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        Color[] pixels = tex.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

}
