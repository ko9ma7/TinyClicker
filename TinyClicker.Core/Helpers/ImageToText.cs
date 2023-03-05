﻿using System;
using Tesseract;
using System.Drawing;
using System.Text.RegularExpressions;

namespace TinyClicker.Core.Helpers;

public class ImageToText
{
    private readonly TesseractEngine _tesseract;
    private readonly ImageEditor _imageEditor;

    public ImageToText(ImageEditor editor)
    {
        _tesseract = new TesseractEngine(@"./tessdata", "pixel", EngineMode.LstmOnly);
        _imageEditor = editor;
    }

    public int ParseBalance(Image window)
    {
        var sourceImage = _imageEditor.GetAdjustedBalanceImage(window);
        string result;
        try
        {
            using (var page = _tesseract.Process(sourceImage, PageSegMode.SingleLine))
            {
                result = page.GetText().Trim();
                sourceImage.Dispose();

                return ResultToBalance(result);
            }
        }
        catch (Exception)
        {
            return -1;
        }
    }

    private int ResultToBalance(string result)
    {
        if (result.Contains('M'))
        {
            int endIndex = result.IndexOf('M');
            result = result[..endIndex];
            int dotIndex = result.IndexOf('.');
            if (dotIndex == 1)
            {
                result = TrimWithRegex(result);
                result += "000";
            }
            else if (dotIndex == 2)
            {
                result = TrimWithRegex(result);
                result += "0000";
            }
            else if (dotIndex == 3)
            {
                result = TrimWithRegex(result);
                result += "00000";
            }

            return Convert.ToInt32(result);
        }
        else if (result.Contains(' '))
        {
            int endIndex = result.IndexOf(' ');
            result = result[..endIndex];

            return Convert.ToInt32(TrimWithRegex(result));
        }
        else
        {
            return Convert.ToInt32(TrimWithRegex(result));
        }
    }

    string TrimWithRegex(string str)
    {
        return Regex.Replace(str, "[^0-9]", "").Trim();
    }
}
