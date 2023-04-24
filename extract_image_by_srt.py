import sys
sys.path.append('..')
import srt
from EPRReaderPY.eprRead import *
# timestamp should multiply 100 times // OK
import openslide
import os
from PIL import Image
import numpy as np

class srt_md_unit():
    def __init__(self, index, start, end, content, img_path):
        self.index = index
        self.start = start
        self.end = end
        self.content = content
        self.img_path = img_path

    

# define a function that can get the image by timestamp
def get_window_by_fixation(datum, epr, slide):
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

def find_prop_window(level0_windows, mode):
    
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
    width = screenPixelWidth
    height = screenPixelHeight
    if X0 + screenPixelWidth > slide.level_dimensions[level][0]:
        
        width = slide.level_dimensions[level][0] - X0
        # if slide.level_dimensions[level][0] - screenPixelWidth < 0 :
            
        
    if Y0 + screenPixelHeight > slide.level_dimensions[level][1]:
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
        
        
    

if __name__ == '__main__':
    # get the timestamp of each sub
    epr_file = '1-4-2_肝细胞坏死__-_40x.epr'
    srt_file = open('1-4-2_肝细胞坏死__-_40x.srt')
    slide_file = '1-4-2_肝细胞坏死__-_40x.ndpi'
    picture_mode = 'fixation'
    img_folder = f'1-4-2_肝细胞坏死__-_40x-img-{picture_mode}'
    
    epr = EPRread(epr_file)
    srt_content = srt.parse(srt_file)
    # check img_folder is exist, if not, create it
    if not os.path.exists(img_folder):
        os.mkdir(img_folder)
        
    epr_pointer = 0

    srt_list = []
    screenPixelWidth = epr.screenPixelWidth
    screenPixelHeight = epr.screenPixelHeight
    
    slide = openslide.OpenSlide(slide_file)

    for sub in srt_content:
        # time is milliseconds
        start_time = (sub.start.seconds * 1000000 + sub.start.microseconds)  / 1000
        # end time
        end_time = (sub.end.seconds * 1000000 + sub.end.microseconds) / 1000
        level0_windows = []
        
        # traverse the datumFrame of epr.datum until the millitimestamp is larger than the time
        # sub start epr frame
        while epr_pointer < len(epr.datum)-1 and epr.datum[epr_pointer].millitimestamp <= start_time:
            epr_pointer += 1
        # now the epr.datum[epr_pointer] is the dataframe we want
        # get the absolute x,y position of the slide image
        # get points during the sub
        while epr_pointer < len(epr.datum)-1 and epr.datum[epr_pointer].millitimestamp <= end_time:
            
            # level0_window = get_window_by_screenpath(epr.datum[epr_pointer], epr, slide)
            level0_window, drop_flag = get_window_by_fixation(epr.datum[epr_pointer], epr, slide)
            if drop_flag != True: level0_windows.append(level0_window)
            epr_pointer += 1
            
        img_name = img_folder + '_' + str(sub.index) + '_' + str(epr_pointer) + '.png'
        img_path = img_folder + '/' + img_name
        # if x and y < 0, then set x and y = 0
        # arr = np.array(level0_windows)
        # max = np.max(arr, axis = 0)
        # max_index = arr.argmax(axis=0)
        # min = np.min(arr, axis = 0)
        # min_index = arr.argmin(axis=0)
        
        # x, y, w, h, level = min[0], min[1], max[2]-min[0], max[3]-min[1]
        x, y, w, h, level = find_prop_window(level0_windows, picture_mode)
        # read the image from the slide in level 0
        # level = arr[max_index[4]][4]
        while w > 1920 or h > 1080:
            level += 1
            w, h = w // 2, h // 2
            
        RGBA_img = slide.read_region((x // (2**level), y // (2**level)), level, (w, h))
        # save RGBA_img to img_path, RGBA_img is a PIL.Image.Image object
        # img = PIL.Image.fromarray(RGBA_img, 'RGBA')
        RGBA_img.save(img_path, 'PNG')
        RGBA_img.close()
        a = 1