// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Pipeline.Telemetry.Abstractions;

/// <summary>
/// Interface for collecting metrics from pipeline operations.
/// </summary>
public interface IMetrics
{
    /// <summary>
    /// Increments a counter metric.
    /// </summary>
    /// <param name="metric">The metric name.</param>
    /// <param name="label">The label for categorization.</param>
    void IncrementCounter(string metric, string label);

    /// <summary>
    /// Records a histogram observation (typically for latency measurements).
    /// </summary>
    /// <param name="metric">The metric name.</param>
    /// <param name="label">The label for categorization.</param>
    /// <param name="valueMs">The value in milliseconds.</param>
    void ObserveHistogram(string metric, string label, long valueMs);
}
