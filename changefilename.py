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
        slide_name, ext = getnamefrompath(reslabels[i][1])
        slide_num = slide_name.split('_')[0] + '_' + slide_name.split('_')[1]
        slide_label = slide_name.split('_')[2]
        slide_last = slide_name.split('_')[3]
        current_label = ''
        for j in range(0, lrow):
            if(reslabels[i][2].__contains__(labels[j][0])):
                current_label = labels[j][1]
                break
        if(current_label != ''):
            if(current_label != slide_label):
                recheck_name = slide_num + '-' + current_label + '-' + slide_last + ext
                recheck_flag = 0
                reslabels[i].append(recheck_name)
                reslabels[i].append(recheck_flag)
            else:
                recheck_name = slide_num + '-' + current_label + '-' + slide_last + ext
                recheck_flag = 1
                reslabels[i].append(recheck_name)
                reslabels[i].append(recheck_flag)
        else:
            recheck_name = '0'
            recheck_flag = 2
            reslabels[i].append(recheck_name)
            reslabels[i].append(recheck_flag)
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

def genresfile():
    slide_path = r'Z:\pathology_group\Database\WsisAnalysisWithGaze\皮肤病理数据集（全部）'
    slide_paths, slide_names = getfiles(slide_path)
    res_path = r'C:\Users\shund\Desktop\注视信息\标签'
    res_paths, label_names = getfiles(res_path)
    labelfile = r'C:\Users\shund\Desktop\注视信息\标签.xls'
    resultpath = r'C:\Users\shund\Desktop\注视信息\recheck'
    for resfile in res_paths:
        resfilename, ext = getnamefrompath(resfile)
        recheckfile = resultpath + '\\' + resfilename + ext
        result = rechecklabel(resfile, labelfile)
        writelisttoxls(result, recheckfile)


if __name__ == "__main__":
    totalpath = r'Z:\pathology_group\Database\WsisAnalysisWithGaze\22.1.16重新整理阅片会数据'
    recheckpath = r'Z:\pathology_group\Database\WsisAnalysisWithGaze\22.1.16重新整理阅片会数据\整理好'
    quespath = r'Z:\pathology_group\Database\WsisAnalysisWithGaze\22.1.16重新整理阅片会数据\有问题'
    resultpath = r'C:\Users\shund\Desktop\注视信息\recheck'
    res_paths, res_names = getfiles(resultpath)
    for res in res_paths:
        res_name, ext = getnamefrompath(res)
        reslabels, rows = getlabels(res)
        for i in range(0, rows):
            if(reslabels[i][4] == 0):
                targetfilename = recheckpath + '\\' + res_name + '\\' + reslabels[i][3]
                copyfile(reslabels[i][1], targetfilename)
                print('已将' + reslabels[i][1] + '粘贴至' + targetfilename)
                targetfilename = quespath + '\\' + res_name + '\\' + reslabels[i][3]
                copyfile(reslabels[i][1], targetfilename)
                print('已将'+reslabels[i][1]+'粘贴至'+targetfilename)
            elif(reslabels[i][4] == 1):
                targetfilename = recheckpath + '\\' + res_name + '\\' + reslabels[i][3]
                copyfile(reslabels[i][1], targetfilename)
                print('已将' + reslabels[i][1] + '粘贴至' + targetfilename)





