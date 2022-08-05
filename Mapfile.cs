using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using Rampastring.Tools;
using MapEditor.TileInfo;
using System.Linq.Expressions;

namespace MapEditor
{
    class Mapfile
    {
        public int Width;
        public int Height;
        public int MapTheater;
        public List<IsoTile> Tile_input_list;
        public void CreateIsoTileList(string filePath)
        {
            var MapFile = new IniFile(filePath);
            var MapPackSections = MapFile.GetSection(Constants.MapPackName);
            var MapSize = MapFile.GetStringValue("Map", "Size", "0,0,0,0");
            string IsoMapPack5String = "";

            int sectionIndex = 1;
            while (MapPackSections.KeyExists(sectionIndex.ToString()))
            {
                IsoMapPack5String += MapPackSections.GetStringValue(sectionIndex.ToString(), "");
                sectionIndex++;
            }

            string[] sArray = MapSize.Split(',');
            Width = Int32.Parse(sArray[2]);
            Height = Int32.Parse(sArray[3]);
            int cells = (Width * 2 - 1) * Height;
            IsoTile[,] Tiles = new IsoTile[Width * 2 - 1, Height];//这里值得注意
            byte[] lzoData = Convert.FromBase64String(IsoMapPack5String);

            //Console.WriteLine(cells);
            int lzoPackSize = cells * 11 + 4;
            var isoMapPack = new byte[lzoPackSize];
            uint totalDecompressSize = Format5.DecodeInto(lzoData, isoMapPack);//TODO 源，目标 输入应该是解码后长度，isoMapPack被赋值解码值了
                                                                               //uint	0 to 4,294,967,295	Unsigned 32-bit integer	System.UInt32
            var mf = new MemoryFile(isoMapPack);

            //Console.WriteLine(BitConverter.ToString(lzoData));
            int count = 0;
            //List<List<IsoTile>> TilesList = new List<List<IsoTile>>(Width * 2 - 1);
            Tile_input_list = new List<IsoTile>();
            //Console.WriteLine(TilesList.Capacity);
            for (int i = 0; i < cells; i++)
            {
                ushort rx = mf.ReadUInt16();//ushort	0 to 65,535	Unsigned 16-bit integer	System.UInt16
                ushort ry = mf.ReadUInt16();
                short tilenum = mf.ReadInt16();//short	-32,768 to 32,767	Signed 16-bit integer	System.Int16
                short zero1 = mf.ReadInt16();//Reads a 2-byte signed integer from the current stream and advances the current position of the stream by two bytes.
                byte subtile = mf.ReadByte();//Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
                byte z = mf.ReadByte();
                byte zero2 = mf.ReadByte();

                count++;
                int dx = rx - ry + Width - 1;

                int dy = rx + ry - Width - 1;
                //Console.WriteLine("{1}", rx, ry, tilenum, subtile, z, dx, dy,count);
                //上面是一个线性变换 旋转45度、拉长、平移
                if (dx >= 0 && dx < 2 * Width &&
                    dy >= 0 && dy < 2 * Height)
                {
                    var tile = new IsoTile((ushort)dx, (ushort)dy, rx, ry, z, tilenum, subtile);//IsoTile定义是NumberedMapObject

                    Tiles[(ushort)dx, (ushort)dy / 2] = tile;//给瓷砖赋值
                    Tile_input_list.Add(tile);
                }
            }
            //用来检查有没有空着的
            for (ushort y = 0; y < Height; y++)
            {
                for (ushort x = 0; x < Width * 2 - 1; x++)
                {
                    var isoTile = Tiles[x, y];//从这儿来看，isoTile指的是一块瓷砖，Tile是一个二维数组，存着所有瓷砖
                                              //isoTile的定义在TileLayer.cs里
                    if (isoTile == null)
                    {
                        // fix null tiles to blank
                        ushort dx = (ushort)(x);
                        ushort dy = (ushort)(y * 2 + x % 2);
                        ushort rx = (ushort)((dx + dy) / 2 + 1);
                        ushort ry = (ushort)(dy - rx + Width + 1);
                        Tiles[x, y] = new IsoTile(dx, dy, rx, ry, 0, 0, 0);
                    }
                }

            }
        }

        public void SaveIsoMapPack5(string path)
        {
            long di = 0;
            int cells = (Width * 2 - 1) * Height;
            int lzoPackSize = cells * 11 + 4;
            var isoMapPack2 = new byte[lzoPackSize];
            foreach (var tile in Tile_input_list)
            {
                var bs = tile.ToMapPack5Entry().ToArray();//ToMapPack5Entry的定义在MapObjects.cs
                                                          //ToArray将ArrayList转换为Array：
                Array.Copy(bs, 0, isoMapPack2, di, 11);//把bs复制给isoMapPack,从di索引开始复制11个字节
                di += 11;//一次循环复制11个字节
            }

            var compressed = Format5.Encode(isoMapPack2, 5);

            string compressed64 = Convert.ToBase64String(compressed);
            int j = 1;
            int idx = 0;

            var saveFile = new IniFile(path);
            saveFile.AddSection(Constants.MapPackName);
            var saveMapPackSection = saveFile.GetSection(Constants.MapPackName);

            while (idx < compressed64.Length)
            {
                int adv = Math.Min(74, compressed64.Length - idx);//74是什么
                saveMapPackSection.SetStringValue(j.ToString(), compressed64.Substring(idx, adv));
                j++;
                idx += adv;//idx=adv+1
            }
            saveFile.WriteIniFile();
        }
        public void SaveWorkingMapPack(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            var mapPack = new IniFile(path);
            mapPack.AddSection("mapPack");
            var mapPackSection = mapPack.GetSection("mapPack");
            int mapPackIndex = 1;
            mapPackSection.SetStringValue("0", "Dx,Dy,Rx,Ry,Z,TileNum,SubTile");
            mapPack.SetStringValue("Map", "Size", Width.ToString() + "," + Height.ToString());

            for (int i = 0; i < Tile_input_list.Count; i++)
            {
                var isoTile = Tile_input_list[i];
                mapPackSection.SetStringValue(mapPackIndex++.ToString(),
                       isoTile.Dx.ToString() + "," +
                       isoTile.Dy.ToString() + "," +
                       isoTile.Rx.ToString() + "," +
                       isoTile.Ry.ToString() + "," +
                       isoTile.Z.ToString() + "," +
                       isoTile.TileNum.ToString() + "," +
                       isoTile.SubTile.ToString());
            }
            mapPack.WriteIniFile();
        }

        public void CreateEmptyMap(int width, int height)
        {
            Width = width;
            Height = height;
            Tile_input_list = new List<IsoTile>();

            int cells = (Width * 2 - 1) * Height;
            IsoTile[,] Tiles = new IsoTile[Width * 2 - 1, Height];
            for (ushort y = 0; y < Height; y++)
            {
                for (ushort x = 0; x < Width * 2 - 1; x++)
                {
                    ushort dx = (ushort)(x);
                    ushort dy = (ushort)(y * 2 + x % 2);
                    ushort rx = (ushort)((dx + dy) / 2 + 1);
                    ushort ry = (ushort)(dy - rx + Width + 1);
                    Tiles[x, y] = new IsoTile(dx, dy, rx, ry, 0, (int)Common._000_Empty, 0);
                    Tile_input_list.Add(Tiles[x, y]);
                }
            }
        }

        public void CreateMapbyBitMap(string filename)
        {
            var srcBitmap = (Bitmap)Bitmap.FromFile(filename, false);
            Width = srcBitmap.Width;
            Height = srcBitmap.Height;
            Tile_input_list = new List<IsoTile>();

            int cells = (Width * 2 - 1) * Height;
            IsoTile[,] Tiles = new IsoTile[Width * 2 - 1, Height];
            for (ushort y = 0; y < Height; y++)
            {
                for (ushort x = 0; x < Width * 2 - 1; x++)
                {
                    ushort dx = (ushort)(x);
                    ushort dy = (ushort)(y * 2 + x % 2);
                    ushort rx = (ushort)((dx + dy) / 2 + 1);
                    ushort ry = (ushort)(dy - rx + Width + 1);

                    int bmpX = (int)Math.Floor((decimal)(x / 2));
                    int bmpY = y;
                    var color = srcBitmap.GetPixel(bmpX, bmpY);
                    int drawTile;
                    if (color.B > color.R)
                    {
                        if (color.B > color.G)
                        {
                            Random random = new Random(x * y);
                            drawTile = random.Next(322, 326);//sea
                        }
                        else
                        {
                            drawTile = (int)Common._000_Empty;//grass
                        }
                    }
                    else
                    {
                        if (color.R > color.G)
                        {
                            drawTile = 493;//sand
                        }
                        else
                        {
                            drawTile = (int)Common._000_Empty;//grass
                        }
                    }
                    Tiles[x, y] = new IsoTile(dx, dy, rx, ry, 0, (short)drawTile, 0);
                    Tile_input_list.Add(Tiles[x, y]);
                }
            }
        }

        public void SaveFullMap(string path)
        {
            var fullMap = new IniFile(Constants.TemplateMapPath);
            fullMap.SetStringValue("Map", "Size", "0,0,"+ Width.ToString() + "," + Height.ToString());
            fullMap.SetStringValue("Map", "LocalSize", "2,5," + (Width - 4).ToString() + "," + (Height - 11).ToString());
            fullMap.SetStringValue("Map", "Theater", Enum.GetName(typeof(Theater), MapTheater));
            fullMap.WriteIniFile(path);
            SaveIsoMapPack5(path);
        }
        public void LoadWorkingMapPack(string path)
        {
            Tile_input_list = new List<IsoTile>();
            var mapPack = new IniFile(path);
            var mapPackSection = mapPack.GetSection("mapPack");
            string[] size = mapPack.GetStringValue("Map", "Size", "0,0").Split(',');
            Width = int.Parse(size[0]);
            Height = int.Parse(size[1]);

            int i = 1;
            while (mapPackSection.KeyExists(i.ToString()))
            {
                if (mapPackSection.KeyExists(i.ToString()))
                {
                    string[] isoTileInfo = mapPackSection.GetStringValue(i.ToString(), "").Split(',');
                    var isoTile = new IsoTile(ushort.Parse(isoTileInfo[0]),
                        ushort.Parse(isoTileInfo[1]),
                        ushort.Parse(isoTileInfo[2]),
                        ushort.Parse(isoTileInfo[3]),
                        (byte)int.Parse(isoTileInfo[4]),
                        short.Parse(isoTileInfo[5]),
                        (byte)int.Parse(isoTileInfo[6]));
                    Tile_input_list.Add(isoTile);
                    i++;
                }
            }
        }
    }
}
