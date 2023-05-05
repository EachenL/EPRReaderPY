from typing import List 
from dataclasses import dataclass 
from datetime import timedelta
from .doctor_detail import DoctorDetail
from .raw_data_frame import RawDataFrame
from .data_frame import DataFrame

@dataclass 
class EasyPathologyRecord:
    # 文件创建时间
    createDateTime: timedelta

    # 文件版本号
    version: int

    # 每秒帧率
    framesPerSecond: int

    # EPR文件的总帧数
    frameCount: int

    # EPR文件中记录的屏幕横向分辨率
    screenPixelWidth: int

    # EPR文件中记录的屏幕纵向分辨率
    screenPixelHeight: int

    # 刚进入阅片时的宽度
    initialWidth: int

    # 刚进入阅片时的高度
    initialHeight: int

    # 阅片时，最大倍率的宽度
    maxLevelWidth: int

    # 阅片时，最大倍率的高度
    maxLevelHeight: int

    # 切片文件的最大宽度
    maxSlideWidth: int

    # 切片文件的最大高度
    maxSlideHeight: int

    # 最大level，也就是刚进入阅片的level
    maxLevel: int

    # 最小level
    minLevel: int

    # 医生总共看了多少个level
    totalLevelCount: int

    # EPR文件中记录的屏幕物理宽度，单位是cm
    actualScreenWidthCentimeter: float

    # EPR文件中记录的屏幕物理高度，单位是cm
    actualScreenHeightCentimeter: float

    # EPR文件中记录的切片文件名，不含路径，带拓展名
    slideName: str

    # EPR文件中记录的切片的快速哈希值
    quickHash: str

    # EPR文件中记录的医生信息
    doctorDetail: DoctorDetail

    # EPR文件中记录的医生注释
    additionalComment: str

    # EPR文件中记录的医生判断类型字符串
    selectedType: str

    # 数据帧的原始数据
    rawDataFrames: List[RawDataFrame]

    # 数据帧
    dataFrames: List[DataFrame]

    # 记录当前EPR文件的路径
    eprFilePath: str

    # 对应了每个VirtualLevel的缩放
    levelScales: List[int]

    # 总时间
    totalTime: timedelta

    additionalInfoSet: any