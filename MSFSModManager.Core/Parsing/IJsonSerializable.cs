// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using Newtonsoft.Json.Linq;

namespace MSFSModManager.Core
{
    public interface IJsonSerializable
    {
        JToken Serialize();
    }
}