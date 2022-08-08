using MapEditor.TileInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapEditor
{
    class Program
    {
        void CreateRandomMap(int times)
        {
            for (int i = 0; i < times; i++)
            { 
                var mapFile = new MapFile();

                WorkingMap.Initialize(200, 200, Theater.TEMPERATE);
                int range = WorkingMap.Width + WorkingMap.Height;

                //AbstractMap.SetMapUnit(6, 6, "011");
                WorkingMap.SetMapUnitByEntropy();
                WorkingMap.FillRemainingEmptyUnitMap();
                WorkingMap.PlaceMapUnitByAbsMapMatrix();

                mapFile.Width = WorkingMap.Width;
                mapFile.Height = WorkingMap.Height;
                mapFile.MapTheater = WorkingMap.Theater;
                mapFile.IsoTileList = WorkingMap.CreateTileList();
                mapFile.OverlayList = WorkingMap.OverlayList;
                mapFile.Unit = WorkingMap.CreateUnitINI();
                mapFile.Infantry = WorkingMap.CreateInfantryINI();
                mapFile.Structure = WorkingMap.CreateStructureINI();
                mapFile.Terrain = WorkingMap.CreateTerrainINI();
                mapFile.Aircraft = WorkingMap.CreateAircraftINI();
                mapFile.Smudge = WorkingMap.CreateSmudgeINI();
                mapFile.SaveFullMap(Constants.WorkFolder + "随机地图" + i + ".yrm");
            }

            Console.WriteLine("press any key to exist...");
            Console.ReadKey();
        }
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

            /*            var _instance = new Program();
                        _instance.CreateRandomMap(10);*/



            var mapFile = new MapFile();

            WorkingMap.Initialize(200, 200, Theater.TEMPERATE);
            int range = WorkingMap.Width + WorkingMap.Height;

            //AbstractMap.SetMapUnit(6, 6, "011");
            WorkingMap.SetMapUnitByEntropy();
            WorkingMap.FillRemainingEmptyUnitMap();
            WorkingMap.PlaceMapUnitByAbsMapMatrix();

            mapFile.Width = WorkingMap.Width;
            mapFile.Height = WorkingMap.Height;
            mapFile.MapTheater = WorkingMap.Theater;
            mapFile.IsoTileList = WorkingMap.CreateTileList();
            mapFile.OverlayList = WorkingMap.OverlayList;
            mapFile.Unit = WorkingMap.CreateUnitINI();
            mapFile.Infantry = WorkingMap.CreateInfantryINI();
            mapFile.Structure = WorkingMap.CreateStructureINI();
            mapFile.Terrain = WorkingMap.CreateTerrainINI();
            mapFile.Aircraft = WorkingMap.CreateAircraftINI();
            mapFile.Smudge = WorkingMap.CreateSmudgeINI();
            mapFile.SaveFullMap(Constants.FilePath);
            //mapFile.RenderMap(Constants.FilePath);

            Console.WriteLine("press any key to exist...");
            Console.ReadKey();




            //_instance.CreateEmptyMap(80, 80);

            //_instance.SaveWorkingMapPack();
            /*            var _instance = new MapFile();
                        _instance.CreateIsoTileList(@"C:\Users\hanfangxu\Documents\GitHub\MapEditor_RA2\TileInfo\TEMPERATE\045.map");
                        _instance.ReadOverlay(@"C:\Users\hanfangxu\Documents\GitHub\MapEditor_RA2\TileInfo\TEMPERATE\045.map");
                        _instance.SaveWorkingOverlay(Constants.WorkFolder + "overlay.txt");
                        _instance.SaveOverlay(@"C:\Users\hanfangxu\Documents\GitHub\MapEditor_RA2\TileInfo\TEMPERATE\045.map");*/
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
