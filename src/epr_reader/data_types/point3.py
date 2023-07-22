from dataclasses import dataclass
from dataclasses_json import dataclass_json


@dataclass_json
@dataclass
class Point3: 
    """
    一个三维点
    Converted from Point3F.cs by ChatGPT
    """

    x: float 
    y: float 
    z: float

    def __eq__(self, other):
        if not isinstance(other, Point3):
            return NotImplemented
        return self.x == other.x and self.y == other.y and self.z == other.z

    def __hash__(self):
        return hash((self.x, self.y, self.z))

    def __add__(self, other):
        if not isinstance(other, Point3):
            return NotImplemented
        return Point3(self.x + other.x, self.y + other.y, self.z + other.z)

    def __sub__(self, other):
        if not isinstance(other, Point3):
            return NotImplemented
        return Point3(self.x - other.x, self.y - other.y, self.z - other.z)

    def __mul__(self, other):
        if not isinstance(other, float):
            return NotImplemented
        return Point3(self.x * other, self.y * other, self.z * other)

    def __truediv__(self, other):
        if not isinstance(other, float):
            return NotImplemented
        return Point3(self.x / other, self.y / other, self.z / other)

    def __str__(self):
        return f"({self.x}, {self.y}, {self.z})"

    def __repr__(self):
        return f"Point3(x={self.x}, y={self.y}, z={self.z})"