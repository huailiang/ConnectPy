#!/usr/bin/env python

import socket
import time
from threading import Thread


def RecvData(s: socket):
    while True:
        data = s.recvfrom(1024)
        print(data)
        # print(data.encode('utf-8'))


addr = ('127.0.0.1', 8900)
s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
s.bind(('', 8901))

tr = Thread(target=RecvData, args=[s])
tr.start()
while True:
    data = "hello world"
    s.sendto(data.encode('utf-8'), addr)
    time.sleep(0.5)

s.close()
