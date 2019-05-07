import socket
import sys

def main():
    """Expects team to be a string and station to be an integer"""
    if len(sys.argv) >= 3:
        team = sys.argv[1]
        station = sys.argv[2]
        
        host = "192.168.0.101"
        port = 5000

        s = socket.socket()
        s.connect((host, port))

        message = team + str(station)

        data = "Not sent"
        while data == "Not sent":
            s.send(message.encode("UTF-8"))
            data = s.recv(1024).decode("UTF-8")

        s.close()
    else:
        team = "red"
        station = 2
        
        host = "192.168.0.101"
        port = 5000

        s = socket.socket()
        s.connect((host, port))

        message = team + str(station)

        data = "Not sent"
        while data == "Not sent":
            s.send(message.encode("UTF-8"))
            data = s.recv(1024).decode("UTF-8")

        s.close()

main()
