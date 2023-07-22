from typing import List 
from dataclasses import dataclass
from dataclasses_json import dataclass_json
from .raw_data_frame import RawDataFrame
from datetime import timedelta


@dataclass_json
@dataclass 
class DataFrame(RawDataFrame): 
    """ 
    用于储存每一帧的数据，包含处理后数据
    """

    beginTime: timedelta
    """这一帧的起始时间。由于第一帧的时间戳往往大于0，这个是减去第一帧后运算出来的"""

    endTime: timedelta

    radius: float
    """视野焦点半径（像素，相对于最大缩放)"""

    blinkValue: float
    """一个0-1的值，表示眨眼，1完全闭眼，0完全睁开"""

    velocity: float
    """视线的角速度，角度制"""

    frameIndex: int
    """当前帧的下标"""

    isFrameStateEqualsToPrevFrame: List[bool]