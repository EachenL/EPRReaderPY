import sys
sys.path.append('..')
import srt
from EPRReaderPY.eprRead import *
# timestamp should multiply 100 times // OK
import openslide
import os
from PIL import Image
import numpy as np
import read_srt
import json
import hdbscan


# epr_file = '../1-4-2/1-4-2_肝细胞坏死__-_40x.epr'
# srt_file = '../1-4-2/1-4-2_肝细胞坏死__-_40x.srt'
# slide_file = '../1-4-2/1-4-2_肝细胞坏死__-_40x.ndpi'
picture_mode = 'fixation'
# img_folder = f'../1-4-2/1-4-2_肝细胞坏死__-_40x-img-{picture_mode}'
class srt_md_unit():
    def __init__(self, index, start, end, content, img_path):
        self.index = index
        self.start = start
        self.end = end
        self.content = content
        self.img_path = img_path

def get_focus_by_HDBSCAN(xy, l):
    cluster = hdbscan.HDBSCAN(min_cluster_size=10)
    
    cluster_labels = cluster.fit_predict(xy)
    
    return
        

# define a function that can get the image by timestamp
def get_window_by_fixation(datum, epr, slide):
    '''
    return a list [x, y, level]
    x, y is the coordinate of the eye position, then scale to the level 0
    level, is the scale-level of the eye position before scale
    '''
    X0 = -datum.screenX + datum.eyeX
    Y0 = -datum.screenY + datum.eyeY
    level = datum.level - epr.minlevel
    drop_flag = False

    if level < 0:
        a = 1
    if X0 < 0:
        drop_flag = True
        X0 = 0

    if Y0 < 0:
        drop_flag = True
        Y0 = 0


    if X0 > slide.level_dimensions[level][0]:
        drop_flag = True
        X0 = slide.level_dimensions[level][0]
   
    if Y0 > slide.level_dimensions[level][1]:
        drop_flag = True
        Y0 = slide.level_dimensions[level][1]
    
    

    level_window = [X0, Y0]    
    level_window = np.array(level_window)
    level0_window = list(level_window * (2 ** level))
    level0_window.append(level)
    return level0_window, drop_flag

def find_prop_window(level0_windows, mode, minlevel):
    
    arr = np.array(level0_windows)
    max = np.max(arr, axis = 0)
    max_index = arr.argmax(axis=0)
    min = np.min(arr, axis = 0)
    min_index = arr.argmin(axis=0)
    if mode == 'screen_path':
        
        x, y, w, h = min[0], min[1], max[2]-min[0], max[3]-min[1]
        # read the image from the slide in level 0
        level = arr[max_index[4]][4]
        return x, y, w, h, level
    
    if mode == 'fixation':
        x, y, w, h = min[0], min[1], max[0]-min[0], max[1]-min[1]
        # read the image from the slide in level 0
        level = arr[max_index[2]][2]
        return x, y, w, h, level

def get_window_by_screenpath(datum, epr, slide):
    X0 = datum.screenX
    Y0 = datum.screenY
    level = datum.level - epr.minlevel
    
    if level < 0:
        a = 1
    if X0 > 0:
        X0 = 0
    else:
        X0 = -X0
    if Y0 > 0:
        Y0 = 0
    else:
        Y0 = -Y0
    width = epr.screenPixelWidth
    height = epr.screenPixelHeight
    if X0 + epr.screenPixelWidth > slide.level_dimensions[level][0]:
        
        width = slide.level_dimensions[level][0] - X0
        # if slide.level_dimensions[level][0] - screenPixelWidth < 0 :
            
        
    if Y0 + epr.screenPixelHeight > slide.level_dimensions[level][1]:
        height = slide.level_dimensions[level][1] - Y0
        # if slide.level_dimensions[level][1] - screenPixelHeight < 0:
            
    
    X1, Y1 = X0 + width, Y0 + height
    level_window = [X0, Y0, X1, Y1]    
    level_window = np.array(level_window)
    level0_window = list(level_window * (2 ** level))
    level0_window.append(level)
    return level0_window



class srt_datumn():
    def __init__(self, x0, y0, width, height, x1, y1, level):
        self.level_window = [[x0, y0], [x1, y1]]
        self.width, self.height = width, height
        self.level = level
        self.level0_window = self.level_window * (level+1)
        

def get_part_start_end_time(part, srt_content):
    
    start_index, end_index = part['index_range'].split('-')
    start_index, end_index = int(start_index)-1, int(end_index)-1
    # srt_content = list(srt_content)
    start_time = srt_content[start_index].start.seconds*1000 + srt_content[start_index].start.microseconds/1000
    end_time = srt_content[end_index].end.seconds*1000 + srt_content[end_index].end.microseconds/1000
    return start_time, end_time

