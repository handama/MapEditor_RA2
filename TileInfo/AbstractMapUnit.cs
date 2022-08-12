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
        public int Width = Constants.SideLength;
        public int Height = Constants.SideLength;
        public AbstractTileType[,] AbsTileType = new AbstractTileType[Constants.SideLength, Constants.SideLength];
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
                        if (tile.Rx == i + Constants.StartingXY && tile.Ry == j + Constants.StartingXY)
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
                        if (overlay.Tile.Rx == i + Constants.StartingXY && overlay.Tile.Ry == j + Constants.StartingXY)
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
                    if (tile.Rx == Constants.StartingXY - 3 && tile.Ry == i + Constants.StartingXY && tile.TileNum == WorkingMap.IndicatorNum)
                    {
                        NWConnectionType = i;
                    }
                    if (tile.Rx == Constants.StartingXY + Width + 2 && tile.Ry == i + Constants.StartingXY && tile.TileNum == WorkingMap.IndicatorNum)
                    {
                        SEConnectionType = i;
                    }
                    if (tile.Ry == Constants.StartingXY - 3 && tile.Rx == i + Constants.StartingXY && tile.TileNum == WorkingMap.IndicatorNum)
                    {
                        NEConnectionType = i;
                    }
                    if (tile.Ry == Constants.StartingXY + Width + 2 && tile.Rx == i + Constants.StartingXY && tile.TileNum == WorkingMap.IndicatorNum)
                    {
                        SWConnectionType = i;
                    }
                }
            }
            foreach (var tile in map.IsoTileList)
            { 
                if (tile.Rx < Constants.StartingXY - 5 && tile.TileNum == WorkingMap.IndicatorNum)
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
                    if (unit != null && unit.RelativeX < Constants.SideLength && unit.RelativeX >= 0 && unit.RelativeY < Constants.SideLength && unit.RelativeY >= 0)
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
                    if (infantry != null && infantry.RelativeX < Constants.SideLength && infantry.RelativeX >= 0 && infantry.RelativeY < Constants.SideLength && infantry.RelativeY >= 0)
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
                    if (structure != null && structure.RelativeX < Constants.SideLength && structure.RelativeX >= 0 && structure.RelativeY < Constants.SideLength && structure.RelativeY >= 0)
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
                    //out of bounder is disabled here for placing lamps
                    if (terrain != null ) //&& terrain.RelativeX < Constants.SideLength && terrain.RelativeX >= 0 && terrain.RelativeY < Constants.SideLength && terrain.RelativeY >= 0)
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
                    if (aircraft != null && aircraft.RelativeX < Constants.SideLength && aircraft.RelativeX >= 0 && aircraft.RelativeY < Constants.SideLength && aircraft.RelativeY >= 0)
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
                    if (smudge != null && smudge.RelativeX < Constants.SideLength && smudge.RelativeX >= 0 && smudge.RelativeY < Constants.SideLength && smudge.RelativeY >= 0)
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
                    if (waypoint != null && waypoint.RelativeX < Constants.SideLength && waypoint.RelativeX >= 0 && waypoint.RelativeY < Constants.SideLength && waypoint.RelativeY >= 0)
                        WaypointList.Add(waypoint);
                }
            }
        }
    }
}
