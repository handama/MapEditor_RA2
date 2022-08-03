using MapEditor.TileInfo;
using Rampastring.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace MapEditor
{
    public class AbstractMap
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int[] Size { get; private set; }
        public AbstractTile[,] AbsTile { get; private set; }
        public int Theater { get; private set; }
        public List<TileCombination> TileCombinationList { get; private set; }
        public List<TileCombinationType> TileCombinationTypeList { get; private set; }

        public void Initialize(int width, int height, Theater theater)
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
                    absTile.Initialize(x, y, this);
                    AbsTile[x, y] = absTile;
                }
            }

            SterilizeTileCombination();
        }

        public void SterilizeTileCombination()
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
        public void PlaceTileCombination(int tileNum, int x, int y, int z = 0)
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
                        absTile.SetProperty(x + i, y + j, z, this, absTileType);
                        AbsTile[x + i, y + j] = absTile;
                    }
                }
            }
            var tileCombination = new TileCombination();
            tileCombination.Initialize(x, y, AbsTile[x, y].Z, tileNum, this);
            TileCombinationList.Add(tileCombination);
        }

        public int[,] GetConnectOptions(string direction, int tileNum)
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

        public void PlaceConnectionTC(TileCombination tileCombination, int[] connectOptionInfo)
        {
            int tileNum = connectOptionInfo[0];
            int x = connectOptionInfo[1] + tileCombination.X;
            int y = connectOptionInfo[2] + tileCombination.Y;
            int z = connectOptionInfo[3] + tileCombination.Z;
            PlaceTileCombination(tileNum, x, y, z);
        }

        public void RandomPlaceTileCombination(int times)
        {
            for (int i = 0; i < times; i++)
            {
                var list = TileCombinationList;
                for (int j = 0; j < list.Count; j++)
                {
                    var tileCombination = list[i];
                    if (!tileCombination.NEConnected)
                    {
                        var options = GetConnectOptions("NE", tileCombination.TileNum);
                        if (options != null)
                        {
                            Random r = new Random(tileCombination.X * tileCombination.Y);
                            int selectedIndex = r.Next(0, options.GetLength(0));
                            int[] option = { options[selectedIndex, 0],
                            options[selectedIndex, 1],
                            options[selectedIndex, 2],
                            options[selectedIndex, 3] };
                            PlaceConnectionTC(tileCombination, option);
                        }
                    }
                    if (!tileCombination.NWConnected)
                    {
                        var options = GetConnectOptions("NW", tileCombination.TileNum);
                        if (options != null)
                        {
                            Random r = new Random((tileCombination.X - tileCombination.Y) * (int)DateTime.Now.ToFileTimeUtc());
                            int selectedIndex = r.Next(0, options.GetLength(0));
                            int[] option = { options[selectedIndex, 0],
                            options[selectedIndex, 1],
                            options[selectedIndex, 2],
                            options[selectedIndex, 3] };
                            PlaceConnectionTC(tileCombination, option);
                        }
                    }
                    if (!tileCombination.SEConnected)
                    {
                        var options = GetConnectOptions("SE", tileCombination.TileNum);
                        if (options != null)
                        {
                            Random r = new Random(tileCombination.X * tileCombination.Y);
                            int selectedIndex = r.Next(0, options.GetLength(0));
                            int[] option = { options[selectedIndex, 0],
                            options[selectedIndex, 1],
                            options[selectedIndex, 2],
                            options[selectedIndex, 3] };
                            PlaceConnectionTC(tileCombination, option);
                        }
                    }
                    if (!tileCombination.SWConnected)
                    {
                        var options = GetConnectOptions("SW", tileCombination.TileNum);
                        if (options != null)
                        {
                            Random r = new Random(tileCombination.X * tileCombination.Y);
                            int selectedIndex = r.Next(0, options.GetLength(0));
                            int[] option = { options[selectedIndex, 0],
                            options[selectedIndex, 1],
                            options[selectedIndex, 2],
                            options[selectedIndex, 3] };
                            PlaceConnectionTC(tileCombination, option);
                        }
                    }
                }
            }
        }

        public List<IsoTile> CreateTileList()
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
        public string GetFileJson(string filepath)
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
