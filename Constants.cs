using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapEditor
{
    static class Constants
    {
        public static string WorkFolder = @".\";
        public static string FileName = "map.yrm";
        public static string SaveFileName = "Output.txt";
        public static string MapPackName = "IsoMapPack5";
        public static string FilePath = WorkFolder + FileName;
        public static string SaveFilePath = WorkFolder + SaveFileName;
        public static string BitMapName = "bitmap.bmp";
        public static string BitMapPath = WorkFolder + BitMapName;
        public static string TemplateMapName = "templateMap.map";
        public static string TemplateMapPath = WorkFolder + TemplateMapName;
    }
}
