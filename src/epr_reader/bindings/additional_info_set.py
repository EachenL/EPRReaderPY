from typing import List 
from dataclasses import dataclass
from dataclasses_json import dataclass_json
from .roi import Roi
from .segmented_roi import SegmentedRoi


@dataclass_json
@dataclass 
class AdditionalInfoSet:
    """
    EPR文件中的附加信息
    """
    
    segmentedRoiList: List[SegmentedRoi]
    """智能Roi列表"""

    roiList: List[Roi]
    """Roi列表"""

    DiagnosisClassifyTree: any
    """诊断树，暂时没有定义"""
    
    # non-default argument must before default argument
    threshold: float = 30.0
    """暂时没什么用"""
