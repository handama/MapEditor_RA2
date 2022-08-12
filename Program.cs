using MapEditor.TileInfo;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace MapEditor
{
    class Program
    {
        void CreateRandomMap(int times)
        {
            var r = new Random();
            for (int i = 0; i < times; i++)
            {
                var mapFile = new MapFile();

                WorkingMap.Initialize(r.Next(140,200), r.Next(130, 200), Theater.NEWURBAN);
                int range = WorkingMap.Width + WorkingMap.Height;

                int player = r.Next(1000);
                if (player > 900)
                {
                    WorkingMap.PlacePlayerLocation(2, "NW");
                    WorkingMap.PlacePlayerLocation(2, "SW");
                    WorkingMap.PlacePlayerLocation(2, "NE");
                    WorkingMap.PlacePlayerLocation(2, "SE");
                }
                else if (player > 800)
                {
                    WorkingMap.PlacePlayerLocation(1, "NW");
                    WorkingMap.PlacePlayerLocation(1, "SW");
                    WorkingMap.PlacePlayerLocation(1, "NE");
                    WorkingMap.PlacePlayerLocation(1, "SE");
                }
                else if (player > 700)
                {
                    WorkingMap.PlacePlayerLocation(4, "NW");
                    WorkingMap.PlacePlayerLocation(4, "SE");
                }
                else if (player > 500)
                {
                    WorkingMap.PlacePlayerLocation(1, "N");
                    WorkingMap.PlacePlayerLocation(1, "S");
                    WorkingMap.PlacePlayerLocation(1, "W");
                    WorkingMap.PlacePlayerLocation(1, "E");
                    WorkingMap.PlacePlayerLocation(1, "NE");
                    WorkingMap.PlacePlayerLocation(1, "SE");
                    WorkingMap.PlacePlayerLocation(1, "NW");
                    WorkingMap.PlacePlayerLocation(1, "SW");
                }
                else if (player > 400)
                {
                    WorkingMap.PlacePlayerLocation(3, "N");
                    WorkingMap.PlacePlayerLocation(3, "S");
                }
                else if (player > 300)
                {
                    WorkingMap.PlacePlayerLocation(3, "E");
                    WorkingMap.PlacePlayerLocation(3, "W");
                }
                else if (player > 200)
                {
                    WorkingMap.PlacePlayerLocation(2, "N");
                    WorkingMap.PlacePlayerLocation(2, "S");
                    WorkingMap.PlacePlayerLocation(2, "W");
                    WorkingMap.PlacePlayerLocation(2, "E");
                }
                else if (player > 100)
                {
                    WorkingMap.PlacePlayerLocation(2, "N");
                    WorkingMap.PlacePlayerLocation(2, "S");
                    WorkingMap.PlacePlayerLocation(2, "SW");
                    WorkingMap.PlacePlayerLocation(2, "NE");
                }
                else
                {
                    WorkingMap.PlacePlayerLocation(3, "SW");
                    WorkingMap.PlacePlayerLocation(3, "NE");
                }


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
                mapFile.SaveFullMap(Constants.WorkFolder + "随机地图" + i + ".yrm");

                mapFile.CorrectPreviewSize(Constants.WorkFolder + "随机地图" + i + ".yrm");
                mapFile.CalculateStartingWaypoints(Constants.WorkFolder + "随机地图" + i + ".yrm");
                mapFile.RandomSetLighting(Constants.WorkFolder + "随机地图" + i + ".yrm");
                mapFile.RenderMapAndGeneratePreview(Constants.WorkFolder + "随机地图" + i + ".yrm");
                mapFile.AddComment(Constants.WorkFolder + "随机地图" + i + ".yrm");
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

            var _instance = new Program();
            _instance.CreateRandomMap(20);



            var mapFile = new MapFile();

            WorkingMap.Initialize(140, 140, Theater.NEWURBAN);
            int range = WorkingMap.Width + WorkingMap.Height;

            /*WorkingMap.PlacePlayerLocation(1, "N");
            WorkingMap.PlacePlayerLocation(1, "S");
            WorkingMap.PlacePlayerLocation(1, "W");
            WorkingMap.PlacePlayerLocation(1, "E");*/
            WorkingMap.PlacePlayerLocation(2, "NE");
            WorkingMap.PlacePlayerLocation(2, "SE");
            WorkingMap.PlacePlayerLocation(2, "NW");
            WorkingMap.PlacePlayerLocation(2, "SW");

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

            //var bitmap = new Bitmap(@"C:\Users\hanfangxu\Documents\GitHub\MapEditor_RA2\test.bmp");
            //mapFile.GenerateMapPreview(bitmap, Constants.FilePath);
            mapFile.CorrectPreviewSize(Constants.FilePath);
            mapFile.CalculateStartingWaypoints(Constants.FilePath);
            mapFile.RandomSetLighting(Constants.FilePath);
            mapFile.RenderMapAndGeneratePreview(Constants.FilePath); 
            mapFile.AddComment(Constants.FilePath);




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
