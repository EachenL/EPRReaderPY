import urllib3
import requests
import json
import os
from .launcher import Launcher, Command
from .bindings.easy_pathology_record import EasyPathologyRecord
import time
import psutil
class read_epr:
	
	# def __init__(self):
	# 	self.server = Launcher(Command)
	# 	self.server.run()
	# 	time.sleep(1)
	# 	print('server is running.')

		
	def read(self, file_path: str, read_header_only=False):

		register_url = 'http://localhost:6082/Reader/ReadEpr'
		header = {
			"Content-Type": "application/json"
		}
		post = {
			"filePath": file_path,
			"readHeaderOnly": read_header_only
		}
		times = 10
		while times > 0:
			try:
				re = requests.post(url=register_url, json=post, headers=header, timeout=5)
				data = json.loads(re.content)['data']
				times = -1
			except:
				times -= 1
		if times == 0:
			raise Exception('epr读取失败')
		data = EasyPathologyRecord(**data)
		# print(data)
		return data
