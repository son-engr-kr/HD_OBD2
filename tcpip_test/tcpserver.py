import socket
import time
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind(('127.0.0.1', 3333))
print("listen...")
server_socket.listen(0)

client_socket, addr = server_socket.accept()
print("accept!!")
while True:
    client_socket.send(b"hello")
    time.sleep(1)