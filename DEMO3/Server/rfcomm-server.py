from bluetooth import *

port = 22

server_sock=BluetoothSocket( RFCOMM )
server_sock.bind(("",port))
server_sock.listen(22)
                   
print("Waiting for connection on RFCOMM channel %d" % port)

client_sock, client_info = server_sock.accept()
print("Accepted connection from ", client_info)
client_sock.send("welcome!")
try:
    while True:
        data = client_sock.recv(1024)
        if len(data) == 0: break
        print("received [%s]" % data)

        if(data == "open"):
            open_message = "opening door, welcome"
            client_sock.send(open_message)
            print("opening door")

except IOError:
    pass

print("disconnected")

client_sock.close()
server_sock.close()
print("all done")
