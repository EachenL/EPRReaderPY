'''采样视点筛选，生成注视循序区域列表'''

from recRead import *
import numpy as np
import matplotlib.pyplot as plt
import openslide
# from sklearn import metrics
# from sklearn.cluster import Birch
from sklearn.cluster import DBSCAN
import copy
import inspect
import cv2
import imageio
'''返回突变点索引列表，从突变点开始进入下一个区域'''






def check_grad(list_name:list):
    list_len = len(list_name)
    indi = [i + 1 for i in range(list_len) if i != list_len-1 and list_name[i] != list_name[i + 1]]
    indi_len = len(indi)
    if indi_len > 0:
        change_index = [indi[i] for i in range(indi_len) if i != indi_len-1 and indi[i+1]-indi[i] > 7 ] # 间隔八个采样点
        print('indi',indi)                                                                              # 才算进入下一个区域
        change_index.append(indi[-1])
        print('change_index', change_index)
        region_num = len(change_index) + 1
    else: # 当region label无越变时
        #print('该region仅一个子区域')
        region_num = 1
        change_index = [list_len-1]
    return region_num,change_index

def get_variable_name(variable):
    callers_local_vars = inspect.currentframe().f_back.f_locals.items()
    return [var_name for var_name, var_val in callers_local_vars if var_val is variable]

'''传入视点 绘制顺序图'''


