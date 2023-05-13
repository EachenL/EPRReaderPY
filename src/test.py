import json
from epr_reader.bindings.easy_pathology_record import EasyPathologyRecord
from epr_reader import read_epr
if __name__ == "__main__":
    epr_reader = read_epr()
    while(1):
        epr = epr_reader.read(r'D:\苗原\2022年5月5日_2\6-3-2_主动脉粥样硬化_-_40x.epr')
    with open(r'..\samples\吴泽教学视频_1532677.json', encoding='utf-8') as f:
        epr_dict = json.load(f)['data']
        epr = EasyPathologyRecord(**epr_dict)
        print(epr)
