from eprRead import EPRread
from olderEPRReader import oldEPRread
from recReader import RECreader
from os import path
import os
import xlwt
import xlrd





if __name__ == "__main__":
    path = r'C:\Users\shund\Desktop\EPR数据\1-5批注释信息\第四批'
    result = xlwt.Workbook()
    resdir = os.path.join(path, '第四批res.xls')
    sheet = result.add_sheet('shit1')
    dirs = os.listdir(path)
    all = os.walk(path)
    count = 0
    for root, dirs, files in all:
        for file in files:
            if file.__contains__('.epr'):
                eprfile = EPRread(os.path.join(root, file))
                typestr = eprfile.typeStr
                name = eprfile.slideName
                sheet.write(count, 0, name)
                sheet.write(count, 1, typestr)
                count = count + 1
    result.save(resdir)
    # rec = RECreader(recpath, ercpath)
    # oldepr = oldEPRread(eprpath)
    # type = oldepr.typeStr
    # slidename = oldepr.slideName
