﻿using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TinyClicker.Core.Logging;
using TinyClicker.Core.Services;

namespace TinyClicker.Core.Logic;

public class ClickerActionsRepository
{
    private readonly IConfigService _configService;
    private readonly IWindowsApiService _windowsApiService;
    private readonly IOpenCvService _openCvService;
    private readonly ILogger _logger;

    public ClickerActionsRepository(
        IConfigService configService,
        IWindowsApiService windowsApiService,
        IOpenCvService openCvService,
        ILogger logger)
    {
        _configService = configService;
        _windowsApiService = windowsApiService;
        _openCvService = openCvService;
        _logger = logger;

        FloorPrices = CalculateFloorPrices();
    }

    private Dictionary<int, int> FloorPrices { get; }
    private DateTime AttemptNextFloorBuildAt { get; set; } = DateTime.Now;

    public void CancelHurryConstruction()
    {
        _logger.Log("Exiting the construction menu");
        ClickAndWaitSec(100, 375, 1); // Cancel action
    }

    public void CollectFreeBux(int lParam)
    {
        _logger.Log("Collecting free bux");

        var location = GetLocationFromLParam(lParam);
        _windowsApiService.SendClick(location.X + 20, location.Y + 20);
        WaitMs(300);

        if (_openCvService.TryFindOnScreen(Button.FreeBuxGiftButton, out var collectLocation))
        {
            _windowsApiService.SendClick(collectLocation.X + 20, collectLocation.Y + 20);
            WaitMs(300);
            _windowsApiService.SendClick(225, 375); // Collect bux
        }
    }

    public void ClickOnChute(int location)
    {
        _logger.Log("Clicking on the parachute");
        _windowsApiService.SendClick(location);
        WaitMs(500);
        if (_openCvService.IsImageOnScreen(GameWindow.WatchAdPromptCoins) && _configService.Config.CurrentFloor >= _configService.Config.WatchAdsFromFloor)
        {
            TryWatchAds();
        }
        else if (_configService.Config.WatchBuxAds && _configService.Config.CurrentFloor >= _configService.Config.WatchAdsFromFloor)
        {
            if (_openCvService.IsImageOnScreen(GameWindow.WatchAdPromptBux))
            {
                TryWatchAds();
            }
        }
        else
        {
            _windowsApiService.SendClick(155, 380); // Decline the video offer
        }
    }

    public void CloseAd()
    {
        _logger.Log("Closing the advertisement");

        _windowsApiService.SendClick(22, 22);
        _windowsApiService.SendClick(311, 22);
        _windowsApiService.SendClick(310, 10);
        _windowsApiService.SendClick(310, 41);
        _windowsApiService.SendClick(311, 22);
        _windowsApiService.SendClick(302, 52);
        _windowsApiService.SendClick(319, 15);
        _windowsApiService.SendClick(317, 15);

        CheckForLostAdsReward();
    }

    public void CloseChuteNotification()
    {
        _logger.Log("Closing the parachute notification");
        _windowsApiService.SendClick(165, 375); // Close the notification
    }

    public void ExitRoofCustomizationMenu()
    {
        _logger.Log("Exiting the roof customization menu");
        PressExitButton();
        WaitMs(500);
    }

    public void PressContinue(int location)
    {
        _logger.Log("Clicking continue");

        _windowsApiService.SendClick(location);
        WaitMs(500);
        MoveUp();
    }

    public void Restock()
    {
        _logger.Log("Restocking");
        MoveDown();
        WaitMs(500);
        ClickAndWaitMs(100, 480, 500); // Stock all
        ClickAndWaitMs(225, 375, 500); 

        if (_openCvService.IsImageOnScreen(Button.FullyStockedBonus))
        {
            ClickAndWaitSec(165, 375, 1); // Close the bonus tooltip
            MoveUp();
            WaitSec(1);
        }
        else
        {
            MoveUp();
            WaitSec(1);
        }
    }

    public void PressFreeBuxButton()
    {
        _logger.Log("Pressing free bux icon");
        ClickAndWaitSec(25, 130, 1);
        ClickAndWaitSec(230, 375, 1);
    }

