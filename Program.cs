using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MapEditor.TileInfo;

namespace MapEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            var _instance = new Mapfile();

            
            AbstractMap.Initialize(100, 100, Theater.TEMPERATE);
            int range = AbstractMap.Width + AbstractMap.Height;

            //AbstractMap.SetMapUnit(6, 6, "001");
            AbstractMap.SetMapUnitByEntropy();
            AbstractMap.PlaceMapUnitByAbstractUnitMap();
            //AbstractMap.PlaceTileCombination(295, 100, 100);
            //AbstractMap.RandomPlaceTileCombination(10000);

            /*            for (int i = 0; i < range; i++)
                        {
                            for (int j = 0; j < range; j++)
                            {
                                absMap.PlaceTileCombination(311, i, j);
                            }
                        }*/




            _instance.Width = AbstractMap.Width;
            _instance.Height = AbstractMap.Height;
            _instance.MapTheater = AbstractMap.Theater;
            _instance.Tile_input_list = AbstractMap.CreateTileList();
            _instance.SaveFullMap(Constants.FilePath);

            Console.WriteLine("press any key to exist...");
            Console.ReadKey();


            //_instance.CreateEmptyMap(80, 80);

            //_instance.SaveWorkingMapPack();

            //_instance.CreateIsoTileList(@"C:\Users\hanfangxu\Documents\GitHub\MapEditor_RA2\TileInfo\TEMPERATE\001.map");
            //_instance.SaveWorkingMapPack(Constants.WorkFolder + "mapPack.txt");
            //_instance.CreateIsoTileList(Constants.FilePath);
            //_instance.SaveWorkingMapPack(Constants.WorkFolder + "mapPack.txt");

            //_instance.LoadWorkingMapPack(Constants.WorkFolder + "mapPack.txt");
            //_instance.SaveFullMap(Constants.FilePath);


            //bit2yrm
            //_instance.CreateMapbyBitMap(Constants.BitMapPath);
            //_instance.SaveFullMap(Constants.FilePath);
        }
    }
}
