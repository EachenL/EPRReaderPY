import json
from epr_reader.bindings.easy_pathology_record import EasyPathologyRecord

if __name__ == "__main__":
    with open(r'samples\吴泽教学视频_1532677.json', encoding='utf-8') as f:
        epr_dict = json.load(f)['data']
        epr = EasyPathologyRecord(**epr_dict)
        print(epr)
