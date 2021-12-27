// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using MSFSModManager.Core;
using MSFSModManager.Core.PackageSources;

namespace MSFSModManager.CLI
{
    
    class ConsoleProgressMonitor : IProgressMonitor
    {

        private ConsoleStatusLines _statusLines;

        public ConsoleProgressMonitor(ConsoleStatusLines statusLines)
        {
            _statusLines = statusLines;
        }

        public void RequestPending(string packageId)
        {
            ConsoleRenderer.LineHandle line = _statusLines.GetLineHandle(packageId);
            line.Clear();
            line.Write($"Request pending for {packageId} ...");
        }

        public void DownloadStarted(IDownloadProgressMonitor monitor)
        {
            ConsoleRenderer.LineHandle line = _statusLines.GetLineHandle(monitor.PackageId);

            ProgressBar bar = new ProgressBar($"downloading {monitor.PackageId} {monitor.Version}", $"{monitor.TotalSize / (1024*1024)} MB", line);
            bar.Render();
            monitor.UserData = bar;
            monitor.DownloadProgress += OnDownloadProgress;
        }

        void OnDownloadProgress(IDownloadProgressMonitor monitor)
        {
            ProgressBar bar = (ProgressBar)monitor.UserData!;
        
            bar.Update(monitor.CurrentPercentage);
            bar.Render();
        }

        public void ExtractionCompleted(string packageId, IVersionNumber versionNumber)
        {
            ConsoleRenderer.LineHandle line = _statusLines.GetLineHandle(packageId);
            
            line.Clear();
            line.Write($"Extracting {packageId} {versionNumber} completed.");
        }

        public void ExtractionStarted(string packageId, IVersionNumber versionNumber)
        {
            ConsoleRenderer.LineHandle line = _statusLines.GetLineHandle(packageId);

            line.Clear();
            line.Write($"Extracting {packageId} {versionNumber} ...");
        }
    }
}