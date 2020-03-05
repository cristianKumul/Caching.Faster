﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace FASTER.core
{
    /// <summary>
    /// Scan buffering mode
    /// </summary>
    public enum ScanBufferingMode
    {
        /// <summary>
        /// Buffer only current page being scanned
        /// </summary>
        SinglePageBuffering,

        /// <summary>
        /// Buffer current and next page in scan sequence
        /// </summary>
        DoublePageBuffering
    }

    /// <summary>
    /// Scan iterator interface for FASTER log
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    /// <typeparam name="Value"></typeparam>
    public interface IFasterScanIterator<Key, Value> : IDisposable
    {
        /// <summary>
        /// Get next record
        /// </summary>
        /// <param name="recordInfo"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>True if record found, false if end of scan</returns>
        bool GetNext(out RecordInfo recordInfo, out Key key, out Value value);

        /// <summary>
        /// Current address
        /// </summary>
        long CurrentAddress { get; }

        /// <summary>
        /// NextAddress address
        /// </summary>
        long NextAddress { get; }
    }
}
