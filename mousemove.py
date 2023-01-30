import time

import pyautogui

##

##

time.sleep(1)

pyautogui.moveRel(-500, 500)
pyautogui.mouseDown()
pyautogui.mouseUp()
print(pyautogui.position())