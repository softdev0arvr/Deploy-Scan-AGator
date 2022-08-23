﻿using ScottPlot;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ScanAGator.GUI
{
    public partial class AnalysisResultsControl : UserControl
    {
        public AnalysisResultsControl()
        {
            InitializeComponent();
            formsPlot2.AxesChanged += FormsPlot2_AxesChanged;
        }

        private void FormsPlot2_AxesChanged(object sender, EventArgs e)
        {
            formsPlot1.Plot.MatchAxis(formsPlot2.Plot, horizontal: true, vertical: false);
            formsPlot1.Refresh();
        }

        public void ShowResult(Analysis.AnalysisResult result)
        {
            double[] xs = GetTimesMsec(result);

            formsPlot1.Plot.Clear();
            formsPlot1.Plot.AddScatterPoints(xs, result.GreenCurve.Values, Color.FromArgb(30, Color.Green));
            formsPlot1.Plot.AddScatterPoints(xs, result.RedCurve.Values, Color.FromArgb(30, Color.Red));
            formsPlot1.Plot.AddScatterLines(xs, result.SmoothGreenCurve.Values, Color.Green);
            formsPlot1.Plot.AddScatterLines(xs, result.SmoothRedCurve.Values, Color.Red);
            ShadeBaseline(formsPlot1.Plot, result);
            formsPlot1.Plot.SetAxisLimits(yMin: 0);
            formsPlot1.Plot.YLabel("Fluorescence (AFU)");
            formsPlot1.Refresh();

            formsPlot2.Plot.Clear();
            formsPlot2.Plot.AddScatterLines(xs, result.SmoothDeltaGreenOverRedCurve.Values);
            ShadeBaseline(formsPlot2.Plot, result);
            formsPlot2.Plot.AddHorizontalLine(0, Color.Black, 0, ScottPlot.LineStyle.Dash);
            formsPlot2.Plot.YLabel("ΔF/F (%)");
            formsPlot2.Plot.MatchLayout(formsPlot1.Plot);
            formsPlot2.Refresh();

            dataExportControl1.ShowResult(result);
        }

        private static void ShadeBaseline(Plot plt, Analysis.AnalysisResult result)
        {
            plt.AddHorizontalSpan(
                xMin: result.Settings.Baseline.Min * result.Settings.Xml.MsecPerPixel,
                xMax: result.Settings.Baseline.Max * result.Settings.Xml.MsecPerPixel,
                color: Color.FromArgb(20, Color.Black));
        }

        private double[] GetTimesMsec(Analysis.AnalysisResult result)
        {
            return DataGen.Consecutive(result.GreenCurve.Values.Length, result.Settings.Xml.MsecPerPixel);
        }
    }
}
