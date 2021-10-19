﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Point = OpenCvSharp.Point;

namespace TinyClicker
{
    internal static class Clicker
    {
        static bool suspended = false;

        public static Dictionary<string, int> matchedImages = new Dictionary<string, int>();
        public static Dictionary<string, Image> images = Actions.FindImages();
        public static Dictionary<string, Mat> templates = Actions.MakeTemplates(images);

        public static void StartClicker()
        {
            //PlayRaffle();
            //Thread.Sleep(10000);
            int processId = Actions.processId;

            while (processId != -1 && !suspended)
            {
                MatchImages();
                string dateTimeNow = DateTime.Now.ToString("HH:mm:ss");
                foreach (var image in matchedImages)
                {
                    Console.WriteLine(dateTimeNow + " Found {0}", image.Key);
                }
                if (matchedImages.Count == 0)
                {
                    Console.WriteLine(dateTimeNow + " Nothing was found");
                }
                PerformActions();
                Thread.Sleep(1000); // Object detection performed ~once a second
                matchedImages.Clear();
            }
        }

        static void MatchImages()
        {
            Image gameWindow = Actions.MakeScreenshot();
            var windowBitmap = new Bitmap(gameWindow);
            gameWindow.Dispose();

            Mat reference = BitmapConverter.ToMat(windowBitmap);
            windowBitmap.Dispose();

            foreach (var template in templates)
            {
                //var imageBitmap = new Bitmap(image.Value);
                //Mat template = BitmapConverter.ToMat(imageBitmap);
                //imageBitmap.Dispose();
                //MatType.CV_32FC1
                using (Mat res = new(reference.Rows - template.Value.Rows + 1, reference.Cols - template.Value.Cols + 1, MatType.CV_32FC1))
                {
                    //Convert input images to gray
                    Mat gref = reference.CvtColor(ColorConversionCodes.BGR2GRAY);
                    Mat gtpl = template.Value.CvtColor(ColorConversionCodes.BGR2GRAY);

                    Cv2.MatchTemplate(gref, gtpl, res, TemplateMatchModes.CCoeffNormed);
                    Cv2.Threshold(res, res, 0.7, 1.0, ThresholdTypes.Tozero);
                    gref.Dispose();
                    gtpl.Dispose();
                    GC.Collect();

                    while (!suspended)
                    {
                        double minval, maxval, threshold = 0.78; // default 0.5
                        Point minloc, maxloc;
                        Cv2.MinMaxLoc(res, out minval, out maxval, out minloc, out maxloc);
                        res.Dispose();
                        
                        if (maxval >= threshold)
                        {
                            if (!matchedImages.ContainsKey(template.Key))
                            {
                                matchedImages.Add(template.Key, Actions.MakeParam(maxloc.X, maxloc.Y));
                                //Console.WriteLine(image.Key + " is found");
                            }
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            //GC.Collect(); // Important
        }

        static void PerformActions()
        {
            foreach (var key in matchedImages.Keys)
            {
                switch (key)
                {
                    case "roofCustomizationWindow": Actions.ExitRoofCustomizationMenu(); break;
                    case "hurryConstructionPrompt": Actions.CancelHurryConstruction(); break;
                    case "closeAd": 
                    case "closeAd_2":
                    case "closeAd_3":
                    case "closeAd_4":
                    case "closeAd_5": 
                    case "closeAd_6": 
                    case "closeAd_7": 
                    case "closeAd_8": Actions.CloseAd(); break;
                    case "continueButton": Actions.PressContinue(); break;
                    case "foundCoinsChuteNotification": Actions.CloseChuteNotification(); break;
                    case "restockButton": Actions.Restock(); break;
                    case "freeBuxCollectButton": Actions.CollectFreeBux(); break;
                    case "freeBuxButton": Actions.PressFreeBuxButton(); break;
                    case "giftChute": Actions.ClickOnChute(); break;
                    case "backButton": Actions.PressExitButton(); break;
                    case "elevatorButton": Actions.RideElevator(); break;
                    case "questButton": Actions.PressQuestButton(); break;
                    case "completedQuestButton": Actions.CompleteQuest(); break;
                    case "watchAdPromptBux": Actions.WatchAd(); break;
                    case "findBitizens": Actions.FindBitizens(); break;
                    case "deliverBitizens": Actions.DeliverBitizens(); break;
                    default: break;
                }
                break;
            }
        }
    }
}