    public void RideElevator()
    {
        _logger.Log("Riding the elevator");
        ClickAndWaitSec(21, 510, 1); // Move up

        if (_openCvService.IsImageOnScreen(Button.BackButton))
        {
            WaitMs(300);
            PressExitButton();
        }
        else if (_openCvService.TryFindOnScreen(Button.Continue, out var location))
        {
            // Click continue in case a new bitizen moved in
            _windowsApiService.SendClick(location.X, location.Y); 
        }
        else
        {
            while (!_openCvService.IsImageOnScreen(Button.MenuButton))
            {
                WaitMs(500);
            }

            MoveUp();
            _configService.Config.ElevatorRides++;
            _configService.SaveConfig();
        }
    }

    public void PressQuestButton(int location)
    {
        _logger.Log("Clicking on the quest button");
        _windowsApiService.SendClick(location);
        WaitMs(500);

        if (_openCvService.IsImageOnScreen(GameWindow.DeliverBitizens))
        {
            DeliverBitizens();
        }
        else if (_openCvService.IsImageOnScreen(GameWindow.FindBitizens))
        {
            FindBitizens();
        }
        else
        {
            ClickAndWaitMs(90, 440, 500); // Skip the quest
            ClickAndWaitMs(230, 380, 0);  // Confirm
        }
    }

    public void FindBitizens()
    {
        _logger.Log("Skipping the quest");
        ClickAndWaitMs(95, 445, 500); // Skip the quest
        ClickAndWaitMs(225, 380, 0);  // Confirm skip
    }

    public void DeliverBitizens()
    {
        _logger.Log("Delivering bitizens");
        _windowsApiService.SendClick(230, 440); // Continue
    }

    public void OpenTheGame(int location)
    {
        WaitSec(1);
        _windowsApiService.SendClick(location);
        WaitSec(7);
    }

    public void CheckForLostAdsReward()
    {
        WaitMs(500);
        if (_openCvService.IsImageOnScreen(GameWindow.AdsLostReward))
        {
            _windowsApiService.SendClick(240, 344); // Click "Keep watching"
            WaitSec(15);
        }
        else
        {
            _windowsApiService.SendClick(240, 344);
        }
    }

    public void CheckForExitButton()
    {
        if (_openCvService.IsImageOnScreen(Button.BackButton))
        {
            PressExitButton();
        }
    }

    public void CloseNewFloorMenu()
    {
        _logger.Log("Exiting from new floor menu");
        PressExitButton();
    }

    public void CloseBuildNewFloorNotification()
    {
        _logger.Log("Closing the new floor notification");
        _windowsApiService.SendClick(105, 320); // Click no
    }

    public void CompleteQuest(int location)
    {
        _logger.Log("Completing the quest");

        WaitMs(500);
        _windowsApiService.SendClick(location);
    }

    public void CollectNewScience(int location)
    {
        _logger.Log("Collecting new science");
        _windowsApiService.SendClick(location);
        WaitMs(500);
        ClickAndWaitMs(150, 110, 300);
        PressExitButton();
        WaitMs(300);
        PressExitButton();
    }

    public void CheckForNewFloor(int currentFloor, int balance)
    {
        if (currentFloor >= _configService.Config.RebuildAtFloor)
        {
            WaitMs(500);
            RebuildTower();
            return;
        }

        if (balance <= FloorPrices[currentFloor + 1] || currentFloor >= _configService.Config.RebuildAtFloor)
        {
            return;
        }

        if (_openCvService.IsImageOnScreen(Button.ElevatorButton))
        {
            WaitMs(300);
            RideElevator();
            return;
        }

        if (_openCvService.IsImageOnScreen(GameWindow.Lobby))
        {
            _logger.Log("Found lobby window");
            PressExitButton();
            MoveUp();
            return;
        }

        BuildNewFloor();

        if (_openCvService.IsImageOnScreen(GameWindow.NewFloorNoCoinsNotification))
        {
            AttemptNextFloorBuildAt = DateTime.Now.AddSeconds(3);
            _logger.Log("Not enough coins for a new floor");
            _windowsApiService.SendClick(230, 380); // continue
            return;
        }

        if (_openCvService.IsImageOnScreen(Button.BackButton))
        {
            return;
        }

        CheckForNewFloor(_configService.Config.CurrentFloor, balance - FloorPrices[currentFloor + 1]);
    }

