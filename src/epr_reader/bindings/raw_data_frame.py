from typing import List 
from dataclasses import dataclass 
from ..data_types.point3 import Point3 
from ..data_types.point2 import Point2 
from .frame_state import FrameState

@dataclass 
class RawDataFrame: 
    """ 
    用于存储到epr中的原始帧数据 
    Converted from RawDataFrame.cs by ChatGPT
    """
    
    #: 眼睛的注视x坐标，相对于屏幕左上角
    gazeX: int

    #: 眼睛的注视y坐标，相对于屏幕左上角
    gazeY: int

    #: 缩放level，需要注意这个Level并不是OpenSlide里的真实Level
    virtualLevel: int

    #: 屏幕左上角相对于切片左上角的x坐标
    screenX: int

    #: 屏幕左上角相对于切片左上角的y坐标
    screenY: int

    #: 左眼和眼动仪的相对坐标，单位是cm
    leftEyePosition: Point3

    #: 右眼和眼动仪的相对坐标，单位是cm
    rightEyePosition: Point3

    #: 眼动数据有效性，24版本+
    isEyeTrackerDataValid: bool

    #: 单位是秒
    timeStamp: float

    #: 鼠标在屏幕上的X坐标（像素）
    cursorX: int

    #: 鼠标在屏幕上的Y坐标（像素）
    cursorY: int

    #: 当前帧所有“状态”的列表
    frameStates: List[FrameState]

    @property
    def eyeDistanceToScreen(self) -> float:
        """
        头部距离屏幕的高度，单位是cm
        """
        return (self.leftEyePosition.z + self.rightEyePosition.z) / 20

    @property
    def gazePosition(self) -> Point2:
        return Point2(self.gazeX, self.gazeY)

    def __str__(self):
        return f"{self.gazeX}: {self.gazeY}"