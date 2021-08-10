
from struct import *
from math import  *
import openslide
import xlwt
import matplotlib.pyplot as plt
import numpy as np
import zipfile
import os
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


class EPRread:
    def __init__(self, EPRfile: str, pathndpi:str, eye_radius):

        '''
               读取EPR文件。
        '''
        # epr本质上是压缩包，先解压
        with open(EPRfile, "rb") as br:
            # 先读取rec文件尾
            br.seek(-4, 2)
            leng = unpack("i", br.read(4))[0]  # 注释信息总长度
            br.seek(-leng, 2)
            self.__comment = readStr(br) #医生的注释信息
            self.__type = readStr(br)
            self.__eye_radius = eye_radius
            # 从rec文件头读取医生信息
            br.seek(0, 0)
            isMan = unpack("?", br.read(1))[0]#unpack("?",br.read(n))[0] n代表读取的字节数 0代表第一个列表
            age = unpack("H", br.read(2))[0]
            time = unpack("H", br.read(2))[0]
            level = unpack("H", br.read(2))[0] & 0b11111111
            self.__doctor = [isMan, age, time, level]
            self.__fileName = readStr(br)
            self.quickHash = readStr(br)  # 快速哈希值
            # 从erc文件未读取一个整数，代表记录的总帧数
            br.seek(-4, 2)
            self.__tickNum = unpack("i", br.read(4))[0]
            # 从erc文件头读取两个整数，代表录制时的屏幕宽高
            br.seek(0, 0)
            width = unpack("i", br.read(4))[0]
            height = unpack("i", br.read(4))[0]
            mmheight = unpack("f", br.read(4))[0]
            mmwidth = unpack("f", br.read(4))[0]
            self.__screen = [width, height, mmwidth, mmheight]

            #######求能显示的绝对坐标的大小######
            slide = openslide.OpenSlide(pathndpi)
            [m, n] = slide.level_dimensions[0]
            k = ( mmwidth/ m + mmheight / n) / 2   #m,n为该原始切片的大小【m,n】 k为屏幕中能够显示的绝对坐标的大小 mm/个,
            #print('[m,n]is',[m,n])
            # print(width, height)
            # print(mmwidth,mmheight)
            # print('k is ',k)

            #######添加data数据######self.__data.append([x, y, z, l, v, sx, sy,t,theta,r,degree])
            self.__data = []
            self.__offset = []
            self.__X = []; self.point_radius=[]
            self.__Y = []; self.point_deg =[]
            self.now_level = []
            self.__scan = []
            self.Z = []; self.__T = []
            nowTick = 0
            l = 8           # 不同mode将改变当前帧的l
            while True:
                if nowTick >= self.__tickNum:
                    break
                # 获取模式信息
                mode = unpack("i", br.read(4))[0]
                if mode == 0:  # 移动
                    #br.seek(8, 1)
                    sx = -int(unpack("f", br.read(4))[0] - width / 2) * l
                    sy = int(unpack("f", br.read(4))[0] + height / 2) * l
                    # print('rawsy:', sy/l-height / 2)
                    nowTick += 1
                    x = unpack("i", br.read(4))[0]
                    y = unpack("i", br.read(4))[0]
                    z = unpack("f", br.read(4))[0]
                    t = unpack("f", br.read(4))[0]
                    theta = atan((sqrt(pow((x-sx), 2)+pow((y-sy), 2))*8*k/l)/z)  # 计算视线角度=rad的数值表示
                    deg = theta *180/pi
                    r =(z*tan(theta+self.__eye_radius/180*pi)-z*tan(theta))*l/(8*k) # r为当前倍率下视野半径在最低倍率下的映射
                    # print('r is:',r)
                    # print('视角 is:', theta)
                    # print('z is:', z,'\n')
                    if z > 0:
                        v = True
                    else:
                        v = False
                    self.__data.append([x, y, z, l, v, sx, sy, t, deg,r]) #sx,sy 为屏幕中心坐标
                    self.__offset.append([x, y])
                    self.__X.append(x)
                    self.__Y.append(y)
                    self.point_radius.append(r)
                    self.point_deg.append(deg)
                    self.now_level.append(l)
                    self.Z.append(z)
                    self.__T.append(t)

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
                self.__scan.append([mode, l])
            self.mean_deg = np.mean(self.point_deg)
            self.mean_radius = int(np.mean(self.point_radius))
            # print('theta is ',theta)
            # print('r is ', r)

            ##############求角速度#################################################################################################
            screen = self.__screen
            ppm = (screen[0] / screen[2] + screen[1] / screen[3]) / 2  #像素/毫米，绝大多数屏幕都是方形像素，如果不是也按平均值处理
            self.__av = []
            for i in range(len(self.__data) - 1):
                if self.__data[i][3] != self.__data[i + 1][3]:  # 这一帧是倍率切换帧，不处理，用上一帧数据代替
                    self.__av.append(self.__av[-1])
                    continue
                #同level下 的视点移动
                thisData = self.__data[i]
                thisLevel = thisData[3]
                nextData = self.__data[i + 1]
                nextLevel = nextData[3]
                if isnan(thisData[2]):  # 表示此帧眼动仪未取到头部位置
                    if isnan(nextData[2]):  # 下一帧为nan
                        self.__av.append(0)
                        continue
                    else:
                        h = nextData[2]
                else:
                    if isnan(nextData[2]):  # 下一帧为nan
                        h = thisData[2]
                    else:
                        h = (thisData[2] + nextData[2]) / 2  # 取平均距离
                SX = (thisData[5] + nextData[5]) / 2   # 求屏幕中点绝对平均坐标
                SY = (thisData[6] + nextData[6]) / 2

                #ppm处理是否不合理？   我觉得应该用mm/个坐标 or   k 为 mm/绝对坐标
                ac = dis(thisData[0: 2], [SX,SY]) *thisLevel *k               #ac = 多少毫米 ；  ac = dis(thisData[0: 2], [SX,SY]) / thisLevel/ ppm  ppm为像素/mm
                ad = sroot(ac, h)                                                   #眼睛至A视点距离
                bc = dis(nextData[0: 2], [SX,SY]) * nextLevel  *k
                bd = sroot(bc, h)                                                   #眼睛至B视点距离
                ab = dis([thisData[0] , thisData[1] ],
                         [nextData[0] , nextData[1] ]) * thisLevel*k #两视点距离    # 在thislevel下 相距坐标占多少mm

                cosab = (pow(ad, 2) + pow(bd, 2) - pow(ab, 2)) / (2 * ad * bd)      #两视点间视角 余弦
                if cosab > 1:
                    cosab = 1
                elif cosab < 0:
                    cosab = 0
                angle = acos(cosab)         #两视点间视角（返回pi对应数字值 如 pi=3.14）
                rads = angle / (nextData[-3] - thisData[-3])        #角速度 rad/s
                degree_s = 180 * rads/pi                                 #   degree = 180 * rads  不对吧？
                self.__data[i].append(degree_s)  ##self.__data.append([x, y, z, l, v, sx, sy,t,deg,r,degree_s])
                self.__av.append(degree_s)

