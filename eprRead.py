
from struct import *
from math import  *
import openslide
import xlwt
import matplotlib.pyplot as plt
import numpy as np
import zipfile
import os

class Color:
    def __init__(self, R, G, B, A):
        red = R
        green = G
        black = B
        alpha = A

class MapDatumFrame:
    def __init__(self, X, Y, R, PointCount, Level, Color, StartTick, EndTick, Comment):
        self.x = X
        self.y = Y
        self.r = R
        self.point_count = PointCount
        self.level = Level
        self.color = Color
        self.start_tick = StartTick
        self.end_tick = EndTick
        self.comment = Comment

class DatumFrame:
    def __init__(self, screenX, screenY, level, eyeX, eyeY, headZ, timeStamp):
        self.screenX = screenX
        self.screenY = screenY
        self.level = level
        self.eyeX = eyeX
        self.eyeY = eyeY
        self.headZ = headZ
        self.timeStamp = timeStamp

def level_rate(level_list):
    num = len(level_list)
    num8 = np.sum(level_list == 8 )
    num4= np.sum(level_list == 4)
    num2 = np.sum(level_list == 2)

    rate8 = num8/ num;rate4 = num4/ num;rate2 = num2/ num
    return rate8,rate4,rate2
def dis(p1, p2):
    return sroot(p1[0] - p2[0], p1[1] - p2[1])


def sroot(a, b):
    '''
        求算数平方根
    '''
    return sqrt(pow(a, 2) + pow(b, 2))

# 此类专门读epr文件，所有数据均设为公有
class EPRread:
    def __init__(self, EPRfile: str, pathndpi:str, eye_radius):

        '''
               读取EPR文件。
        '''
        # epr本质上是压缩包，先解压

        # 标注数据
        self.mapdatum_data = []
        # 阅片数据
        self.datum_data = []

        with open(EPRfile, "rb") as br:
            # 从rec文件头读取医生和切片信息

            br.seek(0, 0)
            while True:
                if (checkSegment(br, 'EPRFILE')):
                    # 当前epr文件版本号
                    self.now_version = unpack('i', br.read(4))[0]
                    # 当前录像文件的每秒帧数
                    self.fps = unpack('i', br.read(4))[0]
                    # 录像文件的总帧数
                    self.frame_count = unpack('i', br.read(4))[0]
                else:
                    br.seek(-len('EPRFILE'), 1)

                if (checkSegment(br, 'DOCTOR')):
                    # 医生信息
                    self.doctor_name = readStr(br)
                    self.doctor_is_man = unpack('?', br.read(1))[0]
                    self.doctor_age = unpack('H', br.read(2))[0]
                    self.doctor_work_year = unpack('H', br.read(2))[0]
                    self.doctor_level = unpack('H', br.read(2))[0]
                    self.doctor_readnum = unpack('H', br.read(2))[0]
                else:
                    br.seek(-len('DOCTOR'), 1)

                if (checkSegment(br, 'SLIDE')):
                    # 切片信息
                    self.slide_name = readStr(br)
                    self.quick_hash = readStr(br)
                    self.slidemaxwidth = unpack('q', br.read(8))[0]
                    self.slidemaxheight = unpack('q', br.read(8))[0]
                    self.maxwidth = unpack('q', br.read(8))[0]
                    self.maxheight = unpack('q', br.read(8))[0]
                    self.minlevel = unpack('i', br.read(4))[0]
                else:
                    br.seek(-len('SLIDE'), 1)

                if (checkSegment(br, 'SCREEN')):
                    # 屏幕信息
                    self.screen_width = unpack('i', br.read(4))[0]
                    self.screen_height = unpack('i', br.read(4))[0]
                    self.cm_width = unpack('f', br.read(4))[0]
                    self.cm_height = unpack('f', br.read(4))[0]
                    br.seek(24, 1)
                    break
            if (checkSegment(br, 'DATA')):
                a = self.frame_count

                while (a > 0):
                    a -= 1
                    datum_screenX = unpack('i', br.read(4))[0]
                    datum_screenY = unpack('i', br.read(4))[0]
                    datum_level = unpack('i', br.read(4))[0]
                    datum_eyeX = unpack('i', br.read(4))[0]
                    datum_eyeY = unpack('i', br.read(4))[0]
                    datum_headZ = unpack('d', br.read(8))[0]
                    datum_timeStamp = unpack('d', br.read(8))[0]
                    datumframe = DatumFrame(datum_screenX, datum_screenY, datum_level, datum_eyeX, datum_eyeY,
                                            datum_headZ, datum_timeStamp)
                    self.datum_data.append(datumframe)
            # 读取rec文件尾
            br.seek(-3, 2)

            if(checkSegment(br,'END') != True):
                return
            br.seek(-11, 2)
            offset = unpack('q', br.read(8))[0]
            br.seek(offset, 0)
            while True:
                if(checkSegment(br, 'THRESHOLD')):
                    # 眼动角速度阈值
                    self.threshold = unpack('d', br.read(8))[0]
                    offset = unpack('q', br.read(8))[0]
                    br.seek(offset-8, 0)
                    offset = unpack('q', br.read(8))[0]
                    br.seek(offset, 0)
                if(checkSegment(br, 'MAPDATA')):
                    # 共有多少个标注数据
                    self.mapdata_frame_count = unpack('i', br.read(4))[0]
                    mapdata_frame_count = self.mapdata_frame_count
                    while(mapdata_frame_count > 0):
                        mapdata_frame_count -= 1
                        # 请查阅EPR文件构造文档 MapDatum 数据部分
                        mapdatum_X = unpack('f', br.read(4))[0]
                        mapdatum_Y = unpack('f', br.read(4))[0]
                        mapdatum_Round = unpack('f', br.read(4))[0]
                        mapdatum_point_count = unpack('i', br.read(4))[0]
                        mapdatum_level = unpack('i', br.read(4))[0]
                        mapdatum_R = unpack('f', br.read(4))[0]
                        mapdatum_G = unpack('f', br.read(4))[0]
                        mapdatum_B = unpack('f', br.read(4))[0]
                        mapdatum_A = unpack('f', br.read(4))[0]
                        mapdatum_color = Color(mapdatum_R, mapdatum_G, mapdatum_B, mapdatum_A)
                        mapdatum_start_tick = unpack('i', br.read(4))[0]
                        mapdatum_end_tick = unpack('i', br.read(4))[0]
                        mapdatum_comment = readStr(br)
                        mapdatum_frame = MapDatumFrame(mapdatum_X,mapdatum_Y,mapdatum_Round,mapdatum_point_count,mapdatum_level,mapdatum_color,mapdatum_start_tick,mapdatum_end_tick,mapdatum_comment)
                        self.mapdatum_data.append(mapdatum_frame)
                    offset = unpack('q', br.read(8))[0]
                    br.seek(offset-8, 0)
                    offset = unpack('q', br.read(8))[0]
                    br.seek(offset, 0)
                if(checkSegment(br, 'TAIL')):
                    # 医生的文本注释信息
                    self.comment = readStr(br)
                    if(self.now_version < 22):
                        br.seek(2, 1)
                    # 医生选择的病变类型
                    self.type_str = readStr(br)
                    break










