from typing import List 
from dataclasses import dataclass
from enum import Enum


class Gender(Enum):
    Female = 0
    Male = 1
    Other = 2
    Unknown = 3


class DoctorLevel(Enum):
	"""
	高级
	"""
	Senior = 0

	"""
	中级
	"""
	Intermediate = 1

	"""
	初级
	"""
	Junior = 2

	"""
	学生
	"""
	Student = 3

	"""
	未知
	"""
	Unknown = 4


class SlideReadCount(Enum):
	"""
	1万张一下
	"""
	LessThan10K = 0

	"""
	1万-5万
	"""
	LessThan50KMoreThan10K = 1

	"""
	5万-10万
	"""
	LessThan100KMoreThan50K = 2

	"""
	10万以上
	"""
	MoreThan100K = 3

	"""
	未知
	"""
	Unknown = 4



@dataclass
class DoctorDetail:
    """
    姓名
    """
    name: str

    """
    性别
    """
    gender: Gender

    """
    年龄
    """
    age: int

    """
    从业年数，0表示小于一年
    """
    workYear: int

    """
    职称，0高级，1中级，2初级，3学生
    """
    level: DoctorLevel

    """
    阅片数，0阅片一万张以下，1阅片1w-5w，2 5w-10w，3 10w+
    """
    slideReadCount: SlideReadCount

    """
    使用本软件阅片的数量
    """
    readCountViaEasyPathology: int

    """
    所在单位/医院
    """
    unit: str

    """
    个人介绍
    """
    introduction: str

    """
    头像，jpg编码
    """
    avatarData: List[int]

    """
    医生用户的标识符
    """
    guid: str