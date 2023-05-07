from typing import List 
from dataclasses import dataclass
from .point2 import Point2

@dataclass
class Region2: 
    """
    一个由若干二维点组成的区域
    """

    points: List[Point2]