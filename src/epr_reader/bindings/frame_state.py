from enum import Enum

from dataclasses import dataclass

class FrameStateType(Enum):
    Unknown = 0

    SlideCommentWindow = 1
    """医生的下诊断窗口"""

    SlideToolsWindow = 2
    """切片工具选择窗口"""

    SlideRulerToolWindow = 3
    """尺子工具"""

    SlideMarkPenToolWindow = 4
    """标记笔工具"""

    CellNucleiDetectionTool = 5
    """细胞核探测工具"""

    Next = 0xFF,


@dataclass 
class FrameState:
    type: FrameStateType