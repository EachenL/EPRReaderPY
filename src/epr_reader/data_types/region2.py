from typing import List 
from dataclasses import dataclass
from dataclasses_json import dataclass_json
from .point2 import Point2


@dataclass_json
@dataclass
class Region2: 
    """
    一个由若干二维点组成的区域
    """
    points: List[Point2]
