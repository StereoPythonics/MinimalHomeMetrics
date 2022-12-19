import sys
print(sys.path)
sys.path.append('/home/pi/.local/lib/python2.7/site-packages')
print(sys.path)
import wiringpi as wp

from time import sleep

wp.wiringPiSetupGpio()
wp.pinMode(13,2)
wp.pinMode(12,1)

wp.digitalWrite(12,1)
sleep(1)

for x in range(1,200):
	wp.pwmWrite(13,(1000-(x*5)))
	sleep(0.01)

wp.pwmWrite(13,0)
wp.digitalWrite(12,0)
quit()