def checkSegment(br,s):
    st = str(br.read(len(s)), 'utf-8')
    if(st == s):
        return True
    else:
        br.seek(-len(s), 1)
        return False



def readStr(reader):
    # 获取第一个长度前缀
    len = unpack("B", reader.read(1))[0]
    if len == 0:
        return "0"
    # 判断是否有下一个长度前缀
    if len >> 7 == -1:
        len = len & 0b01111111 + unpack("b", reader.read(1))[0] * 128
        # reader.seek(2, 1)
    else:
        len = len & 0b01111111
        # reader.seek(1, 1)
    try:
        s = str(reader.read(len), 'utf-8')
    except Exception as e:
        print(e)
    return s

if __name__ == "__main__":  # 这里是示例用法
    pathndpi = "C:\\Users\\shund\\Desktop\\SightPoint\\11111.ndpi"
    eprfile = "C:\\Users\\shund\\Desktop\\SightPoint\\111112.epr"
    rec = EPRread(eprfile, pathndpi, 2)
    print(rec.comment)
    level_List = np.array(rec.now_level,np.uint8)

    n8,r4,r2 = level_rate(level_List)

    # book = xlwt.Workbook()
    # sheet1 = book.add_sheet('radius', cell_overwrite_ok=True)
    # listname = [u'视角',u'视野半径',u'角速度']
    # data = rec.getData()
    #
    # for j in range(len(listname)):
    #     sheet1.write(0, j, listname[j])
    #
    # for i in range(len(data)):
    #     line = i+1
    #     sheet1.write(line, 0, data[i][-3])
    #     sheet1.write(line, 1, data[i][-2])
    #     sheet1.write(line, 2, data[i][-1])
    #
    # book.save("D:\\professional learning\\Tracking\\Saved-man\\radius\\result.xlt")


    # x = rec.getX()
    # y = rec.getY()
    # maxx = np.max(x)
    # maxy = np.max(y)