// Copyright (c) Arctium Emulation.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DataExtractor.Maps.Defines
{
    class Map
    {
        public ushort Id { get; set; } // 11 Bits
        public string Name { get; set; }
        public ConcurrentDictionary<ushort, List<MapChunk>> Tiles { get; } =  new ConcurrentDictionary<ushort, List<MapChunk>>();
    }
}
