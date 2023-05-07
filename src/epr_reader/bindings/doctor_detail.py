from typing import List 
from dataclasses import dataclass
from enum import Enum


class Gender(Enum):
    Female = 0
    """女"""

    Male = 1
    """男"""

    Other = 2
    """武装直升机"""

    Unknown = 3


class DoctorLevel(Enum):
	Senior = 0
	"""高级"""

	Intermediate = 1
	"""中级"""

	Junior = 2
	"""初级"""

	Student = 3
	"""学生"""

	Unknown = 4
	"""未知"""


class SlideReadCount(Enum):
	LessThan10K = 0
	"""1万张一下"""

	LessThan50KMoreThan10K = 1
	"""1万-5万"""

	LessThan100KMoreThan50K = 2
	"""5万-10万"""

	MoreThan100K = 3
	"""10万以上"""

	Unknown = 4
	"""未知"""



@dataclass
class DoctorDetail:
    """医生的详细信息"""

    name: str
    """姓名"""

    gender: Gender
    """性别"""

    age: int
    """年龄"""

    workYear: int
    """从业年数，0表示小于一年"""

    level: DoctorLevel
    """职称"""

    slideReadCount: SlideReadCount
    """阅片数"""

    readCountViaEasyPathology: int
    """使用本软件阅片的数量"""

    unit: str
    """所在单位/医院"""

    introduction: str
    """个人介绍"""

    avatarData: List[int]
    """头像，jpg编码"""

    guid: str
    """医生用户的标识符"""