epr_path = '/home/omnisky/hdd_15T_sdd/nsd/1-4-2/1-4-2_肝细胞坏死__-_40x.epr'
slide_path = '/home/omnisky/hdd_15T_sdd/nsd/1-4-2/1-4-2_肝细胞坏死__-_40x.ndpi'
import os
from eprRead import *
from Sequence_region import *



def get_all_filenames(path):
    filenames = []
    for root, dirs, files in os.walk(path):
        for file in files:
            if file.endswith('.ndpi'):
                filenames.append(os.path.basename(os.path.join(root, file)))
    return filenames


def traverse_epr_files(path):
    """
    遍历指定路径下所有以.epr为结尾的文件

    Args:
        path (str): 指定路径

    Returns:
        list: 文件路径列表
    """
    epr_files = []
    for root, dirs, files in os.walk(path):
        for file in files:
            if file.endswith(".epr"):
                epr_files.append(os.path.join(root, file))
    return epr_files

# epr_files = traverse_epr_files(epr_root)
# # print(epr_files)
# slide_files = get_all_filenames(slide_root)
# eprfile = EPRread(epr_files[0])
# slidename = eprfile.slideName
# if slidename in slide_files:
#     print('yes')
# else:
#     print('no')
# a = 1
epr = EPRread(epr_path)
seq = Sequence(slide_path, epr.data, epr.XY, epr.point_radius, epr.now_zoom)
a=1