###########以上求角速度##############################################################
    def getAngularV(self): #获取角速度
        return self.__av

    def getMode(self):
        return self.__scan
    def getDoctorInfo(self):  #获取医生信息
        return self.__doctor
        
    def getSlideFileName(self):  #获取文件信息
        return self.__fileName

    def getScreenInfo(self):  #获取 长宽像素，以及屏幕尺寸（mm）
        return self.__screen

    def getTickNum(self): #获取总帧数
        return self.__tickNum

    def getData(self):  #获取数据
        return self.__data

    def getoffset(self):   #视点的坐标x,y
        return self.__offset

    def getComment(self):
        return self.__comment
    
    def getType(self):   #获取切片病理类型
        return self.__type
    def getX(self):
        return self.__X
    def getY(self):
        return self.__Y
    def getT(self):
        return self.__T

def readStr(reader):
    # 获取第一个长度前缀
    len = unpack("b", reader.read(1))[0]
    if len == 0:
        return "0"
    # 判断是否有下一个长度前缀
    if len >> 7 == -1:
        len = len & 0b01111111 + unpack("b", reader.read(1))[0] * 128
    else:
        len = len & 0b01111111
    return str(reader.read(len), "utf-8")

if __name__ == "__main__":  # 这里是示例用法
    pathndpi = "D:\\lxy_workspace\\scripts\\python\\data\\ndpi\\63356.ndpi"
    rec = EPRread("D:\\lxy_workspace\\scripts\\python\\data\\2020.11.30阅片数据\\man_43_20_0_0\\63356.epr",pathndpi,2)

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