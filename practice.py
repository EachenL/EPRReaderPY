from random import random
from time import perf_counter
import threading
darts = 10000*10000

start = perf_counter()
hits = [0,0,0,0,0,0,0,0,0,0,0]
threadnum = 10
threads = []
def calcnums(start,end,thread):
    for i in range(start, end):
        x, y = random(), random()
        dist = pow(x ** 2 + y ** 2, 0.5)
        if dist <= 1.0:
            hits[thread] = hits[thread] + 1
        print(i)
    print('thread {} have done', thread)



if __name__ == '__main__':
    for i in range(1, threadnum):
        t = threading.Thread(target=calcnums, args=(10000000 * i, 10000000 * (i + 1), i))
        threads.append(t)
    for t in threads:
        t.setDaemon(True)
        t.start()

    # pi = 4 * (hits / darts)
    # print(format(pi))





