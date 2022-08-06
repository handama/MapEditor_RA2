using MapEditor.Technos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Rampastring.Tools;

namespace MapEditor.TileInfo
{
    public class AbstractMapUnit
    {
        public string MapUnitName;
        public const int Width = 15;
        public const int Height = 15;
        public AbstractTileType[,] AbsTileType = new AbstractTileType[Width, Height];
        public int NWConnectionType = -1;
        public int NEConnectionType = -1;
        public int SWConnectionType = -1;
        public int SEConnectionType = -1;
        public int Weight = 0;
        public List<Unit> UnitList { get; private set; }

        public void Initialize(FileInfo file)
        {
            UnitList = new List<Unit>();
            var map = new MapFile();
            map.CreateIsoTileList(file.FullName);
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    var absTileType = new AbstractTileType();
                    foreach (var tile in map.IsoTileList)
                    {
                        if (tile.Rx == i + 13 && tile.Ry == j + 13)
                        {
                            MapUnitName = file.Name.Trim((file.Extension).ToCharArray());
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
                foreach (var tile in map.IsoTileList)
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
            foreach (var tile in map.IsoTileList)
            { 
                if (tile.Rx < 8 && tile.TileNum != -1 && tile.TileNum != 0)
                {
                    Weight++;
                }
            }

            var mapFile = new IniFile(file.FullName);
            if (mapFile.SectionExists("Units"))
            {
                var unitSection = mapFile.GetSection("Units");
                foreach (var unitString in unitSection.Keys)
                {
                    var unit = new Unit();
                    unit.Initialize(unitString.Value);
                    if (unit != null)
                        UnitList.Add(unit);
                }
            }
        }
    }
}
