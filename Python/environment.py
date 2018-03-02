import atexit
import io
import json
import os
import socket
import traceback
import struct
import time

from exception import UnityEnvironmentException, UnityActionException, UnityTimeOutException

class UnityEnvironment(object):
    def __init__(self, base_port=5006):
        atexit.register(self.close)
        self.port = base_port
        self._buffer_size = 12000
        self._python_api = "API-2"
        self._loaded = False
        self._open_socket = False

        print "socket port is:"+str(self.port)+" api: "+str(self._python_api)

        try:
            # Establish communication socket
            self._socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            self._socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
            self._socket.bind(("localhost", self.port))
            self._open_socket = True
        except socket.error:
            self._open_socket = True
            self.close()
            raise socket.error("Couldn't launch new environment because worker number is still in use. ")
        
        self._socket.settimeout(30)
        try:
            try:
                self._socket.listen(1)
                self._conn, _ = self._socket.accept()
                self._conn.settimeout(30)
                p = self._conn.recv(self._buffer_size).decode('utf-8')
                p = json.loads(p)
            except socket.timeout as e:
                raise UnityTimeOutException("The Unity environment took too long to respond. Make sure does not need user interaction to ")

            self._unity_api = p["apiNumber"]
            self._academy_name = p["AcademyName"]
            self._log_path = p["logPath"]
            self._brain_names = p["brainNames"]
            self._external_brain_names = p["externalBrainNames"]
            print self._log_path
            print self._unity_api
            print self._external_brain_names
            print self._brain_names
            self._loaded = True
            self._recv_bytes()
        except UnityEnvironmentException:
            print("The Unity environment exception")
            self.close()
            raise


    def _recv_bytes(self):
        try:
            s = self._conn.recv(self._buffer_size)
            message_length = struct.unpack("I", bytearray(s[:4]))[0]
            s = s[4:]
            while len(s) != message_length:
                s += self._conn.recv(self._buffer_size)
            print("rcv: "+s)
            if s == "EXIT":
                self.close()
            elif s == "ACTION":
                self._send_action("1024M")
                self._recv_bytes()
            else:
                self._recv_bytes()
        except socket.timeout as e:
            # raise UnityTimeOutException("The environment took too long to respond.", self._log_path)
            print("timeout, will close socket")
            self.close()



    def _send_action(self, memory):
        try:
            print("send action")
            action_message = {"memory": memory, "time": time.strftime('%Y-%m-%d %H:%M:%S',time.localtime(time.time()))}
            self._conn.send(json.dumps(action_message).encode('utf-8'))
        except UnityEnvironmentException:
            raise 
        

    def close(self):
        print "env req close with arg, _loaded:"+str(self._loaded)+" _open_socket:"+str(self._open_socket)
        # traceback.print_stack()
        if self._loaded & self._open_socket:
            self._conn.send(b"EXIT")
            self._conn.close()
        if self._open_socket:
            self._socket.close()
            self._loaded = False
        else:
            raise UnityEnvironmentException("No Unity environment is loaded.")
