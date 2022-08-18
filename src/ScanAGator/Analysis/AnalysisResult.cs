﻿using ScanAGator.Imaging;
using System;
using System.Reflection;

namespace ScanAGator.Analysis;

/// <summary>
/// This class contains logic for calculating ΔG/R from analysis settings containing a single ratiometric image
/// </summary>
public class AnalysisResult
{
    public readonly AnalysisSettings Settings;
    public readonly ImageData GreenImageData;
    public readonly ImageData RedImageData;
    public IntensityCurve GreenCurve;
    public IntensityCurve RedCurve;
    public IntensityCurve SmoothGreenCurve;
    public IntensityCurve SmoothRedCurve;
    public IntensityCurve SmoothDeltaGreenCurve;
    public IntensityCurve SmoothDeltaGreenOverRedCurve;

    public static Version Version => new Version(4, 0); // EDIT THIS MANUALLY

    public static string VersionString => $"Scan-A-Gator v{Version.Major}.{Version.Minor}";

    public AnalysisResult(AnalysisSettings settings)
    {
        Settings = settings;

        // images loaded and turned into raw curves
        GreenImageData = settings.Image.GreenData;
        RedImageData = settings.Image.RedData;

        // get the intensity curves for the structure of interest
        GreenCurve = new IntensityCurve(GreenImageData, settings.Structure);
        RedCurve = new IntensityCurve(RedImageData, settings.Structure);

        // calculate smooth curves
        SmoothGreenCurve = GreenCurve.LowPassFiltered(settings.FilterPx);
        SmoothRedCurve = RedCurve.LowPassFiltered(settings.FilterPx);

        // calculate ratios from smoothed curves
        double greenCurveBaseline = GreenCurve.GetMean(settings.Baseline);
        SmoothDeltaGreenCurve = SmoothGreenCurve - greenCurveBaseline;
        SmoothDeltaGreenOverRedCurve = SmoothDeltaGreenCurve / SmoothRedCurve * 100;
    }
}