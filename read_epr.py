import sys
sys.path.append('..')

from EPRReaderPY.eprRead import *

epr_file = '../1-4-2/1-4-2_肝细胞坏死__-_40x.epr'
epr = EPRread(epr_file)
a = 1