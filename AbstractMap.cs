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
        public static List<TileCombination> TileCombinationList { get; private set; }
        public static List<TileCombinationType> TileCombinationTypeList { get; private set; }

        public static void Initialize(int width, int height, Theater theater)
        {
            Width = width;
            Height = height;
            Theater = (int)theater;
            int range = Width + Height;
            AbsTile = new AbstractTile[range, range];
            Size = new int[2] { Width, Height };
            TileCombinationList = new List<TileCombination>();
            TileCombinationTypeList = new List<TileCombinationType>();

            for (int y = 0; y < range; y++)
            {
                for (int x = 0; x < range; x++)
                {
                    var absTile = new AbstractTile();
                    absTile.Initialize(x, y);
                    AbsTile[x, y] = absTile;
                }
            }

            SterilizeTileCombination();
        }

        public static void SterilizeTileCombination()
        {
            string JsonName;
            if (Theater == 0)
                JsonName = Constants.TEMPERATEINIPath;
            else if (Theater == 1)
                JsonName = Constants.SNOWINIPath;
            else if (Theater == 2)
                JsonName = Constants.URBANINIPath;
            else if (Theater == 3)
                JsonName = Constants.NEWURBANINIPath;
            else if (Theater == 4)
                JsonName = Constants.LUNARINIPath;
            else if (Theater == 5)
                JsonName = Constants.DESERTINIPath;
            else
                return;

            string jsonText = GetFileJson(JsonName);
            JObject jsonObj = JObject.Parse(jsonText);

            foreach (JProperty jProperty in jsonObj.Properties())
            {
                var tileCombinationType = JsonConvert.DeserializeObject<TileCombinationType>(jProperty.Value.ToString());
                tileCombinationType.InitializeAfterJson();
                TileCombinationTypeList.Add(tileCombinationType);
            }
        }
        public static void PlaceTileCombination(int tileNum, int x, int y, int z = 0, string direction = "")
        {
            var tileCombinationType = new TileCombinationType();
            foreach (var pTileCombinationType in TileCombinationTypeList)
            {
                if (pTileCombinationType.TileNum == tileNum)
                    tileCombinationType = pTileCombinationType;
            }

            for (int i = 0; i < tileCombinationType.Width; i++)
            {
                for (int j = 0; j < tileCombinationType.Height; j++)
                {
                    if (x + i >= Width + Height || y + j >= Width + Height|| x + i < 0 || y + j < 0)
                    {
                        return;
                    }
                    if (AbsTile[x + i, y + j].Edited)
                        return;
                }
            }
            for (int i = 0; i < tileCombinationType.Width; i++)
            {
                for (int j = 0; j < tileCombinationType.Height; j++)
                {
                    var absTileType = tileCombinationType.AbsTileType[i, j];
                    if (absTileType.Used)
                    {
                        var absTile = new AbstractTile();
                        absTile.SetProperty(x + i, y + j, z, absTileType);
                        AbsTile[x + i, y + j] = absTile;
                    }
                }
            }
            var tileCombination = new TileCombination();
            tileCombination.Initialize(x, y, AbsTile[x, y].Z, tileNum);

            if (direction.Equals("NE"))
                tileCombination.NEConnected = true;
            else if (direction.Equals("NW"))
                tileCombination.NWConnected = true;
            else if (direction.Equals("SW"))
                tileCombination.SWConnected = true;
            else if (direction.Equals("SE"))
                tileCombination.SEConnected = true;

            TileCombinationList.Add(tileCombination);
        }

        public static int[,] GetConnectOptions(string direction, int tileNum)
        {
            foreach (var pTileCombinationType in TileCombinationTypeList)
            {
                if (pTileCombinationType.TileNum == tileNum)
                {
                    if (string.Equals(direction, "NW", StringComparison.OrdinalIgnoreCase) && pTileCombinationType.NWCanConnect)
                        return pTileCombinationType.NWConnectOptions;
                    else if (string.Equals(direction, "NE", StringComparison.OrdinalIgnoreCase) && pTileCombinationType.NECanConnect)
                        return pTileCombinationType.NEConnectOptions;
                    else if (string.Equals(direction, "SW", StringComparison.OrdinalIgnoreCase) && pTileCombinationType.SWCanConnect)
                        return pTileCombinationType.SWConnectOptions;
                    else if (string.Equals(direction, "SE", StringComparison.OrdinalIgnoreCase) && pTileCombinationType.SECanConnect)
                        return pTileCombinationType.SEConnectOptions;
                }
            }
            return null;
        }

        public static void PlaceConnectionTC(TileCombination tileCombination, int[] connectOptionInfo, string direction)
        {
            int tileNum = connectOptionInfo[0];
            int x = connectOptionInfo[1] + tileCombination.X;
            int y = connectOptionInfo[2] + tileCombination.Y;
            int z = connectOptionInfo[3] + tileCombination.Z;
            string rDirection = "";
            if (direction.Equals("NE"))
                rDirection = "SW";
            else if (direction.Equals("NW"))
                rDirection = "SE";
            else if (direction.Equals("SW"))
                rDirection = "NE";
            else if (direction.Equals("SE"))
                rDirection = "NW";
            PlaceTileCombination(tileNum, x, y, z, rDirection);
        }

        public static void RandomPlaceTileCombination(int times)
        {
            int i = 0;
            while (true)
            {
                if (i >= times)
                    return;
                i++;

                int minEntropy = int.MaxValue;
                int minEntropyIndex = 0;
                for (int j = 0; j < TileCombinationList.Count; j++)
                {
                    if (TileCombinationList[j].NEConnected
                    && TileCombinationList[j].NWConnected
                    && TileCombinationList[j].SWConnected
                    && TileCombinationList[j].SEConnected)
                        continue;

                    TileCombinationList[j].SetEntropy();
                    if (TileCombinationList[j].Entropy < minEntropy && TileCombinationList[j].Entropy != 0)
                    {
                        minEntropy = TileCombinationList[j].Entropy;
                        minEntropyIndex = j;
                    }
                }
                if (minEntropy == int.MaxValue)
                    return;

                var tileCombination = TileCombinationList[minEntropyIndex]; 
                if (!tileCombination.NEConnected)
                {
                    var options = GetConnectOptions("NE", tileCombination.TileNum);
                    if (options != null)
                    {
                        int selectedIndex = GetRandomTCWeighted(options);
                        int[] option = { options[selectedIndex, 1],
                        options[selectedIndex, 2],
                        options[selectedIndex, 3],
                        options[selectedIndex, 4], };
                        TileCombinationList[minEntropyIndex].NEConnected = true;
                        PlaceConnectionTC(tileCombination, option, "NE");
                    }
                }
                if (!tileCombination.NWConnected)
                {
                    var options = GetConnectOptions("NW", tileCombination.TileNum);
                    if (options != null)
                    {
                        int selectedIndex = GetRandomTCWeighted(options);
                        int[] option = { options[selectedIndex, 1],
                        options[selectedIndex, 2],
                        options[selectedIndex, 3],
                        options[selectedIndex, 4] };
                        TileCombinationList[minEntropyIndex].NWConnected = true;
                        PlaceConnectionTC(tileCombination, option, "NW");
                    }
                }
                if (!tileCombination.SEConnected)
                {
                    var options = GetConnectOptions("SE", tileCombination.TileNum);
                    if (options != null)
                    {
                        int selectedIndex = GetRandomTCWeighted(options);
                        int[] option = { options[selectedIndex, 1],
                        options[selectedIndex, 2],
                        options[selectedIndex, 3],
                        options[selectedIndex, 4] };
                        TileCombinationList[minEntropyIndex].SEConnected = true;
                        PlaceConnectionTC(tileCombination, option, "SE");
                    }
                }
                if (!tileCombination.SWConnected)
                {
                    var options = GetConnectOptions("SW", tileCombination.TileNum);
                    if (options != null)
                    {
                        int selectedIndex = GetRandomTCWeighted(options);
                        int[] option = { options[selectedIndex, 1],
                        options[selectedIndex, 2],
                        options[selectedIndex, 3],
                        options[selectedIndex, 4] };
                        TileCombinationList[minEntropyIndex].SWConnected = true;
                        PlaceConnectionTC(tileCombination, option, "SW");
                    }
                }
            }
        }

        public static int GetRandomTCWeighted(int[,] options)
        {
            IWeightedRandomizer<string> randomizer = new DynamicWeightedRandomizer<string>();
            for (int i = 0; i < options.GetLength(0); i++)
            {
                randomizer.Add(i.ToString(), options[i, 0]);
            }
            return int.Parse(randomizer.NextWithReplacement());
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
