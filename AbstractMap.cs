using MapEditor.TileInfo;
using Rampastring.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using Weighted_Randomizer;

namespace MapEditor
{
    public static class AbstractMap
    {
        public static int Width { get; private set; }
        public static int Height { get; private set; }
        public static int[] Size { get; private set; }
        public static AbstractTile[,] AbsTile { get; private set; }
        public static int Theater { get; private set; }
        public static List<AbstractMapUnit> AbstractMapUnitList { get; private set; }
        public static AbstractUnitMap[,] AbstractUnitMap { get; private set; }

        public static void Initialize(int width, int height, Theater theater)
        {
            Width = width;
            Height = height;
            Theater = (int)theater;
            int range = Width + Height;
            AbsTile = new AbstractTile[range, range];
            Size = new int[2] { Width, Height };
            AbstractMapUnitList = new List<AbstractMapUnit>();
            AbstractUnitMap = new AbstractUnitMap [(int)Math.Ceiling((float)range / 15.0), (int)Math.Ceiling((float)range / 15.0)];

            for (int y = 0; y < range; y++)
            {
                for (int x = 0; x < range; x++)
                {
                    var absTile = new AbstractTile();
                    absTile.Initialize(x, y);
                    AbsTile[x, y] = absTile;
                }
            }
            for (int i = 0; i < AbstractUnitMap.GetLength(0); i++)
            {
                for (int j = 0; j < AbstractUnitMap.GetLength(1); j++)
                {
                    AbstractUnitMap[i, j] = new AbstractUnitMap();
                }
            }

            for (int i = 0; i < AbstractUnitMap.GetLength(0); i++)
            {
                for (int j = 0; j < AbstractUnitMap.GetLength(1); j++)
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
                                        AbstractUnitMap[i, j].IsOnMap = true;
                                }
                            }
                        }
                    }

                    if (!AbstractUnitMap[i, j].IsOnMap)
                        AbstractUnitMap[i, j].Placed = true;

                    if (IsValidAUM(i,j -1))
                    {
                        if (!AbstractUnitMap[i, j - 1].IsOnMap)
                            AbstractUnitMap[i, j].NEConnected = true;
                    }
                    else
                        AbstractUnitMap[i, j].NEConnected = true;

                    if (IsValidAUM(i, j + 1))
                    {
                        if (!AbstractUnitMap[i, j + 1].IsOnMap)
                            AbstractUnitMap[i, j].SWConnected = true;
                    }
                    else
                        AbstractUnitMap[i, j].SWConnected = true;

                    if (IsValidAUM(i - 1, j))
                    {
                        if (!AbstractUnitMap[i - 1, j].IsOnMap)
                            AbstractUnitMap[i, j].NWConnected = true;
                    }
                    else
                        AbstractUnitMap[i, j].NWConnected = true;

                    if (IsValidAUM(i + 1, j))
                    {
                        if (!AbstractUnitMap[i + 1, j].IsOnMap)
                            AbstractUnitMap[i, j].SEConnected = true;
                    }
                    else
                        AbstractUnitMap[i, j].SEConnected = true;
                }
            }

            SterilizeMapUnit();
        }
        public static bool IsValidAUM(int x, int y)
        {
            if (x < AbstractUnitMap.GetLength(0) && x >= 0 && y < AbstractUnitMap.GetLength(1) && y >= 0)
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
            if (AbstractMap.Theater == 0)
                workingPath = Constants.TEMPERATEPath;
            else if (AbstractMap.Theater == 1)
                workingPath = Constants.SNOWPath;
            else if (AbstractMap.Theater == 2)
                workingPath = Constants.URBANPath;
            else if (AbstractMap.Theater == 3)
                workingPath = Constants.NEWURBANPath;
            else if (AbstractMap.Theater == 4)
                workingPath = Constants.LUNARPath;
            else if (AbstractMap.Theater == 5)
                workingPath = Constants.DESERTPath;
            else
                return;

            DirectoryInfo root = new DirectoryInfo(workingPath);
            foreach (FileInfo f in root.GetFiles())
            {
                var absMapUnit = new AbstractMapUnit();
                absMapUnit.Initialize(f);
                AbstractMapUnitList.Add(absMapUnit);
            }
        }

        public static AbstractMapUnit GetAbstractMapUnitByName(string mapUnitName)
        {
            var absMapUnit = new AbstractMapUnit();
            foreach (var pAbsMapUnit in AbstractMapUnitList)
            {
                if (mapUnitName == pAbsMapUnit.Name)
                {
                    absMapUnit = pAbsMapUnit;
                }
            }
            return absMapUnit;
        }
        public static AbstractUnitMap[] GetNearbyAbstractUnitMapInfo(int x, int y)
        {
            //order : NE NW SW SE
            var absMapUnit = new AbstractUnitMap[4];
            if (IsValidAUM(x, y - 1))
            {
                absMapUnit[0] = AbstractUnitMap[x, y - 1];
            }
            if (IsValidAUM(x - 1, y))
            {
                absMapUnit[1] = AbstractUnitMap[x - 1, y];
            }
            if (IsValidAUM(x, y + 1))
            {
                absMapUnit[2] = AbstractUnitMap[x, y + 1];
            }
            if (IsValidAUM(x + 1, y))
            {
                absMapUnit[3] = AbstractUnitMap[x + 1, y];
            }
            return absMapUnit;
        }

        public static void PlaceMapUnit(int x, int y, string mapUnitName)
        {
            var absMapUnit = GetAbstractMapUnitByName(mapUnitName);
            AbstractUnitMap[x, y].Placed = true;

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

        public static void PlaceMapUnitByAbstractUnitMap()
        {
            for (int i = 0; i < AbstractUnitMap.GetLength(0); i++)
            {
                for (int j = 0; j < AbstractUnitMap.GetLength(1); j++)
                { 
                    if (AbstractUnitMap[i,j].IsOnMap)
                    {
                        PlaceMapUnit(i, j, AbstractUnitMap[i, j].MapUnitName);
                    }
                }
            }
        }
        public static void SetMapUnit(int x, int y, string mapUnitName)
        {
            if (IsValidAUM(x,y))
            {
                AbstractUnitMap[x, y].MapUnitName = mapUnitName;
                AbstractUnitMap[x, y].Placed = true;
            }
                
        }

        public static void SetMapUnitByEntropy()
        {
            bool notAllMapUnitsSet = true;
            int failureTimes = 0;
            while (notAllMapUnitsSet)
            {
                Console.WriteLine("------------------------------");
                UpdateMapUnitInfo();
                notAllMapUnitsSet = false;
                foreach (var absMapUnit in AbstractUnitMap)
                {
                    if (!(absMapUnit.SEConnected
                        && absMapUnit.NWConnected
                        && absMapUnit.SWConnected
                        && absMapUnit.NEConnected))
                    {
                        notAllMapUnitsSet = true;
                    }
                }
                int[] targetMapUnit = GetLowestEntropyMU();
                //order : NE NW SW SE
                var nearbyUnitMap = GetNearbyAbstractUnitMapInfo(targetMapUnit[0], targetMapUnit[1]);

                var validMapUnitList = new List<AbstractMapUnit>();

                for (int i = 0; i < AbstractMapUnitList.Count; i++)
                {
                    if (AbstractMapUnitList[i].Name == "empty")
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

                for (int i = validMapUnitList.Count() -1; i >= 0; i-- )
                {
                    if (validMapUnitList.Count > 1)
                    {
                        var name = validMapUnitList[i].Name;
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
                
                IWeightedRandomizer<string> randomizer = new DynamicWeightedRandomizer<string>();
                if (validMapUnitList.Count > 0)
                {
                    Console.WriteLine("Valid map unit list:");
                    int weight = 0;
                    foreach (var abstractMapUnit in validMapUnitList)
                    {
                        randomizer.Add(abstractMapUnit.Name, abstractMapUnit.Weight);
                        weight += abstractMapUnit.Weight;
                        Console.WriteLine("  Name: " + abstractMapUnit.Name + ", Weight: "+ abstractMapUnit.Weight);
                    }
                    if (weight > 0)
                    {
                        var result = randomizer.NextWithReplacement();
                        AbstractUnitMap[targetMapUnit[0], targetMapUnit[1]].MapUnitName = result;
                        AbstractUnitMap[targetMapUnit[0], targetMapUnit[1]].Placed = true;
                        Console.WriteLine("Choose {0} to place in [{1},{2}]", result, targetMapUnit[0], targetMapUnit[1]);
                    }
                    else
                    {
                        Console.WriteLine("No valid map unit to place in [{0},{1}], because total weight is 0", targetMapUnit[0], targetMapUnit[1]);
                        failureTimes++;
                        if (failureTimes >= AbstractUnitMap.GetLength(0) * AbstractUnitMap.GetLength(1))
                        {
                            Console.WriteLine("No valid map unit to place!");
                            return;
                        }
                    }
                }
                else
                {
                    failureTimes++;
                    if (failureTimes >= AbstractUnitMap.GetLength(0) * AbstractUnitMap.GetLength(1))
                    {
                        Console.WriteLine("No valid map unit to place!");
                        return;
                    }
                }
                UpdateMapUnitInfo();
            }
        }

        public static int[] GetLowestEntropyMU()
        {
            UpdateMapUnitInfo();
            int[] lowestEntropyMU = { 0, 0 };
            int lowestEntropy = int.MaxValue;
            for (int i = 0; i < AbstractUnitMap.GetLength(0); i++)
            {
                for (int j = 0; j < AbstractUnitMap.GetLength(1); j++)
                {
                    if (AbstractUnitMap[i, j].Entropy < lowestEntropy && AbstractUnitMap[i, j].MapUnitName == "empty")
                    {
                        lowestEntropy = AbstractUnitMap[i, j].Entropy;
                        lowestEntropyMU[0] = i;
                        lowestEntropyMU[1] = j;
                    }
                }
            }
            return lowestEntropyMU;
        }

        public static void UpdateMapUnitInfo()
        {
            for (int i = 0; i < AbstractUnitMap.GetLength(0); i++)
            {
                for (int j = 0; j < AbstractUnitMap.GetLength(1); j++)
                {
                    AbstractUnitMap[i, j].Entropy = 50;
                    if (IsValidAUM(i, j - 1))
                    {
                        if (AbstractUnitMap[i, j - 1].MapUnitName != "empty")
                        {
                            AbstractUnitMap[i, j].NEConnected = true;
                            AbstractUnitMap[i, j].Entropy -= 10;
                        }
                    }
                    if (IsValidAUM(i, j + 1))
                    {
                        if (AbstractUnitMap[i, j + 1].MapUnitName != "empty")
                        {
                            AbstractUnitMap[i, j].SWConnected = true;
                            AbstractUnitMap[i, j].Entropy -= 10;
                        }
                    }
                    if (IsValidAUM(i - 1, j))
                    {
                        if (AbstractUnitMap[i - 1, j].MapUnitName != "empty")
                        {
                            AbstractUnitMap[i, j].NWConnected = true;
                            AbstractUnitMap[i, j].Entropy -= 10;
                        }
                    }
                    if (IsValidAUM(i + 1, j))
                    {
                        if (AbstractUnitMap[i + 1, j].MapUnitName != "empty")
                        {
                            AbstractUnitMap[i, j].SEConnected = true;
                            AbstractUnitMap[i, j].Entropy -= 10;
                        }
                    }
                    if (!(AbstractUnitMap[i, j].SEConnected 
                        && AbstractUnitMap[i, j].NWConnected 
                        && AbstractUnitMap[i, j].SWConnected 
                        && AbstractUnitMap[i, j].NEConnected))
                    {
                        if (AbstractUnitMap[i, j].SEConnected)
                            AbstractUnitMap[i, j].Entropy -= 2;
                        if (AbstractUnitMap[i, j].NWConnected)
                            AbstractUnitMap[i, j].Entropy -= 2;
                        if (AbstractUnitMap[i, j].SWConnected)
                            AbstractUnitMap[i, j].Entropy -= 2;
                        if (AbstractUnitMap[i, j].NEConnected)
                            AbstractUnitMap[i, j].Entropy -= 2;
                    }
                    else
                        AbstractUnitMap[i, j].Entropy = 50;
                }
            }
        }
        public static void RandomSetMapUnit()
        {
            IWeightedRandomizer<string> randomizer = new DynamicWeightedRandomizer<string>();
            foreach (var abstractMapUnit in AbstractMapUnitList)
            {
                randomizer.Add(abstractMapUnit.Name, 1);
            }
            for (int i = 0; i < AbstractUnitMap.GetLength(0); i++)
            {
                for (int j = 0; j < AbstractUnitMap.GetLength(1); j++)
                {
                    AbstractUnitMap[i, j].MapUnitName = randomizer.NextWithReplacement();
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
        public static string GetFileJson(string filepath)
        {
            string json = string.Empty;
            using (FileStream fs = new FileStream(filepath, FileMode.Open, System.IO.FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.GetEncoding("utf-8")))
                {
                    json = sr.ReadToEnd().ToString();
                }
            }
            return json;
        }
    }
}
