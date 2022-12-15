﻿using System.ComponentModel;
using System.Threading.Tasks;

namespace TinyClicker;

public class TinyClickerApp
{
    private readonly ScreenScanner _screenScanner;
    private readonly BackgroundWorker _backgroundWorker;
    public TinyClickerApp(BackgroundWorker backgroundWorker, ScreenScanner screenScanner)
    {
        _backgroundWorker = backgroundWorker;
        _screenScanner = screenScanner;
    }

    public void StartInBackground(bool isBluestacks)
    {
        _backgroundWorker.WorkerSupportsCancellation = true;
        _backgroundWorker.DoWork += (s, e) =>
        {
            RunLoop(_backgroundWorker);
        };

        _screenScanner.SetEmulator(isBluestacks);
        _backgroundWorker.RunWorkerAsync();
    }

    public void RunLoop(BackgroundWorker worker)
    {
        while (!worker.CancellationPending)
        {
            _screenScanner.StartIteration();
            Task.Delay(1500).Wait();
        }
    }
}
