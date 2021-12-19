// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace MSFSModManager.Core
{

    /// <summary>
    /// Extension methods for Stream.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Asynchronous version of CopyTo.
        /// </summary>
        public static async Task CopyToAsync(
            this Stream source, Stream destination, IProgress<long> progress, CancellationToken cancellationToken = default(CancellationToken), int bufferSize = 0x1000
        )
        {
            var buffer = new byte[bufferSize];
            int bytesRead;
            long totalRead = 0;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                totalRead += bytesRead;
                progress.Report(totalRead);
            }
        }
    }
}