    private void BuildNewFloor()
    {
        if (AttemptNextFloorBuildAt <= DateTime.Now)
        {
            if (_openCvService.IsImageOnScreen(Button.Continue))
            {
                _windowsApiService.SendClick(230, 380); // Continue
                return;
            }

            if (_openCvService.IsImageOnScreen(Button.BackButton))
            {
                PressExitButton();
                _logger.Log("Clicking Back button while building");
                return;
            }

            _logger.Log("Building new floor");

            ClickAndWaitMs(22, 10, 300); // Move up
            ClickAndWaitMs(300, 360, 400); // Click on a new floor

            if (_openCvService.IsImageOnScreen(Button.Continue))
            {
                _windowsApiService.SendClick(160, 380); // No thanks
                _logger.Log("Clicking Continue button while building");
            }
            else if (_openCvService.IsImageOnScreen(GameWindow.BuildNewFloorNotification))
            {
                _windowsApiService.SendClick(230, 320); // Confirm
                WaitMs(400);

                if (!_openCvService.IsImageOnScreen(GameWindow.NewFloorNoCoinsNotification))
                {
                    _configService.AddOneFloor();
                    _logger.Log("Built a new floor");
                }
                else
                {
                    AttemptNextFloorBuildAt = DateTime.Now.AddSeconds(3);
                    _logger.Log("Not enough coins for a new floor");
                }

                _windowsApiService.SendClick(22, 10); // Move up
            }
        }
        else
        {
            _logger.Log("Too early to build a floor");
            WaitSec(1);
        }
    }

    private void RebuildTower()
    {
        _logger.Log("Rebuilding the tower");
        _configService.SaveStatRebuildTime();
        _configService.Config.ElevatorRides = 0;

        ClickAndWaitMs(305, 570, 350); // menu
        ClickAndWaitMs(165, 435, 350); // rebuild menu
        ClickAndWaitMs(165, 440, 350); // rebuild button
        ClickAndWaitMs(230, 380, 350); // confirm rebuild
        ClickAndWaitMs(160, 440, 350); // continue button
        //ClickAndWaitMs(165, 405, 200); // click to claim easter GT bonus

        _configService.SetCurrentFloor(1);
    }

    public void PassTheTutorial()
    {
        _logger.Log("Passing the tutorial");
        if (_openCvService.IsImageOnScreen(Button.Continue))
        {
            ClickAndWaitMs(170, 435, 400); // Continue
        }

        MoveDown();
        WaitMs(1000);

        ClickAndWaitMs(195, 260, 200); // Build a new floor
        ClickAndWaitMs(230, 380, 200); // Confirm
        ClickAndWaitMs(20, 60, 200);   // Complete quest
        ClickAndWaitMs(170, 435, 200); // Collect bux
        ClickAndWaitMs(170, 435, 200); // Continue
        ClickAndWaitMs(190, 300, 200); // Click on a new floor
        ClickAndWaitMs(240, 150, 200); // Build a residential floor
        ClickAndWaitMs(160, 375, 200); // Continue
        ClickAndWaitMs(20, 60, 200);   // Complete quest
        ClickAndWaitMs(170, 435, 200); // Collect bux
        ClickAndWaitMs(170, 435, 1400);// Continue
        ClickAndWaitSec(21, 510, 4);   // Click on the elevator button

        // Daily rent check (in case it's past midnight)
        if (_openCvService.TryFindOnScreen(Button.FreeBuxCollectButton, out var location))
        {
            ClickAndWaitMs(location.X, location.Y, 500); // Collect daily rent
            ClickAndWaitMs(21, 510, 4000); // Click on elevator button again
        }

        ClickAndWaitMs(230, 380, 200); // Continue
        ClickAndWaitMs(20, 60, 200);   // Complete quest
        ClickAndWaitMs(170, 435, 200); // Collect bux
        ClickAndWaitMs(170, 435, 200); // Continue
        ClickAndWaitMs(190, 200, 200); // Build a new floor
        ClickAndWaitMs(225, 380, 200); // Confirm
        ClickAndWaitMs(200, 200, 200); // Open the new floor
        ClickAndWaitMs(90, 340, 200);  // Build random food floor
        ClickAndWaitMs(170, 375, 200); // Continue
        ClickAndWaitMs(20, 60, 200);   // Complete the quest
        ClickAndWaitMs(170, 435, 200); // Collect bux
        ClickAndWaitMs(170, 435, 200); // Continue
        ClickAndWaitMs(200, 200, 200); // Open the food floor
        ClickAndWaitMs(75, 210, 200);  // Open the hire menu
        ClickAndWaitMs(80, 100, 200);  // Select bitizen
        ClickAndWaitMs(230, 380, 200); // Hire him
        ClickAndWaitMs(160, 380, 200); // Continue on dream job assignment
        ClickAndWaitMs(300, 560, 200); // Exit the food store
        ClickAndWaitMs(20, 60, 200);   // Complete the quest
        ClickAndWaitMs(170, 435, 200); // Collect bux
        ClickAndWaitMs(170, 435, 200); // Continue
        ClickAndWaitMs(200, 200, 200); // Open the food store again
        ClickAndWaitSec(200, 210, 5);  // Request restock of the first item in the store

        // Wait until the floor is restocked
        while (!_openCvService.IsImageOnScreen(Button.RestockButton))
        {
            WaitMs(700);
        }

        ClickAndWaitMs(284, 188, 200); // Press restock button (easter fix)
        ClickAndWaitMs(20, 60, 200);   // Complete the quest
        ClickAndWaitMs(170, 435, 200); // Collect bux
        ClickAndWaitMs(170, 435, 200); // Continue
        ClickAndWaitMs(200, 200, 200); // Open food store again
        ClickAndWaitMs(170, 130, 200); // Click upgrade
        ClickAndWaitMs(230, 375, 200); // Confirm upgrade
        ClickAndWaitMs(165, 375, 200); // Continue
        ClickAndWaitMs(300, 560, 200); // Exit the food store
        ClickAndWaitMs(20, 60, 200);   // Complete the quest
        ClickAndWaitMs(170, 435, 200); // Collect bux
        ClickAndWaitMs(170, 435, 0);   // Collect more bux

        MoveUp();
    }

