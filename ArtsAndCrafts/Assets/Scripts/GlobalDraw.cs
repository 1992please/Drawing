﻿using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

enum EDrawMode
{
    Pin,
    Brush,
    Pattern,
    Figure,
    Fill
}

public class GlobalDraw : MonoBehaviour
{
    public Texture2D BrushTexture;
    public Texture2D PatternTexture;
    [Range(.01f, 2)]
    public float BrushSpacing = .1f;
    public InputField ThicknessInput;
    public Texture2D BaseDrawTexture;
    public RawImage UIImage;
    public event Action<Texture2D> OnClickSendImage;
    public static GlobalDraw singleton;
    private Brush CurrentBrush;
    private Texture2D OutTexture;
    private int Size;
    private Vector2 OldCoord;
    private Color DrawColor;
    private bool bDraw;
    private EDrawMode DrawMode;
    private Vector2 TransRatio;

    [Range(0, 50)]
    private int HistorySize;
    private List<byte[]> History = new List<byte[]>();
    private int HistoryCurrentIndex;

    public void SetDrawColor(Color NewColor)
    {
        DrawColor = NewColor;
    }

    private void Awake()
    {
        if (!singleton)
            singleton = this;

        OutTexture = Draw.LoadImage(BaseDrawTexture);
        UIImage.texture = OutTexture;
        Draw.SetPaintTexture(OutTexture);
        Draw.SetCurrentPatternTexture(Draw.LoadImage(PatternTexture));

        DrawMode = EDrawMode.Pin;
        HistoryCurrentIndex = -1;
    }

    private void Start()
    {
        Draw.RegisterDrawCallbackDebugMessage();
        BrushTexture = Draw.LoadImage(BrushTexture);
        CurrentBrush = new Brush(BrushTexture, BrushSpacing);

        DrawColor = Color.black;
        SetThickness();
        TransRatio.x = OutTexture.width / UIImage.rectTransform.rect.width;
        TransRatio.y = OutTexture.height / UIImage.rectTransform.rect.height;
        SaveToHistory();
    }

    public void OnClickSend()
    {
        if (OnClickSendImage != null)
            OnClickSendImage(OutTexture);
    }

    public void OnClickFill()
    {
        DrawMode = EDrawMode.Fill;
    }

    public void OnClickPin()
    {
        DrawMode = EDrawMode.Pin;
    }

    public void OnClickBrush()
    {
        DrawMode = EDrawMode.Brush;
    }

    public void OnClickPattern()
    {
        DrawMode = EDrawMode.Pattern;
    }

    public void SetThickness()
    {
        Size = int.Parse(ThicknessInput.text);

        BrushTexture = Draw.LoadImage(BrushTexture);
        CurrentBrush = new Brush(BrushTexture, BrushSpacing);

        CurrentBrush.Resize(Size);
    }

    public void OnDrawUp()
    {
        bDraw = false;
        SaveToHistory();
    }

    public void OnDrawDown()
    {
        bDraw = true;

        switch (DrawMode)
        {
            case EDrawMode.Fill:
                {
                    Draw.FloodFillArea(OutTexture, GetMousePositionOnDrawTexture(), DrawColor);
                    OutTexture.Apply();
                }
                break;
            case EDrawMode.Pin:
                {
                    OldCoord = GetMousePositionOnDrawTexture();
                    if (bDraw)
                    {
                        Draw.DrawBrushTip(OutTexture, CurrentBrush, DrawColor, OldCoord);

                        OutTexture.Apply();
                    }
                }
                break;
            case EDrawMode.Pattern:
                {
                    OldCoord = GetMousePositionOnDrawTexture();
                    if (bDraw)
                    {
                        Draw.DrawBrushTipWithTex(OutTexture, CurrentBrush, OldCoord);

                        OutTexture.Apply();
                    }
                }
                break;
        }
    }

    public void OnDrawDrag()
    {
        switch (DrawMode)
        {
            case EDrawMode.Pin:
                {
                    Vector2 CursorPosition = GetMousePositionOnDrawTexture();

                    Vector2 DPosition = (CursorPosition - OldCoord);
                    float DPositionMang = DPosition.magnitude;
                    CurrentBrush.Direction = DPosition / DPositionMang;
                    if (bDraw && CurrentBrush.Spacing < DPositionMang)
                    {
                        OldCoord = Draw.DrawLine(OutTexture, CurrentBrush, DrawColor, OldCoord, CursorPosition);
                        //OldCoord = CursorPositionRelative;
                        OutTexture.Apply();
                    }
                }
                break;
            case EDrawMode.Pattern:
                {
                    Vector2 CursorPosition = GetMousePositionOnDrawTexture();

                    Vector2 DPosition = (CursorPosition - OldCoord);
                    float DPositionMang = DPosition.magnitude;
                    CurrentBrush.Direction = DPosition / DPositionMang;
                    if (bDraw && CurrentBrush.Spacing < DPositionMang)
                    {
                        OldCoord = Draw.DrawLineWithTex(OutTexture, CurrentBrush, OldCoord, CursorPosition);
                        OutTexture.Apply();
                    }
                }
                break;
        }
    }

    public void OnDrawExit()
    {
        bDraw = false;
    }

    Vector2 GetMousePositionOnDrawTexture()
    {
        Vector2 UIImagePos = UIImage.rectTransform.position;
        Vector2 CursorPosition = Input.mousePosition;

        Vector2 CursorPositionRelative = CursorPosition - (UIImagePos + UIImage.rectTransform.rect.min);
        CursorPositionRelative.x *= TransRatio.x;
        CursorPositionRelative.y *= TransRatio.y;
        return CursorPositionRelative;
    }


    // History Functions

    public void OnHistoryForward()
    {
        if(HistoryCurrentIndex < History.Count - 1)
        {
            HistoryCurrentIndex++;
            SetOutTexture(History[HistoryCurrentIndex]);

        }
    } 

    public void OnHistoryBackward()
    {
        if (HistoryCurrentIndex > 0)
        {
            HistoryCurrentIndex--;
            SetOutTexture(History[HistoryCurrentIndex]);
        }
    }

    void SaveToHistory()
    {
        if(HistoryCurrentIndex < History.Count - 1)
        {
            History.RemoveRange(HistoryCurrentIndex + 1, History.Count - 1 - HistoryCurrentIndex);
        }

        History.Add(OutTexture.GetRawTextureData());
        HistoryCurrentIndex++;
    }

    void SetOutTexture(byte[] TexData)
    {
        OutTexture.LoadRawTextureData(TexData);
        Draw.SetPaintTexture(OutTexture);
        OutTexture.Apply();
    }
}
