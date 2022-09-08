using RandomMapGenerator.TileInfo;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using CommandLine;
using Rampastring.Tools;
using System.IO;

namespace RandomMapGenerator
{
    public class Options
    {

        [Option('w', "width", Required = false, HelpText = "确定地图的宽度")]
        public int Width { get; set; }

        [Option('h', "height", Required = false, HelpText = "确定地图的高度")]

        public int Height { get; set; }

        [Option('n', "name", Default = "", Required = false, HelpText = "设置地图的名称")]
        public string Name { get; set; }

        [Option("type", Required = true, HelpText = "指定地图的类型（WorkingFolder下的子文件夹）")]
        public string Type { get; set; }

        [Option('g', "gamemode", Required = false, HelpText = "指定地图的游戏类型")]
        public string Gamemode { get; set; }

        [Option("nep", Required = false, HelpText = "在东北方向放置玩家（参数=数量）")]
        public int NE { get; set; }

        [Option("nwp", Required = false, HelpText = "在西北方向放置玩家（参数=数量）")]
        public int NW { get; set; }

        [Option("sep", Required = false, HelpText = "在东南方向放置玩家（参数=数量）")]
        public int SE { get; set; }

        [Option("swp", Required = false, HelpText = "在西南方向放置玩家（参数=数量）")]
        public int SW { get; set; }

        [Option("np", Required = false, HelpText = "在正北方向放置玩家（参数=数量）")]
        public int N { get; set; }

        [Option("sp", Required = false, HelpText = "在正南方向放置玩家（参数=数量）")]
        public int S { get; set; }

        [Option("wp", Required = false, HelpText = "在正西方向放置玩家（参数=数量）")]
        public int W { get; set; }

        [Option("ep", Required = false, HelpText = "在正东方向放置玩家（参数=数量）")]
        public int E { get; set; }

        [Option("no-thumbnail", Default = false ,Required = false, HelpText = "不渲染地图全图")]
        public bool NoThumbnail { get; set; }

        //[Option("no-thumbnail-output", Default = false, Required = false, HelpText = "不输出地图全图，但是会生成载入缩略图")]
        //public bool NoThumbnailOutput { get; set; }

        [Option('t' ,"total-random", Required = false, HelpText = "完全随机的产生地图（参数=生成数量）")]
        public int TotalRandom { get; set; }

        [Option("number", Required = false, HelpText = "生成地图的数量，不能与total-random共用")]
        public int Number { get; set; }

        [Option('d' ,"damanged-building", Required = false, HelpText = "建筑物将会随机受损")]
        public bool DamangedBuilding { get; set; }

        [Option('s', "smudge", Required = false, HelpText = "随机产生污染与弹坑（参数=生成密度，建议低于0.1）")]
        public double Smudge { get; set; }


    }
    class Program
    {
        public static string WorkingFolder;
        public static string OutputFolder;
        public static string RenderderPath;
        public static string GameFolder;
        public static string TemplateMap;
        public static string ProgramFolder;
        public static string RulesPath;
        public static string ArtPath;

