import time

import pyautogui

time.sleep(1)

pyautogui.moveRel(-500, 500)
pyautogui.mouseDown(button=3, duration=1)
pyautogui.mouseUp()
pyautogui.position()