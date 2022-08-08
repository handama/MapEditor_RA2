using MapEditor.TileInfo;
using Rampastring.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Weighted_Randomizer;
using MapEditor.NonTileObjects;

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
        public static AbstractMap[,] AbstractMapMatrix { get; private set; }
        public static List<int[]> PlacedAbstractMapUnitRecord { get; private set; }
        public static List<FailureAbstractMapUnitRecord> FailureAbsMapUnitRecordList { get; private set; }
        public static List<Unit> UnitList { get; private set; }
        public static List<Infantry> InfantryList { get; private set; }
        public static List<Structure> StructureList { get; private set; }
        public static List<Terrain> TerrainList { get; private set; }
        public static List<Aircraft> AircraftList { get; private set; }
        public static List<Smudge> SmudgeList { get; private set; }
        public static List<Overlay> OverlayList { get; private set; }
        public static void Initialize(int width, int height, Theater theater)
        {
            Width = width;
            Height = height;
            Theater = (int)theater;
            int range = Width + Height;
            AbsTile = new AbstractTile[range, range];
            Size = new int[2] { Width, Height };
            AbstractMapUnitList = new List<AbstractMapUnit>();
            AbstractMapMatrix = new AbstractMap [(int)Math.Ceiling((float)range / 15.0), (int)Math.Ceiling((float)range / 15.0)];
            PlacedAbstractMapUnitRecord = new List<int[]>();
            FailureAbsMapUnitRecordList = new List<FailureAbstractMapUnitRecord>();
            UnitList = new List<Unit>();
            InfantryList = new List<Infantry>();
            StructureList = new List<Structure>();
            TerrainList = new List<Terrain>();
            AircraftList = new List<Aircraft>();
            SmudgeList = new List<Smudge>();
            OverlayList = new List<Overlay>();

            for (int y = 0; y < range; y++)
            {
                for (int x = 0; x < range; x++)
                {
                    var absTile = new AbstractTile();
                    absTile.Initialize(x, y);
                    AbsTile[x, y] = absTile;
                }
            }
            for (int i = 0; i < AbstractMapMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < AbstractMapMatrix.GetLength(1); j++)
                {
                    AbstractMapMatrix[i, j] = new AbstractMap();
                }
            }

            for (int i = 0; i < AbstractMapMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < AbstractMapMatrix.GetLength(1); j++)
                {
                    for (int x = 0; x < 15; x++)
                    {
                        for (int y = 0; y < 15; y++)
                        {
                            
                            if (i * 15 + x < range && j * 15 + y < range)
                            {
                                if (IsValidAT(i * 15 + x, j * 15 + y))
                                {
                                    if (AbsTile[i * 15 + x, j * 15 + y].IsOnMap)
                                        AbstractMapMatrix[i, j].IsOnMap = true;
                                }
                            }
                        }
                    }

                    if (!AbstractMapMatrix[i, j].IsOnMap)
                        AbstractMapMatrix[i, j].Placed = true;

                    if (IsValidAUM(i,j -1))
                    {
                        if (!AbstractMapMatrix[i, j - 1].IsOnMap)
                            AbstractMapMatrix[i, j].NEConnected = true;
                    }
                    else
                        AbstractMapMatrix[i, j].NEConnected = true;

                    if (IsValidAUM(i, j + 1))
                    {
                        if (!AbstractMapMatrix[i, j + 1].IsOnMap)
                            AbstractMapMatrix[i, j].SWConnected = true;
                    }
                    else
                        AbstractMapMatrix[i, j].SWConnected = true;

                    if (IsValidAUM(i - 1, j))
                    {
                        if (!AbstractMapMatrix[i - 1, j].IsOnMap)
                            AbstractMapMatrix[i, j].NWConnected = true;
                    }
                    else
                        AbstractMapMatrix[i, j].NWConnected = true;

                    if (IsValidAUM(i + 1, j))
                    {
                        if (!AbstractMapMatrix[i + 1, j].IsOnMap)
                            AbstractMapMatrix[i, j].SEConnected = true;
                    }
                    else
                        AbstractMapMatrix[i, j].SEConnected = true;
                }
            }

            SterilizeMapUnit();
        }
        public static bool IsValidAUM(int x, int y)
        {
            if (x < AbstractMapMatrix.GetLength(0) && x >= 0 && y < AbstractMapMatrix.GetLength(1) && y >= 0)
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
            if (WorkingMap.Theater == 0)
                workingPath = Constants.TEMPERATEPath;
            else if (WorkingMap.Theater == 1)
                workingPath = Constants.SNOWPath;
            else if (WorkingMap.Theater == 2)
                workingPath = Constants.URBANPath;
            else if (WorkingMap.Theater == 3)
                workingPath = Constants.NEWURBANPath;
            else if (WorkingMap.Theater == 4)
                workingPath = Constants.LUNARPath;
            else if (WorkingMap.Theater == 5)
                workingPath = Constants.DESERTPath;
            else
                return;

            DirectoryInfo root = new DirectoryInfo(workingPath);
            foreach (FileInfo f in root.GetFiles())
            {
                if (f.Extension ==".map" || f.Extension == ".yrm" || f.Extension == ".mpr")
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
        public static AbstractMap[] GetNearbyAbstractMapMemberInfo(int x, int y)
        {
            //order : NE NW SW SE
            var absMapMember = new AbstractMap[4];
            if (IsValidAUM(x, y - 1))
            {
                absMapMember[0] = AbstractMapMatrix[x, y - 1];
            }
            if (IsValidAUM(x - 1, y))
            {
                absMapMember[1] = AbstractMapMatrix[x - 1, y];
            }
            if (IsValidAUM(x, y + 1))
            {
                absMapMember[2] = AbstractMapMatrix[x, y + 1];
            }
            if (IsValidAUM(x + 1, y))
            {
                absMapMember[3] = AbstractMapMatrix[x + 1, y];
            }
            return absMapMember;
        }

        public static void PlaceMapUnitToWorkingMap(int x, int y, string mapUnitName)
        {
            var absMapUnit = GetAbstractMapUnitByName(mapUnitName);
            AbstractMapMatrix[x, y].Placed = true;

            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    if (IsValidAT(x * 15 + i, y * 15 + j))
                    {
                        var absTileType = absMapUnit.AbsTileType[i, j];
                        var absTile = new AbstractTile();
                        absTile.SetProperty(x * 15 + i, y * 15 + j, 0, absTileType);
                        AbsTile[x * 15 + i, y * 15 + j] = absTile;
                    }
                }
            }            
        }

        public static void CreateNonTileObjectLists()
        {
            for (int i = 0; i < AbstractMapMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < AbstractMapMatrix.GetLength(1); j++)
                {
                    var absMapMember = AbstractMapMatrix[i, j];
                    var unitList = absMapMember.GetAbstractMapUnit().UnitList;
                    var infantryList = absMapMember.GetAbstractMapUnit().InfantryList;
                    var structureList = absMapMember.GetAbstractMapUnit().StructureList;
                    var terrainList = absMapMember.GetAbstractMapUnit().TerrainList;
                    var aircraftList = absMapMember.GetAbstractMapUnit().AircraftList;
                    var smudgeList = absMapMember.GetAbstractMapUnit().SmudgeList;
                    var overlayList = absMapMember.GetAbstractMapUnit().OverlayList;

                    if (unitList.Count > 0)
                    {    
                        for (int k = 0; k < unitList.Count; k++)
                        {
                            var newUnit = unitList[k].Clone();
                            newUnit.X = newUnit.RelativeX + i * 15;
                            newUnit.Y = newUnit.RelativeY + j * 15;

                            if (IsValidAT(newUnit.X, newUnit.Y))
                            {
                                UnitList.Add(newUnit);
                            }
                        }
                    }
                    if (infantryList.Count > 0)
                    {
                        for (int k = 0; k < infantryList.Count; k++)
                        {
                            var newInfantry = infantryList[k].Clone();
                            newInfantry.X = newInfantry.RelativeX + i * 15;
                            newInfantry.Y = newInfantry.RelativeY + j * 15;

                            if (IsValidAT(newInfantry.X, newInfantry.Y))
                            {
                                InfantryList.Add(newInfantry);
                            }
                        }
                    }
                    if (structureList.Count > 0)
                    {
                        for (int k = 0; k < structureList.Count; k++)
                        {
                            var newStructure = structureList[k].Clone();
                            newStructure.X = newStructure.RelativeX + i * 15;
                            newStructure.Y = newStructure.RelativeY + j * 15;

                            if (IsValidAT(newStructure.X, newStructure.Y))
                            {
                                StructureList.Add(newStructure);
                            }
                        }
                    }
                    if (terrainList.Count > 0)
                    {
                        for (int k = 0; k < terrainList.Count; k++)
                        {
                            var newTerrain = terrainList[k].Clone();
                            newTerrain.X = newTerrain.RelativeX + i * 15;
                            newTerrain.Y = newTerrain.RelativeY + j * 15;

                            if (IsValidAT(newTerrain.X, newTerrain.Y))
                            {
                                TerrainList.Add(newTerrain);
                            }
                        }
                    }
                    if (aircraftList.Count > 0)
                    {
                        for (int k = 0; k < aircraftList.Count; k++)
                        {
                            var newAircraft = aircraftList[k].Clone();
                            newAircraft.X = newAircraft.RelativeX + i * 15;
                            newAircraft.Y = newAircraft.RelativeY + j * 15;

                            if (IsValidAT(newAircraft.X, newAircraft.Y))
                            {
                                AircraftList.Add(newAircraft);
                            }
                        }
                    }
                    if (smudgeList.Count > 0)
                    {
                        for (int k = 0; k < smudgeList.Count; k++)
                        {
                            var newSmudge = smudgeList[k].Clone();
                            newSmudge.X = newSmudge.RelativeX + i * 15;
                            newSmudge.Y = newSmudge.RelativeY + j * 15;

                            if (IsValidAT(newSmudge.X, newSmudge.Y))
                            {
                                SmudgeList.Add(newSmudge);
                            }
                        }
                    }
                    if (overlayList.Count > 0)
                    {
                        for (int k = 0; k < overlayList.Count; k++)
                        {
                            var newOverlay =  new Overlay(overlayList[k].OverlayID, overlayList[k].OverlayValue);//overlayList[k].Clone();
                            var tile = overlayList[k].Tile;
                            newOverlay.Tile = new IsoTile(tile.Dx, tile.Dy, tile.Rx, tile.Ry, tile.Z, tile.TileNum, tile.SubTile);
                            newOverlay.Tile.Rx = (ushort)(newOverlay.Tile.Rx + i * 15);
                            newOverlay.Tile.Ry = (ushort)(newOverlay.Tile.Ry + j * 15);

                            if (IsValidAT(newOverlay.Tile.Rx, newOverlay.Tile.Ry))
                            {
                                OverlayList.Add(newOverlay);
                            }
                        }
                    }
                }
            }
        }

        public static void PlaceMapUnitByAbsMapMatrix()
        {
            for (int i = 0; i < AbstractMapMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < AbstractMapMatrix.GetLength(1); j++)
                { 
                    if (AbstractMapMatrix[i,j].IsOnMap)
                    {
                        PlaceMapUnitToWorkingMap(i, j, AbstractMapMatrix[i, j].MapUnitName);
                    }
                }
            }
            CreateNonTileObjectLists();
        }
        public static void SetMapUnit(int x, int y, string mapUnitName)
        {
            if (IsValidAUM(x,y))
            {
                AbstractMapMatrix[x, y].MapUnitName = mapUnitName;
                AbstractMapMatrix[x, y].Placed = true;
                RecordPlacedMapUnit(x, y);
            }
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
                AbstractMapMatrix[x, y].MapUnitName = "empty";
                AbstractMapMatrix[x, y].Placed = false;
                UpdateMapUnitInfo();

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
                foreach (var absMapMember in AbstractMapMatrix)
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
                    Console.WriteLine("------------------------------");
                    Console.WriteLine("Successfully place all map units!");
                    Console.WriteLine("------------------------------");
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

                IWeightedRandomizer<string> randomizer = new DynamicWeightedRandomizer<string>();
                if (validMapUnitList.Count > 0)
                {
                    Console.WriteLine("Valid map unit list:");
                    int weight = 0;
                    foreach (var abstractMapUnit in validMapUnitList)
                    {
                        randomizer.Add(abstractMapUnit.MapUnitName, abstractMapUnit.Weight);
                        weight += abstractMapUnit.Weight;
                        Console.WriteLine("  Name: " + abstractMapUnit.MapUnitName + ", Weight: "+ abstractMapUnit.Weight);
                    }
                    if (weight > 0)
                    {
                        var result = randomizer.NextWithReplacement();
                        SetMapUnit(targetMapUnit[0], targetMapUnit[1], result);
                        Console.WriteLine("Choose {0} to place in [{1},{2}]", result, targetMapUnit[0], targetMapUnit[1]);
                        Console.WriteLine("------------------------------");
                    }
                    else
                    {
                        Console.WriteLine("No valid map unit to place in [{0},{1}], because total weight is 0", targetMapUnit[0], targetMapUnit[1]);
                        failureTimes++;
                        Console.WriteLine("------------------------------");
                        if (failureTimes >= Constants.FailureTimes)
                        {
                            Console.WriteLine("No valid map unit to place!");
                            return;
                        }
                    }
                }
                else
                {
                    int[] previousLocation = PlacedAbstractMapUnitRecord.Last();
                    var previousName = AbstractMapMatrix[previousLocation[0], previousLocation[1]].MapUnitName;
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
                        Console.WriteLine("No valid map unit to place!");
                        return;
                    }
                }
            }
        }
        public static void FillRemainingEmptyUnitMap()
        {
            UpdateMapUnitInfo();
            Console.WriteLine("------------------------------");
            Console.WriteLine("Strat filling remaining empty unit map");
            Console.WriteLine("------------------------------");
            int count = 0;
            for (int i = 0; i < AbstractMapMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < AbstractMapMatrix.GetLength(1); j++)
                {
                    var unitMap = AbstractMapMatrix[i, j];
                    if (unitMap.MapUnitName == "empty" && unitMap.IsOnMap && !unitMap.Placed)
                    {
                        var nearbyAbsMapMember = GetNearbyAbstractMapMemberInfo(i,j);
                        var validMapUnitList = GetValidAbsMapUnitList(nearbyAbsMapMember);
                        IWeightedRandomizer<string> randomizer = new DynamicWeightedRandomizer<string>();
                        if (validMapUnitList.Count > 0)
                        {
                            Console.WriteLine("Valid map unit list:");
                            int weight = 0;
                            foreach (var abstractMapUnit in validMapUnitList)
                            {
                                randomizer.Add(abstractMapUnit.MapUnitName, abstractMapUnit.Weight);
                                weight += abstractMapUnit.Weight;
                                Console.WriteLine("  Name: " + abstractMapUnit.MapUnitName + ", Weight: " + abstractMapUnit.Weight);
                            }
                            if (weight > 0)
                            {
                                var result = randomizer.NextWithReplacement();
                                SetMapUnit(i, j, result);
                                count++;
                                Console.WriteLine("Choose {0} to place in [{1},{2}]", result, i,j);
                                Console.WriteLine("------------------------------");
                            }
                            else
                            {
                                Console.WriteLine("No valid map unit to place in [{0},{1}], because total weight is 0", i,j);
                                Console.WriteLine("------------------------------");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Failed to fill empty unit map in [{0},{1}]", i, j);
                            Console.WriteLine("------------------------------");
                        }
                    }
                }
            }
            Console.WriteLine("End of filling remaining empty unit map");
            Console.WriteLine("Counts: {0}",count);
            Console.WriteLine("------------------------------");
        }

        public static List<AbstractMapUnit> GetValidAbsMapUnitList(AbstractMap[] nearbyUnitMap)
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

            for (int i = validMapUnitList.Count() - 1; i >= 0; i--)
            {
                if (validMapUnitList.Count > 1)
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
            for (int i = 0; i < AbstractMapMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < AbstractMapMatrix.GetLength(1); j++)
                {
                    if (AbstractMapMatrix[i, j].Entropy < lowestEntropy && AbstractMapMatrix[i, j].MapUnitName == "empty" && AbstractMapMatrix[i, j].IsOnMap)
                    {
                        lowestEntropy = AbstractMapMatrix[i, j].Entropy;
                        lowestEntropyMU[0] = i;
                        lowestEntropyMU[1] = j;
                    }
                }
            }
            return lowestEntropyMU;
        }

        public static void UpdateMapUnitInfo()
        {
            for (int i = 0; i < AbstractMapMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < AbstractMapMatrix.GetLength(1); j++)
                {
                    AbstractMapMatrix[i, j].Entropy = 50;
                    if (IsValidAUM(i, j - 1))
                    {
                        if (AbstractMapMatrix[i, j - 1].MapUnitName != "empty")
                        {
                            AbstractMapMatrix[i, j].NEConnected = true;
                            AbstractMapMatrix[i, j].Entropy -= 10;
                        }
                    }
                    if (IsValidAUM(i, j + 1))
                    {
                        if (AbstractMapMatrix[i, j + 1].MapUnitName != "empty")
                        {
                            AbstractMapMatrix[i, j].SWConnected = true;
                            AbstractMapMatrix[i, j].Entropy -= 10;
                        }
                    }
                    if (IsValidAUM(i - 1, j))
                    {
                        if (AbstractMapMatrix[i - 1, j].MapUnitName != "empty")
                        {
                            AbstractMapMatrix[i, j].NWConnected = true;
                            AbstractMapMatrix[i, j].Entropy -= 10;
                        }
                    }
                    if (IsValidAUM(i + 1, j))
                    {
                        if (AbstractMapMatrix[i + 1, j].MapUnitName != "empty")
                        {
                            AbstractMapMatrix[i, j].SEConnected = true;
                            AbstractMapMatrix[i, j].Entropy -= 10;
                        }
                    }
                    if (!(AbstractMapMatrix[i, j].SEConnected 
                        && AbstractMapMatrix[i, j].NWConnected 
                        && AbstractMapMatrix[i, j].SWConnected 
                        && AbstractMapMatrix[i, j].NEConnected))
                    {
                        if (AbstractMapMatrix[i, j].SEConnected)
                            AbstractMapMatrix[i, j].Entropy -= 2;
                        if (AbstractMapMatrix[i, j].NWConnected)
                            AbstractMapMatrix[i, j].Entropy -= 2;
                        if (AbstractMapMatrix[i, j].SWConnected)
                            AbstractMapMatrix[i, j].Entropy -= 2;
                        if (AbstractMapMatrix[i, j].NEConnected)
                            AbstractMapMatrix[i, j].Entropy -= 2;
                    }
                    else
                        AbstractMapMatrix[i, j].Entropy = 50;
                }
            }
        }
        public static void RandomSetMapUnit()
        {
            IWeightedRandomizer<string> randomizer = new DynamicWeightedRandomizer<string>();
            foreach (var abstractMapUnit in AbstractMapUnitList)
            {
                randomizer.Add(abstractMapUnit.MapUnitName, 1);
            }
            for (int i = 0; i < AbstractMapMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < AbstractMapMatrix.GetLength(1); j++)
                {
                    AbstractMapMatrix[i, j].MapUnitName = randomizer.NextWithReplacement();
                }
            }
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
    }
}
