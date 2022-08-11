using MapEditor.TileInfo;
using Rampastring.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Weighted_Randomizer;
using MapEditor.NonTileObjects;
using Serilog;

namespace MapEditor
{
    public static class WorkingMap
    {
        public static int Width { get; private set; }
        public static int Height { get; private set; }
        public static int[] Size { get; private set; }
        public static AbstractTile[,] AbsTile { get; private set; }
        public static int Theater { get; private set; }
        public static List<AbstractMapUnit> AbstractMapUnitList { get; private set; }
        public static AbstractMapMember[,] AbstractMapMemberMatrix { get; private set; }
        public static List<int[]> PlacedAbstractMapUnitRecord { get; private set; }
        public static List<FailureAbstractMapUnitRecord> FailureAbsMapUnitRecordList { get; private set; }
        public static List<Unit> UnitList { get; private set; }
        public static List<Infantry> InfantryList { get; private set; }
        public static List<Structure> StructureList { get; private set; }
        public static List<Terrain> TerrainList { get; private set; }
        public static List<Aircraft> AircraftList { get; private set; }
        public static List<Smudge> SmudgeList { get; private set; }
        public static List<Overlay> OverlayList { get; private set; }
        public static List<Waypoint> WaypointList { get; private set; }
        public static int IndicatorNum { get; private set; }
        public static void Initialize(int width, int height, Theater theater)
        {
            Width = width;
            Height = height;
            Theater = (int)theater;
            int range = Width + Height;
            AbsTile = new AbstractTile[range, range];
            Size = new int[2] { Width, Height };
            AbstractMapUnitList = new List<AbstractMapUnit>();
            AbstractMapMemberMatrix = new AbstractMapMember [(int)Math.Ceiling((float)range / (float)Constants.SideLength), (int)Math.Ceiling((float)range / (float)Constants.SideLength)];
            PlacedAbstractMapUnitRecord = new List<int[]>();
            FailureAbsMapUnitRecordList = new List<FailureAbstractMapUnitRecord>();
            UnitList = new List<Unit>();
            InfantryList = new List<Infantry>();
            StructureList = new List<Structure>();
            TerrainList = new List<Terrain>();
            AircraftList = new List<Aircraft>();
            SmudgeList = new List<Smudge>();
            OverlayList = new List<Overlay>();
            WaypointList = new List<Waypoint>();

            for (int y = 0; y < range; y++)
            {
                for (int x = 0; x < range; x++)
                {
                    var absTile = new AbstractTile();
                    absTile.Initialize(x, y);
                    AbsTile[x, y] = absTile;
                }
            }
            for (int i = 0; i < AbstractMapMemberMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < AbstractMapMemberMatrix.GetLength(1); j++)
                {
                    AbstractMapMemberMatrix[i, j] = new AbstractMapMember();
                }
            }

            for (int i = 0; i < AbstractMapMemberMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < AbstractMapMemberMatrix.GetLength(1); j++)
                {
                    AbstractMapMemberMatrix[i, j].IsAllOnVisibleMap = true;
                    for (int x = 0; x < Constants.SideLength; x++)
                    {
                        for (int y = 0; y < Constants.SideLength; y++)
                        {
                            if (i * Constants.SideLength + x < range && j * Constants.SideLength + y < range)
                            {
                                if (IsValidAT(i * Constants.SideLength + x, j * Constants.SideLength + y))
                                {
                                    if (AbsTile[i * Constants.SideLength + x, j * Constants.SideLength + y].IsOnMap)
                                        AbstractMapMemberMatrix[i, j].IsOnMap = true;
                                    if (!AbsTile[i * Constants.SideLength + x, j * Constants.SideLength + y].IsOnVisibleMap)
                                        AbstractMapMemberMatrix[i, j].IsAllOnVisibleMap = false;
                                }
                            }
                        }
                    }
                    if (!AbstractMapMemberMatrix[i, j].IsOnMap)
                        AbstractMapMemberMatrix[i, j].IsAllOnVisibleMap = false;

                    if (!AbstractMapMemberMatrix[i, j].IsOnMap)
                        AbstractMapMemberMatrix[i, j].Placed = true;

                    if (IsValidAUM(i,j -1))
                    {
                        if (!AbstractMapMemberMatrix[i, j - 1].IsOnMap)
                            AbstractMapMemberMatrix[i, j].NEConnected = true;
                    }
                    else
                        AbstractMapMemberMatrix[i, j].NEConnected = true;

                    if (IsValidAUM(i, j + 1))
                    {
                        if (!AbstractMapMemberMatrix[i, j + 1].IsOnMap)
                            AbstractMapMemberMatrix[i, j].SWConnected = true;
                    }
                    else
                        AbstractMapMemberMatrix[i, j].SWConnected = true;

                    if (IsValidAUM(i - 1, j))
                    {
                        if (!AbstractMapMemberMatrix[i - 1, j].IsOnMap)
                            AbstractMapMemberMatrix[i, j].NWConnected = true;
                    }
                    else
                        AbstractMapMemberMatrix[i, j].NWConnected = true;

                    if (IsValidAUM(i + 1, j))
                    {
                        if (!AbstractMapMemberMatrix[i + 1, j].IsOnMap)
                            AbstractMapMemberMatrix[i, j].SEConnected = true;
                    }
                    else
                        AbstractMapMemberMatrix[i, j].SEConnected = true;
                }
            }

