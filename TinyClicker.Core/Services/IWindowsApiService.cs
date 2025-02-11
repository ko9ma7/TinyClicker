﻿using System.Drawing;

namespace TinyClicker.Core.Services;

public interface IWindowsApiService
{
    int MakeLParam(int x, int y);
    Image GetGameScreenshot();
    void SendClick(int location);
    void SendClick(int x, int y);
    void SendEscapeButton();
    int GetRelativeCoordinates(int x, int y);
}