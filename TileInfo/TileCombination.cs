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
        public int Entropy { get; set; }
        public void Initialize(int x, int y, int z, int tileNum)
        {
            TileNum = tileNum;
            X = x;
            Y = y;
            Z = z;
            bool isOnMap = false;
            if (Y > AbstractMap.Size[0] - X - 2
                && Y < 2 * AbstractMap.Size[1] + AbstractMap.Size[0] + 1 - X + 2
                && Y < X + AbstractMap.Size[0] + 2
                && Y > X - AbstractMap.Size[0] - 2)
            {
                isOnMap = true;
            }
            IsOnMap = isOnMap;

            var tcType = this.GetTileCombinationType();
            if (!tcType.NECanConnect)
                NEConnected = true;
            if (!tcType.SECanConnect)
                SEConnected = true;
            if (!tcType.SWCanConnect)
                SWConnected = true;
            if (!tcType.NWCanConnect)
                NWConnected = true;
        }
        public TileCombinationType GetTileCombinationType()
        {
            foreach(var tcType in AbstractMap.TileCombinationTypeList)
            {
                if (tcType.TileNum == TileNum)
                    return tcType;
            }
            return null;
        }
        public void SetEntropy()
        {
            Entropy = 100;
            var tcType = this.GetTileCombinationType();

            if (!IsOnMap || (NWConnected && NEConnected && SWConnected && SEConnected))
            {
                Entropy = 0;
                return;
            }
            for (int i = -3; i < tcType.Width + 3; i++)
            {
                for (int j = -3; j < tcType.Height + 3; j++)
                {
                    if (X + i >= AbstractMap.Width + AbstractMap.Height || Y + j >= AbstractMap.Width + AbstractMap.Height || X + i < 0 || Y + j < 0)
                    {
                        Entropy = 0;
                        return;
                    }
                }
            }

            if (NWConnected)
                Entropy -= 15;
            if (SWConnected)
                Entropy -= 15;
            if (SEConnected)
                Entropy -= 15;
            if (NEConnected)
                Entropy -= 15;

            bool NEHasTC = false;
            bool NWHasTC = false;
            bool SEHasTC = false;
            bool SWHasTC = false;
            for (int x = X - 3; x < X + tcType.Width; x++)
            {
                for (int y = Y - 3; y < Y; y++)
                {
                    if (AbstractMap.AbsTile[x, y].Edited)
                        NEHasTC = true;
                }
            }
            for (int x = X - 3; x < X; x++)
            {
                for (int y = Y ; y < Y + 3 + tcType.Height; y++)
                {
                    if (AbstractMap.AbsTile[x, y].Edited)
                        NWHasTC = true;
                }
            }
            for (int x = X + tcType.Width; x < X + tcType.Width + 3; x++)
            {
                for (int y = Y - 3; y < Y + tcType.Height; y++)
                {
                    if (AbstractMap.AbsTile[x, y].Edited)
                        SEHasTC = true;
                }
            }
            for (int x = X; x < X + tcType.Width + 3; x++)
            {
                for (int y = Y + tcType.Height; y < Y + tcType.Height + 3; y++)
                {
                    if (AbstractMap.AbsTile[x, y].Edited)
                        SWHasTC = true;
                }
            }
            if (NEHasTC)
                Entropy -= 10;
            if (NWHasTC)
                Entropy -= 10;
            if (SEHasTC)
                Entropy -= 10;
            if (SWHasTC)
                Entropy -= 10;
        }
    }
}