        static void Main(string[] args)
        {
            if (args.Count() == 0)
            {
                Console.WriteLine("请从run.bat启动程序！");
                Console.WriteLine("可以运行help.bat查看帮助");
                Console.WriteLine("按任意键退出...");
                Console.ReadKey();
                return;
            }

            Log.Logger = new LoggerConfiguration().WriteTo.File(@".\log\"+"log-.txt", rollingInterval: RollingInterval.Minute).CreateLogger(); //.MinimumLevel.Error()


            var result = Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunOptions)
                .WithNotParsed(HandleParseError);

            Log.CloseAndFlush();
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            if (errs.IsVersion())
            {
                Console.WriteLine("");
                return;
            }

            if (errs.IsHelp())
            {
                Console.WriteLine("按任意键退出...");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("未能识别命令行参数！");
            Console.ReadKey();
            return;
        }

    
        private static void RunOptions(Options option)
        {
            var mapFile = new MapFile();

            var settings = new IniFile("settings.ini").GetSection("settings");
            var workingFokderTemp = settings.GetStringValue("WorkingFolder", ".").EndsWith("\\") ? settings.GetStringValue("WorkingFolder", ".") : settings.GetStringValue("WorkingFolder", ".") + "\\";

            WorkingFolder = (workingFokderTemp + option.Type).EndsWith("\\") ? workingFokderTemp + option.Type : workingFokderTemp + option.Type + "\\";
            Console.WriteLine(WorkingFolder);
            if (!Directory.Exists(WorkingFolder))
            {
                Console.WriteLine("指定的地图类型文件夹不存在！");
                Console.ReadKey();
                return;
            }

            OutputFolder = settings.GetStringValue("OutputFolder", ".").EndsWith("\\") ? settings.GetStringValue("OutputFolder", ".") : settings.GetStringValue("OutputFolder", ".") + "\\";
            ProgramFolder = Environment.CurrentDirectory;

            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

# if x64
            RenderderPath = settings.GetStringValue("RenderderPath64", ".");
#elif x86
            RenderderPath = settings.GetStringValue("RenderderPath32", ".");
#endif
            GameFolder = settings.GetStringValue("GameFolder", ".").EndsWith("\\") ? settings.GetStringValue("GameFolder", ".") : settings.GetStringValue("GameFolder", ".") + "\\";
            TemplateMap = settings.GetStringValue("TemplateMap", "templateMap.map");
            RulesPath = settings.GetStringValue("RulesPath", "rulesmd.ini");
            ArtPath = settings.GetStringValue("ArtPath", "artmd.ini");

            var outputName = settings.GetStringValue("OutputName", "No Name");
            var outputExtension = settings.GetStringValue("OutputExtension", "yrm");
            var internalName = settings.GetStringValue("OutputInternalName", "No Name");


            bool loop = true;
            int count = 0;

            var r = new Random();

            if (option.TotalRandom > 0 && option.Number > 0)
                return;
            
            while (loop)
            {
                Log.Information("******************************************************");
                Log.Information("*******************Creating New Map*******************");
                Log.Information("******************************************************");
                string fullPath = "";
                string internalNameRandom = "";
                if (option.TotalRandom == 0 && option.Number == 0)
                {
                    if (option.Name != "")
                    {
                        outputName = option.Name;
                        internalName = option.Name;
                    }
                    
                    fullPath = OutputFolder + outputName + "." + outputExtension;
                    Console.WriteLine("Generating random map " + outputName + "." + outputExtension + " ...");
                }
                    
                else
                {
                    fullPath = OutputFolder + outputName + (count + 1).ToString() + "." + outputExtension;
                    Console.WriteLine("Generating random map " + outputName + (count + 1).ToString() + "." + outputExtension + " ...");
                    internalNameRandom = internalName + string.Format(" {0:D2}", count + 1);
                }
                    

                //for total random
                int player = r.Next(1300);

                if (option.TotalRandom == 0)
                {
                    if (option.Width > 0 && option.Height > 0)
                        WorkingMap.Initialize(option.Width, option.Height, WorkingFolder);
                    else if (option.Width == 0 && option.Height > 0)
                        WorkingMap.Initialize(r.Next(130, 200), option.Height, WorkingFolder);
                    else if (option.Width > 0 && option.Height == 0)
                        WorkingMap.Initialize(option.Width, r.Next(130, 200), WorkingFolder);
                    else
                        WorkingMap.Initialize(r.Next(130, 200), r.Next(130, 200), WorkingFolder);
                }
                else
                {
                    if (player <=200)
                        WorkingMap.Initialize(r.Next(90, 140), r.Next(90, 140), WorkingFolder);
                    else if (player <= 500)
                        WorkingMap.Initialize(r.Next(100, 170), r.Next(100, 160), WorkingFolder);
                    else if (player <= 1100)
                        WorkingMap.Initialize(r.Next(130, 200), r.Next(130, 200), WorkingFolder);
                    else
                        WorkingMap.Initialize(r.Next(70, 130), r.Next(70, 130), WorkingFolder);
                    //small map case
                }

                //Sterilize once
                if (count == 0)
                    WorkingMap.SterilizeMapUnit(WorkingFolder);

                int range = WorkingMap.Width + WorkingMap.Height;

                if (option.TotalRandom == 0)
                {
                    if (option.NE + option.NW + option.SE + option.SW + option.N + option.S + option.W + option.E > 8)
                    {
                        Log.Error("Player number cannot exceed 8!");
                        return;
                    }
                    else
                    {
                        WorkingMap.PlacePlayerLocation(option.N, "N");
                        WorkingMap.PlacePlayerLocation(option.S, "S");
                        WorkingMap.PlacePlayerLocation(option.W, "W");
                        WorkingMap.PlacePlayerLocation(option.E, "E");
                        WorkingMap.PlacePlayerLocation(option.NE, "NE");
                        WorkingMap.PlacePlayerLocation(option.SE, "SE");
                        WorkingMap.PlacePlayerLocation(option.NW, "NW");
                        WorkingMap.PlacePlayerLocation(option.SW, "SW");
                    }
                }
                else
                {
                    if (player > 1200)
                    {
                        WorkingMap.PlacePlayerLocation(1, "N");
                        WorkingMap.PlacePlayerLocation(1, "SW");
                        WorkingMap.PlacePlayerLocation(1, "SE");
                    }
                    else if (player > 1150)
                    {
                        WorkingMap.PlacePlayerLocation(1, "NE");
                        WorkingMap.PlacePlayerLocation(1, "SW");
                    }
                    else if (player > 1100)
                    {
                        WorkingMap.PlacePlayerLocation(1, "NW");
                        WorkingMap.PlacePlayerLocation(1, "SE");
                    }
                    else if (player > 1000)
                    {
                        WorkingMap.PlacePlayerLocation(2, "NW");
                        WorkingMap.PlacePlayerLocation(2, "SW");
                        WorkingMap.PlacePlayerLocation(2, "NE");
                        WorkingMap.PlacePlayerLocation(2, "SE");
                    }
                    else if (player > 900)
                    {
                        WorkingMap.PlacePlayerLocation(2, "N");
                        WorkingMap.PlacePlayerLocation(2, "S");
                        WorkingMap.PlacePlayerLocation(2, "SE");
                        WorkingMap.PlacePlayerLocation(2, "NW");
                    }
                    else if (player > 800)
                    {
                        WorkingMap.PlacePlayerLocation(4, "NW");
                        WorkingMap.PlacePlayerLocation(4, "SE");
                    }
                    else if (player > 600)
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
                    else if (player > 500)
                    {
                        WorkingMap.PlacePlayerLocation(2, "N");
                        WorkingMap.PlacePlayerLocation(2, "S");
                        WorkingMap.PlacePlayerLocation(2, "W");
                        WorkingMap.PlacePlayerLocation(2, "E");
                        
                    }
                    else if (player > 400)
                    {
                        WorkingMap.PlacePlayerLocation(3, "SW");
                        WorkingMap.PlacePlayerLocation(3, "NE");
                    }
                    else if (player > 300)
                    {
                        WorkingMap.PlacePlayerLocation(3, "N");
                        WorkingMap.PlacePlayerLocation(3, "S");
                    }
                    else if (player > 200)
                    {
                        WorkingMap.PlacePlayerLocation(3, "E");
                        WorkingMap.PlacePlayerLocation(3, "W");
                    }
                    else if (player > 100)
                    {
                        WorkingMap.PlacePlayerLocation(2, "NW");
                        WorkingMap.PlacePlayerLocation(2, "SE");
                    }
                    else
                    {
                        WorkingMap.PlacePlayerLocation(1, "NW");
                        WorkingMap.PlacePlayerLocation(1, "SW");
                        WorkingMap.PlacePlayerLocation(1, "NE");
                        WorkingMap.PlacePlayerLocation(1, "SE");
                    }
                }

                //WorkingMap.RandomPlaceMUInCenter(r.Next(20, 80));

                WorkingMap.SetMapUnitByOrder();
                WorkingMap.FillRemainingEmptyUnitMap();
                WorkingMap.PlaceMapUnitByAbsMapMatrix();

                if (option.DamangedBuilding)
                {
                    int damangeMin = r.Next(10, 90);
                    int damangeMax = r.Next(damangeMin + 10, 200);
                    int destroyP = (100 - damangeMin) / 10 - 2;
                    WorkingMap.ChangeStructureHealth(damangeMin, damangeMax, destroyP);
                    //WorkingMap.ChangeUnitAirInfHealth(damangeMin, damangeMax); //this should not be used
                    // because neutral units will go to neutral service depots
                }

                if (option.Smudge > 0)
                {
                    WorkingMap.RandomPlaceSmudge(option.Smudge); //not stable
                }
                WorkingMap.ReadyForMiniMap();

                mapFile.Width = WorkingMap.Width;
                mapFile.Height = WorkingMap.Height;
                mapFile.MapTheater = WorkingMap.MapTheater;
                mapFile.IsoTileList = WorkingMap.CreateTileList();
                mapFile.OverlayList = WorkingMap.OverlayList;
                mapFile.Unit = WorkingMap.CreateUnitINI();
                mapFile.Infantry = WorkingMap.CreateInfantryINI();
                mapFile.Structure = WorkingMap.CreateStructureINI();
                mapFile.Terrain = WorkingMap.CreateTerrainINI();
                mapFile.Aircraft = WorkingMap.CreateAircraftINI();
                mapFile.Smudge = WorkingMap.CreateSmudgeINI();
                mapFile.Waypoint = WorkingMap.CreateWaypointINI();

                mapFile.SaveFullMap(fullPath);

                //mapFile.CorrectPreviewSize(fullPath);
                mapFile.CalculateStartingWaypoints(fullPath);
                mapFile.RandomSetLighting(fullPath);
                mapFile.ChangeGamemode(fullPath, option.Gamemode);
                mapFile.AddAdditionalINI(fullPath, WorkingFolder + "addition.ini");

                if (option.TotalRandom == 0 && option.Number == 0)
                {
                    mapFile.ChangeName(fullPath, internalName);
                }
                else
                {
                    mapFile.ChangeName(fullPath, internalNameRandom);
                }
                
               /* if (option.NoThumbnailOutput)
                {
                    mapFile.GeneratePreview(fullPath);
                    option.NoThumbnail = true;
                }*/

                if (!option.NoThumbnail)
                    mapFile.RenderMap(fullPath);


                
                mapFile.CreateBitMapbyMap(fullPath);


                mapFile.AddComment(fullPath);

                if (option.TotalRandom == 0 && option.Number == 0)
                    loop = false;
                else
                {
                    count++;
                    if ((option.TotalRandom != 0 && count > option.TotalRandom - 1 )|| (option.Number != 0 && count > option.Number - 1))
                        break;
                }
            }
            WorkingMap.CountMapUnitUsage();
        }
    }
}
