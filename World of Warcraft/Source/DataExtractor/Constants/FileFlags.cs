// Copyright (c) Arctium Emulation.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace DataExtractor.Constants
{
    [Flags]
    enum FileFlags : int
    {
        None       = 0,
        DataOffset = 1,
        Unknown    = 2, // Some Index data stuff?!
        Index      = 4,
    }
}
