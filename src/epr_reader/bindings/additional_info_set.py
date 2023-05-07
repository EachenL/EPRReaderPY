from typing import List 
from dataclasses import dataclass
from .roi import Roi
from .segmented_roi import SegmentedRoi

@dataclass 
class AdditionalInfoSet():
    """
    EPR文件中的附加信息
    """

    threshold: float = 30.0
    """暂时没什么用"""

    roiList: List[Roi]
    """Roi列表"""

    segmentedRoiList: List[SegmentedRoi]
    """智能Roi列表"""

    DiagnosisClassifyTree: any
    """诊断树，暂时没有定义"""