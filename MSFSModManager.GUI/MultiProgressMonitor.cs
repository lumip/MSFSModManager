// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021-2022 Lukas <lumip> Prediger

using System.Collections.Generic;
using MSFSModManager.Core;
using MSFSModManager.Core.PackageSources;

namespace MSFSModManager.GUI
{
    class MultiProgressMonitor : IProgressMonitor
    {
        private List<IProgressMonitor> _monitors;

        public MultiProgressMonitor(IEnumerable<IProgressMonitor> monitors)
        {
            _monitors = new List<IProgressMonitor>(monitors);
        }

        public MultiProgressMonitor()
        {
            _monitors = new List<IProgressMonitor>();
        }

        public void Add(IProgressMonitor monitor)
        {
            _monitors.Add(monitor);
        }

        public void Remove(IProgressMonitor monitor)
        {
            _monitors.Remove(monitor);
        }

        public void DownloadStarted(IDownloadProgressMonitor downloadProgressMonitor)
        {
            _monitors.ForEach(monitor => monitor.DownloadStarted(downloadProgressMonitor));
        }

        public void ExtractionCompleted(string packageId, IVersionNumber versionNumber)
        {
            _monitors.ForEach(monitor => monitor.ExtractionCompleted(packageId, versionNumber));
        }

        public void ExtractionStarted(string packageId, IVersionNumber versionNumber)
        {
            _monitors.ForEach(monitor => monitor.ExtractionStarted(packageId, versionNumber));
        }

        public void RequestPending(string packageId)
        {
            _monitors.ForEach(monitor => monitor.RequestPending(packageId));
        }

        public void CopyingStarted(string packageId, IVersionNumber versionNumber)
        {
            _monitors.ForEach(monitor => monitor.CopyingStarted(packageId, versionNumber));
        }

        public void CopyingCompleted(string packageId, IVersionNumber versionNumber)
        {
            _monitors.ForEach(monitor => monitor.CopyingCompleted(packageId, versionNumber));
        }
    }
}
