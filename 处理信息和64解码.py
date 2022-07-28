# -*- coding: utf-8 -*-
#import struct
import base64
import re

def save_to_file(file_name, contents):
    fh = open(file_name, 'w')
    fh.write(contents)
    fh.close()

#读取yrm文件
def ReadMapFile(file_name):
    mf=open(file_name,'r');
    mf=mf.read();
    linelist = mf.splitlines();
    while '' in linelist:
        linelist.remove('');#去掉换行符号
    return linelist

def ReadTitle(linelist):
    TitleList=[];
    p1 = re.compile('[\[](.*?)[\]]', re.S);
    for line in linelist:
        m=p1.match(line);
        if not m == None:
            #print(m.group(0));
            TitleList.append(m.group(0));
    return TitleList;

#得到表头对应的行数
def ReadTitleIndexList(linelist):
    TitleIndexList=[];
    TitleList=ReadTitle(linelist);
    for line in linelist:
        for title in TitleList:
            if title == line:
                TitleIndexList.append([title,linelist.index(title)]);
    return TitleIndexList


lines=ReadMapFile('test233.yrm');
'''正则化匹配表头'''

#m = p1.match('[IsoMapPack5]');



'''读取所有表头'''
TitleList=ReadTitle(lines);
        

TitleindexList=ReadTitleIndexList(lines);

start=lines.index('[IsoMapPack5]');
index1=TitleList.index('[IsoMapPack5]');
end=TitleindexList[index1+1][1];
alist=lines[start+1:end];



totalstr='';

#i = 0;
for line in alist:
    totalstr=totalstr+line.split("=")[1];
 #   i=i+1;

counts=lines[end-1].count('=');
totalstr=totalstr+'='*(counts-1);#文件末尾的补位符号会被切掉，因此要手动补一下
save_to_file('IsoMapPack5.section', totalstr)  

size_index=TitleList.index('[Map]');
size_index=TitleindexList[size_index][1];
size=lines[size_index+1].split("=")[1];
#size=(size.split(","));
save_to_file('mfs.size', size)  
#解码得到byte array
totalstr=bytes(totalstr,"utf8");#str转byte
a=base64.b64decode(totalstr);#解码
lista=list(a);#十进制数组

print(len(a));
