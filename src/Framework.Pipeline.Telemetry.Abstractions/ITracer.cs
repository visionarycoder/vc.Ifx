// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using VisionaryCoder.Framework.Abstractions.Pipeline;

namespace VisionaryCoder.Framework.Pipeline.Telemetry.Abstractions;

public interface ITracer
{
    ISpan StartSpan(string name);
}