    public void RestartGame()
    {
        _logger.Log("Restarting the app");
        _windowsApiService.SendEscapeButton();
        WaitSec(1);
        _windowsApiService.SendClick(230, 380);
    }

    public void TryWatchAds()
    {
        if (_configService.Config.CurrentFloor >= _configService.Config.WatchAdsFromFloor)
        {
            _logger.Log("Watching the advertisement");
            ClickAndWaitSec(225, 375, 20);
        }
        else
        {
            _windowsApiService.SendClick(105, 380); // Decline the video offer
        }
    }

    private void MoveUp()
    {
        ClickAndWaitSec(22, 10, 1);
    }

    private void MoveDown()
    {
        _windowsApiService.SendClick(230, 537);
    }

    public void PressExitButton()
    {
        _logger.Log("Pressing the exit button");
        _windowsApiService.SendClick(305, 565);
    }

    public void PlayRaffle()
    {
        var lastRaffleTime = _configService.Config.LastRaffleTime;
        var dateTimeNow = DateTime.Now;
        if (lastRaffleTime > dateTimeNow.AddHours(-1))
        {
            return;
        }

        if (_openCvService.IsImageOnScreen(Button.Continue))
        {
            _windowsApiService.SendClick(160, 380); // Continue
        }

        _logger.Log("Playing the raffle");
        WaitMs(500);
        ClickAndWaitMs(300, 570, 500); // Open menu
        ClickAndWaitMs(275, 440, 2000); // Open raffle
        _windowsApiService.SendClick(160, 345); // Enter raffle

        _configService.Config.LastRaffleTime = dateTimeNow;
        _configService.SaveConfig();
    }

    private Point GetLocationFromLParam(int lParam) => new Point((short)lParam, lParam >> 16);

    private static void WaitSec(int seconds)
    {
        var ms = seconds * 1000;
        Task.Delay(ms).Wait();
    }

    private static void WaitMs(int milliseconds)
    {
        Task.Delay(milliseconds).Wait();
    }

    private void ClickAndWaitMs(int locationX, int locationY, int waitMs)
    {
        _windowsApiService.SendClick(locationX, locationY);
        WaitMs(waitMs);
    }

    private void ClickAndWaitSec(int locationX, int locationY, int waitSec)
    {
        _windowsApiService.SendClick(locationX, locationY);
        WaitSec(waitSec);
    }

    private Dictionary<int, int> CalculateFloorPrices()
    {
        var result = new Dictionary<int, int>();
        var floorCostDecrease = _configService.Config.FloorCostDecrease;

        for (int i = 1; i <= 9; i++)
        {
            var floorPrice = 5000 - (5000 * floorCostDecrease / 100);
            result.Add(i, floorPrice);
        }

        for (int i = 10; i <= _configService.Config.RebuildAtFloor + 1; i++)
        {
            var floorCost = 1000 * 1 * (0.5f * (i * i) + 8 * i - 117);

            if (i % 2 != 0)
            {
                floorCost += 500;
            }

            var floorPrice = floorCost - (floorCost * floorCostDecrease / 100);
            result.Add(i, (int)floorPrice);
        }

        return result;
    }
}