class Sequence:
    def __init__(self, path_ndpi:str, data:list,xy:list, R:list, L:list, av=None, z=None):        # xy 与r_list为dimension3 下 2维数组坐标
        # 初步筛选凝视点数据
        self.__data = data
        self.__av = av
        self.__Z = z
        self.__xy = xy
        self.__XY = []
        self.__tick_count = []
        self.__data1 = []
        self.__R = []
        self.__L = []
        self.num_data = len(self.__data)
        for i in range(len(self.__data)):
            # if self.__xy[i] not in self.__XY: # 排除重复点（我认为在聚类过程中不需要排除，在凝视区域生成中为了提高效率可以去重）
            if self.__data[i][-1] <= 30 and self.__data[i][2] > 0:
                self.__tick_count.append(self.__xy.count(self.__xy[i]))  # 计算某个视点出现的次数
                self.__XY.append(self.__xy[i])  # 添加 角速度》30 的凝视点坐标
                self.__data1.append(self.__data[i])  # 角速度 初筛 数据
                self.__R.append(R[i])  # 半径
                self.__L.append(int(L[i]))  # level
        self.__XY = np.array(self.__XY, int)
        self.__R = (np.array(self.__R, int) / 8).tolist()
        print('采样视点个数：', len(self.__xy))
        print('角速度筛选过后视点个数：', len(self.__XY))
        #('l',self.__L)
       # print('line62 region 划分')
        self.region_num,self.change_index = check_grad(self.__L)            # 注视区域数= 倍率变换点数+1
        self.__path_ndpi = path_ndpi
        self.__slide = openslide.OpenSlide(self.__path_ndpi )
        [self.__n, self.__m] = self.__slide.level_dimensions[3]
        
        img = np.array(self.__slide.read_region((0, 0), level=3, size=(self.__n, self.__m)))[:,:,:3]     # 取rgb图像区域
        #img = cv2.cvtColor(ndpi, cv2.COLOR_RGB2RGBA)

        '''注意此处如果用四通道图像处理，保存mask时为白色背景，三通道处理保存mask时为黑色背景'''
        # 建立mask图像矩阵
        self.__mask = np.zeros((self.__m,self.__n,3), np.uint8)
        self.mask_l8 = np.zeros((self.__m, self.__n, 3), np.uint8)
        self.mask_l4 = np.zeros((self.__m, self.__n, 3), np.uint8)
        self.mask_l2 = np.zeros((self.__m, self.__n, 3), np.uint8)
        self.mask_l1 = np.zeros((self.__m, self.__n, 3), np.uint8)
        self.mask_first = np.zeros((self.__m, self.__n, 3), np.uint8)

        '''建立一些属性'''
        # 区域像素值
        self.num_area_all =0
        self.num_area_8= 0;self.num_area_4=0;self.num_area_2=0;self.num_area_1=0
        self.num_area_first = 0

        # 总/各倍率 子区域数
        self.subregion_num = 0
        self.subregion_L8num = 0;self.subregion_L4num = 0
        self.subregion_L2num = 0;self.subregion_L1num = 0

        # 总/各倍率 注视点个数
        self.num_fixation = 0
        self.num_fixation8 = 0;self.num_fixation4 = 0
        self.num_fixation2 = 0;self.num_fixation1 = 0
        self.num_none_saccade = len(self.__XY)
        self.num_saccade = self.num_data - self.num_none_saccade

        # 总/各倍率 注视点坐标
        self.fixation_XY = []
        self.fixation_XY_L8 = [];self.fixation_XY_L4 = []
        self.fixation_XY_L2 = [];self.fixation_XY_L1 = []

        # 总/各倍率 注视点半径

        self.fixation_r = int(np.mean(self.__R))
        self.fixation_r_L8 = 0;self.fixation_r_L4 = 0
        self.fixation_r_L2 = 0;self.fixation_r_L1 = 0

        # 注视点 聚类标签列表
        self.subregion0_pred =[]

        # 首次凝视
        self.first_fixation_id = int
        self.num_first_fixation = int
        self.first_fixation = []
        '''  当 无倍率越变 时，直接聚类 绘制l8注视区域'''
        if self.region_num == 1:
            region0 = self.__XY
            mean_r = self.fixation_r
            model = DBSCAN(eps=mean_r, min_samples = 8)  #
            y_pred = model.fit_predict(region0)
            self.__fixation_0 = model.components_           # 二维数组
            self.__indices_f0 = model.core_sample_indices_

            # 提取 每个区域 注视点聚类的 标签列表
            self.subregion0_pred = [y_pred[i] for i in self.__indices_f0]

            # 获取 子注视区域 总/各倍率 个数
            self.subregion_num, self.change_region0 = check_grad(self.subregion0_pred)  # label突变点索引为区域内第几个注视点
            self.subregion_L8num = copy.deepcopy(self.subregion_num)

            # 寻找 第一个注视点在采样点中的位置  # 第一个子区域 即为首次凝视区域
            self.first_fixation_id = self.__xy.index(list(self.__fixation_0[0]))
            self.num_first_fixation = self.__xy.index(list(self.__fixation_0[self.change_region0[0]])) - self.first_fixation_id

            self.first_fixation = copy.deepcopy(self.__fixation_0[self.change_region0[0]])
            for i in range(len(self.first_fixation)):
                cv2.circle(self.mask_first, (self.__fixation_0[i, 0], self.__fixation_0[i, 1]), mean_r, [255, 255, 255],
                           -1)

            # 获取 总/各倍率 注视点 个数
            self.num_fixation = len(self.__fixation_0)
            self.num_fixation8 = len(self.__fixation_0)

            # 获取 总/各倍率 注视点 坐标
            self.fixation_XY = self.__fixation_0
            self.fixation_XY_L8 = self.__fixation_0

            # 获取 总/各倍率 注视点 半径
            self.fixation_r_L8 = mean_r

            # 提取 每个注视点聚类的 标签列表
            #self.subregion0_pred = [y_pred[i] for i in self.__indices_f0]

            for i in range(len(self.__fixation_0)):
                cv2.circle(self.__mask, (self.__fixation_0[i, 0], self.__fixation_0[i, 1]), mean_r, [0,255,0], -1)

            # 无倍率越变时计算 总/各倍率 注视区域像素个数  （dimension[3]下）
            img_gray = cv2.cvtColor(self.__mask, cv2.COLOR_BGR2GRAY)
            self.num_area_all = np.count_nonzero(img_gray)
            self.num_area_8 = copy.deepcopy(np.count_nonzero(img_gray))
            self.num_area_first = np.count_nonzero(cv2.cvtColor(self.mask_first, cv2.COLOR_BGR2GRAY))
            # 或者self.num_area_8 = np.count_nonzero(self.__mask[:,:,1])

            #self.__overlapping = cv2.addWeighted(img, 0.8, self.__mask, 0.5, 0)

            # 首次凝视区域提取


            # plt.scatter(self.__fixation_0[:, 0], self.__fixation_0[:, 1], marker='o', c=y_pred)
            # cv2.circle(mask, (spar.fixation_l8[i, 0], spar.fixation_l8[i, 1]), 10, [255, 0, 0, 255], -1)
            # 顺序文本
            # font = cv2.FONT_HERSHEY_SIMPLEX
            # cv2.putText(mask, '1', (X_mean_L8, Y_mean_L8), font,
            #             20, (255, 255, 255), 50, lineType=cv2.LINE_AA)


        '''  当 越变位置为1 时'''
        if self.region_num == 2:
            # print('当 越变位置为1 时')
            region0 = copy.deepcopy(self.__XY[:self.change_index[0], :])  # l8 倍率区域
            r0 = self.__R[:self.change_index[0]]

            region1 = copy.deepcopy(self.__XY[self.change_index[0]:, :])  # l4 区域
            r1 = self.__R[self.change_index[0]:]

            self.mask0 = np.zeros((self.__m, self.__n,3), np.uint8)
            self.mask1 = np.zeros((self.__m, self.__n,3), np.uint8)
            
            # l8--region0
            self.mean_r0 = int(np.mean(r0))
            model_0 = DBSCAN(eps=self.mean_r0, min_samples = 8)
            y_pred0 = model_0.fit_predict(region0)
            self.__fixation_0 = model_0.components_
            self.__indices_f0 = model_0.core_sample_indices_

            # 提取注视点聚类标签列表
            self.subregion0_pred = [y_pred0[i] for i in self.__indices_f0]

            # 获取 子注视区域 总/各倍率 个数
            self.region0_sub_num, self.change_region0 = check_grad(self.subregion0_pred)
            self.subregion_num += self.region0_sub_num
            self.subregion_L8num += self.region0_sub_num

            # 寻找 第一个注视点在采样点中的位置  # 第一个子区域 即为首次凝视区域
            self.first_fixation_id = self.__xy.index(list(self.__fixation_0[0]))
            self.num_first_fixation = self.__xy.index(list(self.__fixation_0[self.change_region0[0]])) - self.first_fixation_id

            self.first_fixation = copy.deepcopy(self.__fixation_0[self.change_region0[0]])
            for i in range(len(self.first_fixation)):
                cv2.circle(self.mask_first, (self.__fixation_0[i, 0], self.__fixation_0[i, 1]), self.mean_r0,
                           [255, 255, 255],-1)

            # 获取 总/各倍率 注视点 个数
            self.num_fixation += len(self.__fixation_0) 
            self.num_fixation8 += len(self.__fixation_0)

            # 获取 总/各倍率 注视点 坐标
            self.fixation_XY = copy.deepcopy(self.__fixation_0)
            self.fixation_XY_L8 = self.__fixation_0

            # 获取 总/各倍率 注视点 半径
            self.fixation_r_L8 = self.mean_r0


            for i in range(len(self.__fixation_0)):
                cv2.circle(self.__mask, (self.__fixation_0[i, 0], self.__fixation_0[i, 1]), self.mean_r0, [0,255,0], -1)
                cv2.circle(self.mask0, (self.__fixation_0[i, 0], self.__fixation_0[i, 1]), self.mean_r0, [0,255,0], -1)
                cv2.circle(self.mask_l8, (self.__fixation_0[i, 0], self.__fixation_0[i, 1]), self.mean_r0, [0, 255, 0],
                           -1)
                # 轮廓边界
            # img_gray = cv2.cvtColor(self.__mask, cv2.COLOR_BGR2GRAY)
            # ret, thresh = cv2.threshold(img_gray, 0, 255, cv2.THRESH_BINARY)
            #
            # img, contours, hierarchy = cv2.findContours(thresh, cv2.RETR_LIST, cv2.CHAIN_APPROX_SIMPLE)
            # cv2.drawContours(self.__mask, contours, -1, (0,0,0,255), 10) #(GBR)
            #
            #
            #
            #     # 获取轮廓中心点
            # M = cv2.moments(contours[0])
            # cX = int(M["m10"] / M["m00"])
            # cY = int(M["m01"] / M["m00"])
            # cv2.putText(mask, "1", (cX-20, cY-20),
            #             cv2.FONT_HERSHEY_SIMPLEX, 20, (255, 255, 255, 255), 50)

            # region1
            self.mean_r1 = int(np.mean(r1))
            model_1 = DBSCAN(eps=self.mean_r1, min_samples=8)  #
            y_pred1 = model_1.fit_predict(region1)
            self.__fixation_1 = model_1.components_
            self.__indices_f1 = model_1.core_sample_indices_
            print(self.__indices_f1)
            print( y_pred1)
            region_level = self.__L[self.change_index[0]]

            ###
            # 提取注视点聚类标签列表
            self.subregion1_pred = [y_pred1[i] for i in self.__indices_f1]

            self.region1_sub_num, self.change_region1 = check_grad(self.subregion1_pred)
            self.subregion_num += self.region1_sub_num

            self.num_fixation += len(self.__fixation_1)
            self.fixation_XY = np.append(self.fixation_XY, self.__fixation_1, 0)


            if region_level == 4:
                self.subregion_L4num += self.region1_sub_num    # 获取 子注视区域 总/各倍率 个数
                self.num_fixation4 += len(self.__fixation_1)              # 获取 总/各倍率 注视点 个数
                self.fixation_XY_L4 = self.__fixation_1         # 获取 总/各倍率 注视点 坐标
                self.fixation_r_L4 = self.mean_r1               # 获取 总/各倍率 注视点 半径

            if region_level == 2:
                self.subregion_L2num += self.region1_sub_num  # 获取 子注视区域 总/各倍率 个数
                self.num_fixation2 += len(self.__fixation_1)              # 获取 总/各倍率 注视点 个数
                self.fixation_XY_L2 = self.__fixation_1         # 获取 总/各倍率 注视点 坐标
                self.fixation_r_L2 = self.mean_r1               # 获取 总/各倍率 注视点 半径

            ###

            if region_level == 4:
                for i in range(len(self.__fixation_1)):
                    cv2.circle(self.__mask, (self.__fixation_1[i, 0], self.__fixation_1[i, 1]), self.mean_r1,
                               [255, 255, 0], -1)
                    cv2.circle(self.mask1, (self.__fixation_1[i, 0], self.__fixation_1[i, 1]), self.mean_r1,
                               [255, 255, 0], -1)
                    cv2.circle(self.mask_l4, (self.__fixation_1[i, 0], self.__fixation_1[i, 1]), self.mean_r1,
                               [255, 255, 0], -1)

            elif region_level == 2:
                for i in range(len(self.__fixation_1)):
                    cv2.circle(self.__mask, (self.__fixation_1[i, 0], self.__fixation_1[i, 1]), self.mean_r1,
                               [255, 0, 0], -1)
                    cv2.circle(self.mask1, (self.__fixation_1[i, 0], self.__fixation_1[i, 1]), self.mean_r1,
                               [255, 0, 0], -1)
                    cv2.circle(self.mask_l4, (self.__fixation_1[i, 0], self.__fixation_1[i, 1]), self.mean_r1,
                               [255, 0, 0], -1)
            else:
                raise ValueError(u'region1为其他倍率观看图像')

            # 倍率越变1次 时计算 总/各倍率 注视区域像素个数  （dimension[3]下）
            img_gray = cv2.cvtColor(self.__mask, cv2.COLOR_BGR2GRAY)
            img_gray0 = cv2.cvtColor(self.mask_l8, cv2.COLOR_BGR2GRAY)
            img_gray1 = cv2.cvtColor(self.mask_l4, cv2.COLOR_BGR2GRAY)
            img_gray2 = cv2.cvtColor(self.mask_l2, cv2.COLOR_BGR2GRAY)
            img_gray3 = cv2.cvtColor(self.mask_l1, cv2.COLOR_BGR2GRAY)
            self.num_area_all = np.count_nonzero(img_gray)
            self.num_area_8 = np.count_nonzero(img_gray0)
            self.num_area_4 = np.count_nonzero(img_gray1)
            self.num_area_2 = np.count_nonzero(img_gray2)
            self.num_area_1 = np.count_nonzero(img_gray3)
            self.num_area_first = np.count_nonzero(cv2.cvtColor(self.mask_first, cv2.COLOR_BGR2GRAY))
            #self.__overlapping = cv2.addWeighted(img, 0.8, self.__mask, 0.5, 0)

            # imageio.imwrite("D:\\professional learning\\Tracking\\Saved-man\\62403_0mask0.png", self.mask0)

        '''当 越变位置大于等于2时'''
        if self.region_num > 2:
            prepare_region = locals()
            prepare_r = locals()
            prepare_mask = locals()
            prepare_fixation = locals()

            self.__mask = np.zeros((self.__m,self.__n,3), np.uint8)

            for i in range(self.region_num):
                prepare_region['region' + str(i)] = []

            for j in self.change_index:
                index = self.change_index.index(j)
               # print("第\?个断点",index)
                if index == 0:
                    ''' region0 = l8'''
                    region0 = copy.deepcopy(self.__XY[:j, :])  # 开头
                    r0 = copy.deepcopy(self.__R[:j])
                    self.mean_r0 = int(np.mean(r0))
                    self.mask0 = np.zeros((self.__m, self.__n, 3), np.uint8)
                    model_0 = DBSCAN(eps=self.mean_r0, min_samples=8)  #
                    y_pred0 = model_0.fit_predict(region0)

                    self.__fixation_0 = model_0.components_
                    self.__indices_f0 = model_0.core_sample_indices_

                    ###
                    # 获取 子注视区域 总/各倍率 个数
                   # print("line 303,region0子区域计数")

                    # 提取注视点聚类标签列表
                    self.subregion0_pred = [y_pred0[i] for i in self.__indices_f0]

                    self.region0_sub_num, self.change_region0 = check_grad(self.subregion0_pred)
                    self.subregion_num += self.region0_sub_num
                    self.subregion_L8num += self.region0_sub_num

                    # 寻找 第一个注视点在采样点中的位置  # 第一个子区域 即为首次凝视区域
                    self.first_fixation_id = self.__xy.index(list(self.__fixation_0[0]))
                    self.num_first_fixation = self.__xy.index(list(self.__fixation_0[self.change_region0[0]])) - self.first_fixation_id

                    self.first_fixation = copy.deepcopy(self.__fixation_0[self.change_region0[0]])
                    for i in range(len(self.first_fixation)):
                        cv2.circle(self.mask_first, (self.__fixation_0[i, 0], self.__fixation_0[i, 1]), self.mean_r0,
                                   [255, 255, 255],-1)
                    self.num_area_first = np.count_nonzero(cv2.cvtColor(self.mask_first, cv2.COLOR_BGR2GRAY))

                    # 获取 总/各倍率 注视点 个数
                    self.num_fixation += len(self.__fixation_0)
                    self.num_fixation8 += len(self.__fixation_0)

                    # 获取 总/各倍率 注视点 坐标
                    self.fixation_XY = copy.deepcopy(self.__fixation_0)
                    self.fixation_XY_L8 = copy.deepcopy(self.__fixation_0)

                    # 获取 总/各倍率 注视点 半径
                    self.fixation_r_L8 += self.mean_r0
                    ###

                    for i in range(len(self.__fixation_0)):
                        cv2.circle(self.__mask, (self.__fixation_0[i, 0], self.__fixation_0[i, 1]), self.mean_r0, [0, 255, 0],-1) #
                        cv2.circle(self.mask0, (self.__fixation_0[i, 0], self.__fixation_0[i, 1]), self.mean_r0, [0, 255, 0], -1)
                        cv2.circle(self.mask_l8, (self.__fixation_0[i, 0], self.__fixation_0[i, 1]), self.mean_r0, [0, 255, 0], -1)

                    ''' region1'''
                    region_level = self.__L[self.change_index[0]]
                    region1 = copy.deepcopy(self.__XY[j:self.change_index[index + 1], :])  # 开头补一个区域
                    r1 = copy.deepcopy(self.__R[j:self.change_index[index + 1]])
                    self.mean_r1 = int(np.mean(r1))
                    self.mask1 = np.zeros((self.__m, self.__n, 3), np.uint8)
                    model_1 = DBSCAN(eps=self.mean_r0, min_samples = 8)  #
                    y_pred1 = model_1.fit_predict(region1)
                    self.__fixation_1 = model_1.components_
                    self.__indices_f1 = model_1.core_sample_indices_

                    ###
                   # print("line341 regbion1 子区域计数")
                    # 提取注视点聚类标签列表
                    self.subregion1_pred = [y_pred1[i] for i in self.__indices_f1]
                    self.region1_sub_num, self.change_region1 = check_grad(self.subregion1_pred)
                    self.subregion_num += self.region1_sub_num

                    self.num_fixation += len(self.__fixation_1)
                    self.fixation_XY = np.append(self.fixation_XY, self.__fixation_1, 0)

                    if region_level == 4:
                        self.subregion_L4num += self.region1_sub_num                # 获取 子注视区域 总/各倍率 个数
                        self.num_fixation4 += len(self.__fixation_1)                          # 获取 总/各倍率 注视点 个数
                        self.fixation_XY_L4 = copy.deepcopy(self.__fixation_1)      # 获取 总/各倍率 注视点 坐标
                        self.fixation_r_L4 += self.mean_r1                          # 获取 总/各倍率 注视点 半径

                    if region_level == 2:
                        self.subregion_L2num += self.region1_sub_num                # 获取 子注视区域 总/各倍率 个数
                        self.num_fixation2 += len(self.__fixation_1)                          # 获取 总/各倍率 注视点 个数
                        self.fixation_XY_L2 = copy.deepcopy(self.__fixation_1)      # 获取 总/各倍率 注视点 坐标
                        self.fixation_r_L2 += self.mean_r1                           # 获取 总/各倍率 注视点 半径

                    ### 绘图

                    if region_level == 4:
                        for i in range(len(self.__fixation_1)):
                            cv2.circle(self.__mask, (self.__fixation_1[i, 0], self.__fixation_1[i, 1]), self.mean_r1, [255, 255, 0], -1)
                            cv2.circle(self.mask1, (self.__fixation_1[i, 0], self.__fixation_1[i, 1]), self.mean_r1, [255, 255, 0], -1)
                            cv2.circle(self.mask_l4, (self.__fixation_1[i, 0], self.__fixation_1[i, 1]), self.mean_r1, [255, 255, 0], -1)
                    if region_level == 2:
                        for i in range(len(self.__fixation_1)):
                            cv2.circle(self.__mask, (self.__fixation_1[i, 0], self.__fixation_1[i, 1]), self.mean_r1, [255, 0, 0], -1)
                            cv2.circle(self.mask1, (self.__fixation_1[i, 0], self.__fixation_1[i, 1]), self.mean_r1, [255, 0, 0], -1)
                            cv2.circle(self.mask_l2, (self.__fixation_1[i, 0], self.__fixation_1[i, 1]), self.mean_r1, [255, 0, 0], -1)
                    if region_level == 1:
                        for i in range(len(self.__fixation_1)):
                            cv2.circle(self.__mask, (self.__fixation_1[i, 0], self.__fixation_1[i, 1]), self.mean_r1, [255, 0, 0], -1)
                            cv2.circle(self.mask1, (self.__fixation_1[i, 0], self.__fixation_1[i, 1]), self.mean_r1, [255, 0, 0], -1)
                            cv2.circle(self.mask_l1, (self.__fixation_1[i, 0], self.__fixation_1[i, 1]), self.mean_r1, [255, 0, 0], -1)

                elif index == len(self.change_index)-1:
                        region_level = self.__L[j]

                        prepare_region['region'+str(index+1)] = copy.deepcopy(self.__XY[j:, :])  # 结尾
                        prepare_r['r'+str(index+1)] = copy.deepcopy(self.__R[j:])
                        prepare_mask['self.mask'+str(index+1)] = np.zeros((self.__m, self.__n, 3), np.uint8)

                        region_name = prepare_region['region'+str(index+1)]
                        r_name = prepare_r['r'+str(index+1)]
                        mean_r = int(np.mean(r_name))
                        mask_name = prepare_mask["self.mask"+str(index+1)]

                        model = DBSCAN(eps= mean_r, min_samples = 8)
                        y_pred = model.fit_predict(region_name)
                        prepare_fixation['self.__fixation'+str(index+1)] = model.components_
                        indices = model.core_sample_indices_
                        fixation_name = prepare_fixation['self.__fixation'+str(index+1)]

                        ###
                       # print('line399,最后一个区域计数')
                        # 提取注视点 聚类标签列表
                        subregion_pred = [y_pred[i] for i in indices]
                        region_sub_num, change_region = check_grad(subregion_pred)
                       #print('region'+str(index+1)+'子区域数',region_sub_num)
                        self.subregion_num += region_sub_num

                        self.num_fixation += len(fixation_name)
                        self.fixation_XY = np.append(self.fixation_XY, fixation_name, 0)


                        if region_level == 8:
                            self.subregion_L8num += region_sub_num      # 获取 子注视区域 总/各倍率 个数
                            self.num_fixation8 += len(fixation_name)      # 获取 总/各倍率 注视点 个数
                            self.fixation_XY_L8 = np.append\
                                (self.fixation_XY_L8, fixation_name, 0)     # 获取 总/各倍率 注视点 坐标
                            self.fixation_r_L8 += mean_r           # 获取 总/各倍率 注视点 半径

                        if region_level == 4:
                            self.subregion_L4num += region_sub_num     # 获取 子注视区域 总/各倍率 个数
                            self.num_fixation4 += len(fixation_name)      # 获取 总/各倍率 注视点 个数
                            if len(self.fixation_XY_L4) == 0:
                                self.fixation_XY_L4 = copy.deepcopy( fixation_name)
                            else:
                                self.fixation_XY_L4 = np.append\
                                    (self.fixation_XY_L4, fixation_name, 0)     # 获取 总/各倍率 注视点 坐标
                            self.fixation_r_L4 += mean_r           # 获取 总/各倍率 注视点 半径

                        if region_level == 2:
                            self.subregion_L2num += region_sub_num  # 获取 子注视区域 总/各倍率 个数
                            self.num_fixation2 += len(fixation_name)  # 获取 总/各倍率 注视点 个数
                            if len(self.fixation_XY_L2) == 0:
                                self.fixation_XY_L2 = copy.deepcopy(fixation_name)
                            else:
                                self.fixation_XY_L2 = np.append\
                                    (self.fixation_XY_L2, fixation_name, 0)  # 获取 总/各倍率 注视点 坐标
                            self.fixation_r_L2 += mean_r  # 获取 总/各倍率 注视点 半径

                        # 提取注视点聚类标签列表

                        #print('最后一个区域region_level',region_level)
                        if region_level == 8:
                            for i in range(len(fixation_name)):
                                cv2.circle(self.__mask, (fixation_name[i, 0], fixation_name[i, 1]), mean_r, [0, 255, 0],
                                           -1)  # l8区域 绿色
                                cv2.circle(mask_name, (fixation_name[i, 0], fixation_name[i, 1]), mean_r, [0, 255, 0], -1)
                                cv2.circle(self.mask_l8, (fixation_name[i, 0],fixation_name[i, 1]), mean_r, [0, 255, 0], -1)

                        if region_level == 4:
                            for i in range(len(fixation_name)):
                                cv2.circle(self.__mask, (fixation_name[i, 0], fixation_name[i, 1]), mean_r, [255, 255, 0],
                                           -1)  # l4区域 黄色
                                cv2.circle(mask_name, (fixation_name[i, 0], fixation_name[i, 1]), mean_r, [255, 255, 0], -1)
                                cv2.circle(self.mask_l4, (fixation_name[i, 0], fixation_name[i, 1]), mean_r, [255, 255, 0], -1)

                        if region_level == 2:
                            #print("最后一个区域是2")
                            for i in range(len(fixation_name)):
                                cv2.circle(self.__mask, (fixation_name[i, 0], fixation_name[i, 1]), mean_r, [255, 0, 0],
                                           -1)  # l2区域 橙色
                                cv2.circle(mask_name, (fixation_name[i, 0], fixation_name[i, 1]), mean_r, [255, 0, 0], -1)
                                cv2.circle(self.mask_l2, (fixation_name[i, 0], fixation_name[i, 1]), mean_r, [255, 0, 0], -1)


                        if region_level == 1:
                            for i in range(len(fixation_name)):
                                cv2.circle(self.__mask, (fixation_name[i, 0], fixation_name[i, 1]), mean_r, [255, 0, 0],
                                           -1)  # l1区域 红色
                                cv2.circle(mask_name, (fixation_name[i, 0], fixation_name[i, 1]), mean_r, [255, 0, 0], -1)
                                cv2.circle(self.mask_l1, (fixation_name[i, 0], fixation_name[i, 1]), mean_r, [255, 0, 0], -1)


                        '''如何确保高倍率mask不会被低倍率掩盖住？？  先确保区域大小绘制正确然后  再  手动绘制'''
                else:
                    prepare_region['region' + str(index+1)] = copy.deepcopy(self.__XY[j:self.change_index[index+1], :]) # 中间区域
                    prepare_r['r' + str(index + 1)] = copy.deepcopy(self.__R[j:self.change_index[index+1]])
                    prepare_mask["self.mask" + str(index + 1)] = np.zeros((self.__m, self.__n, 3), np.uint8)

                    region_name =  prepare_region['region' + str(index+1)]
                    r_name = prepare_r['r' + str(index + 1)]
                    mask_name = prepare_mask["self.mask" + str(index + 1)]

                    region_level = self.__L[j]
                    mean_r = int(np.mean(r_name))

                    model = DBSCAN(eps=mean_r,min_samples = 8)
                    y_pred = model.fit_predict(region_name)
                    prepare_fixation['self.__fixation' + str(index + 1)] = model.components_
                    indices = model.core_sample_indices_
                    fixation_name = prepare_fixation['self.__fixation' + str(index + 1)]

                    ###
                   # print('region',str(index + 1),'子区域计数区域')
                    # 提取注视点 聚类标签列表
                    subregion_pred = [y_pred[i] for i in indices]
                    region_sub_num, change_region = check_grad(subregion_pred)
                    self.subregion_num += region_sub_num

                    self.num_fixation += len(fixation_name)
                    self.fixation_XY = np.append(self.fixation_XY, fixation_name, 0)

                    if region_level == 8:
                        self.subregion_L8num += region_sub_num  # 获取 子注视区域 总/各倍率 个数
                        self.num_fixation8 += len(fixation_name)  # 获取 总/各倍率 注视点 个数
                        self.fixation_XY_L8 = np.append \
                            (self.fixation_XY_L8, fixation_name, 0)  # 获取 总/各倍率 注视点 坐标
                        self.fixation_r_L8 += mean_r  # 获取 总/各倍率 注视点 半径

                    if region_level == 4:
                        self.subregion_L4num += region_sub_num  # 获取 子注视区域 总/各倍率 个数
                        self.num_fixation4 += len(fixation_name)  # 获取 总/各倍率 注视点 个数
                        if len(self.fixation_XY_L4) == 0:
                            self.fixation_XY_L4 = copy.deepcopy(fixation_name)
                        else:
                            self.fixation_XY_L4 = np.append \
                                (self.fixation_XY_L4, fixation_name, 0)  # 获取 总/各倍率 注视点 坐标
                        self.fixation_r_L4 += mean_r  # 获取 总/各倍率 注视点 半径

                    if region_level == 2:
                        self.subregion_L2num += region_sub_num  # 获取 子注视区域 总/各倍率 个数
                        self.num_fixation2 += len(fixation_name)  # 获取 总/各倍率 注视点 个数
                        if len(self.fixation_XY_L2) == 0:
                            self.fixation_XY_L2 = copy.deepcopy(fixation_name)
                        else:
                            self.fixation_XY_L2 = np.append \
                                (self.fixation_XY_L2, fixation_name, 0)  # 获取 总/各倍率 注视点 坐标
                        self.fixation_r_L2 += mean_r  # 获取 总/各倍率 注视点 半径

                    # 提取注视点聚类标签列表
                    # self.subregion1_pred = [y_pred[i] for i in self.__indices_f1]
                    ###

                    if region_level == 8:
                        for i in range(len(fixation_name)):
                            cv2.circle(self.__mask, (fixation_name[i, 0], fixation_name[i, 1]), mean_r, [0, 255, 0],
                                       -1)  # l8区域 绿色
                            cv2.circle(mask_name, (fixation_name[i, 0], fixation_name[i, 1]), mean_r, [0, 255, 0], -1)
                            cv2.circle(self.mask_l8, (fixation_name[i, 0], fixation_name[i, 1]), mean_r, [0, 255, 0], -1)

                    if region_level == 4:
                        for i in range(len(fixation_name)):
                            cv2.circle(self.__mask, (fixation_name[i, 0], fixation_name[i, 1]), mean_r, [255, 255, 0],
                                       -1)  # l4区域 黄色
                            cv2.circle(mask_name, (fixation_name[i, 0], fixation_name[i, 1]), mean_r, [255, 255, 0], -1)
                            cv2.circle(self.mask_l4, (fixation_name[i, 0], fixation_name[i, 1]), mean_r, [255, 255, 0], -1)

                    if region_level == 2:
                        for i in range(len(fixation_name)):
                            cv2.circle(self.__mask, (fixation_name[i, 0], fixation_name[i, 1]), mean_r, [255, 0, 0],
                                       -1)  # l2区域 橙色
                            cv2.circle(mask_name, (fixation_name[i, 0], fixation_name[i, 1]), mean_r, [255, 0, 0], -1)
                            cv2.circle(self.mask_l2, (fixation_name[i, 0], fixation_name[i, 1]), mean_r, [255, 0, 0], -1)

                    if region_level == 1:
                        for i in range(len(fixation_name)):
                            cv2.circle(self.__mask, (fixation_name[i, 0], fixation_name[i, 1]), mean_r, [255, 0, 0],
                                       -1)  # l1区域 红色
                            cv2.circle(mask_name, (fixation_name[i, 0], fixation_name[i, 1]), mean_r, [255, 0, 0], -1),
                            cv2.circle(self.mask_l1, (fixation_name[i, 0], fixation_name[i, 1]), mean_r, [255, 0, 0], -1)

        # # 各倍率 注视点半径
        # if self.subregion_L8num != 0:
        #     self.fixation_r_L8 = int(self.fixation_r_L8 / self.region_num)
        # else:
        #     self.fixation_r_L8 = 0
        #
        # if self.subregion_L4num != 0:
        #     self.fixation_r_L4 = int(self.fixation_r_L4 / self.region_num)
        # else:
        #     self.fixation_r_L4 = 0
        #
        # if self.subregion_L2num != 0:
        #     self.fixation_r_L2 = int(self.fixation_r_L2 / self.region_num)
        # else:
        #     self.fixation_r_L2 = 0
        #
        # if self.subregion_L1num != 0:
        #     self.fixation_r_L1 = int(self.fixation_r_L1 / self.region_num)
        # else:
        #     self.fixation_r_L1 = 0
        # 各倍率 注视点半径

            # 多次倍率越变时计算 总/各倍率 注视区域像素个数  （dimension[3]下）
            img_gray = cv2.cvtColor(self.__mask, cv2.COLOR_BGR2GRAY)
            img_gray0 = cv2.cvtColor(self.mask_l8, cv2.COLOR_BGR2GRAY)
            img_gray1 = cv2.cvtColor(self.mask_l4, cv2.COLOR_BGR2GRAY)
            img_gray2 = cv2.cvtColor(self.mask_l2, cv2.COLOR_BGR2GRAY)
            img_gray3 = cv2.cvtColor(self.mask_l1, cv2.COLOR_BGR2GRAY)
            self.num_area_all = np.count_nonzero(img_gray)
            self.num_area_8 = np.count_nonzero(img_gray0)
            self.num_area_4 = np.count_nonzero(img_gray1)
            self.num_area_2 = np.count_nonzero(img_gray2)
            self.num_area_1 = np.count_nonzero(img_gray3)

        self.__overlapping = cv2.addWeighted(img, 0.8, self.__mask, 0.5, 0)
    def region_overlapping(self):
        return self.__overlapping

    def region_mask(self):
        return self.__mask
    def av_spar_L(self):
        return self.__L
    def av_spar_R(self):
        return self.__R
    def graph_size(self):
        return self.__m , self.__n  #  m,n分别为行、列

    def saccade_point(self):
        return self.__XY


