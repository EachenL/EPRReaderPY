from typing import List 
from dataclasses import dataclass 
from datetime import timedelta
from .doctor_detail import DoctorDetail
from .raw_data_frame import RawDataFrame
from .data_frame import DataFrame
from .additional_info_set import AdditionalInfoSet

@dataclass 
class EasyPathologyRecord:

    createDateTime: timedelta
    """文件创建时间"""

    version: int
    """文件版本号"""

    framesPerSecond: int
    """每秒帧率"""

    frameCount: int
    """EPR文件的总帧数"""

    screenPixelWidth: int
    """EPR文件中记录的屏幕横向分辨率"""

    screenPixelHeight: int
    """EPR文件中记录的屏幕纵向分辨率"""

    initialWidth: int
    """刚进入阅片时的宽度"""

    initialHeight: int
    """刚进入阅片时的高度"""

    maxLevelWidth: int
    """阅片时，最大倍率的宽度"""

    maxLevelHeight: int
    """阅片时，最大倍率的高度"""

    maxSlideWidth: int
    """切片文件的最大宽度"""

    maxSlideHeight: int
    """切片文件的最大高度"""

    maxLevel: int
    """最大level，也就是刚进入阅片的level"""

    minLevel: int
    """最小level"""

    totalLevelCount: int
    """医生总共看了多少个level"""

    actualScreenWidthCentimeter: float
    """EPR文件中记录的屏幕物理宽度，单位是cm"""

    actualScreenHeightCentimeter: float
    """EPR文件中记录的屏幕物理高度，单位是cm"""

    slideName: str
    """EPR文件中记录的切片文件名，不含路径，带拓展名"""

    quickHash: str
    """EPR文件中记录的切片的快速哈希值"""

    doctorDetail: DoctorDetail
    """EPR文件中记录的医生信息"""

    additionalComment: str
    """EPR文件中记录的医生注释"""

    selectedType: str
    """EPR文件中记录的医生判断类型字符串"""

    rawDataFrames: List[RawDataFrame]
    """数据帧的原始数据"""

    dataFrames: List[DataFrame]
    """数据帧"""

    eprFilePath: str
    """记录当前EPR文件的路径"""

    levelScales: List[int]
    """对应了每个VirtualLevel的缩放"""

    totalTime: timedelta
    """总时间"""

    additionalInfoSet: AdditionalInfoSet
    """附加信息"""