// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

namespace MSFSModManager.Core
{
    class NullLogger : ILogger
    {
        public void Log(LogLevel level, string message)
        {
        }
    }

    public class GlobalLogger
    {
        public static ILogger Instance = new NullLogger();

        public static void Log(LogLevel logLevel, string message)
        {
            Instance.Log(logLevel, message);
        }
    }
}