import subprocess
import os
import signal
a="blah"


def main(): 
            selection = input("Welcome to the Attacker Toolkit! \n\nHere you can perform a UDP Flood DoS attack, ArpSpoof with MITM UDP Data Modification, or ArpSpoof DoS attack! \n\nSelect your option (1, 2, or 3): ")
            selection  = int(selection)
            if selection == 1:
                #run udpdos.exe
                a= subprocess.call(os.path.abspath(os.getcwd()+"/background/udpdos.exe"))

                try:
                    print("Attack Beginning: \n\n")
                except KeyboardInterrupt:
                    a.send_signal(signal.SIGINT)
            elif selection == 2:
                print("This attack is meant to target a (mock and very basic) DeltaV System.")
                ip1 = input("Input the IP Address of the machine providing xml data (App or Pro+ machine):")
                ip2 = input("Input the IP Address of the machine requesting xml data (Opperator Station):")
                a= subprocess.run([os.path.abspath(os.getcwd()+"/background/arpspoofer.exe"), ip1, ip2])
                a= subprocess.run([os.path.abspath(os.getcwd()+"/background/packetmoder.exe"), ip1])
                try:
                    print("Attack Beginning... \n\n")

                except KeyboardInterrupt:
                    a.send_signal(signal.SIGINT)
            elif selection == 3:
                print("This attack is meant to target a (mock and very basic) DeltaV System.")
                ip1 = input("Input the IP Address of the machine providing xml data (App or Pro+ machine):")
                ip2 = input("Input the IP Address of the machine requesting xml data (Opperator Station):")
                a= subprocess.run([os.path.abspath(os.getcwd()+"/background/arpspoofer.exe"), ip1, ip2])
                try:
                    print("Attack Running...")
                except KeyboardInterrupt:
                    a.send_signal(signal.SIGINT)
            else:
                print("Unknown Selection. Aborting." + selection)
        
            print("Exiting...")


  

if __name__=="__main__":
    try:
        main() 
    except KeyboardInterrupt:
        print("Exiting, no function selected.")