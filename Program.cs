using MapEditor.TileInfo;
using Serilog;
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

                WorkingMap.Initialize(200, 200, Theater.NEWURBAN);
                int range = WorkingMap.Width + WorkingMap.Height;

                //AbstractMap.SetMapUnit(6, 6, "011");
                //WorkingMap.SetMapUnitByEntropy();
                WorkingMap.SetMapUnitByOrder();
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
                mapFile.Waypoint = WorkingMap.CreateWaypointINI();
                mapFile.AddComment(Constants.FilePath);
                mapFile.SaveFullMap(Constants.WorkFolder + "随机地图" + i + ".yrm");
            }

        }
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.File(Constants.WorkFolder+"log\\"+"log-.txt", rollingInterval: RollingInterval.Minute).CreateLogger();
            Log.Information("******************************************************");
            Log.Information("*******************Creating New Map*******************");
            Log.Information("******************************************************");
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

            WorkingMap.Initialize(200, 200, Theater.NEWURBAN);
            int range = WorkingMap.Width + WorkingMap.Height;

            WorkingMap.PlacePlayerLocation(2, "NW");
            WorkingMap.PlacePlayerLocation(2, "SW");
            WorkingMap.PlacePlayerLocation(2, "NE");
            WorkingMap.PlacePlayerLocation(2, "SE");

            //WorkingMap.IncreaseWeightContainsX(6);
            //WorkingMap.SetMapUnitByEntropy();
            WorkingMap.SetMapUnitByOrder();
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
            mapFile.Waypoint = WorkingMap.CreateWaypointINI();
            mapFile.SaveFullMap(Constants.FilePath);
            mapFile.AddComment(Constants.FilePath);
            //mapFile.RenderMap(Constants.FilePath);





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

            Log.CloseAndFlush();
        }
    }
}