def generate_target_picture(level0_windows, part, slide, epr_pointer, img_folder, minlevel):
    img_name = str(part['index_range']) + '.png'
    img_path = img_folder + '/' + img_name
    # if x and y < 0, then set x and y = 0
    # arr = np.array(level0_windows)
    # max = np.max(arr, axis = 0)
    # max_index = arr.argmax(axis=0)
    # min = np.min(arr, axis = 0)
    # min_index = arr.argmin(axis=0)
    
    # x, y, w, h, level = min[0], min[1], max[2]-min[0], max[3]-min[1]
    x, y, w, h, level = find_prop_window(level0_windows, picture_mode, minlevel)
    # read the image from the slide in level 0
    # level = arr[max_index[4]][4]
    # while w > 1920 or h > 1080:
    #     level += 1
    #     w, h = w // 2, h // 2
    # x, y = x // (2**level), y // (2**level)
    l = 0
    while w > 4096 or h > 4096:
        x, y, w, h= x // 2, y // 2, w // 2, h // 2
        l += 1
    RGBA_img = slide.read_region((x, y), l, (w, h))
    print(x, y, w, h, l)
    # save RGBA_img to img_path, RGBA_img is a PIL.Image.Image object
    # img = PIL.Image.fromarray(RGBA_img, 'RGBA')
    RGBA_img.save(img_path, 'PNG')
    RGBA_img.close()
    a = 1
    
def write_part_list_to_file(part_list, json_file):
    with open(json_file, "w") as outfile:
        json.dump(part_list, outfile)
    return


def gen_part_pic(epr_file, srt_file, img_folder, slide_file):
    epr = EPRread(epr_file)
    srt_content = srt.parse(open(srt_file))
    srt_content = list(srt_content)
    # check img_folder is exist, if not, create it
    if not os.path.exists(img_folder):
        os.mkdir(img_folder)
        
    epr_pointer = 0

    srt_list = []
    screenPixelWidth = epr.screenPixelWidth
    screenPixelHeight = epr.screenPixelHeight
    
    slide = openslide.OpenSlide(slide_file)
    part_list = read_srt.get_final_text(srt_file)
    write_part_list_to_file(part_list, img_folder+'/part_list.json')
    for part in part_list:
        posxy, poslevel = [], []
        level0_windows = []

        start_time, end_time = get_part_start_end_time(part, srt_content)
        
        while epr_pointer < len(epr.datum)-1 and epr.datum[epr_pointer].millitimestamp <= start_time:
            epr_pointer += 1
        
        while epr_pointer < len(epr.datum)-1 and epr.datum[epr_pointer].millitimestamp <= end_time:
            # posxy.append([(-epr.datum[epr_pointer].screenX + epr.datum[epr_pointer].eyeX) * (2**(epr.datum[epr_pointer].level-epr.minLevel)), (-epr.datum[epr_pointer].screenY + epr.datum[epr_pointer].eyeY) * (2**(epr.datum[epr_pointer].level-epr.minLevel))])
            # poslevel.append(epr.datum[epr_pointer].level)
            # level0_window = get_window_by_screenpath(epr.datum[epr_pointer], epr, slide)
            level0_window, drop_flag = get_window_by_fixation(epr.datum[epr_pointer], epr, slide)
            if drop_flag != True:
                level0_windows.append(level0_window)
            epr_pointer += 1
        if level0_windows != []:
        # get_focus_by_HDBSCAN(posxy, poslevel)
            generate_target_picture(level0_windows, part, slide, epr_pointer, img_folder, epr.minlevel)
    
    return part_list, srt_content
def write_content_to_md(img_dir, srt_content, part_list, name, img_folder):
    md_file = img_dir + '/' + os.path.basename(img_dir) + '.md'
    with open(md_file, 'a') as f:
        # write file name:
        f.write(f'#  {name}\n\n')
        for part in part_list:
            # write part of a file
            f.write(f"##  {part['title']}\n\n")
            start, end = map(int, part['index_range'].split('-'))
            for i in range(start, end - 1):
                f.write(srt_content[i].content + ',')
            f.write(srt_content[end].content + '。\n')
            # write part picture
            f.write(f"![{part['index_range']}]({img_folder}/{part['index_range']}.png)\n")
     

def gen_md_by_dir(rec_dir, img_dir):
    for root, dirs, files in os.walk(rec_dir):
        epr_file = ''
        srt_file = ''
        img_folder = img_dir
        img_folder = img_folder + '/' + root.split('/')[-1]
        slide_file = ''
        # get files we need
        for file in files:
            ext = file.split('.')[1]
            name = file.split('.')[0]
            if ext == 'epr':
                epr_file = os.path.join(root, file)
            elif ext == 'ndpi':
                slide_file = os.path.join(root, file)
            elif ext == 'srt':
                srt_file = os.path.join(root, file)
            else:
                continue
        # excute mission
        flag = False
        if epr_file != '' and slide_file != '' and srt_file != '':
            while(flag == False):
                try: 
                    part_list, srt_content = gen_part_pic(epr_file, srt_file, img_folder, slide_file)
                    flag = True
                    # gen markdown by result
                    # json_file = img_dir + '/part_list.json'
                    # name = name.split('_')[1]
                    write_content_to_md(img_dir, srt_content, part_list, name, root.split('/')[-1])
                except Exception:
                    print(Exception)
                    continue
        
            
            

if __name__ == '__main__':
    rec_dir = '/home/omnisky/nsd/miaoyuan'
    project_name = 'miaoyuan_lession'
    img_folder = rec_dir + '/' + project_name
    if not os.path.exists(img_folder):
        os.mkdir(img_folder)
    gen_md_by_dir(rec_dir, img_folder)
    