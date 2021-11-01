

# -*- coding: utf-8 -*-
import json
import datetime
import time
import threading
import ctypes
import win32api
import win32gui
import win32con
import pywintypes
import win32ui

#通过win32获取当前鼠标x y
def get_curpos():
   return win32gui.GetCursorPos();



#通过
def get_win_handle(pos):
   return win32gui.WindowFromPoint(pos)

if __name__ == '__main__':
    #time.sleep(2)
    print("开始执行")
    #hwnd = 0  # 窗口的编号，0号表示当前活跃窗口
    hwnd = win32gui.GetDesktopWindow();
    hPen = win32gui.CreatePen(win32con.PS_SOLID, 3, win32api.RGB(255, 0, 255))  # 定义框颜色
    for num in range(100,125):
        win32gui.InvalidateRect(hwnd, None, True)
        win32gui.UpdateWindow(hwnd)
        win32gui.RedrawWindow(hwnd, None, None,win32con.RDW_FRAME | win32con.RDW_INVALIDATE | win32con.RDW_UPDATENOW | win32con.RDW_ALLCHILDREN)

        hwndDC = win32gui.GetDC(hwnd)  # 根据窗口句柄获取窗口的设备上下文DC（Divice Context）

        win32gui.SelectObject(hwndDC, hPen)
        hbrush = win32gui.GetStockObject(win32con.NULL_BRUSH)  # 定义透明画刷，这个很重要！！
        prebrush = win32gui.SelectObject(hwndDC, hbrush)
        win32gui.Rectangle(hwndDC, 447+3*num, 505+3*num, 591+3*num, 545+3*num)  # 左上到右下的坐标
        win32gui.SaveDC(hwndDC);
        win32gui.SelectObject(hwndDC, prebrush)
        # # 回收资源
        #win32gui.DeleteObject(hPen)
        #win32gui.DeleteObject(hbrush)
        #win32gui.DeleteObject(prebrush)
        win32gui.ReleaseDC(hwnd, hwndDC)
        time.sleep(1)