if __name__ =='__main__':
    path_ndpi = "D:\\professional learning\\Tracking\\scandata\\skin\\data\\82986 (2).ndpi"
    rec = RECread("D:\\professional learning\\Tracking\\Saved-man\\82986 (2)_男_42_19_0_0.rec",
                  "D:\\professional learning\\Tracking\\Saved-man\\82986 (2)_男_42_19_0_0.erc", path_ndpi, 2)
    L_list = rec.now_level
    R_list = rec.point_radius
    z = rec.Z
    av = rec.getAngularV()
    xy = np.array(np.array(rec.getoffset())/8,int).tolist()
    data = rec.getData()
    sq = Sequence(path_ndpi,data,xy,R_list,L_list,av,z)
    img = sq.region_overlapping()
    plt.imshow(img)
    plt.show()
    # area_first = sq.mask_first
    # plt.imshow(area_first)
    # plt.show()
    print('总子区域:',sq.subregion_num)
    print('8子区域:', sq.subregion_L8num)
    print('4子区域:', sq.subregion_L4num)
    print('2子区域:', sq.subregion_L2num)
    print('1子区域:', sq.subregion_L1num)

    print('总注视点:', sq.num_fixation)
    print('8注视点:', sq.num_fixation8)
    print('4注视点:', sq.num_fixation4)
    print('2注视点:', sq.num_fixation2)
    print('1注视点:', sq.num_fixation1)

    print('总注视点半径:', sq.fixation_r)             # 其他倍率注视点半径目前有问题 也不需要
    # print('8注视点半径:', sq.fixation_r_L8)
    # print('4注视点半径:', sq.fixation_r_L4)
    # print('2注视点半径:', sq.fixation_r_L2)
    # print('1注视点半径:', sq.fixation_r_L1)

    print('首凝位置:', sq.first_fixation_id)
    print('首凝长度:', sq.num_first_fixation)
