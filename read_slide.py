import openslide
slide_file = '../1-4-2/1-4-2_肝细胞坏死__-_40x.ndpi'
slide = openslide.OpenSlide(slide_file)
print(slide.properties['openslide.mpp-y'])