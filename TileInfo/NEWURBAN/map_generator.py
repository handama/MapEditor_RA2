import os
import math
import configparser
#默认与地图置于同一文件夹下运行
rootdir = "."
#游戏目录
MIXPATH = '-m "D:\Games\YURI\Red Alert 2" '
#是否渲染地图
RENDERMAP=1
#渲染器目录
RENDERERPATH = '"D:\\test\\ColdAI3rdWithCncnet_working\\Map Renderer\\CNCMaps.Renderer.exe" '
#渲染矿石
ORE = '-r '
FullMap = '-F '
#输出PNG
PNG = '-p '
content = []
register = []
#预制好的信息
MAPINFO = """MinPlayers=1
EnforceMaxPlayers=True
GameModes=Standard

"""
#初始序号
number=0
#按照玩家多少排序
order = []
list = []
for root,dirs,files in os.walk(rootdir):
    for file in files:
        list.append(os.path.join(root,file))
for i in range(0,len(list)):
    path = os.path.join(rootdir,list[i])
    if os.path.isfile(path):
        name = os.path.basename(path)
        extension = name.split('.')[-1]
        if extension == 'map': #只渲染map格式
            abspath = os.path.abspath(path)
            print("rendering "+abspath)
            #文档深度为3
            title = abspath[:-4].split("\\")
            TITLE = "["+title[-3]+"\\"+title[-2]+"\\"+title[-1]+"]\n"
            register.append(title[-3]+"\\"+title[-2]+"\\"+ title[-1] +"\n")
            AUTHOR = title[-2]
            content.append(TITLE)
            filepath = '-i "'+abspath+'" '
            outputname = '-o "'+name[:-4]+'" '
            if RENDERMAP:
                command = RENDERERPATH+filepath+PNG+outputname+MIXPATH+ORE+FullMap
                output = os.popen(command)





                        
        
