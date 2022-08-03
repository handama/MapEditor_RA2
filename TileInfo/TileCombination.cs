using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapEditor.TileInfo
{
    public class TileCombination
    {
        public int TileNum { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; private set; }
        public bool IsOnMap { get; private set; }

        public bool NWConnected { get; set; } = false;
        public bool NEConnected { get; set; } = false;
        public bool SEConnected { get; set; } = false;
        public bool SWConnected { get; set; } = false;
        public TileCombinationType NWConnectedType { get; set; }
        public TileCombinationType NEConnectedType { get; set; }
        public TileCombinationType SWConnectedType { get; set; }
        public TileCombinationType SEConnectedType { get; set; }
        public void Initialize(int x, int y, int z, int tileNum, AbstractMap map)
        {
            TileNum = tileNum;
            X = x;
            Y = y;
            Z = z;
            bool isOnMap = false;
            if (Y > map.Size[0] - X - 2
                && Y < 2 * map.Size[1] + map.Size[0] + 1 - X + 2
                && Y < X + map.Size[0] + 2
                && Y > X - map.Size[0] - 2)
            {
                isOnMap = true;
            }
            IsOnMap = isOnMap;
        }
    }
}
