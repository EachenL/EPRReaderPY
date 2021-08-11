from struct import *

def checkSegment(br,s):
    st = str(br.read(len(s)), 'utf-8')
    if(st == s):
        return True
    else:
        br.seek(-len(s), 1)
        return False


def readStr(reader):
    # 获取第一个长度前缀
    len = unpack("B", reader.read(1))[0]
    if len == 0:
        return "0"
    # 判断是否有下一个长度前缀
    if len >> 7 == -1:
        len = len & 0b01111111 + unpack("b", reader.read(1))[0] * 128
        # reader.seek(2, 1)
    else:
        len = len & 0b01111111
        # reader.seek(1, 1)
    s = str(reader.read(len), 'utf-8')
    return s

if __name__ == "__main__":  # 这里是示例用法
    pathndpi = "C:\\Users\\shund\\Desktop\\SightPoint\\11111.ndpi"
    eprfile = "C:\\Users\\shund\\Desktop\\SightPoint\\123.epr"
    br = open(eprfile, 'rb')
    br.seek(0, 0)
    s = readStr(br)
    print(s)