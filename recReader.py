# 对应第1批数据

import os
from struct import *

class RECreader:
    def __init__(self, RECfile: str, ERCfile: str, newVersion=False):
        '''
            读取REC和ERC文件。如果是新版ERC文件则要令newVersion=True
        '''

        typelist = ["黑色素瘤", "基底细胞癌", "良性痣", "其他"]
        try:
            br = open(RECfile, "rb")
            ebr = open(ERCfile, "rb")
        except:
            return
        # 先读取rec文件尾
        try:
            br.seek(-4, 2)
        except:
            print(RECfile)
            return

        len = unpack("i", br.read(4))[0]  # 注释信息总长度
        br.seek(-len, 2)
        self.comment = readStr(br)
        self.type = readStr(br)
        if self.type == None or self.type == '':
            i = 1
        # self.diag = typelist[self.type]
        # 从rec文件头读取医生信息
        br.seek(0, 0)
        isMan = unpack("?", br.read(1))[0]
        age = unpack("H", br.read(2))[0]
        time = unpack("H", br.read(2))[0]
        level = unpack("H", br.read(2))[0]
        self.__doctor = [isMan, age, time, level]
        self.fileName = readStr(br)
        # 从erc文件未读取一个整数，代表记录的总帧数
        ebr.seek(-4, 2)
        self.__tickNum = unpack("i", ebr.read(4))[0]
        # 从erc文件头读取两个整数，代表录制时的屏幕宽高
        ebr.seek(0, 0)
        width = unpack("i", ebr.read(4))[0]
        height = unpack("i", ebr.read(4))[0]
        mmheight = unpack("f", ebr.read(4))[0]
        mmwidth = unpack("f", ebr.read(4))[0]
        self.__screen = [width, height, mmwidth, mmheight]
        self.__data = []
        nowTick = 0
        l = 8
        while True:
            if nowTick >= self.__tickNum:
                break
            # 获取模式信息
            mode = unpack("i", br.read(4))[0]
            if mode == 0:  # 移动
                sx = -int(unpack("f", br.read(4))[0] - width / 2) * l
                sy = int(unpack("f", br.read(4))[0] + height / 2) * l
                nowTick += 1
                x = unpack("i", ebr.read(4))[0]
                y = unpack("i", ebr.read(4))[0]
                z = unpack("f", ebr.read(4))[0]
                t = unpack("f", ebr.read(4))[0]
                if z > 0:
                    v = True
                else:
                    v = False
                self.__data.append([x, y, z, l, v, sx, sy, t])
            elif mode == 1:  # 放大
                if l > 1:
                    l /= 2
            elif mode == 2:  # 缩小
                if l < 8:
                    l *= 2
            elif mode == 3:  # 终止
                break
            else:  # 文件损坏
                raise Exception("Unexpected mode " + str(mode) + " at Tick " + str(nowTick))
    
    def getDoctorInfo(self):
        return self.__doctor
        
    def getSlideFileName(self):
        return self.fileName

    def getScreenInfo(self):
        return self.__screen

    def getTickNum(self):
        return self.__tickNum

    def getData(self):
        return self.__data

    def getComment(self):
        return self.comment
    
    def getType(self):
        return self.type

def readStr(reader):
    # 获取第一个长度前缀
    len = unpack("b", reader.read(1))[0]
    if len == 0:
        return ""
    # 判断是否有下一个长度前缀
    if len >> 7 == 1:
        len = len & 0b01111111 + unpack("b", reader.read(1))[0] * 128
    else:
        len = len & 0b01111111
    return str(reader.read(len), "utf-8")