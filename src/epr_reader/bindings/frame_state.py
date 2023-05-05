from enum import Enum

from dataclasses import dataclass

class FrameStateType(Enum):
    Unknown = 0

    """
    医生的下诊断窗口
    """
    SlideCommentWindow = 1

    """
    切片工具选择窗口
    """
    SlideToolsWindow = 2

    """
    尺子工具
    """
    SlideRulerToolWindow = 3

    """
    标记笔工具
    """
    SlideMarkPenToolWindow = 4

    """
    细胞核探测工具
    """
    CellNucleiDetectionTool = 5

    Next = 0xFF,


@dataclass 
class FrameState:
    type: FrameStateType