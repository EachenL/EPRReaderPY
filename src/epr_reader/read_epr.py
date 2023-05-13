import urllib3
import requests
import json
import os
from .launcher import Launcher, Command
from .bindings.easy_pathology_record import EasyPathologyRecord
class read_epr:
	
	def __init__(self):
		self.server = Launcher(Command)
		self.server.run()
		
	def read(self, file_path: str, read_header_only=False):

		register_url = 'http://localhost:6082/Reader/ReadEpr'
		header = {
			"Content-Type": "application/json"
		}
		post = {
			"filePath": file_path,
			"readHeaderOnly": read_header_only
		}
		re = requests.post(url=register_url, json=post, headers=header)
		data = json.loads(re.content)['data']
		data = EasyPathologyRecord(**data)
		# print(data)
		return data


	# read_epr(r'E:\病理视频教学\吴泽病理教学视频\1532677\1532677.epr', False)