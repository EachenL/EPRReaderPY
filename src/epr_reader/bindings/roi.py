from dataclasses import dataclass 

@dataclass 
class Roi():
    """一个Roi区域，是由若干注视点经过聚类得到的，Roi是一个圆，包含了这些注视点"""

    x: float
    """相对于当前Level"""

    y: float
    """相对于当前Level"""

    radius: float
    """相对于当前Level"""

    index: int
    """在RoiList中的下标"""

    pointCount: int
    """这个Roi包含了多少注视点"""

    level: int
    """所在的Level，注意是虚拟Level"""

    beginFrameIndex: int
    """包含注视点的起始下标"""

    endFrameIndex: int
    """包含注视点的结束下标"""