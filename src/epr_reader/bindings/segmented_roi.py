from typing import List 
from dataclasses import dataclass 
from datetime import timedelta
from ..data_types.region2 import Region2

@dataclass 
class SegmentedRoi():
    """
    由SegmentAnything智能分割的一个Roi区域
    """

    beginTime: timedelta
    """起始显示时间"""

    endTime: timedelta
    """终止显示时间"""

    regions: List[Region2]
    """分割出的所有区域列表"""