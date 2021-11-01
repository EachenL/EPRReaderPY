#对应第2第3批数据
from struct import *
from math import  *


import numpy as np

def checkSegment(br,s):
    try:
        st = str(br.read(len(s)), 'utf-8')
    except Exception:
        print(Exception.args)
        br.seek(-len(s), 1)
    else:
        if (st == s):
            return True
        else:
            br.seek(-len(s), 1)
            return False


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
class oldEPRread:
    def __init__(self, EPRfile: str, eye_radius=1.0):

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
            self.tickNumber = unpack('i', br.read(4))[0]
            self.isMale = unpack('?', br.read(1))[0]
            self.age = unpack('h', br.read(2))[0]
            self.year = unpack('h', br.read(2))[0]
            self.ln = unpack('h', br.read(2))[0]
            self.slideName = readStr(br)
            self.quickHash = readStr(br)
            self.width = unpack('i', br.read(4))[0]
            self.height = unpack('i', br.read(4))[0]
            self.cmWidth = unpack('f', br.read(4))[0]
            self.cmHeight = unpack('f', br.read(4))[0]
            self.maxWidth = unpack('i', br.read(4))[0]
            self.maxHeight = unpack('i', br.read(4))[0]
            # ppm = (self.screenPixelWidth / self.screenCMWidth +
            #        self.screenPixelHeight / self.screenCMHeight) / 2  # 像素/cm
            self.minLevel = 10  # 先将minlevel设一个很大的数
            self.data = []
            self.XY = []
            self.point_radius = []
            self.point_deg = []
            self.now_zoom = []
            self.X = []
            self.Y = []
            self.eX = []
            self.eY = []
            self.headz = []
            self.theta = []
            self.screenX = []
            self.screenY = []
            for i in range(self.tickNumber):
                screenX = unpack('i', br.read(4))[0]
                screenY = unpack('i', br.read(4))[0]
                level = unpack('i', br.read(4))[0]
                eyeX = unpack('i', br.read(4))[0]
                eyeY = unpack('i', br.read(4))[0]
                headZ = unpack('d', br.read(8))[0]
                timeStamp = unpack('d', br.read(8))[0]
                datumframe = DatumFrame(screenX, screenY, level, eyeX, eyeY,
                                        headZ, timeStamp)
                # centerX = self.screenPixelWidth / 2
                # centerY = self.screenPixelHeight / 2

                if level >= 10 and level < 0:
                    continue  # 有时候数据会异常，那就抛弃这一帧
                if i == 0:
                    self.firstLevel = level
                self.minLevel = min(level, self.minLevel)
                dl = self.firstLevel - level  # 第一帧level减这一帧的level，就是放大了几次
                # print("dl",dl)
                v = headZ, eyeX, eyeY > 0
                # print(headZ)
                # self.screenX.append(screenX)
                # self.screenY.append(screenY)
                if headZ > 10 and eyeX > 0 and eyeY > 0:
                    # r = headZ * tan(3.0 / 180) * ppm  # 计算半径r
                    x = (eyeX - screenX) / (2 ** dl)
                    y = (eyeY - screenY) / (2 ** dl)  # ppm----像素/cm
                    if 0 <= x < self.maxWidth and 0 <= y < self.maxHeight:
                        # sx = (centerX - screenX) / (2 ** dl)
                        # sy = (centerY - screenY) / (2 ** dl)
                        # theta = atan((sqrt(pow((eyeX - centerX), 2) + pow((eyeY - centerY), 2))) / (
                        #         ppm * headZ))  # 计算视线角度=rad的数值表示
                        # deg = theta * 180 / pi
                        # r = (headZ * tan(theta + eye_radius / 180 * pi) - headZ * tan(theta)) * ppm / (
                        #         2 ** dl)  # r为当前倍率下视野半径在最低倍率下的映射
                        # print('r is:',r)
                        # zoom = self.first_zoom * (2**dl)


                        # self.data.append([x, y, headZ, dl, v, sx, sy, timeStamp, deg, r])
                        self.XY.append([x, y])
                        # self.point_radius.append(r)
                        # self.point_deg.append(deg)
                        self.now_zoom.append(dl)
                        self.X.append(x)
                        self.Y.append(y)
                        self.eX.append(eyeX)
                        self.eY.append(eyeY)
                        self.headz.append(headZ)
                        # self.theta.append(theta)
            #读文件尾
            br.seek(-4, 2)
            br.seek(-unpack('i', br.read(4))[0], 2)
            self.comment = readStr(br)
            self.type = unpack('h', br.read(2))[0]
            self.typeStr = readStr(br)





def readStr(reader):
    # 获取第一个长度前缀
    s = ''
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
