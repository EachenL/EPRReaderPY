from struct import *

import PIL.Image
import openslide as os
import matplotlib.pyplot as plt
import matplotlib.image as mpimg
from PIL import Image

if __name__ == "__main__":  # 这里是示例用法
    pathndpi = "C:\\Users\\shund\\Desktop\\SightPoint\\11111.ndpi"
    eprfile = "C:\\Users\\shund\\Desktop\\SightPoint\\123.epr"

    image = os.open_slide(pathndpi).read_region((0, 0), 5, (1920, 1080))
    plt.imshow(image)
    plt.show()

