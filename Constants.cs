using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapEditor
{
    static class Constants
    {
        public static string WorkFolder = @"C:\Users\hanfangxu\Documents\GitHub\MapEditor_RA2\";
        public static string FileName = "map.yrm";
        public static string SaveFileName = "Output.txt";
        public static string MapPackName = "IsoMapPack5";
        public static string FilePath = WorkFolder + FileName;
        public static string SaveFilePath = WorkFolder + SaveFileName;
        public static string BitMapName = "bitmap.bmp";
        public static string BitMapPath = WorkFolder + BitMapName;
        public static string TemplateMapName = "templateMap.map";
        public static string TemplateMapPath = WorkFolder + TemplateMapName;
        public static string CommonINIPath = WorkFolder + "Common.json";
        public static string TEMPERATEINIPath = WorkFolder + "TEMPERATE.json";
        public static string SNOWINIPath = WorkFolder + "SNOW.json";
        public static string URBANINIPath = WorkFolder + "URBAN.json";
        public static string NEWURBANINIPath = WorkFolder + "NEWURBAN.json";
        public static string LUNARINIPath = WorkFolder + "LUNAR.json"; 
        public static string DESERTINIPath = WorkFolder + "DESERT.json";
    }
}
