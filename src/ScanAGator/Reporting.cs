﻿using System.IO;

namespace ScanAGator;

public static class Reporting
{
    public static void AnalyzeLinescanFolder(LineScan.LineScanFolder2 lsFolder, LineScanSettings settings)
    {
        string analysisFolder = Path.Combine(lsFolder.FolderPath, "ScanAGator");
        if (!Directory.Exists(analysisFolder))
            Directory.CreateDirectory(analysisFolder);

        // save CSVs for each individual frame
        RatiometricLinescan[] linescans = lsFolder.GetRatiometricLinescanFrames(settings);
        for (int i = 0; i < lsFolder.FrameCount; i++)
        {
            linescans[i].SaveCsv(Path.Combine(analysisFolder, $"Frame-{i + 1}.csv"));
            linescans[i].SaveJson(Path.Combine(analysisFolder, $"Frame-{i + 1}.json"));
        }

        // create plots showing each frame
        Plot.PlotRaw(linescans, Path.GetFileName(lsFolder.FolderPath), Path.Combine(analysisFolder, $"Frames-raw.png"));
        Plot.PlotDGoR(linescans, Path.GetFileName(lsFolder.FolderPath), Path.Combine(analysisFolder, $"Frames-dff.png"));
        
        if (lsFolder.FrameCount > 1)
        {
            // save average CSV
            RatiometricLinescan averageLinescan = lsFolder.GetRatiometricLinescanAverage(settings);
            averageLinescan.SaveCsv(Path.Combine(analysisFolder, $"Frame-average.csv"));
            averageLinescan.SaveJson(Path.Combine(analysisFolder, $"Frame-average.json"));

            // create plot showing average image analysis
            Plot.PlotDGoRAverage(averageLinescan, Path.GetFileName(lsFolder.FolderPath), Path.Combine(analysisFolder, $"Frames-dff-average.png"));
        }

    }
}