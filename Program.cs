using MapEditor.TileInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
                        for (int i = 0; i < 10; i++)
                        {

                            var _instance = new MapFile();
                            WorkingMap.Initialize(150, 150, Theater.TEMPERATE);
                            int range = WorkingMap.Width + WorkingMap.Height;
                            WorkingMap.SetMapUnitByEntropy();
                            WorkingMap.FillRemainingEmptyUnitMap();
                            WorkingMap.PlaceMapUnitByAbstractUnitMap();
                            _instance.Width = WorkingMap.Width;
                            _instance.Height = WorkingMap.Height;
                            _instance.MapTheater = WorkingMap.Theater;
                            _instance.IsoTileList = WorkingMap.CreateTileList();
                            _instance.SaveFullMap(Constants.WorkFolder + "随机地图" + i + ".yrm");
                        }*/


            var mapFile = new MapFile();

            WorkingMap.Initialize(200, 200, Theater.TEMPERATE);
            int range = WorkingMap.Width + WorkingMap.Height;

            //AbstractMap.SetMapUnit(6, 6, "011");
            WorkingMap.SetMapUnitByEntropy();
            WorkingMap.FillRemainingEmptyUnitMap();
            WorkingMap.PlaceMapUnitByAbsMapMatrix();
            WorkingMap.CreateTechnoLists();

            //AbstractMap.PlaceTileCombination(295, 100, 100);
            //AbstractMap.RandomPlaceTileCombination(10000);

            /*            for (int i = 0; i < range; i++)
                        {
                            for (int j = 0; j < range; j++)
                            {
                                absMap.PlaceTileCombination(311, i, j);
                            }
                        }
            */



            mapFile.Width = WorkingMap.Width;
            mapFile.Height = WorkingMap.Height;
            mapFile.MapTheater = WorkingMap.Theater;
            mapFile.IsoTileList = WorkingMap.CreateTileList();
            mapFile.Unit = WorkingMap.CreateUnitINI();
            mapFile.SaveFullMap(Constants.FilePath);
            mapFile.RenderMap(Constants.FilePath);

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
