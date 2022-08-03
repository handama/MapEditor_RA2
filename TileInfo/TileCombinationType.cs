using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static MapEditor.AbstractMap;

namespace MapEditor.TileInfo
{
    public class TileCombinationType
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int TileNum { get; set; }
        public int[,] AbsTileUsed { get; set; }
        public int[,] ZList { get; set; }
        public AbstractTileType[,] AbsTileType { get; set; }
        public bool NWCanConnect { get; set; } = false;
        public bool NECanConnect { get; set; } = false;
        public bool SECanConnect { get; set; } = false;
        public bool SWCanConnect { get; set; } = false;
        //{TileNum,x,y,z}
        public int[,] NWConnectOptions { get; set; }
        public int[,] NEConnectOptions { get; set; }
        public int[,] SWConnectOptions { get; set; }
        public int[,] SEConnectOptions { get; set; }

        public void InitializeAfterJson()
        {
            AbsTileType = new AbstractTileType[Width, Height];
            int subTileIndex = 0;
            for (int j = 0; j < Height; j++)
            {
                for (int i = 0; i < Width; i++)
                {
                    var absTileType = new AbstractTileType();
                    absTileType.TileNum = TileNum;
                    absTileType.SubTile = subTileIndex;
                    subTileIndex++;
                    absTileType.Z = ZList[j, i];
                    absTileType.Used = Convert.ToBoolean(AbsTileUsed[j, i]);
                    AbsTileType[i, j] = absTileType;
                }
            }
        }

        public void Initialize(int tileNum, bool[,] usedTiles, int[,] z)
        {
            TileNum = tileNum;
            Width = usedTiles.GetLength(0);
            Height = usedTiles.GetLength(1);
            AbsTileType = new AbstractTileType[Width, Height];

            int subTileIndex = 0;
            for (int i = 0; i < Height; i++)
            { 
                for (int j = 0; j < Width; j++)
                {
                    var absTileType = new AbstractTileType();
                    absTileType.TileNum = tileNum;
                    absTileType.SubTile = subTileIndex;
                    subTileIndex++;
                    absTileType.Z = z[j, i];
                    absTileType.Used = usedTiles[j, i];
                    AbsTileType[j, i] = absTileType;
                }
            }
        }
    }
}
