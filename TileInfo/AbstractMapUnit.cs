using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MapEditor.TileInfo
{
    public class AbstractMapUnit
    {
        public string Name { get; private set; }
        public const int Width = 15;
        public const int Height = 15;
        public AbstractTileType[,] AbsTileType = new AbstractTileType[Width, Height];
        public int NWConnectionType = -1;
        public int NEConnectionType = -1;
        public int SWConnectionType = -1;
        public int SEConnectionType = -1;
        public int Weight = 0;

        public void Initialize(FileInfo file)
        {
            var map = new Mapfile();
            map.CreateIsoTileList(file.FullName);
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    var absTileType = new AbstractTileType();
                    foreach (var tile in map.Tile_input_list)
                    {
                        if (tile.Rx == i + 13 && tile.Ry == j + 13)
                        {
                            Name = file.Name.Trim(("."+ file.Extension).ToCharArray());
                            absTileType.TileNum = tile.TileNum;
                            absTileType.SubTile = tile.SubTile;
                            absTileType.Z = tile.Z;
                            AbsTileType[i, j] = absTileType;
                        }
                    }
                }
            }
            for (int i = 0; i < Width; i++)
            {
                foreach (var tile in map.Tile_input_list)
                {
                    if (tile.Rx == 10 && tile.Ry == i + 13 && tile.TileNum != -1 && tile.TileNum != 0)
                    {
                        NWConnectionType = i;
                    }
                    if (tile.Rx == 30 && tile.Ry == i + 13 && tile.TileNum != -1 && tile.TileNum != 0)
                    {
                        SEConnectionType = i;
                    }
                    if (tile.Ry == 10 && tile.Rx == i + 13 && tile.TileNum != -1 && tile.TileNum != 0)
                    {
                        NEConnectionType = i;
                    }
                    if (tile.Ry == 30 && tile.Rx == i + 13 && tile.TileNum != -1 && tile.TileNum != 0)
                    {
                        SWConnectionType = i;
                    }
                }
            }
            foreach (var tile in map.Tile_input_list)
            { 
                if (tile.Rx < 8 && tile.TileNum != -1 && tile.TileNum != 0)
                {
                    Weight++;
                }
            }
        }
    }
}
