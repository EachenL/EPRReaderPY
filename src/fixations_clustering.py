from sklearn.cluster import DBSCAN
from sklearn.metrics.pairwise import euclidean_distances
import numpy as np

def dbscan_clustering(data, eps=50, min_samples=10):
    # 将数据转换为坐标数组和时间戳数组
    coords = [(x, y) for timestamp, x, y in data]
    timestamps = [timestamp for timestamp, x, y in data]
    X = np.array(coords)
    
    # 使用DBSCAN算法进行聚类
    dbscan = DBSCAN(eps=eps, min_samples=min_samples)
    dbscan.fit(X)
    
    # 计算聚类半径和中心坐标
    labels = dbscan.labels_
    unique_labels = set(labels) - {-1}
    centers = []
    radii = []
    start_times = []
    end_times = []
    for label in unique_labels:
        indices = np.where(labels == label)[0]
        if len(indices) > 0:
            cluster_points = X[indices]
            cluster_timestamps = np.array(timestamps)[indices]
            # 计算聚类中心
            center = np.mean(cluster_points, axis=0)
            centers.append(center)
            # 计算聚类半径
            distances = euclidean_distances(cluster_points, [center])
            radius = np.max(distances)
            radii.append(radius)
            # 计算聚类起始时间和结束时间
            start_time = np.min(cluster_timestamps)
            end_time = np.max(cluster_timestamps)
            start_times.append(start_time)
            end_times.append(end_time)
    
    # 输出聚类结果
    n_clusters = len(unique_labels)
    print("聚类数量：", n_clusters)
    print("聚类标签：", labels)
    print("聚类中心坐标：", centers)
    print("聚类半径：", radii)
    print("聚类起始时间：", start_times)
    print("聚类结束时间：", end_times)
    
    return labels, centers, radii, start_times, end_times

import hdbscan

def hdbscan_clustering(data, min_cluster_size=10):
    # 将数据转换为坐标数组
    coords = [(x, y) for timestamp, x, y in data]
    X = np.array(coords)
    
    # 使用HDBSCAN算法进行聚类
    hdbscan_clusterer = hdbscan.HDBSCAN(min_cluster_size=min_cluster_size)
    hdbscan_labels = hdbscan_clusterer.fit_predict(X)
    
    # 计算聚类半径和中心坐标
    unique_labels = set(hdbscan_labels) - {-1}
    centers = []
    radii = []
    for label in unique_labels:
        indices = np.where(hdbscan_labels == label)[0]
        if len(indices) > 0:
            cluster_points = X[indices]
            # 计算聚类中心
            center = np.mean(cluster_points, axis=0)
            centers.append(center)
            # 计算聚类半径
            distances = euclidean_distances(cluster_points, [center])
            radius = np.max(distances)
            radii.append(radius)
    
    # 计算聚类起始时间和结束时间
    start_times = []
    end_times = []
    for label in unique_labels:
        indices = np.where(hdbscan_labels == label)[0]
        if len(indices) > 0:
            cluster_timestamps = [data[i][0] for i in indices]
            start_time = np.min(cluster_timestamps)
            end_time = np.max(cluster_timestamps)
            start_times.append(start_time)
            end_times.append(end_time)
    
    # 输出聚类结果
    n_clusters = len(unique_labels)
    print("聚类数量：", n_clusters)
    print("聚类标签：", hdbscan_labels)
    print("聚类中心坐标：", centers)
    print("聚类半径：", radii)
    print("聚类起始时间：", start_times)
    print("聚类结束时间：", end_times)
    
    return hdbscan_labels, centers, radii, start_times, end_times
