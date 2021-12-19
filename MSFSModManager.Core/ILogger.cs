// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

namespace MSFSModManager.Core
{

    public enum LogLevel
    {
        Debug,
        Info,
        Output,
        Warning,
        Error,
        CriticalError
    }

    public interface ILogger
    {
        void Log(LogLevel level, string message);
    }

}