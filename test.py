class Solution:

    def getMinLength(self, pearls ):
        min = 0
        for i in pearls:
            q = pearls.copy()
            q.remove(i)
            for t in q:
                length1 = abs(i[0]-t[0])+abs(i[1]-t[1])
                qq = pearls.copy()
                qq.remove(i)
                qq.remove(t)
                for s in qq:
                    length2 = abs(t[0]-s[0])+abs(t[1]-s[1])
                    length = length1+length2
                    if(length<min or min==0):
                        min = length
        return min


if __name__ == '__main__':
    # pearls = [[0, 0], [0, 2], [1, 1], [10, 10]]
    # pearls = [[0, 0], [1, 2], [5, 6], [8, 9], [0, 2]]
    pearls = [[1,1],[1,2],[3,1],[1,7],[2,6],[2,3],[2,8],[3,6]]
    # for i in range(len(pearls)):
    #     i.append(1)
    a = Solution()
    minimus = a.getMinLength(pearls)
    i = 0

