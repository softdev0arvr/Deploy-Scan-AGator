﻿using SciTIF.LUTs;
using System.Text;

namespace ImageRatioTool;

/// <summary>
/// Results of ROI analysis performed in a rectangular selection of a ratiometric image
/// </summary>
public struct RoiAnalysis
{
    public readonly double[] SortedValues;
    public readonly int NoiseFloorIndex;
    public readonly double NoiseFloor;
    public readonly double Threshold;
    public readonly int ThresholdIndex;
    public readonly Rectangle Rect;
    public int PixelsTotal => SortedValues.Length;
    public int PixelsAboveThreshold => SortedValues.Length - ThresholdIndex - 1;
    public double FractionAboveThreshold => PixelsAboveThreshold / (double)PixelsTotal;

    public double[] SortedRatios;
    public int MedianIndex => SortedRatios.Length / 2;
    public double MedianRatio => SortedRatios[MedianIndex];

    /// <summary>
    /// Analyze the source images using the given rectangular ROI
    /// </summary>
    /// <param name="FullRed">Full size red source image</param>
    /// <param name="FullGreen">Full size green source image</param>
    /// <param name="rect">Rectangular ROI</param>
    /// <param name="noiseFloor">Noise floor will be calculated as this fraction of all pixel values in the ROI</param>
    /// <param name="signalThreshold">Only analyze pixels whose value is greater than this multiple of the noise floor</param>
    public RoiAnalysis(SciTIF.Image FullRed, SciTIF.Image FullGreen, Rectangle rect, double noiseFloor = 0.2, double signalThreshold = 5)
    {
        Rect = rect;
        SciTIF.Image red = FullRed.Crop(rect.Left, rect.Right, rect.Top, rect.Bottom);
        SciTIF.Image green = FullGreen.Crop(rect.Left, rect.Right, rect.Top, rect.Bottom);

        SortedValues = red.Values.OrderBy(x => x).ToArray();
        NoiseFloorIndex = (int)(SortedValues.Length * noiseFloor);
        NoiseFloor = SortedValues[NoiseFloorIndex];

        Threshold = NoiseFloor * signalThreshold;
        for (ThresholdIndex = 0; ThresholdIndex < SortedValues.Length; ThresholdIndex++)
        {
            if (SortedValues[ThresholdIndex] >= Threshold)
                break;
        }

        SortedRatios = GetThresholdedRatios(red, green, Threshold);
    }

    /// <summary>
    /// Return all green over red ratios greater than the given threshold, sorted from low to high
    /// </summary>
    /// <returns></returns>
    private static double[] GetThresholdedRatios(SciTIF.Image red, SciTIF.Image green, double threshold)
    {
        List<double> ratios = new();

        for (int x = 0; x < red.Width; x++)
        {
            for (int y = 0; y < red.Height; y++)
            {
                double redValue = red.GetPixel(x, y);
                if (redValue < threshold)
                    continue;

                double greenValue = green.GetPixel(x, y);
                double ratio = greenValue / redValue;
                ratios.Add(ratio);
            }
        }

        return ratios.OrderBy(x => x).ToArray();
    }

    public string GetSummary()
    {
        StringBuilder sb = new();
        sb.AppendLine($"X: [{Rect.Left}, {Rect.Right}] W={Rect.Width}");
        sb.AppendLine($"Y: [{Rect.Top}, {Rect.Bottom}] H={Rect.Height}");
        sb.AppendLine($"Noise floor: {NoiseFloor:N2}");
        sb.AppendLine($"Signal threshold: {Threshold:N2}");
        sb.AppendLine($"Pixels above threshold: {FractionAboveThreshold * 100:N1}%");
        sb.AppendLine(PixelsAboveThreshold > 0 ? $"G/R: {MedianRatio * 100:N3}%" : "G/R: No pixels in ROI above threshold");
        return sb.ToString();
    }
}