            SterilizeMapUnit();
        }
        public static bool IsValidAUM(int x, int y)
        {
            if (x < AbstractMapMemberMatrix.GetLength(0) && x >= 0 && y < AbstractMapMemberMatrix.GetLength(1) && y >= 0)
                return true;
            else
                return false;
        }
        public static bool IsValidAT(int x, int y)
        {
            if (x < Width + Height && y < Width + Height && x >= 0 && y >= 0)
                return true;
            else
                return false;
        }

        public static void SterilizeMapUnit()
        {
            string workingPath = "";
            if (Theater == 0)
                workingPath = Constants.TEMPERATEPath;
            else if (Theater == 1)
                workingPath = Constants.SNOWPath;
            else if (Theater == 2)
                workingPath = Constants.URBANPath;
            else if (Theater == 3)
                workingPath = Constants.NEWURBANPath;
            else if (Theater == 4)
                workingPath = Constants.LUNARPath;
            else if (Theater == 5)
                workingPath = Constants.DESERTPath;
            else
                return;

            DirectoryInfo root = new DirectoryInfo(workingPath);

            var indicatorMap = new MapFile();
            indicatorMap.CreateIsoTileList(workingPath + "indicator.map");
            IndicatorNum = indicatorMap.IsoTileList[0].TileNum;

            foreach (FileInfo f in root.GetFiles())
            {
                if ((f.Extension ==".map" || f.Extension == ".yrm" || f.Extension == ".mpr") && f.Name != "indicator.map")
                {
                    var absMapUnit = new AbstractMapUnit();
                    absMapUnit.Initialize(f);
                    AbstractMapUnitList.Add(absMapUnit);
                }
            }
        }

        public static AbstractMapUnit GetAbstractMapUnitByName(string mapUnitName)
        {
            var absMapUnit = new AbstractMapUnit();
            foreach (var pAbsMapUnit in AbstractMapUnitList)
            {
                if (mapUnitName == pAbsMapUnit.MapUnitName)
                {
                    absMapUnit = pAbsMapUnit;
                }
            }
            return absMapUnit;
        }
        public static AbstractMapMember[] GetNearbyAbstractMapMemberInfo(int x, int y)
        {
            //order : NE NW SW SE
            var absMapMember = new AbstractMapMember[4];
            if (IsValidAUM(x, y - 1))
            {
                absMapMember[0] = AbstractMapMemberMatrix[x, y - 1];
            }
            if (IsValidAUM(x - 1, y))
            {
                absMapMember[1] = AbstractMapMemberMatrix[x - 1, y];
            }
            if (IsValidAUM(x, y + 1))
            {
                absMapMember[2] = AbstractMapMemberMatrix[x, y + 1];
            }
            if (IsValidAUM(x + 1, y))
            {
                absMapMember[3] = AbstractMapMemberMatrix[x + 1, y];
            }
            return absMapMember;
        }

        public static void PlaceMapUnitToWorkingMap(int x, int y, string mapUnitName)
        {
            var absMapUnit = GetAbstractMapUnitByName(mapUnitName);
            AbstractMapMemberMatrix[x, y].Placed = true;

            for (int i = 0; i < Constants.SideLength; i++)
            {
                for (int j = 0; j < Constants.SideLength; j++)
                {
                    if (IsValidAT(x * Constants.SideLength + i, y * Constants.SideLength + j))
                    {
                        var absTileType = absMapUnit.AbsTileType[i, j];
                        var absTile = new AbstractTile();
                        absTile.SetProperty(x * Constants.SideLength + i, y * Constants.SideLength + j, 0, absTileType);
                        AbsTile[x * Constants.SideLength + i, y * Constants.SideLength + j] = absTile;
                    }
                }
            }            
        }

        public static void CreateNonTileObjectLists()
        {
            Log.Information("******************************************************");
            Log.Information("Start creating non-tile objects");
            for (int i = 0; i < AbstractMapMemberMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < AbstractMapMemberMatrix.GetLength(1); j++)
                {
                    var absMapMember = AbstractMapMemberMatrix[i, j];
                    var unitList = absMapMember.GetAbstractMapUnit().UnitList;
                    var infantryList = absMapMember.GetAbstractMapUnit().InfantryList;
                    var structureList = absMapMember.GetAbstractMapUnit().StructureList;
                    var terrainList = absMapMember.GetAbstractMapUnit().TerrainList;
                    var aircraftList = absMapMember.GetAbstractMapUnit().AircraftList;
                    var smudgeList = absMapMember.GetAbstractMapUnit().SmudgeList;
                    var overlayList = absMapMember.GetAbstractMapUnit().OverlayList;
                    var waypointList = absMapMember.GetAbstractMapUnit().WaypointList;

                    if (unitList != null && unitList.Count > 0)
                    {    
                        for (int k = 0; k < unitList.Count; k++)
                        {
                            var newUnit = unitList[k].Clone();
                            newUnit.X = newUnit.RelativeX + i * Constants.SideLength;
                            newUnit.Y = newUnit.RelativeY + j * Constants.SideLength;

                            if (IsValidAT(newUnit.X, newUnit.Y))
                            {
                                UnitList.Add(newUnit);
                                Log.Information("Add unit [{0}] in [{1},{2}]", newUnit.Name, newUnit.X, newUnit.Y);
                            }
                        }
                    }
                    if (infantryList != null && infantryList.Count > 0)
                    {
                        for (int k = 0; k < infantryList.Count; k++)
                        {
                            var newInfantry = infantryList[k].Clone();
                            newInfantry.X = newInfantry.RelativeX + i * Constants.SideLength;
                            newInfantry.Y = newInfantry.RelativeY + j * Constants.SideLength;

                            if (IsValidAT(newInfantry.X, newInfantry.Y))
                            {
                                InfantryList.Add(newInfantry);
                                Log.Information("Add infantry [{0}] in [{1},{2}]", newInfantry.Name, newInfantry.X, newInfantry.Y);
                            }
                        }
                    }
                    if (structureList != null && structureList.Count > 0)
                    {
                        for (int k = 0; k < structureList.Count; k++)
                        {
                            var newStructure = structureList[k].Clone();
                            newStructure.X = newStructure.RelativeX + i * Constants.SideLength;
                            newStructure.Y = newStructure.RelativeY + j * Constants.SideLength;

                            if (IsValidAT(newStructure.X, newStructure.Y))
                            {
                                StructureList.Add(newStructure);
                                Log.Information("Add structure [{0}] in [{1},{2}]", newStructure.Name, newStructure.X, newStructure.Y);
                            }
                        }
                    }
                    if (terrainList != null && terrainList.Count > 0)
                    {
                        for (int k = 0; k < terrainList.Count; k++)
                        {
                            var newTerrain = terrainList[k].Clone();
                            newTerrain.X = newTerrain.RelativeX + i * Constants.SideLength;
                            newTerrain.Y = newTerrain.RelativeY + j * Constants.SideLength;

                            if (IsValidAT(newTerrain.X, newTerrain.Y))
                            {
                                TerrainList.Add(newTerrain);
                                Log.Information("Add terrain [{0}] in [{1},{2}]", newTerrain.Name, newTerrain.X, newTerrain.Y);
                            }
                        }
                    }
                    if (aircraftList != null && aircraftList.Count > 0)
                    {
                        for (int k = 0; k < aircraftList.Count; k++)
                        {
                            var newAircraft = aircraftList[k].Clone();
                            newAircraft.X = newAircraft.RelativeX + i * Constants.SideLength;
                            newAircraft.Y = newAircraft.RelativeY + j * Constants.SideLength;

                            if (IsValidAT(newAircraft.X, newAircraft.Y))
                            {
                                AircraftList.Add(newAircraft);
                                Log.Information("Add aircraft [{0}] in [{1},{2}]", newAircraft.Name, newAircraft.X, newAircraft.Y);
                            }
                        }
                    }
                    if (smudgeList != null && smudgeList.Count > 0)
                    {
                        for (int k = 0; k < smudgeList.Count; k++)
                        {
                            var newSmudge = smudgeList[k].Clone();
                            newSmudge.X = newSmudge.RelativeX + i * Constants.SideLength;
                            newSmudge.Y = newSmudge.RelativeY + j * Constants.SideLength;

                            if (IsValidAT(newSmudge.X, newSmudge.Y))
                            {
                                SmudgeList.Add(newSmudge);
                                Log.Information("Add smudge [{0}] in [{1},{2}]", newSmudge.Name, newSmudge.X, newSmudge.Y);
                            }
                        }
                    }
                    if (waypointList != null && waypointList.Count > 0)
                    {
                        for (int k = 0; k < waypointList.Count; k++)
                        {
                            var newWaypoint = waypointList[k].Clone();
                            newWaypoint.X = newWaypoint.RelativeX + i * Constants.SideLength;
                            newWaypoint.Y = newWaypoint.RelativeY + j * Constants.SideLength;

                            if (IsValidAT(newWaypoint.X, newWaypoint.Y))
                            {
                                WaypointList.Add(newWaypoint);
                                Log.Information("Add waypoint [{0}] in [{1},{2}]", WaypointList.Count() - 1, newWaypoint.X, newWaypoint.Y);
                            }
                        }
                    }
                    if (overlayList != null && overlayList.Count > 0)
                    {
                        for (int k = 0; k < overlayList.Count; k++)
                        {
                            var newOverlay =  new Overlay(overlayList[k].OverlayID, overlayList[k].OverlayValue);//overlayList[k].Clone();
                            var tile = overlayList[k].Tile;
                            newOverlay.Tile = new IsoTile(tile.Dx, tile.Dy, tile.Rx, tile.Ry, tile.Z, tile.TileNum, tile.SubTile);
                            newOverlay.Tile.Rx = (ushort)(newOverlay.Tile.Rx + i * Constants.SideLength);
                            newOverlay.Tile.Ry = (ushort)(newOverlay.Tile.Ry + j * Constants.SideLength);

                            if (IsValidAT(newOverlay.Tile.Rx, newOverlay.Tile.Ry))
                            {
                                OverlayList.Add(newOverlay);
                                Log.Information("Add overlay [{0},{1}] in [{2},{3}]", newOverlay.OverlayID, newOverlay.OverlayValue, newOverlay.Tile.Rx, newOverlay.Tile.Ry);
                            }
                        }
                    }
                }
            } 
            Log.Information("End of creating non-tile objects");
            Log.Information("******************************************************");
        }

        public static void PlaceMapUnitByAbsMapMatrix()
        {
            for (int i = 0; i < AbstractMapMemberMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < AbstractMapMemberMatrix.GetLength(1); j++)
                { 
                    if (AbstractMapMemberMatrix[i,j].IsOnMap)
                    {
                        PlaceMapUnitToWorkingMap(i, j, AbstractMapMemberMatrix[i, j].MapUnitName);
                    }
                }
            }
            CreateNonTileObjectLists();
        }
        public static void SetMapUnit(int x, int y, string mapUnitName)
        {
            if (IsValidAUM(x,y))
            {
                AbstractMapMemberMatrix[x, y].MapUnitName = mapUnitName;
                AbstractMapMemberMatrix[x, y].Placed = true;
                RecordPlacedMapUnit(x, y);
            }
        }

        public static void SetMapUnitTest(int x, int y, string mapUnitName)
        {
            if (IsValidAUM(x, y))
            {
                AbstractMapMemberMatrix[x, y].MapUnitName = mapUnitName;
                AbstractMapMemberMatrix[x, y].Placed = true;
            }
        }
        public static void DeleteMapUnitTest(int x, int y)
        {
            if (IsValidAUM(x, y))
            {
                AbstractMapMemberMatrix[x, y].MapUnitName = "empty";
                AbstractMapMemberMatrix[x, y].Placed = false;
            }
        }

        public static void RandomSetMapUnit(int x, int y, List<string> mapUnitName)
        {
            IWeightedRandomizer<string> randomizer = new DynamicWeightedRandomizer<string>();
            foreach(var name in mapUnitName)
            {
                randomizer.Add(name, 1);
            }
            SetMapUnit(x, y, randomizer.NextWithReplacement());
        }

        public static void RecordPlacedMapUnit(int x, int y)
        {
            int[] record = { x, y };
            PlacedAbstractMapUnitRecord.Add(record);
        }
        public static void DeleteMapUnit(int x, int y)
        {
            if (IsValidAUM(x, y))
            {
                AbstractMapMemberMatrix[x, y].MapUnitName = "empty";
                AbstractMapMemberMatrix[x, y].Placed = false;
                UpdateMapUnitInfo();
                Log.Warning("Delete [{0},{1}] because the next step has no valid options", x, y);
                Log.Information("");
                int removeIndex = -1;
                for(int i = 0; i < PlacedAbstractMapUnitRecord.Count; i++)
                {
                    int[] record = PlacedAbstractMapUnitRecord[i];
                    if (record[0] == x && record[1] == y)
                        removeIndex = i;
                }
                if (removeIndex > -1)
                    PlacedAbstractMapUnitRecord.RemoveAt(removeIndex);
            }
        }

        public static void SetMapUnitByEntropy()
        {
            bool notAllMapUnitsSet = true;
            int failureTimes = 0;
            while (notAllMapUnitsSet)
            {
                UpdateMapUnitInfo();
                notAllMapUnitsSet = false;
                foreach (var absMapMember in AbstractMapMemberMatrix)
                {
                    if (!(absMapMember.SEConnected
                        && absMapMember.NWConnected
                        && absMapMember.SWConnected
                        && absMapMember.NEConnected))
                    {
                        notAllMapUnitsSet = true;
                    }
                }
                int[] targetMapUnit = GetLowestEntropyMU();
                if (targetMapUnit[0] == -1 && targetMapUnit[1] == -1)
                {
                    Log.Information("******************************************************");
                    Log.Information("Successfully place all map units!");
                    Log.Information("******************************************************");
                    return;
                }
                //order : NE NW SW SE
                var nearbyAbsMapMember = GetNearbyAbstractMapMemberInfo(targetMapUnit[0], targetMapUnit[1]);
                var validMapUnitList = GetValidAbsMapUnitList(nearbyAbsMapMember);

                var failureRecord = new FailureAbstractMapUnitRecord();
                List<int> deleteUnitMap = new List<int>();
                foreach (var record in FailureAbsMapUnitRecordList)
                {
                    if (record.IsTargetFailureRecord(targetMapUnit[0], targetMapUnit[1]))
                            failureRecord = record;
                }
                for (int i = 0; i < validMapUnitList.Count; i++)
                {
                    if (failureRecord.IsInFailureRecord(validMapUnitList[i].MapUnitName))
                        deleteUnitMap.Add(i);
                }
                for (int i = validMapUnitList.Count() - 1; i >= 0; i--)
                { 
                    foreach(int record in deleteUnitMap)
                    {
                        if (i == record)
                            validMapUnitList.RemoveAt(i);
                    }
                }

                //check after placing some MU, its nearby MUs have options or not.
                var nearbyAbsMapMemberOfNE = GetNearbyAbstractMapMemberInfo(targetMapUnit[0], targetMapUnit[1] - 1);
                var nearbyAbsMapMemberOfNW = GetNearbyAbstractMapMemberInfo(targetMapUnit[0] - 1, targetMapUnit[1]);
                var nearbyAbsMapMemberOfSE = GetNearbyAbstractMapMemberInfo(targetMapUnit[0] + 1, targetMapUnit[1]);
                var nearbyAbsMapMemberOfSW = GetNearbyAbstractMapMemberInfo(targetMapUnit[0], targetMapUnit[1] + 1);

                if (validMapUnitList.Count > 0)
                {
                    for (int i = validMapUnitList.Count() - 1; i >= 0; i--)
                    {
                        SetMapUnitTest(targetMapUnit[0], targetMapUnit[1], validMapUnitList[i].MapUnitName);
                        var validMapUnitListOfNE = GetValidAbsMapUnitList(nearbyAbsMapMemberOfNE);
                        if (validMapUnitListOfNE.Count < 1)
                        {
                            Log.Warning(validMapUnitList[i].MapUnitName + " is removed because it will cause further struggle");
                            validMapUnitList.RemoveAt(i);
                        }
                            
                        DeleteMapUnitTest(targetMapUnit[0], targetMapUnit[1]);
                    }
                }

                IWeightedRandomizer<string> randomizer = new DynamicWeightedRandomizer<string>();
                if (validMapUnitList.Count > 0)
                {
                    Log.Information("Valid map unit list:");
                    int weight = 0;
                    foreach (var abstractMapUnit in validMapUnitList)
                    {
                        weight += abstractMapUnit.Weight;
                    }
                    if (weight > 0)
                    {
                        foreach (var abstractMapUnit in validMapUnitList)
                        {
                            randomizer.Add(abstractMapUnit.MapUnitName, abstractMapUnit.Weight);
                            weight += abstractMapUnit.Weight;
                            Log.Information("  Name: " + abstractMapUnit.MapUnitName + ", Weight: " + abstractMapUnit.Weight);
                        }
                    }
                    else
                    {
                        foreach (var abstractMapUnit in validMapUnitList)
                        {
                            randomizer.Add(abstractMapUnit.MapUnitName, 1);
                            weight += abstractMapUnit.Weight;
                            Log.Information("  Name: " + abstractMapUnit.MapUnitName + ", Weight: 1");
                        }
                        Log.Warning("Weights are modified because all of them are 0");
                    }
                    var result = randomizer.NextWithReplacement();
                    SetMapUnit(targetMapUnit[0], targetMapUnit[1], result);
                    Log.Information("Choose {0} to place in [{1},{2}]", result, targetMapUnit[0], targetMapUnit[1]);
                    Log.Information("");
                }
                else
                {
                    int[] previousLocation = PlacedAbstractMapUnitRecord.Last();
                    var previousName = AbstractMapMemberMatrix[previousLocation[0], previousLocation[1]].MapUnitName;
                    var failure = new FailureAbstractMapUnitRecord();
                    int existFailureIndex = -1;
                    for (int i = 0; i < FailureAbsMapUnitRecordList.Count; i++)
                    {
                        if (FailureAbsMapUnitRecordList[i].IsTargetFailureRecord(previousLocation[0], previousLocation[1]))
                            existFailureIndex = i;
                    }
                    if (existFailureIndex > -1)
                    {
                        FailureAbsMapUnitRecordList[existFailureIndex].AddFailureRecord(previousLocation[0], previousLocation[1], previousName);
                    }
                    else
                    {
                        failure.AddFailureRecord(previousLocation[0], previousLocation[1], previousName);
                        FailureAbsMapUnitRecordList.Add(failure);
                    }
                    foreach (var failureRec in FailureAbsMapUnitRecordList)
                    {
                        if (failureRec.IsTargetFailureRecord(targetMapUnit[0], targetMapUnit[1]))
                            failureRec.Name.Clear();
                    }
                    DeleteMapUnit(previousLocation[0], previousLocation[1]);


                    failureTimes++;
                    if (failureTimes >= Constants.FailureTimes)
                    {
                        Log.Error("No valid map unit to place!");
                        return;
                    }
                }
            }
        }

        public static int[] GetFirstEmptyMapMember()
        {
            for (int j = 0; j < AbstractMapMemberMatrix.GetLength(1); j++)
            {
                for (int i = 0; i < AbstractMapMemberMatrix.GetLength(0); i++)
                {
                    if (AbstractMapMemberMatrix[i, j].IsOnMap && AbstractMapMemberMatrix[i, j].MapUnitName == "empty")
                        return new int[] { i, j };
                }
            }
            return new int[] { 0, 0 };
        }
        public static void SetMapUnitByOrder()
        {
            bool notAllMapUnitsSet = true;
            int failureTimes = 0;
            while (notAllMapUnitsSet)
            {
                UpdateMapUnitInfo();
                notAllMapUnitsSet = false;
                foreach (var absMapMember in AbstractMapMemberMatrix)
                {
                    if (!(absMapMember.SEConnected
                        && absMapMember.NWConnected
                        && absMapMember.SWConnected
                        && absMapMember.NEConnected))
                    {
                        notAllMapUnitsSet = true;
                    }
                }
                int[] targetMapUnit = GetFirstEmptyMapMember();
                if (targetMapUnit[0] == -1 && targetMapUnit[1] == -1)
                {
                    Log.Information("******************************************************");
                    Log.Information("Successfully place all map units!");
                    Log.Information("******************************************************");
                    return;
                }
                //order : NE NW SW SE
                var nearbyAbsMapMember = GetNearbyAbstractMapMemberInfo(targetMapUnit[0], targetMapUnit[1]);

                var validMapUnitList = GetValidAbsMapUnitList(nearbyAbsMapMember);

                var failureRecord = new FailureAbstractMapUnitRecord();
                List<int> deleteUnitMap = new List<int>();
                foreach (var record in FailureAbsMapUnitRecordList)
                {
                    if (record.IsTargetFailureRecord(targetMapUnit[0], targetMapUnit[1]))
                        failureRecord = record;
                }
                for (int i = 0; i < validMapUnitList.Count; i++)
                {
                    if (failureRecord.IsInFailureRecord(validMapUnitList[i].MapUnitName))
                        deleteUnitMap.Add(i);
                }
                for (int i = validMapUnitList.Count() - 1; i >= 0; i--)
                {
                    foreach (int record in deleteUnitMap)
                    {
                        if (i == record)
                            validMapUnitList.RemoveAt(i);
                    }
                }

                //check after placing some MU, its nearby MUs have options or not.
                var nearbyAbsMapMemberOfNE = GetNearbyAbstractMapMemberInfo(targetMapUnit[0], targetMapUnit[1] - 1);
                var nearbyAbsMapMemberOfNW = GetNearbyAbstractMapMemberInfo(targetMapUnit[0] - 1, targetMapUnit[1]);
                var nearbyAbsMapMemberOfSE = GetNearbyAbstractMapMemberInfo(targetMapUnit[0] + 1, targetMapUnit[1]);
                var nearbyAbsMapMemberOfSW = GetNearbyAbstractMapMemberInfo(targetMapUnit[0], targetMapUnit[1] + 1);

                if (validMapUnitList.Count > 0)
                {
                    for (int i = validMapUnitList.Count() - 1; i >= 0; i--)
                    {
                        SetMapUnitTest(targetMapUnit[0], targetMapUnit[1], validMapUnitList[i].MapUnitName);
                        var validMapUnitListOfNE = GetValidAbsMapUnitList(nearbyAbsMapMemberOfNE);
                        if (validMapUnitListOfNE.Count < 1)
                        {
                            Log.Warning(validMapUnitList[i].MapUnitName + " is removed because it will cause further struggle");
                            validMapUnitList.RemoveAt(i);
                        }

                        DeleteMapUnitTest(targetMapUnit[0], targetMapUnit[1]);
                    }
                }

                IWeightedRandomizer<string> randomizer = new DynamicWeightedRandomizer<string>();
                if (validMapUnitList.Count > 0)
                {
                    Log.Information("Valid map unit list:");
                    int weight = 0;
                    foreach (var abstractMapUnit in validMapUnitList)
                    {
                        weight += abstractMapUnit.Weight;
                    }
                    if (weight > 0)
                    {
                        foreach (var abstractMapUnit in validMapUnitList)
                        {
                            randomizer.Add(abstractMapUnit.MapUnitName, abstractMapUnit.Weight);
                            weight += abstractMapUnit.Weight;
                            Log.Information("  Name: " + abstractMapUnit.MapUnitName + ", Weight: " + abstractMapUnit.Weight);
                        }
                    }
                    else
                    {
                        foreach (var abstractMapUnit in validMapUnitList)
                        {
                            randomizer.Add(abstractMapUnit.MapUnitName, 1);
                            weight += abstractMapUnit.Weight;
                            Log.Information("  Name: " + abstractMapUnit.MapUnitName + ", Weight: 1");
                        }
                        Log.Warning("Weights are modified because all of them are 0");
                    }
                    var result = randomizer.NextWithReplacement();
                    SetMapUnit(targetMapUnit[0], targetMapUnit[1], result);
                    Log.Information("Choose {0} to place in [{1},{2}]", result, targetMapUnit[0], targetMapUnit[1]);
                    Log.Information("");
                }
                else
                {
                    int[] previousLocation = PlacedAbstractMapUnitRecord.Last();
                    var previousName = AbstractMapMemberMatrix[previousLocation[0], previousLocation[1]].MapUnitName;
                    var failure = new FailureAbstractMapUnitRecord();
                    int existFailureIndex = -1;
                    for (int i = 0; i < FailureAbsMapUnitRecordList.Count; i++)
                    {
                        if (FailureAbsMapUnitRecordList[i].IsTargetFailureRecord(previousLocation[0], previousLocation[1]))
                            existFailureIndex = i;
                    }
                    if (existFailureIndex > -1)
                    {
                        FailureAbsMapUnitRecordList[existFailureIndex].AddFailureRecord(previousLocation[0], previousLocation[1], previousName);
                    }
                    else
                    {
                        failure.AddFailureRecord(previousLocation[0], previousLocation[1], previousName);
                        FailureAbsMapUnitRecordList.Add(failure);
                    }
                    foreach (var failureRec in FailureAbsMapUnitRecordList)
                    {
                        if (failureRec.IsTargetFailureRecord(targetMapUnit[0], targetMapUnit[1]))
                            failureRec.Name.Clear();
                    }
                    DeleteMapUnit(previousLocation[0], previousLocation[1]);


                    failureTimes++;
                    if (failureTimes >= Constants.FailureTimes)
                    {
                        Log.Error("No valid map unit to place!");
                        return;
                    }
                }
            }
        }
        public static void FillRemainingEmptyUnitMap()
        {
            UpdateMapUnitInfo();
            Log.Information("******************************************************");
            Log.Information("Strat filling remaining empty unit map");
            Log.Information("******************************************************");
            int count = 0;
            for (int i = 0; i < AbstractMapMemberMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < AbstractMapMemberMatrix.GetLength(1); j++)
                {
                    var unitMap = AbstractMapMemberMatrix[i, j];
                    if (unitMap.MapUnitName == "empty" && unitMap.IsOnMap && !unitMap.Placed)
                    {
                        var nearbyAbsMapMember = GetNearbyAbstractMapMemberInfo(i,j);
                        var validMapUnitList = GetValidAbsMapUnitList(nearbyAbsMapMember);
                        IWeightedRandomizer<string> randomizer = new DynamicWeightedRandomizer<string>();
                        if (validMapUnitList.Count > 0)
                        {
                            Log.Information("Valid map unit list:");
                            int weight = 0;
                            foreach (var abstractMapUnit in validMapUnitList)
                            {
                                weight += abstractMapUnit.Weight;
                            }
                            if (weight > 0)
                            {
                                foreach (var abstractMapUnit in validMapUnitList)
                                {
                                    randomizer.Add(abstractMapUnit.MapUnitName, abstractMapUnit.Weight);
                                    weight += abstractMapUnit.Weight;
                                    Log.Information("  Name: " + abstractMapUnit.MapUnitName + ", Weight: " + abstractMapUnit.Weight);
                                }
                            }
                            else
                            {
                                foreach (var abstractMapUnit in validMapUnitList)
                                {
                                    randomizer.Add(abstractMapUnit.MapUnitName, 1);
                                    weight += abstractMapUnit.Weight;
                                    Log.Information("  Name: " + abstractMapUnit.MapUnitName + ", Weight: 1");
                                    Log.Information("Weights are modified because all of them are 0");
                                }
                            }
                            var result = randomizer.NextWithReplacement();
                            SetMapUnit(i, j, result);
                            count++;
                            Log.Information("Choose {0} to place in [{1},{2}]", result, i,j);
                            Log.Information("");
                        }
                        else
                        {
                            Log.Error("Failed to fill empty unit map in [{0},{1}]", i, j);
                            Log.Information("");
                        }
                    }
                }
            }
            Log.Information("End of filling remaining empty unit map");
            Log.Information("Counts: {0}",count);
            Log.Information("******************************************************");
        }

        public static List<AbstractMapUnit> GetValidAbsMapUnitList(AbstractMapMember[] nearbyUnitMap)
        {
            var validMapUnitList = new List<AbstractMapUnit>();

            for (int i = 0; i < AbstractMapUnitList.Count; i++)
            {
                if (AbstractMapUnitList[i].MapUnitName == "empty")
                    continue;
                int conditionsMet = 0;
                if (nearbyUnitMap[0] != null)
                {
                    if (nearbyUnitMap[0].MapUnitName != "empty" && nearbyUnitMap[0].IsOnMap)
                    {
                        if (AbstractMapUnitList[i].NEConnectionType == nearbyUnitMap[0].GetAbstractMapUnit().SWConnectionType)
                            conditionsMet++;
                    }
                    else
                        conditionsMet++;
                }
                else
                    conditionsMet++;
                if (nearbyUnitMap[1] != null)
                {
                    if (nearbyUnitMap[1].MapUnitName != "empty" && nearbyUnitMap[1].IsOnMap)
                    {
                        if (AbstractMapUnitList[i].NWConnectionType == nearbyUnitMap[1].GetAbstractMapUnit().SEConnectionType)
                            conditionsMet++;
                    }
                    else
                        conditionsMet++;
                }
                else
                    conditionsMet++;
                if (nearbyUnitMap[2] != null)
                {
                    if (nearbyUnitMap[2].MapUnitName != "empty" && nearbyUnitMap[2].IsOnMap)
                    {
                        if (AbstractMapUnitList[i].SWConnectionType == nearbyUnitMap[2].GetAbstractMapUnit().NEConnectionType)
                            conditionsMet++;
                    }
                    else
                        conditionsMet++;
                }
                else
                    conditionsMet++;
                if (nearbyUnitMap[3] != null)
                {
                    if (nearbyUnitMap[3].MapUnitName != "empty" && nearbyUnitMap[3].IsOnMap)
                    {
                        if (AbstractMapUnitList[i].SEConnectionType == nearbyUnitMap[3].GetAbstractMapUnit().NWConnectionType)
                            conditionsMet++;
                    }
                    else
                        conditionsMet++;
                }
                else
                    conditionsMet++;

                if (conditionsMet == 4)
                    validMapUnitList.Add(AbstractMapUnitList[i]);
            }

            int validCount = 0;
            foreach (var mapUnit in validMapUnitList)
            {
                if (mapUnit.Weight > 0)
                    validCount++;
            }
            //remove the same Map Unit with nearby
            for (int i = validMapUnitList.Count() - 1; i >= 0; i--)
            {

                if (validCount > 1)
                {
                    var name = validMapUnitList[i].MapUnitName;
                    if (nearbyUnitMap[0] != null)
                    {
                        if (nearbyUnitMap[0].MapUnitName == name)
                        {
                            validMapUnitList.RemoveAt(i);
                            continue;
                        }
                    }
                    if (nearbyUnitMap[1] != null)
                    {
                        if (nearbyUnitMap[1].MapUnitName == name)
                        {
                            validMapUnitList.RemoveAt(i);
                            continue;
                        }
                    }
                    if (nearbyUnitMap[2] != null)
                    {
                        if (nearbyUnitMap[2].MapUnitName == name)
                        {
                            validMapUnitList.RemoveAt(i);
                            continue;
                        }
                    }
                    if (nearbyUnitMap[3] != null)
                    {
                        if (nearbyUnitMap[3].MapUnitName == name)
                        {
                            validMapUnitList.RemoveAt(i);
                            continue;
                        }
                    }
                }
            }
            return validMapUnitList;
        }

        public static int[] GetLowestEntropyMU()
        {
            UpdateMapUnitInfo();
            int[] lowestEntropyMU = { -1, -1 };
            int lowestEntropy = int.MaxValue;
            for (int i = 0; i < AbstractMapMemberMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < AbstractMapMemberMatrix.GetLength(1); j++)
                {
                    if (AbstractMapMemberMatrix[i, j].Entropy < lowestEntropy && AbstractMapMemberMatrix[i, j].MapUnitName == "empty" && AbstractMapMemberMatrix[i, j].IsOnMap)
                    {
                        lowestEntropy = AbstractMapMemberMatrix[i, j].Entropy;
                        lowestEntropyMU[0] = i;
                        lowestEntropyMU[1] = j;
                    }
                }
            }
            return lowestEntropyMU;
        }

        public static void UpdateMapUnitInfo()
        {
            for (int i = 0; i < AbstractMapMemberMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < AbstractMapMemberMatrix.GetLength(1); j++)
                {
                    AbstractMapMemberMatrix[i, j].Entropy = 50;
                    if (IsValidAUM(i, j - 1))
                    {
                        if (AbstractMapMemberMatrix[i, j - 1].MapUnitName != "empty")
                        {
                            AbstractMapMemberMatrix[i, j].NEConnected = true;
                            AbstractMapMemberMatrix[i, j].Entropy -= 10;
                        }
                    }
                    if (IsValidAUM(i, j + 1))
                    {
                        if (AbstractMapMemberMatrix[i, j + 1].MapUnitName != "empty")
                        {
                            AbstractMapMemberMatrix[i, j].SWConnected = true;
                            AbstractMapMemberMatrix[i, j].Entropy -= 10;
                        }
                    }
                    if (IsValidAUM(i - 1, j))
                    {
                        if (AbstractMapMemberMatrix[i - 1, j].MapUnitName != "empty")
                        {
                            AbstractMapMemberMatrix[i, j].NWConnected = true;
                            AbstractMapMemberMatrix[i, j].Entropy -= 10;
                        }
                    }
                    if (IsValidAUM(i + 1, j))
                    {
                        if (AbstractMapMemberMatrix[i + 1, j].MapUnitName != "empty")
                        {
                            AbstractMapMemberMatrix[i, j].SEConnected = true;
                            AbstractMapMemberMatrix[i, j].Entropy -= 10;
                        }
                    }
                    if (!(AbstractMapMemberMatrix[i, j].SEConnected 
                        && AbstractMapMemberMatrix[i, j].NWConnected 
                        && AbstractMapMemberMatrix[i, j].SWConnected 
                        && AbstractMapMemberMatrix[i, j].NEConnected))
                    {
                        if (AbstractMapMemberMatrix[i, j].SEConnected)
                            AbstractMapMemberMatrix[i, j].Entropy -= 2;
                        if (AbstractMapMemberMatrix[i, j].NWConnected)
                            AbstractMapMemberMatrix[i, j].Entropy -= 2;
                        if (AbstractMapMemberMatrix[i, j].SWConnected)
                            AbstractMapMemberMatrix[i, j].Entropy -= 2;
                        if (AbstractMapMemberMatrix[i, j].NEConnected)
                            AbstractMapMemberMatrix[i, j].Entropy -= 2;
                    }
                    else
                        AbstractMapMemberMatrix[i, j].Entropy = 50;
                }
            }
        }
        
        public static int[] GetCentralAbsMapMemberLocation()
        {
            int range = Width + Height;
            int[] location = { (int)Math.Ceiling((float)range / 2.0 / (float)Constants.SideLength), (int)Math.Ceiling((float)range / 2.0 / (float)Constants.SideLength) };
            return location;
        }

        //make sure a group of Abs Map Units have enough place to place in four corners
        public static int[] GetEnoughPlaceAbsMapMemberLocation(string direction, int length)
        {
            int[] result = new int[2] { 1, 1 };
            if (direction == "NW")
            {

                for (int i = 0; i < AbstractMapMemberMatrix.GetLength(0); i++)
                {
                    for (int j = 0; j < AbstractMapMemberMatrix.GetLength(1); j++)
                    {
                        if (IsValidAUM(i, j) && IsValidAUM(i, j + length - 1))
                        {
                            if (AbstractMapMemberMatrix[i, j].IsAllOnVisibleMap && AbstractMapMemberMatrix[i, j + length - 1].IsAllOnVisibleMap)
                            {
                                result[0] = i;
                                result[1] = j;
                                return result;
                            }
                        }
                    }
                }
            }
            if (direction == "NE")
            {

                for (int i = 0; i < AbstractMapMemberMatrix.GetLength(1); i++)
                {
                    for (int j = 0; j < AbstractMapMemberMatrix.GetLength(0); j++)
                    {
                        if (IsValidAUM(j, i) && IsValidAUM(j + length - 1, i))
                        {
                            if (AbstractMapMemberMatrix[j, i].IsAllOnVisibleMap && AbstractMapMemberMatrix[j + length - 1, i].IsAllOnVisibleMap)
                            {
                                result[0] = j;
                                result[1] = i;
                                return result;
                            }
                        }
                    }
                }
            }
            if (direction == "SW")
            {

                for (int i = AbstractMapMemberMatrix.GetLength(1) - 1 ; i >= 0; i--)
                {
                    for (int j = AbstractMapMemberMatrix.GetLength(0) - 1; j >= 0 ; j--)
                    {
                        if (IsValidAUM(j, i) && IsValidAUM(j + length - 1, i))
                        {
                            if (AbstractMapMemberMatrix[j, i].IsAllOnVisibleMap && AbstractMapMemberMatrix[j + length - 1, i].IsAllOnVisibleMap)
                            {
                                result[0] = j;
                                result[1] = i;
                                return result;
                            }
                        }
                    }
                }
            }
            if (direction == "SE")
            {

                for (int i = AbstractMapMemberMatrix.GetLength(0) - 1; i >= 0; i--)
                {
                    for (int j = AbstractMapMemberMatrix.GetLength(1) - 1; j >= 0; j--)
                    {
                        if (IsValidAUM(i, j) && IsValidAUM(i, j + length - 1))
                        {
                            if (AbstractMapMemberMatrix[i, j].IsAllOnVisibleMap && AbstractMapMemberMatrix[i, j + length - 1].IsAllOnVisibleMap)
                            {
                                result[0] = i;
                                result[1] = j;
                                return result;
                            }
                        }
                    }
                }
            }
            return result;
        }

        public static List<IsoTile> CreateTileList()
        {
            var tileList = new List<IsoTile>();
            int range = Width + Height;
            for (int y = 0; y < range; y++)
            {
                for (int x = 0; x < range; x++)
                {
                    var absTile = AbsTile[x, y];
                    if (absTile.IsOnMap)
                    {
                        var tile = new IsoTile((ushort)(2 * absTile.X - 2 - absTile.Y),
                            (ushort)(absTile.X + absTile.Y - Width - 1),
                            (ushort)absTile.X,
                            (ushort)absTile.Y,
                            (byte)absTile.Z,
                            (short)absTile.TileNum,
                            (byte)absTile.SubTile);
                        tileList.Add(tile);
                    }
                }
            } 
            return tileList;
        }

        public static IniSection CreateUnitINI()
        {
            var unitIniSection = new IniSection("Units");
            int index = 0;
            foreach (var unit in UnitList)
            {
                unitIniSection.AddKey(index.ToString(), unit.CreateINIValue());
                index++;
            }
            return unitIniSection;
        }
        public static IniSection CreateInfantryINI()
        {
            var infantryIniSection = new IniSection("Infantry");
            int index = 0;
            foreach (var infantry in InfantryList)
            {
                infantryIniSection.AddKey(index.ToString(), infantry.CreateINIValue());
                index++;
            }
            return infantryIniSection;
        }
        public static IniSection CreateStructureINI()
        {
            var structureIniSection = new IniSection("Structures");
            int index = 0;
            foreach (var structure in StructureList)
            {
                structureIniSection.AddKey(index.ToString(), structure.CreateINIValue());
                index++;
            }
            return structureIniSection;
        }
        public static IniSection CreateTerrainINI()
        {
            var terrainIniSection = new IniSection("Terrain");
            foreach (var terrain in TerrainList)
            {
                var iniLine = terrain.CreateINILine();
                terrainIniSection.AddKey(iniLine.Key, iniLine.Value);
            }
            return terrainIniSection;
        }
        public static IniSection CreateAircraftINI()
        {
            var aircraftIniSection = new IniSection("Aircraft");
            int index = 0;
            foreach (var aircraft in AircraftList)
            {
                aircraftIniSection.AddKey(index.ToString(), aircraft.CreateINIValue());
                index++;
            }
            return aircraftIniSection;
        }
        public static IniSection CreateSmudgeINI()
        {
            var smudgeIniSection = new IniSection("Smudge");
            int index = 0;
            foreach (var smudge in SmudgeList)
            {
                smudgeIniSection.AddKey(index.ToString(), smudge.CreateINIValue());
                index++;
            }
            return smudgeIniSection;
        }
        public static IniSection CreateWaypointINI()
        {
            var waypointIniSection = new IniSection("Waypoints");
            int index = 0;
            foreach (var waypoint in WaypointList)
            {
                var iniLine = waypoint.CreateINILine();
                waypointIniSection.AddKey(index.ToString(), iniLine.Value);
                index++;
            }
            return waypointIniSection;
        }
        public static void PlacePlayerLocation(int number, string direction)
        {
            List<string> startingUnits = new List<string>();
            foreach (var absMU in AbstractMapUnitList)
            {
                if (absMU.MapUnitName.Contains("spawn"))
                {
                    startingUnits.Add(absMU.MapUnitName);
                }
            }
            int[] playerLocation = new int[2];
            if (direction == "NW")
                playerLocation = GetEnoughPlaceAbsMapMemberLocation("NW", number);
            if (direction == "SW")
                playerLocation = GetEnoughPlaceAbsMapMemberLocation("SW", number);
            if (direction == "SE")
                playerLocation = GetEnoughPlaceAbsMapMemberLocation("SE", number);
            if (direction == "NE")
                playerLocation = GetEnoughPlaceAbsMapMemberLocation("NE", number);

            if (direction == "NW" || direction == "SE")
            {
                for (int i = 0; i < number; i++)
                {
                    RandomSetMapUnit(playerLocation[0], playerLocation[1] + i, startingUnits);
                    Log.Information("Player is set in abstract map member [{0},{1}]", playerLocation[0], playerLocation[1] + i);
                }
            }
            else if (direction == "SW" || direction == "NE")
            {
                for (int i = 0; i < number; i++)
                {
                    RandomSetMapUnit(playerLocation[0] + i, playerLocation[1], startingUnits);
                    Log.Information("Player is set in abstract map member [{0},{1}]", playerLocation[0] + i, playerLocation[1]);
                }
            }
        }

        public static void IncreaseWeightContainsX(int type, int times = 1)
        {
            type -= 1;
            for (int j = 0; j < times; j ++)
            {
                for (int i = 0; i < AbstractMapUnitList.Count; i++)
                {
                    var absMU = AbstractMapUnitList[i];
                    if (absMU.Weight != 0)
                    {
                        if (absMU.NEConnectionType == type)
                        {
                            AbstractMapUnitList[i].Weight += 1;
                        }
                        if (absMU.NWConnectionType == type)
                        {
                            AbstractMapUnitList[i].Weight += 1;
                        }
                        if (absMU.SEConnectionType == type)
                        {
                            AbstractMapUnitList[i].Weight += 1;
                        }
                        if (absMU.SWConnectionType == type)
                        {
                            AbstractMapUnitList[i].Weight += 1;
                        }
                    }
                }
            }
        }
    }
}
