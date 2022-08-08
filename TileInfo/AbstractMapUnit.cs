using MapEditor.NonTileObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Rampastring.Tools;
using MapEditor.NonTileObjects;

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
        public List<Infantry> InfantryList { get; private set; }
        public List<Structure> StructureList { get; private set; }
        public List<Terrain> TerrainList { get; private set; }
        public List<Aircraft> AircraftList { get; private set; }
        public List<Smudge> SmudgeList { get; private set; }
        public List<Overlay> OverlayList { get; private set; }
        public List<Waypoint> WaypointList { get; private set; }

        public void Initialize(FileInfo file)
        {
            UnitList = new List<Unit>();
            InfantryList = new List<Infantry>();
            StructureList = new List<Structure>();
            TerrainList = new List<Terrain>();
            AircraftList = new List<Aircraft>();
            SmudgeList = new List<Smudge>();
            OverlayList = new List<Overlay>();
            WaypointList = new List<Waypoint>();

            var map = new MapFile();
            map.CreateIsoTileList(file.FullName);
            var overlayList = map.ReadOverlay(file.FullName);

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
                    foreach (var overlay in overlayList)
                    {
                        if (overlay.Tile.Rx == i + 13 && overlay.Tile.Ry == j + 13)
                        {
                            overlay.Tile.Rx = (ushort)i;
                            overlay.Tile.Ry = (ushort)j;
                            OverlayList.Add(overlay);
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
                if (tile.Rx < 8 && tile.TileNum == 327)
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
                    if (unit != null && unit.RelativeX < 15 && unit.RelativeX >= 0 && unit.RelativeY < 15 && unit.RelativeY >= 0)
                        UnitList.Add(unit);
                }
            }
            if (mapFile.SectionExists("Infantry"))
            {
                var infantrySection = mapFile.GetSection("Infantry");
                foreach (var infantryString in infantrySection.Keys)
                {
                    var infantry = new Infantry();
                    infantry.Initialize(infantryString.Value);
                    if (infantry != null && infantry.RelativeX < 15 && infantry.RelativeX >= 0 && infantry.RelativeY < 15 && infantry.RelativeY >= 0)
                        InfantryList.Add(infantry);
                }
            }
            if (mapFile.SectionExists("Structures"))
            {
                var structureSection = mapFile.GetSection("Structures");
                foreach (var structureString in structureSection.Keys)
                {
                    var structure = new Structure();
                    structure.Initialize(structureString.Value);
                    if (structure != null && structure.RelativeX < 15 && structure.RelativeX >= 0 && structure.RelativeY < 15 && structure.RelativeY >= 0)
                        StructureList.Add(structure);
                }
            }
            if (mapFile.SectionExists("Terrain"))
            {
                var terrainSection = mapFile.GetSection("Terrain");
                foreach (var terrainLine in terrainSection.Keys)
                {
                    var terrain = new Terrain();
                    terrain.Initialize(terrainLine);
                    if (terrain != null && terrain.RelativeX < 15 && terrain.RelativeX >= 0 && terrain.RelativeY < 15 && terrain.RelativeY >= 0)
                        TerrainList.Add(terrain);
                }
            }
            if (mapFile.SectionExists("Aircraft"))
            {
                var aircraftSection = mapFile.GetSection("Aircraft");
                foreach (var aircraftString in aircraftSection.Keys)
                {
                    var aircraft = new Aircraft();
                    aircraft.Initialize(aircraftString.Value);
                    if (aircraft != null && aircraft.RelativeX < 15 && aircraft.RelativeX >= 0 && aircraft.RelativeY < 15 && aircraft.RelativeY >= 0)
                        AircraftList.Add(aircraft);
                }
            }
            if (mapFile.SectionExists("Smudge"))
            {
                var smudgeSection = mapFile.GetSection("Smudge");
                foreach (var smudgeString in smudgeSection.Keys)
                {
                    var smudge = new Smudge();
                    smudge.Initialize(smudgeString.Value);
                    if (smudge != null && smudge.RelativeX < 15 && smudge.RelativeX >= 0 && smudge.RelativeY < 15 && smudge.RelativeY >= 0)
                        SmudgeList.Add(smudge);
                }
            }
            if (mapFile.SectionExists("Waypoints"))
            {
                var waypointSection = mapFile.GetSection("Waypoints");
                foreach (var waypointLine in waypointSection.Keys)
                {
                    var waypoint = new Waypoint();
                    waypoint.Initialize(waypointLine);
                    if (waypoint != null && waypoint.RelativeX < 15 && waypoint.RelativeX >= 0 && waypoint.RelativeY < 15 && waypoint.RelativeY >= 0)
                        WaypointList.Add(waypoint);
                }
            }
        }
    }
}
