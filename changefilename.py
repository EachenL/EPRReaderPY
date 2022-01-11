from eprRead import EPRread
from olderEPRReader import oldEPRread
from recReader import RECreader
import os
import xlwt
import xlrd
from shutil import copyfile


# 获得excel文件中的所有数据，并存入labels中
# file为excel文件路径
def getlabels(file):
    lfile = xlrd.open_workbook(file)
    lfilesheets = lfile.sheet_names()
    labels = []
    rows = 0
    for i in range(len(lfilesheets)):
        nowsheet = lfile.sheet_by_name(lfilesheets[i])
        nrow = nowsheet.nrows
        ncol = nowsheet.ncols
        rows = rows + nrow
        for j in range(nrow):
            labels.append(nowsheet.row_values(j))
    return labels, rows


def getfiles(dir):
    all = os.walk(dir)
    filepaths = []
    filenames = []
    for root, dirs, files in all:
        filenames = filenames + files
        for file in files:
            filepaths.append(os.path.join(root, file))
    return filepaths, filenames


def getnamefrompath(path):
    namewithext = os.path.split(path)[1]
    name = os.path.splitext(namewithext)[0]
    ext = os.path.splitext(namewithext)[1]
    return name, ext


# 5th dataset
def rechecklabel(resfile, labelfile):
    reslabels, resrow = getlabels(resfile)
    labels, lrow = getlabels(labelfile)
    newlabel = []
    for i in range(0, resrow):
        flag = False
        for j in range(0, lrow):
            if reslabels[i][2].__contains__(labels[j][0]):
                nlabel = reslabels[i][0].rsplit('_', 1)[0] + '_' + labels[j][1] + '_' + '0' + getnamefrompath(reslabels[i][0])[1]
                reslabels[i].append(nlabel)
                flag = True
                break
        if flag == False:
            reslabels[i].append(' ')
    return reslabels


def writelisttoxls(list, target):
    workbook = xlwt.Workbook()
    sheet1 = workbook.add_sheet('sheet1')
    i = 0
    for r in list:
        for c in range(len(r)):
            sheet1.write(i, c, r[c])
        i = i + 1
    workbook.save(target)


def addpathtoexcel(list, origin):
    paths, names = getfiles(origin)
    for path in paths:
        for row in range(len(list)):
            if os.path.split(path)[1] == list[row][0]:
                list[row].append(path)
                break
    return list



def copytotarget(dir, target):
    filepaths, filenames = getfiles(dir)
    for file in filepaths:
        content, nrows = getlabels(file)
        for row in content:
            filepath = row[1]
            copyfile(filepath, os.path.join(target, row[3]))
            print(filepath)

    #遍历dir得出excel文件
    #根据excel文件中的路径将文件复制到目的文件夹
    #更改文件名


if __name__ == "__main__":

    labelpath = r'Z:\pathology_group\Database\WsisAnalysisWithGaze\医生阅片划分数据集\label'
    labels = os.walk(labelpath)
    print(labels)
    # dirpath = labels(0)
    # dirnames = labels(1)
    # filenames = labels(2)
    targetpath = r'Z:\pathology_group\Database\WsisAnalysisWithGaze\经过医生初次确认的数据集\文件名经过校准的slide文件'
    # copytotarget(labelpath, targetpath)

    # slidepath = r'Z:\pathology_group\Database\WsisAnalysisWithGaze\医生阅片划分数据集\第三批public_melanoma\slide'
    # outputpath = r'Z:\pathology_group\Database\WsisAnalysisWithGaze\经过医生初次确认的数据集\文件名经过校准的slide文件\第五批'
    resfile = r'Z:\pathology_group\Database\WsisAnalysisWithGaze\皮肤病理数据集（全部）\郑医生\2021.10.19\郑松\第五批res.xls'
    outfile = r'Z:\pathology_group\Database\WsisAnalysisWithGaze\皮肤病理数据集（全部）\郑医生\2021.10.19\郑松\第五批recheck.xls'
    labelfile = r'Z:\pathology_group\Database\WsisAnalysisWithGaze\1\标签.xls'
    # slidepaths, slidenames = getfiles(slidepath)  # 遍历slide文件得出路径和文件名列表
    # reslabels, resnrow = getlabels(resfile)  # 得到res.xls文件中的标签
    label, labelrow = getlabels(labelfile)  # 得到label.xls文件中的标签
    labels = rechecklabel(resfile, labelfile)
    writelisttoxls(labels, outfile)
    # labels = addpathtoexcel(labels, slidepath)
    # writelisttoxls(labels, outfile)
    i = 1
    # name1, extname = getnamefrompath(files[0])
    # full = name1 + extname
    # labels, nrow = getlabels(labelfile)
    # name2 = labels[1][1]
    # i =1
    # [filename, label] = getlabels(lfilesheets)
    # for root, dirs, files in os.walk(slidepath):
    #     for file in files:




