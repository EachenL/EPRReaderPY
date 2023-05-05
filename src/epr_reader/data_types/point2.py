from dataclasses import dataclass

@dataclass
class Point2: 
    """
    一个二维点
    Converted from Point2D.cs by ChatGPT
    """

    x: float 
    y: float 

    def __eq__(self, other):
        if not isinstance(other, Point2):
            return NotImplemented
        return self.x == other.x and self.y == other.y

    def __hash__(self):
        return hash((self.x, self.y, self.z))

    def __add__(self, other):
        if not isinstance(other, Point2):
            return NotImplemented
        return Point2(self.x + other.x, self.y + other.y)

    def __sub__(self, other):
        if not isinstance(other, Point2):
            return NotImplemented
        return Point2(self.x - other.x, self.y - other.y)

    def __mul__(self, other):
        if not isinstance(other, float):
            return NotImplemented
        return Point2(self.x * other, self.y * other)

    def __truediv__(self, other):
        if not isinstance(other, float):
            return NotImplemented
        return Point2(self.x / other, self.y / other)

    def __str__(self):
        return f"({self.x}, {self.y})"

    def __repr__(self):
        return f"Point2(x={self.x}, y={self.y})"