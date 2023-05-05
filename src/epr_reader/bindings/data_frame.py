from typing import List 
from dataclasses import dataclass 
from .raw_data_frame import RawDataFrame
from datetime import timedelta

@dataclass 
class DataFrame(RawDataFrame): 
    """ 
    用于储存每一帧的数据，包含处理后数据
    """

    """这一帧的起始时间。由于第一帧的时间戳往往大于0，这个是减去第一帧后运算出来的"""
    BeginTime: timedelta

    EndTime: timedelta

    """视野焦点半径（像素，相对于最大缩放)"""
    Radius: float

    """一个0-1的值，表示眨眼，1完全闭眼，0完全睁开"""
    BlinkValue: float

    """视线的角速度，角度制"""
    Velocity: float

    """当前帧的下标"""
    FrameIndex: int

    IsFrameStateEqualsToPrevFrame: List[bool]